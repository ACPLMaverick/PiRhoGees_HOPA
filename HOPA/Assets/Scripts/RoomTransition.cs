using UnityEngine;
using System.Collections;

public class RoomTransition : MonoBehaviour
{
    #region public

    public Room RoomTo;
    public bool Locked = false;
    public bool DeactivateAfterRoomFinished = true;

    #endregion

    #region private

    TransitionRing _tRing;
    bool _ringHelper = false;
    bool _ringHelperLocal = false;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        InputManager.Instance.OnInputClickUp.AddListener(OnClickUp);
        _tRing = GetComponentInChildren<TransitionRing>();
        _ringHelper = RoomTo.Locked;
        _ringHelperLocal = Locked;

        if(_ringHelper || _ringHelperLocal)
        {
            _tRing.gameObject.SetActive(false);
        }

        if(DeactivateAfterRoomFinished)
            RoomTo.FinishedEvent.AddListener(OnRoomFinished);
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(_tRing != null)
        {
            if((_ringHelper && !RoomTo.Locked) )
            {
                // just unlocked
                _tRing.gameObject.SetActive(true);
                StartCoroutine(Utility.FadeCoroutine(_tRing.GetComponent<SpriteRenderer>(), 0.0f, 1.0f, 0.3f, true));
                _ringHelper = RoomTo.Locked;
            }
            else if((!_ringHelper && RoomTo.Locked))
            {
                // just locked
                StartCoroutine(Utility.FadeCoroutine(_tRing.GetComponent<SpriteRenderer>(), 1.0f, 0.0f, 0.3f, false));
                _ringHelper = RoomTo.Locked;
            }

            if ( (_ringHelperLocal && !Locked))
            {
                // just unlocked
                _tRing.gameObject.SetActive(true);
                StartCoroutine(Utility.FadeCoroutine(_tRing.GetComponent<SpriteRenderer>(), 0.0f, 1.0f, 0.3f, true));
                _ringHelperLocal = Locked;
            }
            else if ( (!_ringHelperLocal && Locked))
            {
                // just locked
                StartCoroutine(Utility.FadeCoroutine(_tRing.GetComponent<SpriteRenderer>(), 1.0f, 0.0f, 0.3f, false));
                _ringHelperLocal = Locked;
            }
        }
	}

    protected void OnClickUp(Vector2 screenPos, Collider2D hitCollider2D)
    {
        if(hitCollider2D != null && hitCollider2D.gameObject == gameObject && !Locked && !RoomTo.Locked)
        {
            GameManager.Instance.TransitionToRoom(RoomTo);
        }
    }

    protected void OnRoomFinished(Room room)
    {
        RoomTo.FinishedEvent.RemoveListener(OnRoomFinished);
        gameObject.SetActive(false);
    }

    #endregion
}
