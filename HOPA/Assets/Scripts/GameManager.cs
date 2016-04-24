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
    public Text ClearedText;
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
            r.AllPickableObjectsCollectedEvent.AddListener(new UnityEngine.Events.UnityAction<Room>(OnRoomCommonPickablesCollected));
        }

        //RoomsInGame[0].AllPickableObjectsCollectedEvent.AddListener(new UnityEngine.Events.UnityAction(OnRoom0PickablesCollected));

        ClearedText.GetComponent<CanvasGroup>().alpha = 0.0f;
        ClearedText.gameObject.SetActive(false);
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
        CameraManager.Instance.Enabled = CurrentRoom.CameraEnabled;
        EquipmentManager.Instance.FlushOnNextRoom();
        StartCoroutine(EndMoveCoroutine());
    }

    private void OnRoomCommonPickablesCollected(Room r)
    {
        StartCoroutine(ClearedTextCoroutine(r));
    }

    //private void OnRoom0PickablesCollected()
    //{
    //    StartCoroutine(OnRoom0PickablesCollectedCoroutine());
    //}

    private IEnumerator ClearedTextCoroutine(Room r)
    {
        ClearedText.gameObject.SetActive(true);
        RectTransform rt = ClearedText.GetComponent<RectTransform>();
        CanvasGroup cg = ClearedText.GetComponent<CanvasGroup>();
        Vector2 startScale = new Vector2(0.5f, 0.5f);
        float startAlpha = 0.0f;
        Vector2 targetScale = new Vector2(1.0f, 1.0f);
        float targetAlpha = 1.0f;
        float timeSecondsWait = 1.5f;
        float timeSecondsIn = 3.0f;
        float timeSecondsStay = 3.0f;
        float timeSecondsOut = 1.2f;

        yield return new WaitForSeconds(timeSecondsWait);

        float cTime = Time.time;

        while (Time.time - cTime <= timeSecondsIn)
        {
            float lerpValue = (Time.time - cTime) / timeSecondsIn;

            Vector2 finalScale = Vector2.Lerp(startScale, targetScale, lerpValue);
            float finalAlpha = Mathf.Lerp(startAlpha, targetAlpha, lerpValue);

            rt.localScale = finalScale;
            cg.alpha = finalAlpha;

            yield return null;
        }
        rt.localScale = targetScale;
        cg.alpha = targetAlpha;

        yield return new WaitForSeconds(timeSecondsStay);

        cTime = Time.time;

        while (Time.time - cTime <= timeSecondsOut)
        {
            float lerpValue = (Time.time - cTime) / timeSecondsOut;

            Vector2 finalScale = Vector2.Lerp(targetScale, startScale, lerpValue);
            float finalAlpha = Mathf.Lerp(targetAlpha, startAlpha, lerpValue);

            rt.localScale = finalScale;
            cg.alpha = finalAlpha;

            yield return null;
        }
        rt.localScale = startScale;
        cg.alpha = startAlpha;
        ClearedText.gameObject.SetActive(false);

        if (r.NextRoom != null)
        {
            if(r.NextRoom.Locked)
            {
                r.NextRoom.Locked = false;
                r.NextRoom.UnlockMapButton();
            }
            TransitionToRoom(r.NextRoom);
        }
        else
        {
            EquipmentManager.Instance.OpenMapArbitrarily();
        }

        yield return null;
    }

    //private IEnumerator OnRoom0PickablesCollectedCoroutine()
    //{
    //    yield return new WaitForSeconds(4.0f);
    //}
}
