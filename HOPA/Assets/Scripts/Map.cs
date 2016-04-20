﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

public class Map : PickableUsableObject
{
    #region public

    public Image MapObject;
    public AudioClip SoundUnfold;
    public AudioClip SoundFold;

    #endregion

    #region private

    // Buttons
    private Dictionary<Button, MapButton> _buttons;
    private bool _isEnabled = false;
    private CanvasGroup _mapCanvasGroup;

    #endregion

    #region functions

    // Use this for initialization
    override protected void Start ()
    {
        base.Start();

        _buttons = new Dictionary<Button, MapButton>();
        _mapCanvasGroup = MapObject.GetComponent<CanvasGroup>();

        Button[] buttons = MapObject.gameObject.GetComponentsInChildren<Button>();
        int count = buttons.Length;

        for(int i = 0; i < count; ++i)
        {
            if(buttons[i].name.Contains("MapButtonLoc"))
            {
                MapButton mp = buttons[i].gameObject.GetComponent<MapButton>();
                _buttons.Add(buttons[i], mp);
                mp.ClickedEvent.AddListener(new UnityAction<MapButton>(OnMapButtonClick));
            }
            else if(buttons[i].name.Contains("MapButtonBack"))
            {
                buttons[i].onClick.AddListener(new UnityAction(HideMap));
            }
        }
        MapObject.gameObject.SetActive(false);
        _mapCanvasGroup.alpha = 0.0f;
    }
	
	// Update is called once per frame
	override protected void Update ()
    {
        base.Update();
	}

    protected void OnMapButtonClick(MapButton mp)
    {
        //Debug.Log("Invoked for button " + mp.gameObject.name + " on room " + mp.AssociatedRoom.name);

        if(_isEnabled)
        {
            GameManager.Instance.TransitionToRoom(mp.AssociatedRoom);
            HideMap();
        }
    }

    protected void ShowMap()
    {
        StartCoroutine(MapVisibilityCoroutine(1.0f, 1.0f, true));
        MapObject.gameObject.SetActive(true);
        AudioManager.Instance.PlayClip(SoundUnfold, 0.0f);
        InputManager.Instance.InputAllEventsEnabled = false;
    }

    protected void HideMap()
    {
        StartCoroutine(MapVisibilityCoroutine(1.0f, 0.0f, false));
        _isEnabled = false;
        AudioManager.Instance.PlayClip(SoundFold, 0.0f);
    }

    protected IEnumerator MapVisibilityCoroutine(float timeSeconds, float targetOpacity, bool isUsableOnFinal)
    {
        float cTime = Time.time;
        float startOpacity = _mapCanvasGroup.alpha;

        while (Time.time - cTime <= timeSeconds)
        {
            float lerpValue = (Time.time - cTime) / timeSeconds;
            float finalOpacity = Mathf.Lerp(startOpacity, targetOpacity, lerpValue);
            _mapCanvasGroup.alpha = finalOpacity;
            yield return null;
        }
        _mapCanvasGroup.alpha = targetOpacity;
        MapObject.gameObject.SetActive(isUsableOnFinal);
        _isEnabled = isUsableOnFinal;

        if(!_isEnabled)
        {
            InputManager.Instance.InputAllEventsEnabled = true;
        }

        yield return null;
    }

    protected override void PerformActionOnClick(GameObject other)
    {
        if(other == null || other == gameObject)
        {
            if(_isEnabled)
            {
                HideMap();
            }
            else
            {
                ShowMap();
            }
        }
        else
        {
            Debug.Log(other.name);
        }
    }

    #endregion
}
