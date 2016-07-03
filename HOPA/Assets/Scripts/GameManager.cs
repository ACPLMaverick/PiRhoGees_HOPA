using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    #region const

    private const float WAIT_FOR_ENDING_PHOTO_SECONDS = 7.5f;

    #endregion

    #region public

    public Room CurrentRoom;

    public Room RoomPrologue;
    public Room RoomFirst;
    public Room RoomFirstPuzzle;
    public Room RoomSecond;
    public Room RoomSecondPuzzle;
    public Room RoomThird;
    public Room RoomFourth;
    public EndingPhoto EndingPhoto;
    public Image FadeImage;
    public ItemInfo ItemInfoGroup;
    public PauseMenu PauseMenuGroup;

    public float RoomTransitionTime = 2.0f;

    #endregion

    #region private

    private Room _nextRoom = null;
    private bool _moveInProgress = false;
    private Coroutine _moveCoroutine = null;
    private Coroutine _endCoroutine = null;

    #endregion

    // Use this for initialization
    void Start ()
    {
        RoomFirst.FinishedEvent.AddListener(new UnityEngine.Events.UnityAction<Room>(OnRoomCommonPickablesCollected));
        RoomFirstPuzzle.FinishedEvent.AddListener(new UnityEngine.Events.UnityAction<Room>(OnRoomAssignPuzzleFinished));
        RoomSecond.FinishedEvent.AddListener(new UnityEngine.Events.UnityAction<Room>(OnRoomCommonPickablesCollected));
        RoomSecondPuzzle.FinishedEvent.AddListener(new UnityEngine.Events.UnityAction<Room>(OnRoomAssignPuzzleFinished));



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
	    // for tetin
        //if(Input.GetKeyUp(KeyCode.P))
        //{
        //    OnRoomCommonPickablesCollected(RoomFirst);
        //}
	}

    public void TransitionToRoom(Room room)
    {
        _nextRoom = room;
        int t = 7;
        if(room is RoomPuzzleAssign)
        {
            t = 11;
        }

        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        if (_endCoroutine != null)
            StopCoroutine(_endCoroutine);

        _moveCoroutine = StartCoroutine(StartMoveCoroutine(t));
        if (CurrentRoom.ParentRoom == null && _nextRoom.ParentRoom != CurrentRoom)
        {
            AudioManager.Instance.PlayMusic(room.AmbientTheme, RoomTransitionTime);
        }
    }

    public void TogglePauseMenu()
    {
        if(PauseMenuGroup.gameObject.activeSelf)
        {
            PauseMenuGroup.Hide();
        }
        else
        {
            PauseMenuGroup.Show();
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

        SceneChangeManager.Instance.ChangeScene(0);
    }

    private IEnumerator StartMoveCoroutine(int tutorialMsg)
    {
        _moveInProgress = true;
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
        TutorialManager.Instance.GoStepFurther(tutorialMsg);
        _moveInProgress = false;

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
        PickableHintManager.Instance.Flush();
        //_nextRoom = CurrentRoom.PuzzleRoom;

        if (tmpCurrentRoom.ParentRoom == null && _nextRoom.ParentRoom != tmpCurrentRoom)
        {
            EquipmentManager.Instance.FlushOnNextRoom();
        }

        CurrentRoom.Initialize();   // nothing will happen if already initialized
        CurrentRoom.Enter();

        CameraManager.Instance.RecalculateToCurrentRoom();
        CameraManager.Instance.Enabled = CurrentRoom.CameraEnabled;

        if (_endCoroutine != null)
            StopCoroutine(_endCoroutine);
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        _endCoroutine = StartCoroutine(EndMoveCoroutine());
    }

    private void OnRoomCommonPickablesCollected(Room r)
    {
        EndingPhoto.Show(r.EndingPhotoSprite, r.EndingPhotoTextSprite);
        StartCoroutine(RoomFinishedCoroutine(r));
    }

    private void OnRoomAssignPuzzleFinished(Room r)
    {
        EndingPhoto.Show(r.EndingPhotoSprite, r.EndingPhotoTextSprite);
        StartCoroutine(RoomFinishedCoroutine(r));
    }

    //private void OnRoom0PickablesCollected()
    //{
    //    StartCoroutine(OnRoom0PickablesCollectedCoroutine());
    //}

    private IEnumerator RoomFinishedCoroutine(Room r)
    {
        yield return new WaitForSeconds(WAIT_FOR_ENDING_PHOTO_SECONDS);

        //t.gameObject.SetActive(true);
        //RectTransform rt = t.GetComponent<RectTransform>();
        //CanvasGroup cg = t.GetComponent<CanvasGroup>();
        /*
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

            //rt.localScale = finalScale;
            //cg.alpha = finalAlpha;

            yield return null;
        }
        //rt.localScale = targetScale;
        //cg.alpha = targetAlpha;

        yield return new WaitForSeconds(timeSecondsStay);

        cTime = Time.time;

        while (Time.time - cTime <= timeSecondsOut)
        {
            float lerpValue = (Time.time - cTime) / timeSecondsOut;

            Vector2 finalScale = Vector2.Lerp(targetScale, startScale, lerpValue);
            float finalAlpha = Mathf.Lerp(targetAlpha, startAlpha, lerpValue);

            //rt.localScale = finalScale;
            //cg.alpha = finalAlpha;

            yield return null;
        }
        //rt.localScale = startScale;
        //cg.alpha = startAlpha;
        //t.gameObject.SetActive(false);

        //r.Leave();
        */

        if (r.PuzzleRoom != null)
        {
            if(r.PuzzleRoom.Locked)
            {
                r.PuzzleRoom.Locked = false;
                r.PuzzleRoom.UnlockMapPart();
            }
            TransitionToRoom(r.PuzzleRoom);

            //TutorialManager.Instance.GoStepFurther(11);
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
