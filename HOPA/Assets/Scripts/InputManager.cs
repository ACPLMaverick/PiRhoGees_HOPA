using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using UnityEditorInternal;
using System.Reflection;
using UnityEngine.EventSystems;

public class InputManager : Singleton<InputManager>
{

    #region public

    public delegate void InputClickUpEvent(Vector2 screenPos, Collider2D hitCollider2D);
    public delegate void InputClickDownEvent(Vector2 screenPos, Collider2D hitCollider2D);
    public delegate void InputHoldEvent(Vector2 screenPos, Collider2D hitCollider2D);
    public delegate void InputZoomEvent(float amount);
    public delegate void InputMoveEvent(Vector2 currentScreenPos, Vector2 direction, Collider2D hitCollider2D);

    public static event InputClickUpEvent OnInputClickUp;
    public static event InputClickDownEvent OnInputClickDown;
    public static event InputHoldEvent OnInputHold;
    public static event InputZoomEvent OnInputZoom;
    public static event InputMoveEvent OnInputMove;
    public static event InputMoveEvent OnInputMoveExclusive;

    public bool InputAllEventsEnabled = true;
    public bool InputClickUpEventsEnabled = true;
    public bool InputClickDownEventsEnabled = true;
    public bool InputHoldEventsEnabled = true;
    public bool InputZoomEventsEnabled = true;
    public bool InputMoveEventsEnabled = true;
    public bool InputMoveEventsExclusiveEnabled = true;

    #endregion

    #region properties

    public Vector2 CursorCurrentPosition { get; private set; }

    #endregion

    #region private

    private Vector2 _cursorPrevPosition;
    private Vector2 _cursor2PrevPosition;
    private bool _canInvokeMoveExclusive = false;
    private bool _isMove;
    private Collider2D _invokeMoveExclusiveCollider2DHelper = null;
    private Collider2D _dummyCollider2D;
    private float _diffPinchHelper;
    //private KeyValuePair<int, SortedList<int, Collider2D>>[] _sortLayerList;
    //private int _sLayerCount;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        //int[] sLayers = GetSortingLayerUniqueIDs();
        //_sLayerCount = sLayers.Length;
        //_sortLayerList = new KeyValuePair<int, SortedList<int, Collider2D>>[_sLayerCount];
        //for(int i = 0; i < _sLayerCount; ++i)
        //{
        //    _sortLayerList[i] = new KeyValuePair<int, SortedList<int, Collider2D>>(sLayers[i], new SortedList<int, Collider2D>());
        //}
        _dummyCollider2D = new Collider2D();
        _cursorPrevPosition = new Vector2();
        CursorCurrentPosition = _cursorPrevPosition;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // code for mobile phone input
	    if(Application.isMobilePlatform)
        {
            if(Input.touchCount == 1)
            {
                Touch cTouch1 = Input.GetTouch(0);
                CursorCurrentPosition = cTouch1.position;

                // ClickUp
                if (cTouch1.phase == TouchPhase.Ended)
                {
                    if (OnInputClickUp != null && InputClickUpEventsEnabled && InputAllEventsEnabled && !_isMove)
                    {
                        OnInputClickUp(cTouch1.position, GetCollider2DUnderCursor());

                        if (_canInvokeMoveExclusive)
                        {
                            _canInvokeMoveExclusive = false;
                            _invokeMoveExclusiveCollider2DHelper = null;
                        }
                    }
                    else if(_isMove)
                    {
                        _isMove = false;
                    }
                }

                // ClickDown
                if (cTouch1.phase == TouchPhase.Began && OnInputClickDown != null && InputClickDownEventsEnabled && InputAllEventsEnabled)
                {
                    OnInputClickDown(cTouch1.position, GetCollider2DUnderCursor());
                }

                // hold
                if (cTouch1.phase == TouchPhase.Stationary && OnInputHold != null && InputHoldEventsEnabled && InputAllEventsEnabled)
                {
                    OnInputHold(cTouch1.position, GetCollider2DUnderCursor());
                }

                // move
                if (cTouch1.phase == TouchPhase.Moved && InputMoveEventsEnabled && InputAllEventsEnabled)
                {
                    Collider2D uc = GetCollider2DUnderCursor();
                    if (OnInputMove != null) OnInputMove(cTouch1.position, cTouch1.position - _cursorPrevPosition, uc);

                    if(!_isMove)
                    {
                        _isMove = true;
                    }

                    if(!_canInvokeMoveExclusive)
                    {
                        _canInvokeMoveExclusive = true;
                        _invokeMoveExclusiveCollider2DHelper = uc;
                    }
                    else
                    {
                        if (OnInputMoveExclusive != null && InputMoveEventsExclusiveEnabled && InputAllEventsEnabled) OnInputMoveExclusive(cTouch1.position, cTouch1.position - _cursorPrevPosition, _invokeMoveExclusiveCollider2DHelper);
                    }
                }

                _cursorPrevPosition = cTouch1.position;
            }
            else if(Input.touchCount == 2)
            {
                Touch cTouch1 = Input.GetTouch(0);
                Touch cTouch2 = Input.GetTouch(1);

                // zoom
                if (cTouch1.phase == TouchPhase.Moved && cTouch2.phase == TouchPhase.Moved && OnInputZoom != null && InputZoomEventsEnabled && InputAllEventsEnabled)
                {
                    /*
                    // pinch gesture
                    // Two touch positions with given directions are in fact two rays. Checking if the rays intersect (zoom in) or not (zoom out)
                    // more info: http://stackoverflow.com/questions/2931573/determining-if-two-rays-intersect
                    Vector2 pos1 = cTouch1.position;
                    Vector2 pos2 = cTouch2.position;
                    Vector2 delta1 = pos1 - _cursorPrevPosition;
                    Vector2 delta2 = pos2 - _cursor2PrevPosition;
                    float u, v;

                    u = (pos1.y * delta2.x + delta2.y * pos2.x - pos2.y * delta2.x - delta2.y * pos1.x) / (delta1.x * delta2.y - delta1.y * delta2.x);
                    v = (pos1.x + delta1.x * u - pos2.x) / delta2.x;

                    // rays intersect - zoom in
                    float amount = (delta1.sqrMagnitude + delta2.sqrMagnitude) * 0.1f;

                    // rays do not intersect - zoom out
                    if(u == 0.0f || v == 0.0f || float.IsInfinity(u) || float.IsInfinity(v))
                    {
                        amount = 0.0f;
                    }
                    else if (u < 0.0f && v < 0.0f)
                    {
                        amount *= -1.0f;
                    }

                    string debugSTR = delta1.ToString() + " | " + delta2.ToString() + " | " + u.ToString() + " | " + v.ToString() + " | " + amount.ToString();

                    Debug.Log(debugSTR);
                    OnInputZoom(amount);
                    */

                    // bo chciałem być fajny a wyszło jak zwykle

                    Vector2 pos1 = cTouch1.position;
                    Vector2 pos2 = cTouch2.position;
                    Vector2 delta1 = pos1 - _cursorPrevPosition;
                    Vector2 delta2 = pos2 - _cursor2PrevPosition;

                    float dot = Vector2.Dot(delta1, delta2);

                    if(dot < 0.7f)
                    {
                        float fl = delta1.magnitude;
                        float sl = delta2.magnitude;
                        float mp = -0.05f;
                        float diff = (pos1 - pos2).magnitude;
                        if(diff - _diffPinchHelper < 0)
                        {
                            mp *= -1.0f;
                        }
                        _diffPinchHelper = diff;
                        float amount = (fl + sl) * mp;

                        if (OnInputZoom != null) OnInputZoom(amount);
                    }
                }

                _cursorPrevPosition = cTouch1.position;
                _cursor2PrevPosition = cTouch2.position;
            }
        }
            
        // code for editor or PC test input
        else
        {
            CursorCurrentPosition = Input.mousePosition;

            // ClickUp
            if(Input.GetMouseButtonUp(0) && OnInputClickUp != null && InputClickUpEventsEnabled && InputAllEventsEnabled)
            {
                OnInputClickUp(Input.mousePosition, GetCollider2DUnderCursor());
            }

            // ClickDown
            if (Input.GetMouseButtonDown(0) && OnInputClickDown != null && InputClickDownEventsEnabled && InputAllEventsEnabled)
            {
                OnInputClickDown(Input.mousePosition, GetCollider2DUnderCursor());
            }

            // hold
            if (Input.GetMouseButton(0) && OnInputHold != null && InputHoldEventsEnabled && InputAllEventsEnabled)
            {
                OnInputHold(Input.mousePosition, GetCollider2DUnderCursor());
            }

            // move
            if(Input.GetMouseButton(1) && _cursorPrevPosition != new Vector2(Input.mousePosition.x, Input.mousePosition.y) && InputMoveEventsEnabled && InputAllEventsEnabled)
            {
                Collider2D uc = GetCollider2DUnderCursor();
                if(OnInputMove != null) OnInputMove(Input.mousePosition, new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _cursorPrevPosition, uc);

                if (!_canInvokeMoveExclusive)
                {
                    _canInvokeMoveExclusive = true;
                    _invokeMoveExclusiveCollider2DHelper = uc;
                }
                else
                {
                    if (OnInputMoveExclusive != null && InputMoveEventsExclusiveEnabled && InputAllEventsEnabled) OnInputMoveExclusive(Input.mousePosition, new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _cursorPrevPosition, _invokeMoveExclusiveCollider2DHelper);
                }
            }

            // move exclusive cleanup
            if(Input.GetMouseButtonUp(1))
            {
                if (_canInvokeMoveExclusive)
                {
                    _canInvokeMoveExclusive = false;
                    _invokeMoveExclusiveCollider2DHelper = null;
                }
            }

            // zoom
            if (Input.mouseScrollDelta.y != 0.0f && OnInputZoom != null && InputZoomEventsEnabled && InputAllEventsEnabled)
            {
                OnInputZoom(-Input.mouseScrollDelta.y);
            }

            _cursorPrevPosition = Input.mousePosition;
        }

        // showing pause menu on back button pressed
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GameManager.Instance.ShowPauseMenu();
        }
    }

    public RaycastHit2D[] GetRaycastHitsUnderCursor()
    {
        Vector3 clickPos;
        if (Application.isMobilePlatform)
        {
            clickPos = Input.GetTouch(0).position;
        }
        else
        {
            clickPos = Input.mousePosition;
        }
        clickPos = Camera.main.ScreenToWorldPoint(clickPos);
        return Physics2D.RaycastAll(clickPos, clickPos, 0.01f);
    }

    public Collider2D GetCollider2DUnderCursor()
    {
        //for(int i = 0; i < _sLayerCount; ++i)
        //{
        //    _sortLayerList[i].Value.Clear();
        //}

        RaycastHit2D[] hits = GetRaycastHitsUnderCursor();
        int hitCount = hits.Length;

        if(EventSystem.current.IsPointerOverGameObject())
        {
            return _dummyCollider2D;    // for camera manager purpose
        }
        else if(hitCount != 0)
        {
            return hits[0].collider;
        }
        else
        {
            return null;
        }

        //for(int i = 0; i < hitCount; ++i)
        //{
        //    int layerID = hits[i].collider.gameObject.GetComponent<SpriteRenderer>().sortingLayerID;
        //    int orderInLayer = hits[i].collider.gameObject.GetComponent<SpriteRenderer>().sortingOrder;
        //    // find layer the collider's object is on
        //    SortedList<int, Collider2D> layer = null;
        //    for (int j = 0; j < _sLayerCount; ++j)
        //    {
        //        if(_sortLayerList[j].Key == layerID)
        //        {
        //            layer = _sortLayerList[j].Value;
        //            break;
        //        }
        //    }

        //    // add to that layer with given sort order. This will sort automatically.
        //    layer.Add(orderInLayer, hits[i].collider);
        //}

        //// iterate from the furthest layer and hightest order and pick first object found

        //Collider2D highestHit = null;
        //for (int i = _sLayerCount - 1; i > 0; --i)
        //{
        //    int mObjectCountOnLayer = _sortLayerList[i].Value.Count;

        //    if(mObjectCountOnLayer != 0)
        //    {
        //        highestHit = _sortLayerList[i].Value.Values[mObjectCountOnLayer - 1];
        //        break;
        //    }
        //}

        ////Debug.Log(highestHit.gameObject.name);

        //return highestHit;
    }

    // Get the unique sorting layer IDs -- tossed this in for good measure
    //public int[] GetSortingLayerUniqueIDs()
    //{
    //    Type internalEditorUtilityType = typeof(InternalEditorUtility);
    //    PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
    //    return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
    //}

    #endregion
}
