using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIEvents : MonoBehaviour {
    #region Public

    public GameObject OptionsPanel;
    public GameObject CreditsPanel;
    public GameObject QuitPanel;
    public GameObject TutorialPanel;

    #endregion
  

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

    #region Options Panel Events
    public void OnMuteClick()
    {
        AudioManager.Instance.ToggleMute();
    }

    public void OnClearProgressClick()
    {
        PlayerPrefs.DeleteAll();
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
        SceneManager.LoadScene(1);
    }

    #endregion
}
