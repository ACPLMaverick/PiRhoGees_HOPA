using UnityEngine;
using System.Collections;

public class TutorialManager : Singleton<TutorialManager> {

    #region public

    public bool IsEnabled;
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

    void FinishTutorial()
    {
        IsEnabled = false;
    }

    public void GoStepFurther()
    {
        _currentTutorialStep++;
        _stepHasChanged = true;
    }

    #endregion
}
