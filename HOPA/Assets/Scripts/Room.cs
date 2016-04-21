using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class RoomUnityEvent : UnityEvent<Room> { };

public class Room : MonoBehaviour
{
    #region events

    public RoomUnityEvent AllPickableObjectsCollectedEvent;

    #endregion

    #region public

    public Room NextRoom = null;
    // public Message NextMessage;
    public bool Locked = false;
    public bool CameraEnabled = true;
    public MapButton AssociatedMapButton;

    #endregion

    #region properties

    public List<PickableObject> PickablePickedObjects { get; private set; }
    public List<PickableObject> PickableObjects { get; private set; }
    public List<PickableUsableObject> PickableUsableObjects { get; private set; }

    #endregion

    protected virtual void Awake()
    {
        // gather all pickableobjects in a room 

        AllPickableObjectsCollectedEvent = new RoomUnityEvent();

        PickableObjects = new List<PickableObject>();
        PickablePickedObjects = new List<PickableObject>();
        PickableUsableObjects = new List<PickableUsableObject>();
        PickableObject[] objs = this.gameObject.GetComponentsInChildren<PickableObject>();
        foreach (PickableObject obj in objs)
        {
            if(obj.GetType() == typeof(PickableObject))
            {
                PickableObjects.Add(obj);
            }
            else if(obj.GetType() == typeof(PickableUsableObject))
            {
                PickableUsableObjects.Add((PickableUsableObject)obj);
            }
        }

        PickableObject.OnPickedUp += RemoveOnPickup;
    }

    // Use this for initialization
    void Start ()
    {
        if(Locked)
        {
            AssociatedMapButton.Lock();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    private void RemoveOnPickup(PickableObject obj)
    {
        if (obj.GetType() == typeof(PickableObject))
        {
            //PickableObjects.Remove(obj);
            PickablePickedObjects.Add(obj);

            if(PickableObjects.Count == PickablePickedObjects.Count)
            {
                AllPickableObjectsCollectedEvent.Invoke(this);
            }
        }
        else if (obj.GetType() == typeof(PickableUsableObject))
        {
            PickableUsableObjects.Remove((PickableUsableObject)obj);
        }
        
    }

    public void UnlockMapButton()
    {
        AssociatedMapButton.Unlock();
    }
}
