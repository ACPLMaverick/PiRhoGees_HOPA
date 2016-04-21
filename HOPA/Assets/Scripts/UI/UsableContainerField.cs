using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class PointerUnityEvent : UnityEvent<PointerEventData>
{

}

public class UsableContainerField : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    #region public

    public PointerUnityEvent DragEvent;
    public PointerUnityEvent PointerUpEvent;
    public PointerUnityEvent PointerDownEvent;

    #endregion

    #region properties

    public Image UsableImage { get; protected set; }
    public CanvasGroup UsableCanvasGroup { get; protected set; }
    public Button ButtonComponent { get; protected set; }
    public UsableContainer Container { get; protected set; }

    #endregion

    #region functions

    protected virtual void Awake()
    {
        DragEvent = new PointerUnityEvent();
        PointerUpEvent = new PointerUnityEvent();
        PointerDownEvent = new PointerUnityEvent();
        UsableImage = gameObject.GetComponent<Image>();
        UsableCanvasGroup = UsableImage.gameObject.GetComponent<CanvasGroup>();
        ButtonComponent = UsableImage.gameObject.GetComponent<Button>();
        Container = GetComponentInParent<UsableContainer>();
    }

    // Use this for initialization
    void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void OnDrag(PointerEventData eventData)
    {
        DragEvent.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUpEvent.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDownEvent.Invoke(eventData);
    }

    #endregion
}
