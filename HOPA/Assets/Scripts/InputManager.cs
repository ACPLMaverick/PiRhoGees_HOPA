using UnityEngine;
using System.Collections;

public class InputManager : Singleton<InputManager>
{

    #region public

    public delegate void InputClickUpEvent(Vector2 screenPos, Collider hitCollider);
    public delegate void InputClickDownEvent(Vector2 screenPos, Collider hitCollider);
    public delegate void InputHoldEvent(Vector2 screenPos, Collider hitCollider);
    public delegate void InputZoomEvent(float amount);
    public delegate void InputMoveEvent(Vector2 currentScreenPos, Vector2 direction, Collider hitCollider);

    public static event InputClickUpEvent OnInputClickUp;
    public static event InputClickDownEvent OnInputClickDown;
    public static event InputHoldEvent OnInputHold;
    public static event InputZoomEvent OnInputZoom;
    public static event InputMoveEvent OnInputMove;
    public static event InputMoveEvent OnInputMoveExclusive;

    #endregion

    #region private

    private Vector2 _cursorPrevPosition;
    private bool _canInvokeMoveExclusive = false;
    private Collider _invokeMoveExclusiveColliderHelper = null;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        _cursorPrevPosition = new Vector2();
	}
	
	// Update is called once per frame
	void Update ()
    {
        // code for mobile phone input
	    if(Application.isMobilePlatform)
        {
            Touch cTouch1 = Input.GetTouch(0);
            Touch cTouch2 = Input.GetTouch(1);

            if(Input.touchCount == 1)
            {
                // ClickUp
                if (cTouch1.phase == TouchPhase.Ended && OnInputClickUp != null)
                {
                    OnInputClickUp(cTouch1.position, GetColliderUnderCursor());

                    if(_canInvokeMoveExclusive)
                    {
                        _canInvokeMoveExclusive = false;
                        _invokeMoveExclusiveColliderHelper = null;
                    }
                }

                // ClickDown
                if (cTouch1.phase == TouchPhase.Began && OnInputClickDown != null)
                {
                    OnInputClickDown(cTouch1.position, GetColliderUnderCursor());
                }

                // hold
                if (cTouch1.phase == TouchPhase.Stationary && OnInputHold != null)
                {
                    OnInputHold(cTouch1.position, GetColliderUnderCursor());
                }

                // move
                if (cTouch1.phase == TouchPhase.Moved)
                {
                    Collider uc = GetColliderUnderCursor();
                    if(OnInputMove != null) OnInputMove(cTouch1.position, cTouch1.position - _cursorPrevPosition, uc);

                    if(!_canInvokeMoveExclusive)
                    {
                        _canInvokeMoveExclusive = true;
                        _invokeMoveExclusiveColliderHelper = uc;
                    }
                    else
                    {
                        if(OnInputMoveExclusive != null) OnInputMoveExclusive(cTouch1.position, cTouch1.position - _cursorPrevPosition, _invokeMoveExclusiveColliderHelper);
                    }
                }
            }

            // zoom
            if (cTouch1.phase == TouchPhase.Moved && cTouch2.phase == TouchPhase.Moved && OnInputZoom != null && Input.touchCount == 2)
            {
                // pinch gesture
                // Two touch positions with given directions are in fact two rays. Checking if the rays intersect (zoom in) or not (zoom out)
                // more info: http://stackoverflow.com/questions/2931573/determining-if-two-rays-intersect
                Vector2 pos1 = cTouch1.position;
                Vector2 pos2 = cTouch2.position;
                Vector2 delta1 = cTouch1.deltaPosition;
                Vector2 delta2 = cTouch2.deltaPosition;
                float u, v;

                u = (pos1.y * delta2.x + delta2.y * pos2.x - pos2.y * delta2.x - delta2.y * pos1.x) / (delta1.x * delta2.y - delta1.y * delta2.x);
                v = (pos1.x + delta1.x * u - pos2.x) / delta2.x;

                // rays intersect - zoom in
                float amount = (delta1.sqrMagnitude + delta2.sqrMagnitude) * 0.25f;
                
                // rays do not intersect - zoom out
                if(u <= 0.0f || v <= 0.0f)
                {
                    amount *= -1.0f;
                }

                OnInputZoom(amount);
            }

            _cursorPrevPosition = cTouch1.position;
        }
        // code for editor or PC test input
        else
        {
            // ClickUp
            if(Input.GetMouseButtonUp(0) && OnInputClickUp != null)
            {
                OnInputClickUp(Input.mousePosition, GetColliderUnderCursor());
            }

            // ClickDown
            if (Input.GetMouseButtonDown(0) && OnInputClickDown != null)
            {
                OnInputClickDown(Input.mousePosition, GetColliderUnderCursor());
            }

            // hold
            if (Input.GetMouseButton(0) && OnInputHold != null)
            {
                OnInputHold(Input.mousePosition, GetColliderUnderCursor());
            }

            // move
            if(Input.GetMouseButton(1) && _cursorPrevPosition != new Vector2(Input.mousePosition.x, Input.mousePosition.y))
            {
                Collider uc = GetColliderUnderCursor();
                if(OnInputMove != null) OnInputMove(Input.mousePosition, new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _cursorPrevPosition, uc);

                if (!_canInvokeMoveExclusive)
                {
                    _canInvokeMoveExclusive = true;
                    _invokeMoveExclusiveColliderHelper = uc;
                }
                else
                {
                    if (OnInputMoveExclusive != null) OnInputMoveExclusive(Input.mousePosition, new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _cursorPrevPosition, _invokeMoveExclusiveColliderHelper);
                }
            }

            // move exclusive cleanup
            if(Input.GetMouseButtonUp(1))
            {
                if (_canInvokeMoveExclusive)
                {
                    _canInvokeMoveExclusive = false;
                    _invokeMoveExclusiveColliderHelper = null;
                }
            }

            // zoom
            if (Input.mouseScrollDelta.y != 0.0f && OnInputZoom != null)
            {
                OnInputZoom(-Input.mouseScrollDelta.y);
            }

            _cursorPrevPosition = Input.mousePosition;
        }
    }

    private Collider GetColliderUnderCursor()
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
        RaycastHit hit;
        Physics.Raycast(new Ray(clickPos, new Vector3(0.0f, 0.0f, 1.0f)), out hit);

        return hit.collider;
    }

    #endregion
}
