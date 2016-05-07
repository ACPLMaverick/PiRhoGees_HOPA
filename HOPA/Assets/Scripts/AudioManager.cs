using UnityEngine;
using System.Collections;

public class AudioManager : Singleton<AudioManager>
{
    #region public

    public AudioSource MainAudioSource;
    public bool IsAudioMuted;

    #endregion

    #region functions

    protected override void Awake()
    {
        _destroyOnLoad = false;
        base.Awake();
    }

    // Use this for initialization
    void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void PlayClip(AudioClip clip, float delaySeconds)
    {
        MainAudioSource.clip = clip;
        MainAudioSource.PlayDelayed(delaySeconds);
    }

    public bool ToggleMute()
    {
        if(IsAudioMuted)
        {
            IsAudioMuted = false;
        }
        else
        {
            IsAudioMuted = true;
        }
        MainAudioSource.mute = IsAudioMuted;
        return IsAudioMuted;
    }

    #endregion
}
