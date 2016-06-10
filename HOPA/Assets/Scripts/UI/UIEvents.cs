using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEvents : MonoBehaviour {
    #region Private

    private Text _txtToggleSoundOn;
    private Text _txtToggleSoundOff;

    #endregion

    #region Public

    public Button AudioToggleButton;
    public GameObject OptionsPanel;
    public GameObject CreditsPanel;
    public GameObject QuitPanel;
    public GameObject TutorialPanel;

    #endregion
    void Start()
    {
        _txtToggleSoundOff = AudioToggleButton.GetComponentsInChildren<Text>()[0];
        _txtToggleSoundOn = AudioToggleButton.GetComponentsInChildren<Text>()[1];
        PlayerPrefs.SetInt("bFingerprint",0);

        ToggleSoundText(AudioManager.Instance.IsAudioMuted);
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            OptionsPanel.SetActive(false);
            CreditsPanel.SetActive(false);
            TutorialPanel.SetActive(false);
            QuitPanel.SetActive(true);
        }
    }

    public void OnBackClick(GameObject panel)
    {
        panel.SetActive(false);
    }

    #region Main Buttons Events
    public void OnStartGameClick()
    {
        TutorialPanel.SetActive(true);
        //SceneManager.LoadScene(1);
    }

    public void OnOptionsClick()
    {
        OptionsPanel.SetActive(true);
    }

    public void OnCredtisClick()
    {
        CreditsPanel.SetActive(true);
    }
    #endregion

    private void ToggleSoundText(bool stateTo)
    {
        _txtToggleSoundOff.gameObject.SetActive(!stateTo);
        _txtToggleSoundOn.gameObject.SetActive(stateTo);
    }

    #region Options Panel Events
    public void OnMuteClick()
    {
        //AudioManager.Instance.ToggleMute();
        ToggleSoundText(AudioManager.Instance.ToggleMute());
    }

    public void OnClearProgressClick()
    {
        PlayerPrefs.DeleteAll();
    }

    public void OnToggleFingerprintClick()
    {
        PlayerPrefs.SetInt("bFingerprint", 1);
    }
    #endregion

    #region Quit Panel Events

    public void OnYesClick()
    {
        Application.Quit();
    }

    public void OnNoClick()
    {
        QuitPanel.SetActive(false);
    }

    #endregion

    #region Tutorial Panel Events

    public void OnTutorialClick(int value)
    {
        PlayerPrefs.SetInt("Tutorial", value);
        SceneChangeManager.Instance.ChangeScene(1);
    }

    #endregion
}
