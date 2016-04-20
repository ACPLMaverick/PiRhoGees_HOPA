using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    #region public

    public Room CurrentRoom;
    public List<Room> RoomsInGame;
    public Image FadeImage;
    public float RoomTransitionTime = 2.0f;

    #endregion

    #region private

    private Room _nextRoom = null;

    #endregion

    // Use this for initialization
    void Start ()
    {
        //RoomsInGame = new List<Room>();

        foreach(Room r in RoomsInGame)
        {
            r.AllPickableObjectsCollectedEvent.AddListener(new UnityEngine.Events.UnityAction(OnRoomCommonPickablesCollected));
        }

        RoomsInGame[0].AllPickableObjectsCollectedEvent.AddListener(new UnityEngine.Events.UnityAction(OnRoom0PickablesCollected));
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void TransitionToRoom(Room room)
    {
        _nextRoom = room;
        StartCoroutine(StartMoveCoroutine());
    }

    private IEnumerator StartMoveCoroutine()
    {
        float cTime = Time.time;
        FadeImage.gameObject.SetActive(true);
        FadeImage.canvasRenderer.SetAlpha(0.0f);

        while (Time.time - cTime <= RoomTransitionTime * 0.5f)
        {
            float lerpValue = (Time.time - cTime) / (RoomTransitionTime * 0.5f);
            FadeImage.canvasRenderer.SetAlpha(lerpValue);
            yield return null;
        }
        FadeImage.canvasRenderer.SetAlpha(1.0f);
        MoveToCurrentRoom();

        yield return null;
    }

    private IEnumerator EndMoveCoroutine()
    {
        float cTime = Time.time;

        while (Time.time - cTime <= RoomTransitionTime * 0.5f)
        {
            float lerpValue = (Time.time - cTime) / (RoomTransitionTime * 0.5f);
            FadeImage.canvasRenderer.SetAlpha(1.0f - lerpValue);
            yield return null;
        }
        FadeImage.canvasRenderer.SetAlpha(0);
        FadeImage.gameObject.SetActive(false);
        yield return null;
    }

    private void MoveToCurrentRoom()
    {
        CurrentRoom = _nextRoom;
        _nextRoom = null;

        CameraManager.Instance.RecalculateToCurrentRoom();
        EquipmentManager.Instance.FlushOnNextRoom();
        StartCoroutine(EndMoveCoroutine());
    }

    private void OnRoomCommonPickablesCollected()
    {
        Debug.Log("ALL PICKABLES IN CURRENT ROOM COLLECTED!");
    }

    private void OnRoom0PickablesCollected()
    {
        Debug.Log("All pickables in Room 0 collected.");
    }
}
