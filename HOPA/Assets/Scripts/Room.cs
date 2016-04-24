using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class RoomUnityEvent : UnityEvent<Room> { };

public class Room : MonoBehaviour
{
    #region events

    public RoomUnityEvent InitializeEvent;
    public RoomUnityEvent CompletedEvent;

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

    #region protected

    protected bool _initialized;
    protected bool _finished;

    #endregion

    protected virtual void Awake()
    {
        // gather all pickableobjects in a room 

        CompletedEvent = new RoomUnityEvent();
        InitializeEvent = new RoomUnityEvent();

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

        AssociatedMapButton.AssociatedRoom = this;
    }

    // Use this for initialization
    protected virtual void Start ()
    {
        if(Locked)
        {
            AssociatedMapButton.Lock();
        }
    }

    // Update is called once per frame
    protected virtual void Update ()
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
                Finish();
            }
        }
        else if (obj.GetType() == typeof(PickableUsableObject))
        {
            PickableUsableObjects.Remove((PickableUsableObject)obj);
        }
        
    }

    public void Initialize()
    {
        if(!_initialized)
        {
            _initialized = true;
            OnInitialize();
        }
    }

    public void Finish()
    {
        if(!_finished)
        {
            _finished = true;
            OnFinished();
        }
    }

    public void UnlockMapButton()
    {
        AssociatedMapButton.Unlock();
    }

    protected virtual void OnInitialize()
    {
        InitializeEvent.Invoke(this);
    }

    protected virtual void OnFinished()
    {
        CompletedEvent.Invoke(this);
    }
}
