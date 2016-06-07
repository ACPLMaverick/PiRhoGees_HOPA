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
    public Sprite EndingPhotoSprite;
    public Sprite EndingPhotoTextSprite;
    public AudioClip AmbientTheme;
    public Room PuzzleRoom = null;
    public Room ParentRoom = null;                // not necessary to be set if it is set when Room is assigned as child somewhere else
    public List<Room> ChildRooms;
    public float CameraZoomMin = 0.1f;
    public float CameraZoomMax = 3.0f;
    public bool PickableAllowed = true;
    public bool Locked = false;
    public bool CameraEnabled = true;

    #endregion

    #region properties

    public List<uint> PickablePickedObjectIDs { get; private set; }
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
        PickablePickedObjectIDs = new List<uint>();
        PickableUsableObjects = new List<PickableUsableObject>();
    }

    // Use this for initialization
    protected virtual void Start ()
    {
        PickableObject[] objs = GetComponentsInChildren<PickableObject>();
        AssignPickables(objs);

        foreach(Room r in ChildRooms)
        {
            objs = r.GetComponentsInChildren<PickableObject>();
            AssignPickables(objs);
            r.ParentRoom = this;
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
            PickablePickedObjectIDs.Add(obj.ID);

            if(PickableObjects.Count == PickablePickedObjectIDs.Count)
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

            if(AssociatedMapPart != null)
                AssociatedMapPart.Finish();

            OnFinished();
        }
    }

    public void Enter()
    {
        if(!_inRoom)
        {
            _inRoom = true;
            EquipmentManager.Instance.ButtonBack.GetComponent<BackButton>().UpdateOnCurrentRoom();
            EquipmentManager.Instance.PanelPickableList.GetComponent<PanelGeneric>().Hide(true);
            EquipmentManager.Instance.PanelUsableList.GetComponent<PanelGeneric>().Hide(true);

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

    protected void AssignPickables(PickableObject[] array)
    {
        foreach (PickableObject obj in array)
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
}
