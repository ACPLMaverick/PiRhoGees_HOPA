using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class AssignableObjectUnityEvent : UnityEvent<AssignableObject> { }

public class AssignableObject : MonoBehaviour
{
    #region events

    public AssignableObjectUnityEvent AssignedEvent = new AssignableObjectUnityEvent();
    public Color AssignedColor;

    #endregion

    #region public

    public AssigneeObject Assignee;
    public float PickedUpScaleMultiplier = 1.5f;
    public float PutDownPositionFixTimeSeconds = 0.5f;

    #endregion

    #region properties

    public bool IsAssigned { get; protected set; }
    public AssigneeObject CurrentAssignee { get; set; }

    #endregion

    #region protected

    protected bool _isDrag;
    protected Vector2 _vMin;
    protected Vector2 _vMax;
    protected TextMesh _textMesh;
    protected Vector3 _beginPosition;
    protected Vector3 _beginScale;
    protected AssigneeObject _tempAssignee = null;

    #endregion

    #region functions

    // Use this for initialization
    protected void Start ()
    {
        // setting transform min and max fixed on to room corners, as they aren't taken into consideration
        // mode is also arbitrarily set to box
        CurrentAssignee = null;

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
        InputManager.Instance.OnInputClickDown.AddListener(OnPickUp);
        InputManager.Instance.OnInputClickUp.AddListener(OnPutDown);
        InputManager.Instance.OnInputMoveExclusive.AddListener(OnDrag);

        _beginPosition = GetComponent<Transform>().position;
        _beginScale = GetComponent<Transform>().localScale;

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
        if(_isDrag)
        {
            GetComponent<Transform>().localScale /= PickedUpScaleMultiplier;
        }
        if (hitCollider2D != null && hitCollider2D.gameObject == gameObject && !IsAssigned)
        {
            if (CurrentAssignee != null)
            {
                CurrentAssignee.CurrentAssignable = null;
                CurrentAssignee = null;
            }

            Transform trans = GetComponent<Transform>();
            trans.localScale *= PickedUpScaleMultiplier;
            trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y, trans.localPosition.z - 1.0f);
            trans = _textMesh.GetComponent<Transform>();
            trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y, trans.localPosition.z - 1.0f);
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sortingOrder = 99;
            _isDrag = true;
        }
    }

    protected void OnPutDown(Vector2 screenPos, Collider2D hitCollider2D)
    {
        if ((hitCollider2D != null && hitCollider2D.gameObject == gameObject && !IsAssigned) || _isDrag)
        {
            Transform trans = GetComponent<Transform>();
            trans.localScale /= PickedUpScaleMultiplier;
            trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y, trans.localPosition.z + 1.0f);
            trans = _textMesh.GetComponent<Transform>();
            trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y, trans.localPosition.z + 1.0f);
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sortingOrder = 0;

            if (CurrentAssignee != null)
            {
                CurrentAssignee.CurrentAssignable = null;
                CurrentAssignee = null;
            }

            RaycastHit2D[] hits = InputManager.Instance.GetRaycastHitsUnderCursor();
            int l = hits.Length;

            Vector3 lerpTargetPosition = _beginPosition;
            Vector3 lerpTargetScale = _beginScale;
            AssigneeObject assignee = null;


            for (int i = 0; i < l; ++i)
            {
                if(hits[i].collider != null && 
                    (assignee = hits[i].collider.GetComponent<AssigneeObject>()) != null && 
                    assignee.CurrentAssignable == null)
                {
                    lerpTargetPosition = assignee.AssignableSnapTransform.position;
                    lerpTargetScale = assignee.AssignableSnapTransform.localScale;

                    assignee.CurrentAssignable = this;
                    CurrentAssignee = assignee;

                    if(assignee == Assignee)
                    {
                        _tempAssignee = assignee;
                    }

                    break;
                }
            }

            _isDrag = false;
            StartCoroutine(FlyToPositionCoroutine(GetComponent<Transform>().position, lerpTargetPosition, GetComponent<Transform>().localScale, lerpTargetScale, PutDownPositionFixTimeSeconds));
        }
    }

    protected void OnDrag(Vector2 currentScreenPos, Vector2 direction, Collider2D hitCollider2D)
    {
        if (hitCollider2D != null && hitCollider2D.gameObject == gameObject && !IsAssigned)
        {
            Vector3 wp1 = Camera.main.ScreenToWorldPoint(currentScreenPos);
            Vector3 wp2 = Camera.main.ScreenToWorldPoint(currentScreenPos + direction);
            Vector3 deltaP = wp2 - wp1;
            deltaP.z = 0.0f;

            GetComponent<Transform>().position += deltaP;
        }
    }

    protected IEnumerator FlyToPositionCoroutine(Vector3 startPos, Vector3 targetPos, Vector3 startScale, Vector3 targetScale, float timeSeconds)
    {
        float cTime = Time.time;
        GetComponent<Transform>().position = startPos;
        GetComponent<Transform>().localScale = startScale;

        while (Time.time - cTime <= timeSeconds)
        {
            float lerpValue = (Time.time - cTime) / timeSeconds;
            Vector3 finalPos = Vector3.Lerp(startPos, targetPos, lerpValue);
            Vector3 finalScale = Vector3.Lerp(startScale, targetScale, lerpValue);
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
