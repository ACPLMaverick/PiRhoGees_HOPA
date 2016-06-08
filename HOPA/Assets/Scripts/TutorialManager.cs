using UnityEngine;
using System.Collections;

public class TutorialManager : Singleton<TutorialManager> {

    #region public

    public bool IsEnabled;
    public bool EquipmentWasShown;
    public GameObject[] TutorialMessages;

    #endregion

    #region private

    private int _currentTutorialStep;
    private bool _stepHasChanged;

    #endregion

    #region functions

    protected override void Awake()
    {
        int i = PlayerPrefs.GetInt("Tutorial");
        if(i == 1)
        {
            IsEnabled = true;
        }
        else
        {
            IsEnabled = false;
        }
        base.Awake();
    }

    // Use this for initialization
    void Start () {
        _currentTutorialStep = 0;
        _stepHasChanged = true;
	}
	
	// Update is called once per frame
	void Update () {
	    if(IsEnabled)
        {
            DetectCurrentTutorialStep();
        }
	}

    void DetectCurrentTutorialStep()
    {
        if (_stepHasChanged)
        {
            for (int i = 0; i < TutorialMessages.Length; ++i)
            {
                if (i != _currentTutorialStep)
                {
                    TutorialMessages[i].SetActive(false);
                }
            }

            if (_currentTutorialStep >= TutorialMessages.Length)
            {
                FinishTutorial();
                return;
            }

            TutorialMessages[_currentTutorialStep].SetActive(true);
            _stepHasChanged = false;
        }
    }

    public void FinishTutorial()
    {
        IsEnabled = false;
    }

    public void HideCurrentMessage()
    {
        if(IsEnabled)
            TutorialMessages[_currentTutorialStep].SetActive(false);
    }

    public void GoStepFurther()
    {
        _currentTutorialStep++;
        _stepHasChanged = true;
    }

    public void GoStepFurther(int i)
    {
        if(_currentTutorialStep == i - 1)
        {
            _currentTutorialStep = i;
            _stepHasChanged = true;
        }
    }

    public void ShowMessageOutOfQueue(int i)
    {
        HideCurrentMessage();
        if(IsEnabled)
        {
            TutorialMessages[i].SetActive(true);
        }
    }

    public void DisableEquipmentHint()
    {
        EquipmentWasShown = true;
    }
    #endregion
}
