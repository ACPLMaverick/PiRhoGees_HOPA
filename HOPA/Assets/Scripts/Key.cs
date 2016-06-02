﻿using UnityEngine;
using System.Collections.Generic;

public class Key : PickableUsableObject
{
    #region const

    protected const string UNLOCK_SOUND_ASSET_PATH = "Sounds/pick_item";

    #endregion

    #region public

    public List<DisappearableObject> LockedDisappearableObjects;
    public AudioClip SoundUnlock;

    #endregion

    #region functions

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();

        int ldCount = LockedDisappearableObjects.Count;
        for(int i = 0; i < ldCount; ++i)
        {
            LockedDisappearableObjects[i].LockWithKey(this);
        }

        if(SoundUnlock == null)
        {
            SoundUnlock = AudioManager.Instance.DefaultSoundKeyUnlock;
        }
	}
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();
	}

    protected override void PerformActionOnClick(GameObject other)
    {
        base.PerformActionOnClick(other);

        if(other != null && other.GetComponent<DisappearableObject>() != null &&
            LockedDisappearableObjects.Contains(other.GetComponent<DisappearableObject>()))
        {
            other.GetComponent<DisappearableObject>().UnlockWithKey(this);
            AudioManager.Instance.PlayClip(SoundUnlock);
        }
    }

    #endregion
}
