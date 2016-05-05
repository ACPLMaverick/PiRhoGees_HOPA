using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class RoomUnityEvent : UnityEvent<Room> { };

public class Room : MonoBehaviour
{
    #region events

    public RoomUnityEvent InitializeEvent;
    public RoomUnityEvent FinishedEvent;

    #endregion

    #region public

    public string Name;
    public string Description;
    public Sprite MapSprite;
    public Room NextRoom = null;
    public Room PrevRoom = null;
    public float CameraZoomMin = 0.1f;
    public float CameraZoomMax = 3.0f;
    public bool PickableAllowed = true;
    public bool Locked = false;
    public bool CameraEnabled = true;

    #endregion

    #region properties

    public List<PickableObject> PickablePickedObjects { get; private set; }
    public List<PickableObject> PickableObjects { get; private set; }
    public List<PickableUsableObject> PickableUsableObjects { get; private set; }

    public MapPart AssociatedMapPart { get; set; }

    #endregion

    #region protected

    protected bool _initialized;
    protected bool _finished;
    protected bool _inRoom;

    #endregion

    protected virtual void Awake()
    {
        // gather all pickableobjects in a room 

        FinishedEvent = new RoomUnityEvent();
        InitializeEvent = new RoomUnityEvent();

        PickableObjects = new List<PickableObject>();
        PickablePickedObjects = new List<PickableObject>();
        PickableUsableObjects = new List<PickableUsableObject>();
        PickableObject[] objs = this.gameObject.GetComponentsInChildren<PickableObject>();
        foreach (PickableObject obj in objs)
        {
            if (obj.GetType() == typeof(PickableObject))
            {
                PickableObjects.Add(obj);
            }
            else if (obj.GetType() == typeof(PickableUsableObject))
            {
                PickableUsableObjects.Add((PickableUsableObject)obj);
            }

            if (PickableAllowed)
            {
                obj.OnPickedUp.AddListener(new UnityAction<PickableObject>(RemoveOnPickup));
            }
        }
    }

    // Use this for initialization
    protected virtual void Start ()
    {

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

    public void Enter()
    {
        if(!_inRoom)
        {
            _inRoom = true;

            if(NextRoom != null && !NextRoom.PickableAllowed)
            {
                EquipmentManager.Instance.ButtonBack.GetComponent<BackButton>().ShowAsPuzzle(true);
                EquipmentManager.Instance.DisplayBackButton(true);

                if(NextRoom.Locked)
                {
                    EquipmentManager.Instance.ButtonBack.interactable = false;
                }
                else
                {
                    EquipmentManager.Instance.ButtonBack.interactable = true;
                }
            }
            else if (PrevRoom != null)
            {
                EquipmentManager.Instance.ButtonBack.GetComponent<BackButton>().ShowAsPuzzle(false);
                EquipmentManager.Instance.DisplayBackButton(true);
            }
            else
            {
                EquipmentManager.Instance.DisplayBackButton(false);
            }

            OnEntered();
        }
    }

    public void Leave()
    {
        if(_inRoom)
        {
            _inRoom = false;

            OnLeft();
        }
    }

    public void UnlockMapPart()
    {
        if(AssociatedMapPart != null)
        {
            AssociatedMapPart.Unlock();
        }
    }

    protected virtual void OnInitialize()
    {
        InitializeEvent.Invoke(this);
    }

    protected virtual void OnFinished()
    {
        FinishedEvent.Invoke(this);
    }

    protected virtual void OnEntered()
    {

    }

    protected virtual void OnLeft()
    {

    }
}
