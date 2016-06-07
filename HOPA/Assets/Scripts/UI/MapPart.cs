using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class MapButtonEvent : UnityEvent<MapPart> { };

public class MapPart : MonoBehaviour
{
    #region public

    public UnityEvent<MapPart> ClickedEvent;

    #endregion

    #region properties

    public Room AssociatedRoom { get; set; }

    #endregion

    #region protected

    protected Button _roomButton;
    protected Text _roomText;
    protected Image _finishedImg;

    #endregion

    #region functions

    protected virtual void Awake()
    {
        ClickedEvent = new MapButtonEvent();

        _roomButton = GetComponentInChildren<Button>();
        _roomButton.onClick.AddListener(new UnityAction(OnClick));
        _roomText = GetComponentInChildren<Text>();
        _finishedImg = GetComponentsInChildren<Image>(true)[2];
    }

    // Use this for initialization
    void Start ()
    {
        _roomButton.GetComponent<Image>().sprite = AssociatedRoom.MapSprite;
        _roomText.text = AssociatedRoom.Description;
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Lock()
    {
        _roomButton.interactable = false;
    }

    public void Unlock()
    {
        _roomButton.interactable = true;
    }

    public void Finish()
    {
        _roomButton.interactable = false;
        _finishedImg.gameObject.SetActive(true);
    }

    protected void OnClick()
    {
        if (ClickedEvent != null)
        {
            ClickedEvent.Invoke(this);
        }
    }

    #endregion
}
