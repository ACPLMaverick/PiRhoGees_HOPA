﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FormUIManager : MonoBehaviour {

    #region constants
    public const string MaleHeader = "Szanowny Panie";
    public const string MaleAccusative = "Pana";
    public const string MaleNominative = "Pan";
    public const string FemaleHeader = "Szanowna Pani";
    public const string FemaleAccusative = "Panią";
    public const string FemaleNominative = "Pani";
    #endregion

    #region public
    public Text FormHeaderText;
    public InputField FirstNameInput;
    public Toggle MaleToggle;
    public Toggle FemaleToggle;
    public Button ProceedButton;

    public Text LetterHeaderText;
    public Text LetterContextText;
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

    public void ShowLetter(string gender)
    {
        string tmp = "";

        TurnOffForm();

        LetterHeaderText.gameObject.SetActive(true);
        switch (gender)
        {
            case "K":
                tmp = string.Format(LetterHeaderText.text, FemaleHeader);
                break;
            case "M":
                tmp = string.Format(LetterHeaderText.text, MaleHeader);
                break;
        }
        LetterHeaderText.text = tmp;

        LetterContextText.gameObject.SetActive(true);
        switch(gender)
        {
            case "K":
                tmp = string.Format(LetterContextText.text, FemaleAccusative, FemaleNominative);
                break;
            case "M":
                tmp = string.Format(LetterContextText.text, MaleAccusative, MaleNominative);
                break;
        }
        LetterContextText.text = tmp;
    }

    public void TurnOffForm()
    {
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

        ShowLetter(PlayerPrefs.GetString("Gender"));
    }
    #endregion
}
