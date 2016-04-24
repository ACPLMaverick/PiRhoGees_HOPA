using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[System.Serializable]
public class PickableUsableObject : PickableObject
{
    #region events

    #endregion

    #region constants
    
    #endregion

    #region public

    public List<PickableUsableObject> InteractableObjects;  // sth else will be here
    public Dictionary<PickableUsableObject, List<PickableUsableObject>> MergableObjects;

    #endregion

    #region properties

    public bool IsInEquipment { get; protected set; }

    #endregion

    #region private

    private UsableContainer _container = null;
    private Vector2 _startSlotPosition;
    private bool _actionsLocked = false;
    private Transform _tempTransform = null;
    private Canvas _canvasForSelectedUsable = null;

    #endregion

    #region functions 

    // Use this for initialization
    protected override void Start ()
    {
        _canvasForSelectedUsable = FindObjectOfType<Canvas>();
        IsInEquipment = false;
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();
	}

    public void AssignEquipmentContainer(UsableContainer container)
    {
        _container = container;
        GetComponent<SpriteRenderer>().enabled = false;
        _container.UsableField.PointerUpEvent.AddListener(new UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData>(OnClickUpInEquipment));
        _container.UsableField.DragEvent.AddListener(new UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData>(OnClickHoldInEquipment));
        _container.UsableField.PointerDownEvent.AddListener(new UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData>(OnClickDownInEquipment));
    }

    protected override void PickUp(Vector2 position, Collider2D col)
    {
        if (col != null && col.gameObject == this.gameObject && EquipmentManager.Instance.EquipmentFreeContainersAvailable)
        {
            Vector3 tgt = Vector3.zero, scl = Vector3.zero;

            if (EquipmentManager.Instance.CurrentMode == EquipmentManager.EquipmentMode.USABLES)
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.PanelPickableList.transform.position);
            }
            else
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.ButtonEquipmentPickableToggle.transform.position);
            }
            tgt.z = transform.position.z;

            StartCoroutine(FlyToTarget(tgt, scl, FADE_OUT_TIME_SEC));


            EquipmentManager.Instance.AddObjectToPool(this, FADE_OUT_TIME_SEC);
            InputManager.OnInputClickDown -= PickUp;

            InvokeOnPickedUp(this);

            // here will play professional animation, for now just simple coroutine
            // destruction will also be performed somewhat smarter
        }
    }

    protected override void FinishedFlying()
    {
        IsInEquipment = true;
    }

    protected void OnClickUpInEquipment(PointerEventData eventData)
    {
        if (!_actionsLocked)
        {
            _actionsLocked = true;
            StartCoroutine(OnClickUpReturnToSlotCoroutine(_container.UsableField.GetComponent<RectTransform>().position, _startSlotPosition, 0.5f));

            // check for mouse collisions with scene objects
            Collider2D col = InputManager.Instance.GetCollider2DUnderCursor();
            int objectsLayerID = LayerMask.NameToLayer("Objects");
            if (col != null && col.gameObject.layer == objectsLayerID)
            {
                PerformActionOnClick(col.gameObject);
                return;
            }

            // check for collisions with equipment elements
            PointerEventData pData = new PointerEventData(EventSystem.current);
            pData.position = InputManager.Instance.CursorCurrentPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pData, results);

            int length = results.Count;
            for(int i = 0; i < length; ++i)
            {
                if (results[i].gameObject.layer == objectsLayerID && results[i].gameObject != _container.UsableField.gameObject)
                {
                    UsableContainerField field = results[i].gameObject.GetComponent<UsableContainerField>();
                    if (field != null && !field.Container.IsFree)
                    {
                        PerformActionOnClick(field.Container.AssociatedObject.gameObject);
                        return;
                    }
                }
            }

            PerformActionOnClick(null);
        }
    }

    protected void OnClickDownInEquipment(PointerEventData eventData)
    {
        if (!_actionsLocked)
        {
            _tempTransform = _container.transform;
            _container.UsableField.transform.SetParent(_canvasForSelectedUsable.transform, true);
            _startSlotPosition = _container.UsableField.GetComponent<RectTransform>().position;
        }
    }

    protected void OnClickHoldInEquipment(PointerEventData eventData)
    {
        if(!_actionsLocked)
        {
            _container.UsableField.GetComponent<RectTransform>().position = eventData.position;
        }
    }

    protected System.Collections.IEnumerator OnClickUpReturnToSlotCoroutine(Vector2 startPos, Vector2 targetPos, float timeSeconds)
    {
        float cTime = Time.time;
        _container.UsableField.GetComponent<RectTransform>().position = startPos;

        while (Time.time - cTime <= timeSeconds)
        {
            float lerpValue = (Time.time - cTime) / timeSeconds;
            Vector2 finalPos = Vector2.Lerp(startPos, targetPos, lerpValue);
            _container.UsableField.GetComponent<RectTransform>().position = finalPos;
            yield return null;
        }
        _container.UsableField.GetComponent<RectTransform>().position = targetPos;
        _actionsLocked = false;
        _container.UsableField.transform.SetParent(_tempTransform, true);

        yield return null;
    }

    protected virtual void PerformActionOnClick(GameObject other)
    {
        Debug.Log("Not implemented.");
    }

    #endregion
}
