using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;

public class MapButtonEvent : UnityEvent<MapButton> { };

public class MapButton : MonoBehaviour
{
    #region public

    public Room AssociatedRoom;
    public UnityEvent<MapButton> ClickedEvent;

    #endregion

    #region functions

    protected virtual void Awake()
    {
        ClickedEvent = new MapButtonEvent();
        GetComponent<Button>().onClick.AddListener(new UnityAction(OnClick));
    }

    // Use this for initialization
    void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void OnClick()
    {
        if (ClickedEvent != null)
        {
            ClickedEvent.Invoke(this);
        }
    }

    #endregion
}
