using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class UnityEventPickedUp : UnityEvent<PickableObject> {  };

public class PickableObject : MonoBehaviour
{
    #region events

    public UnityEventPickedUp OnPickedUp;

    #endregion

    #region constants

    protected const float FADE_OUT_TIME_SEC = 1.0f;
    protected const string DEFAULT_SOUND_ASSET_PATH = "Sounds/pick_item";

    #endregion

    #region public

    public uint ID = 0;
    public string Name;
    public string Description;
    public AudioClip PickUpSound;

    #endregion

    #region properties

    public Button AssociatedListElement
    {
        get
        {
            return _associatedListElement;
        }
        
        set
        {
            if(_associatedListElement != null)
            {
                _associatedListElement.onClick.RemoveListener(_actionOnListElementClick);
            }
            _associatedListElement = value;
            _associatedListElement.onClick.AddListener(_actionOnListElementClick);
        }
    }
    public bool FrameLocked
    {
        get
        {
            return _frameLocked;
        }
        set
        {
            _frameLockedHelper = value;
        }
    }

    public bool Picked { get { return _picked; } }


    #endregion

    #region protected

    protected bool _frameLocked = false;
    protected bool _frameLockedHelper = false;
    protected Button _associatedListElement;
    protected bool _picked = false;
    protected UnityAction _actionOnListElementClick;

    #endregion

    #region functions 

    protected virtual void Awake()
    {
        OnPickedUp = new UnityEventPickedUp();
        _actionOnListElementClick = new UnityAction(OnListElementClick);
    }

    // Use this for initialization
    protected virtual void Start ()
    {
        InputManager.Instance.OnInputClickUp.AddListener(PickUp);

        if(PickUpSound == null)
        {
            PickUpSound = AudioManager.Instance.DefaultSoundPickUp;
        }
	}
	
	// Update is called once per frame
	protected virtual void Update ()
    {
	    if(_frameLockedHelper != _frameLocked)
        {
            _frameLocked = _frameLockedHelper;
        }
	}

    protected virtual void OnListElementClick()
    {
        if (Description.Length != 0)
        {
            GameManager.Instance.ItemInfoGroup.Show(GetComponent<SpriteRenderer>().sprite, Name, Description);
        }
        else
        {
            GameManager.Instance.ItemInfoGroup.Show(GetComponent<SpriteRenderer>().sprite, Name);
        }

        TutorialManager.Instance.GoStepFurther(9);
    }

    protected virtual void PickUp(Vector2 position, Collider2D col)
    {
        if (col != null && col.gameObject == this.gameObject && !_picked && !_frameLocked)
        {
            col.gameObject.transform.SetParent(Camera.main.transform, true);
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

            AudioManager.Instance.PlayClip(PickUpSound);

            EquipmentManager.Instance.AddObjectToList(this, FADE_OUT_TIME_SEC);
            InputManager.Instance.OnInputClickUp.RemoveListener(PickUp);
            PickableHintManager.Instance.Flush();
            //col.gameObject.transform.SetParent(Camera.main.transform, true);

            _picked = true;
            OnPickedUp.Invoke(this);

            // here will play professional animation, for now just simple coroutine
            // destruction will also be performed somewhat smarter
        }
    }

    protected virtual void FinishedFlying()
    {
        if(AssociatedListElement != null)
        {
            EquipmentManager.Instance.ChangeTextPickedStatus(AssociatedListElement.GetComponent<Text>(), true);
            AssociatedListElement.GetComponent<Button>().onClick.RemoveListener(_actionOnListElementClick);
        }
        GameObject.DestroyImmediate(this.gameObject);
    }

    protected IEnumerator FlyToTarget(Vector3 targetPos, Vector3 targetScale, float time)
    {
        float currentTime = Time.time;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        while(Time.time - currentTime <= time)
        {
            float lerp = (Time.time - currentTime) / time;

            transform.position = Vector3.Lerp(startPos, targetPos, lerp);
            transform.localScale = Vector3.Lerp(startScale, targetScale, lerp);
            yield return null;
        }

        FinishedFlying();
        yield return null;
    }

    #endregion
}
