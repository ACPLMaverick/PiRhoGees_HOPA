using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    #region private

    private CanvasGroup _grp;
    private Button _btnToggleSound;
    private Text _txtToggleSoundOn;
    private Text _txtToggleSoundOff;
    private Button _btnExit;
    private Button _btnReturn;

    #endregion

    #region functions

    protected virtual void Awake()
    {
        _grp = GetComponent<CanvasGroup>();
        Button[] btns = GetComponentsInChildren<Button>();

        foreach (Button b in btns)
        {
            if (b.name.Contains("ToggleSound"))
            {
                _btnToggleSound = b;
            }
            else if (b.name.Contains("Exit"))
            {
                _btnExit = b;
            }
            else if (b.name.Contains("Return"))
            {
                _btnReturn = b;
            }
        }

        _btnToggleSound.onClick.AddListener(new UnityEngine.Events.UnityAction(OnToggleSoundButtonClick));
        _btnExit.onClick.AddListener(new UnityEngine.Events.UnityAction(OnExitButtonClick));
        _btnReturn.onClick.AddListener(new UnityEngine.Events.UnityAction(OnReturnButtonClick));

        _txtToggleSoundOff = _btnToggleSound.GetComponentsInChildren<Text>()[0];
        _txtToggleSoundOn = _btnToggleSound.GetComponentsInChildren<Text>()[1];

        ToggleSoundText(AudioManager.Instance.IsAudioMuted);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            StartCoroutine(FadeCoroutine(0.0f, 1.0f, 0.5f, true));
        }
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(FadeCoroutine(1.0f, 0.0f, 0.5f, false));
        }
    }

    private void ToggleSoundText(bool stateTo)
    {
        _txtToggleSoundOff.gameObject.SetActive(!stateTo);
        _txtToggleSoundOn.gameObject.SetActive(stateTo);
    }

    private IEnumerator FadeCoroutine(float fadeStart, float fadeTarget, float timeSec, bool active)
    {
        float currentTime = Time.time;

        _grp.alpha = fadeStart;

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;

            _grp.alpha = Mathf.Lerp(fadeStart, fadeTarget, lerp);
            yield return null;
        }
        _grp.alpha = fadeTarget;
        gameObject.SetActive(active);

        yield return null;
    }

    private void OnToggleSoundButtonClick()
    {
        ToggleSoundText(AudioManager.Instance.ToggleMute());
    }

    private void OnExitButtonClick()
    {
        GameManager.Instance.ExitGame();
    }

    private void OnReturnButtonClick()
    {
        Hide();
    }

    #endregion
}
