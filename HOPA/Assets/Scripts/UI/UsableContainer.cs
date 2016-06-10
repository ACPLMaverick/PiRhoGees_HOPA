using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UsableContainer : MonoBehaviour
{
    #region properties

    public UsableContainerField UsableField { get; private set; }
    public bool IsFree { get; set; }
    public PickableUsableObject AssociatedObject { get; private set; }

    #endregion

    #region private



    #endregion

    // Use this for initialization
    protected virtual void Awake()
    {
        IsFree = true;
        UsableField = GetComponentInChildren<UsableContainerField>();
    }

    void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void AssignEquipmentUsable(PickableUsableObject obj)
    {
        IsFree = false;
        AssociatedObject = obj;
        Sprite spr = AssociatedObject.GetComponent<SpriteRenderer>().sprite;
        UsableField.UsableImage.sprite = spr;
        UsableField.UsableImage.enabled = true;
        if(isActiveAndEnabled)
        {
            StartCoroutine(AddObjectToPoolFadeCoroutine(0.6f, 0.0f, 1.0f, true));
        }
        else
        {
            UsableField.UsableCanvasGroup.alpha = 1.0f;
        }
    }

    public void UnassignEquipmentUsable()
    {
        if(AssociatedObject != null)
        {
            IsFree = true;
            StartCoroutine(AddObjectToPoolFadeCoroutine(0.6f, 1.0f, 0.0f, false));
        }
    }

    private IEnumerator AddObjectToPoolFadeCoroutine(float timeSeconds, float startOpacity, float targetOpacity, bool assign)
    {
        float cTime = Time.time;
        UsableField.UsableCanvasGroup.alpha = startOpacity;

        while (Time.time - cTime <= timeSeconds)
        {
            float lerpValue = (Time.time - cTime) / timeSeconds;
            float finalOpacity = Mathf.Lerp(startOpacity, targetOpacity, lerpValue);
            UsableField.UsableCanvasGroup.alpha = finalOpacity;
            yield return null;
        }
        UsableField.UsableCanvasGroup.alpha = targetOpacity;

        if(!assign)
        {
            AssociatedObject = null;
            UsableField.UsableImage.enabled = false;
        }

        yield return null;
    }
}
