using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    #region public

    public Room CurrentRoom;

    public Room RoomPrologue;
    public Room RoomFirst;
    public Room RoomFirstPuzzle;
    public Room RoomSecond;
    public Room RoomSecondPuzzle;
    public Room RoomThird;
    public Room RoomFourth;

    public Image FadeImage;
    public ItemInfo ItemInfoGroup;
    public PauseMenu PauseMenuGroup;
    public Text ClearedText;
    public Text PuzzleSolvedText;
    public float RoomTransitionTime = 2.0f;

    #endregion

    #region private

    private Room _nextRoom = null;

    #endregion

    // Use this for initialization
    void Start ()
    {
        RoomFirst.FinishedEvent.AddListener(new UnityEngine.Events.UnityAction<Room>(OnRoomCommonPickablesCollected));
        RoomFirstPuzzle.FinishedEvent.AddListener(new UnityEngine.Events.UnityAction<Room>(OnRoomAssignPuzzleFinished));
        RoomSecond.FinishedEvent.AddListener(new UnityEngine.Events.UnityAction<Room>(OnRoomCommonPickablesCollected));
        RoomSecondPuzzle.FinishedEvent.AddListener(new UnityEngine.Events.UnityAction<Room>(OnRoomAssignPuzzleFinished));

        ClearedText.GetComponent<CanvasGroup>().alpha = 0.0f;
        ClearedText.gameObject.SetActive(false);

        PuzzleSolvedText.GetComponent<CanvasGroup>().alpha = 0.0f;
        PuzzleSolvedText.gameObject.SetActive(false);

        CameraManager.Instance.Enabled = CurrentRoom.CameraEnabled;
        CurrentRoom.Initialize();
        CurrentRoom.Enter();
        AudioManager.Instance.PlayMusic(CurrentRoom.AmbientTheme, 0.5f);
        ItemInfoGroup.gameObject.SetActive(false);
        PauseMenuGroup.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void TransitionToRoom(Room room)
    {
        _nextRoom = room;
        StartCoroutine(StartMoveCoroutine());
        if (CurrentRoom.ParentRoom == null && _nextRoom.ParentRoom != CurrentRoom)
        {
            AudioManager.Instance.PlayMusic(room.AmbientTheme, RoomTransitionTime);
        }
    }

    public void ShowPauseMenu()
    {
        PauseMenuGroup.Show();
    }

    public void ExitGame()
    {
        StartCoroutine(ExitGameCoroutine(0));
    }

    private IEnumerator ExitGameCoroutine(int sceneIndex)
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

        int cScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(0);
        SceneManager.UnloadScene(cScene);
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

        //Thanks to this, tutorial message will appear when screen fades out
        TutorialManager.Instance.GoStepFurther();

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
        CurrentRoom.Leave();
        Room tmpCurrentRoom = CurrentRoom;
        CurrentRoom = _nextRoom;
        _nextRoom = CurrentRoom.PuzzleRoom;

        if (tmpCurrentRoom.ParentRoom == null && _nextRoom.ParentRoom != tmpCurrentRoom)
        {
            EquipmentManager.Instance.FlushOnNextRoom();
        }

        CurrentRoom.Initialize();   // nothing will happen if already initialized
        CurrentRoom.Enter();

        CameraManager.Instance.RecalculateToCurrentRoom();
        CameraManager.Instance.Enabled = CurrentRoom.CameraEnabled;

        StartCoroutine(EndMoveCoroutine());
    }

    private void OnRoomCommonPickablesCollected(Room r)
    {
        StartCoroutine(RoomFinishedCoroutine(r, ClearedText));
    }

    private void OnRoomAssignPuzzleFinished(Room r)
    {
        StartCoroutine(RoomFinishedCoroutine(r, PuzzleSolvedText));
    }

    //private void OnRoom0PickablesCollected()
    //{
    //    StartCoroutine(OnRoom0PickablesCollectedCoroutine());
    //}

    private IEnumerator RoomFinishedCoroutine(Room r, Text t)
    {
        t.gameObject.SetActive(true);
        RectTransform rt = t.GetComponent<RectTransform>();
        CanvasGroup cg = t.GetComponent<CanvasGroup>();
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
        t.gameObject.SetActive(false);

        //r.Leave();

        if (r.PuzzleRoom != null)
        {
            if(r.PuzzleRoom.Locked)
            {
                r.PuzzleRoom.Locked = false;
                r.PuzzleRoom.UnlockMapPart();
            }
            TransitionToRoom(r.PuzzleRoom);
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
