using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisappearableObject : MonoBehaviour
{
    #region enums

    public enum DisappearableMode
    {
        APPEAR,
        DISAPPEAR
    };

    #endregion

    #region public

    public AudioClip SoundDisappear;
    public List<RoomTransition> AssociatedTransitions;
    public GameObject ObjectToDisappear;
    public float TimeFadeOutSeconds = 1.0f;
    public DisappearableMode Mode = DisappearableMode.DISAPPEAR;
    public bool NonDirect = false;
    public bool DestroyOnClicked = true;
    public bool DeactivateOnClicked = true;
    public bool Repeatable = false;
    public bool AffectChildren = true;
    public bool AffectParent = true;

    #endregion

    #region protected

    protected bool _disappearInProgress = false;
    protected SpriteRenderer _otdSpriteRenderer;
    protected DisappearableObject[] _doInChildren;
    protected List<Key> _keysWhichLockMe = new List<Key>();

    protected bool IsLockedByKey
    {
        get
        {
            return _keysWhichLockMe.Count != 0;
        }
    }


    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        if (ObjectToDisappear == null)
        {
            ObjectToDisappear = gameObject;
        }

        _otdSpriteRenderer = ObjectToDisappear.GetComponent<SpriteRenderer>();
        if(Mode == DisappearableMode.DISAPPEAR)
        {
            _otdSpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
        {
            _otdSpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }

        _doInChildren = ObjectToDisappear.GetComponentsInChildren<DisappearableObject>();

        PickableObject[] po = ObjectToDisappear.GetComponentsInChildren<PickableObject>();
        int l = po.Length;
        for(int i = 0; i < l; ++i)
        {
            po[i].FrameLocked = true;
        }

        InputManager.Instance.OnInputClickUp.AddListener(OnClickUp);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void LockWithKey(Key key)
    {
        _keysWhichLockMe.Add(key);
    }

    public void UnlockWithKey(Key key)
    {
        if(_keysWhichLockMe.Contains(key))
        {
            _keysWhichLockMe.Remove(key);

            if (_keysWhichLockMe.Count == 0)
            {
                OnClickUp(Camera.main.WorldToScreenPoint(GetComponent<Transform>().position), GetComponent<Collider2D>());
            }
        }
    }

    protected virtual void OnClickUp(Vector2 screenPos, Collider2D hitCollider2D)
    {
        if(gameObject != null && 
            hitCollider2D != null && 
            hitCollider2D.gameObject == gameObject && 
            !NonDirect &&
            !IsLockedByKey)
        {
            OnClickUpInternal(screenPos, hitCollider2D);
        }
    }

    protected virtual void OnClickUpInternal(Vector2 screenPos, Collider2D hitCollider2D)
    {
        if(!_disappearInProgress)
        {
            if (SoundDisappear != null)
            {
                AudioManager.Instance.PlayClip(SoundDisappear);
            }

            if (Mode == DisappearableMode.DISAPPEAR)
            {
                gameObject.SetActive(true);
                StartCoroutine(FadeCoroutine(1.0f, 0.0f, TimeFadeOutSeconds, !DeactivateOnClicked, DestroyOnClicked));
            }
            else
            {
                gameObject.SetActive(true);
                StartCoroutine(FadeCoroutine(0.0f, 1.0f, TimeFadeOutSeconds, true, DestroyOnClicked));
            }

            foreach (RoomTransition rt in AssociatedTransitions)
            {
                rt.Locked = !rt.Locked;
            }

            _disappearInProgress = true;

            if (AffectChildren)
            {
                int l = _doInChildren.Length;
                for (int i = 0; i < l; ++i)
                {
                    _doInChildren[i].OnClickUpInternal(screenPos, _doInChildren[i].GetComponent<Collider2D>());

                    PickableObject comp;
                    if ((comp = _doInChildren[i].GetComponent<PickableObject>()) != null)
                    {
                        comp.FrameLocked = false;
                    }
                }
            }

            if (AffectParent && ObjectToDisappear.GetComponent<Transform>().parent != null && ObjectToDisappear.GetComponent<Transform>().parent.gameObject.GetComponent<DisappearableObject>() != null)
            {
                ObjectToDisappear.GetComponent<Transform>().parent.gameObject.GetComponent<DisappearableObject>().OnClickUpInternal(
                    screenPos, ObjectToDisappear.GetComponent<Transform>().parent.gameObject.GetComponent<Collider2D>());
            }
        }
    }

    protected IEnumerator FadeCoroutine(float fadeStart, float fadeTarget, float timeSec, bool active, bool destroy)
    {
        float currentTime = Time.time;
        float alpha = fadeStart;
        _otdSpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, alpha);

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;

            alpha = Mathf.Lerp(fadeStart, fadeTarget, lerp);
            _otdSpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            yield return null;
        }
        alpha = fadeTarget;
        _otdSpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, alpha);

        if (Repeatable)
        {
            _disappearInProgress = false;
            if (Mode == DisappearableMode.APPEAR)
            {
                Mode = DisappearableMode.DISAPPEAR;
            }
            else
            {
                Mode = DisappearableMode.APPEAR;
            }
        }

        if (destroy)
        {
            InputManager.Instance.OnInputClickUp.RemoveListener(OnClickUp);
            DestroyImmediate(gameObject);
        }
        else
        {
            gameObject.SetActive(active);
        }

        yield return null;
    }

    #endregion
}
