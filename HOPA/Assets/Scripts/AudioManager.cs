using UnityEngine;
using System.Collections;

public class AudioManager : Singleton<AudioManager>
{
    #region public

    public AudioSource MainAudioSource;
    public bool IsAudioMuted;

    #endregion

    #region functions

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

    public void ToggleMute()
    {
        if(IsAudioMuted)
        {
            MainAudioSource.mute = false;
            IsAudioMuted = false;
        }
        else
        {
            MainAudioSource.mute = true;
            IsAudioMuted = true;
        }
    }

    #endregion
}
