using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    #region properties

    public List<PickableObject> PickableObjects { get; private set; }
    public List<UsableObject> UsableObjects { get; private set; }

    #endregion

    protected virtual void Awake()
    {
        // gather all pickableobjects in a room 

        PickableObjects = new List<PickableObject>();
        UsableObjects = new List<UsableObject>();
        PickableObject[] objs = this.gameObject.GetComponentsInChildren<PickableObject>();
        foreach (PickableObject obj in objs)
        {
            if(obj.GetType() == typeof(PickableObject))
            {
                PickableObjects.Add(obj);
            }
            else if(obj.GetType() == typeof(UsableObject))
            {
                UsableObjects.Add((UsableObject)obj);
            }
        }

        PickableObject.OnPickedUp += RemoveOnPickup;
    }

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    private void RemoveOnPickup(PickableObject obj)
    {
        if (obj.GetType() == typeof(PickableObject))
        {
            PickableObjects.Remove(obj);
        }
        else if (obj.GetType() == typeof(UsableObject))
        {
            UsableObjects.Remove((UsableObject)obj);
        }
        
    }
}
