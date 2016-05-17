using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class MapDirectionButtonEvent : UnityEvent<MapDirectionButton> { };

public class MapDirectionButton : MonoBehaviour {

    #region public

    public InputManager.SwipeDirection AssociatedDirection;
    public UnityEvent<MapDirectionButton> ClickedEvent;

    #endregion

    #region functions

    protected virtual void Awake()
    {
        ClickedEvent = new MapDirectionButtonEvent();
        GetComponent<Button>().onClick.AddListener(new UnityAction(OnClick));
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
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