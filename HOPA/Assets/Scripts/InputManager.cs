using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using UnityEditorInternal;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InputEventClickUp : UnityEvent<Vector2, Collider2D> { }
public class InputEventClickDown : UnityEvent<Vector2, Collider2D> { }
public class InputEventHold : UnityEvent<Vector2, Collider2D> { }
public class InputEventZoom : UnityEvent<float> { }
public class InputEventMove : UnityEvent<Vector2, Vector2, Collider2D> { }  //source, direction
public class InputEventSwipe : UnityEvent<Vector2, InputManager.SwipeDirection, float, Collider2D> { } // length
public class InputEventShake : UnityEvent<Vector3> { }

public class InputManager : Singleton<InputManager>
{
    #region enums

    public enum SwipeDirection
    {
        UP, 
        RIGHT,
        DOWN,
        LEFT
    }

    #endregion

    #region public

    public InputEventClickUp OnInputClickUp = new InputEventClickUp();
    public InputEventClickDown OnInputClickDown = new InputEventClickDown();
    public InputEventHold OnInputHold = new InputEventHold();
    public InputEventZoom OnInputZoom = new InputEventZoom();
    public InputEventMove OnInputMove = new InputEventMove();
    public InputEventMove OnInputMoveExclusive = new InputEventMove();
    public InputEventSwipe OnInputSwipe = new InputEventSwipe();
    public InputEventShake OnInputShake = new InputEventShake();

    public bool InputAllEventsEnabled = true;       // does not take swipe into consideration because I'm lazy
    public bool InputClickUpEventsEnabled = true;
    public bool InputClickDownEventsEnabled = true;
    public bool InputHoldEventsEnabled = true;
    public bool InputZoomEventsEnabled = true;
    public bool InputMoveEventsEnabled = true;
    public bool InputMoveEventsExclusiveEnabled = true;
    public bool InputSwipeEventsEnabled = true;
    public bool InputShakeEventsEnabled = true;

    public float SwipePercentageOfScreenDiag = 50.0f;
    public float ShakeVectorMinimumSqrLength = 10.0f;
    public float ShakeVectorMinimumSqrLengthPC = 1000.0f;
    public float ShakeTimeMinBetweenSeconds = 2.0f;

    #endregion

    #region properties

    public Vector2 CursorCurrentPosition { get; private set; }

    #endregion

    #region private

    private float _swipeMinimumLength;

    private Vector2 _cursorPrevPosition;
    private Vector2 _cursor2PrevPosition;
    private bool _canInvokeMoveExclusive = false;
    private Collider2D _invokeMoveExclusiveCollider2DHelper = null;
    private Collider2D _dummyCollider2D;

    private Collider2D _swipeHelperCol;
    private Vector2 _swipeHelperPos;

    private Vector3 _shakeLast;
    private float _shakeLastTimeSeconds = 0.0f;

    private float _diffPinchHelper;
    private bool _isPointerOnGui;
    private bool _isPointerOnGuiOld;
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

        float wFlt = (float)Screen.width;
        float hFlt = (float)Screen.height;
        _swipeMinimumLength = SwipePercentageOfScreenDiag * 0.01f * Mathf.Sqrt(wFlt * wFlt + hFlt * hFlt);
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
                Collider2D colUC = GetCollider2DUnderCursor();

                // ClickUp
                if (cTouch1.phase == TouchPhase.Ended)
                {
                    if (InputClickUpEventsEnabled && InputAllEventsEnabled /*&& !_isMove*/)
                    {
                        OnInputClickUp.Invoke(cTouch1.position, colUC);

                        if (_canInvokeMoveExclusive)
                        {
                            _canInvokeMoveExclusive = false;
                            _invokeMoveExclusiveCollider2DHelper = null;
                        }
                    }
                    //else if(_isMove)
                    //{
                    //    _isMove = false;
                    //}
                }

                // SWIPE OUT 
                if (cTouch1.phase == TouchPhase.Ended && InputSwipeEventsEnabled)
                {
                    Vector2 diff = cTouch1.position - _swipeHelperPos;
                    if (/*colUC == _swipeHelperCol &&*/ diff.magnitude >= _swipeMinimumLength)
                    {
                        SwipeDirection dir = GetSwipeDirectionFromVector(diff);
                        Debug.Log("SWIPE OUT " + dir.ToString());
                        OnInputSwipe.Invoke(_swipeHelperPos, dir, diff.magnitude, colUC);
                        _swipeHelperCol = null;
                        _swipeHelperPos = Vector2.zero;
                    }
                }

                // ClickDown
                if (cTouch1.phase == TouchPhase.Began && InputClickDownEventsEnabled && InputAllEventsEnabled)
                {
                    _cursorPrevPosition = cTouch1.position;
                    OnInputClickDown.Invoke(cTouch1.position, colUC);
                }

                // Swipe IN
                if(cTouch1.phase == TouchPhase.Began && InputSwipeEventsEnabled)
                {
                    Debug.Log("SWIPE IN");
                    _swipeHelperCol = colUC;
                    _swipeHelperPos = cTouch1.position;
                }

                // hold
                if (cTouch1.phase == TouchPhase.Stationary && InputHoldEventsEnabled && InputAllEventsEnabled)
                {
                    OnInputHold.Invoke(cTouch1.position, colUC);
                }

                // move
                if (cTouch1.phase == TouchPhase.Moved && InputMoveEventsEnabled && InputAllEventsEnabled)
                {
                    if (OnInputMove != null) OnInputMove.Invoke(cTouch1.position, cTouch1.position - _cursorPrevPosition, colUC);

                    //if(!_isMove)
                    //{
                    //    _isMove = true;
                    //}

                    if(!_canInvokeMoveExclusive)
                    {
                        _canInvokeMoveExclusive = true;
                        _invokeMoveExclusiveCollider2DHelper = colUC;
                    }
                    else
                    {
                        if (OnInputMoveExclusive != null && InputMoveEventsExclusiveEnabled && InputAllEventsEnabled) OnInputMoveExclusive.Invoke(cTouch1.position, cTouch1.position - _cursorPrevPosition, _invokeMoveExclusiveCollider2DHelper);
                    }
                }

                _cursorPrevPosition = cTouch1.position;
            }
            else if(Input.touchCount == 2)
            {
                Touch cTouch1 = Input.GetTouch(0);
                Touch cTouch2 = Input.GetTouch(1);

                // zoom
                if (cTouch1.phase == TouchPhase.Moved && cTouch2.phase == TouchPhase.Moved && InputZoomEventsEnabled && InputAllEventsEnabled)
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

                        if (OnInputZoom != null) OnInputZoom.Invoke(amount);
                    }
                }

                _cursorPrevPosition = cTouch1.position;
                _cursor2PrevPosition = cTouch2.position;
            }


            // shake
            if(InputShakeEventsEnabled && InputAllEventsEnabled)
            {
                Vector3 shakeInput = Input.acceleration;

                if(shakeInput.sqrMagnitude >= ShakeVectorMinimumSqrLength &&
                    Time.time - _shakeLastTimeSeconds >= ShakeTimeMinBetweenSeconds)
                {
                    _shakeLastTimeSeconds = Time.time;
                    Debug.Log("SHAKEKURWA " + shakeInput.sqrMagnitude.ToString());
                    OnInputShake.Invoke(shakeInput);
                }
            }
        }
            
        // code for editor or PC test input
        else
        {
            CursorCurrentPosition = Input.mousePosition;
            Collider2D colUC = GetCollider2DUnderCursor();

            // ClickUp
            if (Input.GetMouseButtonUp(0) && InputClickUpEventsEnabled && InputAllEventsEnabled)
            {
                OnInputClickUp.Invoke(Input.mousePosition, colUC);
            }

            // SWIPE OUT 
            if (Input.GetMouseButtonUp(0) && InputSwipeEventsEnabled)
            {
                Vector2 diff = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _swipeHelperPos;
                if (/*colUC == _swipeHelperCol &&*/ diff.magnitude >= _swipeMinimumLength)
                {
                    OnInputSwipe.Invoke(_swipeHelperPos, GetSwipeDirectionFromVector(diff), diff.magnitude, colUC);
                    _swipeHelperCol = null;
                    _swipeHelperPos = Vector2.zero;
                }
            }

            // ClickDown
            if (Input.GetMouseButtonDown(0) && InputClickDownEventsEnabled && InputAllEventsEnabled)
            {
                OnInputClickDown.Invoke(Input.mousePosition, colUC);
            }

            // Swipe IN
            if (Input.GetMouseButtonDown(0) && InputSwipeEventsEnabled)
            {
                _swipeHelperCol = colUC;
                _swipeHelperPos = Input.mousePosition;
            }

            // hold
            if (Input.GetMouseButton(0) && InputHoldEventsEnabled && InputAllEventsEnabled)
            {
                OnInputHold.Invoke(Input.mousePosition, colUC);
            }

            // move
            if(Input.GetMouseButton(1) && _cursorPrevPosition != new Vector2(Input.mousePosition.x, Input.mousePosition.y) && InputMoveEventsEnabled && InputAllEventsEnabled)
            {
                OnInputMove.Invoke(Input.mousePosition, new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _cursorPrevPosition, colUC);

                if (!_canInvokeMoveExclusive)
                {
                    _canInvokeMoveExclusive = true;
                    _invokeMoveExclusiveCollider2DHelper = colUC;
                }
                else
                {
                    if (OnInputMoveExclusive != null && InputMoveEventsExclusiveEnabled && InputAllEventsEnabled) OnInputMoveExclusive.Invoke(Input.mousePosition, new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _cursorPrevPosition, _invokeMoveExclusiveCollider2DHelper);
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
            if (Input.mouseScrollDelta.y != 0.0f && InputZoomEventsEnabled && InputAllEventsEnabled)
            {
                OnInputZoom.Invoke(-Input.mouseScrollDelta.y);
            }

            _cursorPrevPosition = Input.mousePosition;
        }

        // shake simulation
        if (InputShakeEventsEnabled && InputAllEventsEnabled)
        {
                Vector3 mPositionNoZ = Input.mousePosition;
                mPositionNoZ.z = 0.0f;
                Vector3 cShakeDiff = mPositionNoZ - _shakeLast;
                _shakeLast = mPositionNoZ;

                if (cShakeDiff.sqrMagnitude >= ShakeVectorMinimumSqrLengthPC &&
                    Input.GetMouseButton(2) &&
                    Time.time - _shakeLastTimeSeconds >= ShakeTimeMinBetweenSeconds)
                {
                    _shakeLastTimeSeconds = Time.time;
                    OnInputShake.Invoke(cShakeDiff);
                }
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
        return Physics2D.RaycastAll(clickPos, clickPos, 0.01f, Physics2D.DefaultRaycastLayers, -10.0f, 10.0f);
    }

    public bool IsPointerOnGui()
    {
        //Debug.Log("IPG EXTERNAL: " + _isPointerOnGui.ToString());
        if (Application.isMobilePlatform)
            return _isPointerOnGuiOld;
        else
            return _isPointerOnGui;
    }

    private bool IsPointerOnGuiInternal()
    {
        int pointerID = 0;
        if (!Application.isMobilePlatform)
        {
            pointerID = -1;
        }

        // returning previous - on mobile it seems to fix that annoying bug
        _isPointerOnGuiOld = _isPointerOnGui;
        _isPointerOnGui = EventSystem.current.IsPointerOverGameObject(pointerID);
        //Debug.Log("IPG INTERNAL: " + _isPointerOnGui.ToString());

        return IsPointerOnGui();
    }

    public Collider2D GetCollider2DUnderCursor()
    {
        //for(int i = 0; i < _sLayerCount; ++i)
        //{
        //    _sortLayerList[i].Value.Clear();
        //}

        RaycastHit2D[] hits = GetRaycastHitsUnderCursor();
        int hitCount = hits.Length;

        //List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
        //PointerEventData pData = new PointerEventData(EventSystem.current);
        //EventSystem.current.RaycastAll(pData, uiRaycastResults);

        

        //if(uiRaycastResults.Count != 0)
        if (IsPointerOnGuiInternal())
        {
            //Debug.Log("IM: UI HIT");
            return _dummyCollider2D;    // for camera manager purpose
        }
        else if(hitCount != 0)
        {
            //Debug.Log("IM: OBJ HIT");
            return hits[0].collider;
        }
        else
        {
            //Debug.Log("IM: NTH HIT");
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

    private SwipeDirection GetSwipeDirectionFromVector(Vector2 diff)
    {
        SwipeDirection dir = 0;

        diff.Normalize();
        diff.x = -diff.x;
        Vector2[] unitVectors = new Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
        int selectedID = 0;

        float angle, coangle, minAngle = 360.0f;

        for (int i = 0; i < 4; ++i)
        {
            angle = Vector2.Angle(diff, unitVectors[i]);
            coangle = 360.0f - angle;

            angle = Mathf.Min(angle, coangle);
            if(angle < minAngle)
            {
                minAngle = angle;
                selectedID = i;
            }
        }

        dir = (SwipeDirection)selectedID;
        return dir;
    }

    #endregion
}
