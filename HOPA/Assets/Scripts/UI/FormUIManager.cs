using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FormUIManager : MonoBehaviour {

    #region public
    public Image LocationBackground;
    public Sprite LocationBackgroundReplacement;

    public Image FormBackgroundImage;
    public Text FormHeaderText;
    public InputField FirstNameInput;
    public Toggle MaleToggle;
    public Toggle FemaleToggle;
    public Button ProceedButton;
    #endregion

    #region private
    private string _firstName;
    private string _gender;
    #endregion

    #region functions
    // Use this for initialization
    void Start () {
        _firstName = "";
        _gender = "";
	}
	
	// Update is called once per frame
	void Update () {
        if(ProceedButton != null)
            TurnOnProceedButton();
	}

    public void CheckEmptyGender()
    {
        if (!MaleToggle.isOn && !FemaleToggle.isOn)
        {
            _gender = "";
            Debug.Log(_gender);
        }
    }

    public void TurnOnProceedButton()
    {
        if (_firstName != "" && _gender != "")
        {
            ProceedButton.interactable = true;
        }
        else
        {
            ProceedButton.interactable = false;
        }
    }

    public void TurnOffForm()
    {
        FormBackgroundImage.gameObject.SetActive(false);
        FormHeaderText.gameObject.SetActive(false);
        FirstNameInput.gameObject.SetActive(false);
        MaleToggle.gameObject.SetActive(false);
        FemaleToggle.gameObject.SetActive(false);
        ProceedButton.gameObject.SetActive(false);
    }
    #endregion

    #region ui_events_functions
    public void OnFirstNameInputEndEdit()
    {
        _firstName = FirstNameInput.text;
    }

    public void OnMaleToggleValueChange()
    {
        if (MaleToggle.isOn)
        {
            _gender = "M";
            FemaleToggle.isOn = false;
            Debug.Log(_gender);
        }

        CheckEmptyGender();
    }

    public void OnFemaleToggleValueChange()
    {
        if (FemaleToggle.isOn)
        {
            _gender = "K";
            MaleToggle.isOn = false;
            Debug.Log(_gender);
        }

        CheckEmptyGender();
    }

    public void OnProceedButtonClick()
    {
        PlayerPrefs.SetString("FirstName", _firstName);
        PlayerPrefs.SetString("Gender", _gender);

        //LocationBackground.sprite = LocationBackgroundReplacement;
        //LetterButton.interactable = true;
        //TurnOffForm();
        SceneChangeManager.Instance.ChangeScene(2);
    }

    #endregion
}
