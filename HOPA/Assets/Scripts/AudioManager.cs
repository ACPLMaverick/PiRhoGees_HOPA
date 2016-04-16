using UnityEngine;
using System.Collections;

public class AudioManager : Singleton<AudioManager>
{
    #region public

    public AudioSource MainAudioSource;

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

    #endregion
}
