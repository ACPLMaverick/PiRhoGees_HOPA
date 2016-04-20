using UnityEngine;
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

    #endregion

    #region functions 

    // Use this for initialization
    protected override void Start ()
    {
        IsInEquipment = false;
        base.Start();

        InputManager.OnInputClickUp += OnClickInEquipment;
	}
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();
	}

    protected override void PickUp(Vector2 position, Collider2D col)
    {
        if (col != null && col.gameObject == this.gameObject)
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
        if (EquipmentManager.Instance.CurrentMode == EquipmentManager.EquipmentMode.USABLES)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
        IsInEquipment = true;
    }

    protected void OnClickInEquipment(Vector2 screenPos, Collider2D hitCollider)
    {
        if(hitCollider != null && hitCollider.gameObject == this.gameObject && IsInEquipment)
        {
            RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, Vector3.forward, 0.01f);

            if(hit.collider != null && (hit.collider.gameObject.tag == "Usable" || hit.collider.gameObject.tag == "PickableUsable"))
            {
                PerformActionOnClick(hit.collider.gameObject);
            }
            else
            {
                PerformActionOnClick(null);
            }
        }
    }

    protected virtual void PerformActionOnClick(GameObject other)
    {
        Debug.Log("Not implemented.");
    }

    #endregion
}
