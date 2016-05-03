using UnityEngine;
using System.Collections;

public class RoomTransition : MonoBehaviour
{
    #region public

    public Room RoomTo;
    public bool Locked = false;

    #endregion

    #region private

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        InputManager.OnInputClickUp += OnClickUp;
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    protected void OnClickUp(Vector2 screenPos, Collider2D hitCollider2D)
    {
        if(hitCollider2D != null && hitCollider2D.gameObject == gameObject && !Locked && !RoomTo.Locked)
        {
            GameManager.Instance.TransitionToRoom(RoomTo);
        }
    }

    #endregion
}
