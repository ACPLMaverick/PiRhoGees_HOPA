using UnityEngine;
using System.Collections;

public class PickableObject : MonoBehaviour
{
    #region events

    public delegate void PickedUp(PickableObject sender);

    public static event PickedUp OnPickedUp;

    #endregion

    #region constants

    protected const float FADE_OUT_TIME_SEC = 1.0f;

    #endregion

    #region public

    public uint ID = 0;
    public string Name;

    #endregion

    #region functions 

    // Use this for initialization
    protected virtual void Start ()
    {
        InputManager.OnInputClickDown += PickUp;
	}
	
	// Update is called once per frame
	protected virtual void Update ()
    {
	
	}

    protected virtual void PickUp(Vector2 position, Collider col)
    {
        if (col != null && col.gameObject == this.gameObject)
        {
            Vector3 tgt = Vector3.zero, scl = Vector3.zero;

            if(EquipmentManager.Instance.CurrentMode == EquipmentManager.EquipmentMode.PICKABLES)
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.PanelPickableList.transform.position);
            }
            else
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.ButtonEquipmentPickableToggle.transform.position);
            }
            tgt.z = transform.position.z;

            StartCoroutine(FlyToTarget(tgt, scl, FADE_OUT_TIME_SEC));


            EquipmentManager.Instance.AddObjectToList(this, FADE_OUT_TIME_SEC);
            InputManager.OnInputClickDown -= PickUp;

            InvokeOnPickedUp(this);

            // here will play professional animation, for now just simple coroutine
            // destruction will also be performed somewhat smarter
        }
    }

    protected virtual void FinishedFlying()
    {
        GameObject.DestroyImmediate(this.gameObject);
    }

    protected IEnumerator FlyToTarget(Vector3 targetPos, Vector3 targetScale, float time)
    {
        float currentTime = Time.time;
        Vector3 startPos = transform.localPosition;
        Vector3 startScale = transform.localScale;
        while(Time.time - currentTime <= time)
        {
            float lerp = (Time.time - currentTime) / time;

            transform.localPosition = Vector3.Lerp(startPos, targetPos, lerp);
            transform.localScale = Vector3.Lerp(startScale, targetScale, lerp);
            yield return null;
        }

        FinishedFlying();
        yield return null;
    }

    #endregion

    #region eventCallers

    protected void InvokeOnPickedUp(PickableObject arg)
    {
        if (OnPickedUp != null)
            OnPickedUp(arg);
    }

    #endregion
}
