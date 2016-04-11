using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UsableObject : PickableObject
{
    #region events

    #endregion

    #region constants
    
    #endregion

    #region public

    public List<UsableObject> InteractableObjects;  // sth else will be here
    public Dictionary<UsableObject, List<UsableObject>> MergableObjects;

    #endregion

    #region functions 

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();
	}

    protected override void PickUp(Vector2 position, Collider col)
    {
        if (col != null && col.gameObject == this.gameObject)
        {
            Vector3 tgt = Vector3.zero, scl = Vector3.zero;

            if (EquipmentManager.Instance.CurrentMode == EquipmentManager.EquipmentMode.USABLES)
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.PanelPickableList.transform.position);
                scl = Vector3.one;
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
    }

    #endregion
}
