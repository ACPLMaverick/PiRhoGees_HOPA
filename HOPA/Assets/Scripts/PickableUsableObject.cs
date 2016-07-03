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

    public AudioClip UseSound;
    public List<PickableUsableObject> InteractableObjects;  // sth else will be here
    public Dictionary<PickableUsableObject, List<PickableUsableObject>> MergableObjects;
    public GameObject TargetObject; //To decide with which object, PUObject will interract

    #endregion

    #region properties

    public UsableContainer AssociatedContainer
    {
        get
        {
            return _container;
        }
    }
    public bool IsInEquipment { get; protected set; }

    #endregion

    #region private

    private UsableContainer _container = null;
    private Vector2 _startSlotPosition;
    private bool _actionsLocked = false;
    private Transform _tempTransform = null;
    private Canvas _canvasForSelectedUsable = null;
    [SerializeField]
    private bool DestroyOnUse = false;

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
        if(container != null)
        {
            _container = container;
            GetComponent<SpriteRenderer>().enabled = false;
            _container.UsableField.PointerUpEvent.AddListener(OnClickUpInEquipment);
            _container.UsableField.DragEvent.AddListener(OnClickHoldInEquipment);
            _container.UsableField.PointerDownEvent.AddListener(OnClickDownInEquipment);
        }
        else if(_container != null)
        {
            _container.UsableField.PointerUpEvent.RemoveListener(OnClickUpInEquipment);
            _container.UsableField.DragEvent.RemoveListener(OnClickHoldInEquipment);
            _container.UsableField.PointerDownEvent.RemoveListener(OnClickDownInEquipment);
            _container = null;
        }
    }

    protected override void PickUp(Vector2 position, Collider2D col)
    {
        if (col != null && col.gameObject == this.gameObject && EquipmentManager.Instance.EquipmentFreeContainersAvailable && !_picked)
        {
            Vector3 tgt = Vector3.zero, scl = Vector3.zero;

            if (EquipmentManager.Instance.CurrentMode == EquipmentManager.EquipmentMode.USABLES && 
                !EquipmentManager.Instance.PanelPickableList.GetComponent<PanelGeneric>().Hidden)
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.PanelPickableList.transform.position);
            }
            else
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.ButtonEquipmentPickableToggle.transform.position);
            }
            tgt.z = transform.position.z;

            StartCoroutine(FlyToTarget(tgt, scl, FADE_OUT_TIME_SEC));

            AudioManager.Instance.PlayClip(PickUpSound);

            EquipmentManager.Instance.AddObjectToPool(this, FADE_OUT_TIME_SEC);
            InputManager.Instance.OnInputClickUp.RemoveListener(PickUp);

            _picked = true;
            OnPickedUp.Invoke(this);

            // here will play professional animation, for now just simple coroutine
            // destruction will also be performed somewhat smarter
            
            // yeah right.
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
            FloatingParticleManager.Instance.Hide();

            bool actionDone = false;
            // check for mouse collisions with scene objects
            RaycastHit2D[] hits = InputManager.Instance.GetRaycastHitsUnderCursor();
            int objectsLayerID = LayerMask.NameToLayer("Objects");
            int hl = hits.Length;
            if(hl == 0)
            {
                actionDone = true;
            }

            for(int i = 0; i < hl; ++i)
            {
                if (!actionDone && hits[i].collider != null && hits[i].collider.gameObject.layer == objectsLayerID)
                {
                    actionDone = PerformActionOnClick(hits[i].collider.gameObject);
                }
            }
            

            if(!actionDone)
            {
                // check for collisions with equipment elements
                PointerEventData pData = new PointerEventData(EventSystem.current);
                pData.position = InputManager.Instance.CursorCurrentPosition;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pData, results);

                int length = results.Count;
                for (int i = 0; i < length; ++i)
                {
                    if (results[i].gameObject.layer == objectsLayerID && results[i].gameObject != _container.UsableField.gameObject)
                    {
                        UsableContainerField field = results[i].gameObject.GetComponent<UsableContainerField>();
                        if (field != null && !field.Container.IsFree)
                        {
                            actionDone = PerformActionOnClick(field.Container.AssociatedObject.gameObject);
                        }
                    }
                }
            }
            
            if(!actionDone)
            {
                PerformActionOnClick(null);
            }

            _actionsLocked = true;

            if(actionDone && DestroyOnUse)
            {
                EquipmentManager.Instance.RemoveObjectFromPool(this, 0.6f);
                StartCoroutine(OnClickUpDestroyCoroutine(_container.UsableField, _startSlotPosition, 1.2f));
            }
            else
            {
                StartCoroutine(OnClickUpReturnToSlotCoroutine(_container.UsableField.GetComponent<RectTransform>().position, _startSlotPosition, 0.5f));
            }
        }
    }

    protected void OnClickDownInEquipment(PointerEventData eventData)
    {
        if (!_actionsLocked)
        {
            _tempTransform = _container.transform;
            _container.UsableField.transform.SetParent(_canvasForSelectedUsable.transform, true);
            _startSlotPosition = _container.UsableField.GetComponent<RectTransform>().position;
            FloatingParticleManager.Instance.Show(_container.UsableField.gameObject, TargetObject);
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

    protected System.Collections.IEnumerator OnClickUpDestroyCoroutine(UsableContainerField fieldRef, Vector2 targetPos, float lagSeconds)
    {
        yield return new WaitForSeconds(lagSeconds);
        fieldRef.GetComponent<RectTransform>().position = targetPos;
        fieldRef.transform.SetParent(_tempTransform, true);
        Destroy(this);
        yield return null;
    }

    protected virtual bool PerformActionOnClick(GameObject other)
    {
        if(other != null && other.Equals(TargetObject))
        {
            if (UseSound != null)
                AudioManager.Instance.PlayClip(UseSound);

            //Debug.Log(other.name);
            if (other.GetComponent<Animator>() != null) other.GetComponent<Animator>().enabled = true;
            return true;
        }

        return false;
    }

    protected void ScheduleToDestroyOnClickUp()
    {
        DestroyOnUse = true;
    }

    #endregion
}
