using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class AssignableObjectUnityEvent : UnityEvent<AssignableObject> { }

public class AssignableObject : MonoBehaviour
{
    #region events

    public AssignableObjectUnityEvent AssignedEvent;
    public Color AssignedColor;

    #endregion

    #region public

    public AssigneeObject Assignee;
    public float PickedUpScaleMultiplier = 1.5f;
    public float PutDownPositionFixTimeSeconds = 0.5f;

    #endregion

    #region properties

    public bool IsAssigned { get; protected set; }

    #endregion

    #region protected

    protected bool _isDrag;
    protected Vector2 _vMin;
    protected Vector2 _vMax;
    protected TextMesh _textMesh;
    protected Vector2 _beginPosition;
    protected AssigneeObject _tempAssignee = null;

    #endregion

    #region functions

    // Use this for initialization
    protected void Start ()
    {
        // setting transform min and max fixed on to room corners, as they aren't taken into consideration
        // mode is also arbitrarily set to box

        AssignedEvent = new AssignableObjectUnityEvent();

        Room parentRoom = GetComponentInParent<Room>();
        if(parentRoom == null)
        {
            Debug.Log("nie umiesz");
            parentRoom = GetComponentInParent<RoomPuzzleAssign>();
        }
        SpriteRenderer bgSprite = parentRoom.GetComponent<SpriteRenderer>();
        Transform bgTransform = parentRoom.GetComponent<Transform>();

        _vMin = bgTransform.localToWorldMatrix * bgSprite.sprite.rect.min;
        _vMax = bgTransform.localToWorldMatrix * bgSprite.sprite.rect.max;

        // assigning callback functions to inputmanager events
        InputManager.OnInputClickDown += OnPickUp;
        InputManager.OnInputClickUp += OnPutDown;
        InputManager.OnInputMoveExclusive += OnDrag;

        _beginPosition = GetComponent<Transform>().position;

        _textMesh = GetComponentInChildren<TextMesh>();
        _textMesh.GetComponent<Renderer>().sortingLayerID = GetComponent<Renderer>().sortingLayerID;
        _textMesh.GetComponent<Renderer>().sortingOrder = 100 + GetComponent<Renderer>().sortingOrder;
	}
	
	// Update is called once per frame
	protected void Update ()
    {

	}

    protected void OnPickUp(Vector2 screenPos, Collider2D hitCollider2D)
    {
        if (hitCollider2D != null && hitCollider2D.gameObject == gameObject && !IsAssigned)
        {
            GetComponent<Transform>().localScale *= PickedUpScaleMultiplier;
            _isDrag = true;
        }
    }

    protected void OnPutDown(Vector2 screenPos, Collider2D hitCollider2D)
    {
        if (hitCollider2D != null && hitCollider2D.gameObject == gameObject && !IsAssigned)
        {
            GetComponent<Transform>().localScale /= PickedUpScaleMultiplier;

            RaycastHit2D[] hits = InputManager.Instance.GetRaycastHitsUnderCursor();
            int l = hits.Length;

            Vector2 lerpTargetPosition = _beginPosition;
            Vector2 lerpTargetScale = GetComponent<Transform>().localScale;
            AssigneeObject assignee = null;

            for (int i = 0; i < l; ++i)
            {
                if(hits[i].collider != null && (assignee = hits[i].collider.GetComponent<AssigneeObject>()) != null)
                {
                    lerpTargetPosition = assignee.AssignableSnapTransform.position;
                    lerpTargetScale = assignee.AssignableSnapTransform.localScale;

                    if(assignee == Assignee)
                    {
                        _tempAssignee = assignee;
                    }

                    break;
                }
            }

            StartCoroutine(FlyToPositionCoroutine(GetComponent<Transform>().position, lerpTargetPosition, GetComponent<Transform>().localScale, lerpTargetScale, PutDownPositionFixTimeSeconds));
        }
        else if(_isDrag)
        {
            GetComponent<Transform>().localScale /= PickedUpScaleMultiplier;
        }

        _isDrag = false;
    }

    protected void OnDrag(Vector2 currentScreenPos, Vector2 direction, Collider2D hitCollider2D)
    {
        if (hitCollider2D != null && hitCollider2D.gameObject == gameObject && !IsAssigned)
        {
            Vector3 wp1 = Camera.main.ScreenToWorldPoint(currentScreenPos);
            Vector3 wp2 = Camera.main.ScreenToWorldPoint(currentScreenPos + direction);
            Vector3 deltaP = wp2 - wp1;

            GetComponent<Transform>().position += deltaP;
        }
    }

    protected IEnumerator FlyToPositionCoroutine(Vector2 startPos, Vector2 targetPos, Vector2 startScale, Vector2 targetScale, float timeSeconds)
    {
        float cTime = Time.time;
        GetComponent<Transform>().position = startPos;
        GetComponent<Transform>().localScale = startScale;

        while (Time.time - cTime <= timeSeconds)
        {
            float lerpValue = (Time.time - cTime) / timeSeconds;
            Vector2 finalPos = Vector2.Lerp(startPos, targetPos, lerpValue);
            Vector2 finalScale = Vector2.Lerp(startScale, targetScale, lerpValue);
            GetComponent<Transform>().position = finalPos;
            GetComponent<Transform>().localScale = finalScale;
            yield return null;
        }
        GetComponent<Transform>().position = targetPos;
        GetComponent<Transform>().localScale = targetScale;

        if(_tempAssignee != null)
        {
            IsAssigned = true;
            AssignedEvent.Invoke(this);

            // change color in order to show player that his choice is correct
            GetComponent<SpriteRenderer>().color = AssignedColor;
        }

        yield return null;
    }

    #endregion
}
