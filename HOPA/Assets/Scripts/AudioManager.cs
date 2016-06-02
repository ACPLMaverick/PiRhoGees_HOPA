using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    #region public

    public AudioSource MainAudioSource;
    public bool IsAudioMuted;

    public AudioClip DefaultSoundPickUp;
    public AudioClip DefaultSoundKeyUnlock;

    #endregion

    #region private

    private struct MusicQueueItem
    {
        public AudioClip clip;
        public float fadeTime;

        public MusicQueueItem(AudioClip clp, float tm)
        {
            clip = clp;
            fadeTime = tm;
        }
    }
    
    private AudioSource _musicAudioSourceFirst;
    private AudioSource _musicAudioSourceSecond;
    private Queue<MusicQueueItem> _musicChangeQueue;

    #endregion

    #region functions

    protected override void Awake()
    {
        _musicChangeQueue = new Queue<MusicQueueItem>();
        _destroyOnLoad = false;
        _musicAudioSourceFirst = Instantiate(MainAudioSource);
        _musicAudioSourceSecond = Instantiate(MainAudioSource);
        _musicAudioSourceFirst.transform.parent = transform;
        _musicAudioSourceSecond.transform.parent = transform;

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

    public void PlayClip(AudioClip clip, float delaySeconds = 0.0f)
    {
        MainAudioSource.clip = clip;
        MainAudioSource.PlayDelayed(delaySeconds);
    }

    public bool ToggleMute()
    {
        IsAudioMuted = !IsAudioMuted;
        MainAudioSource.mute = IsAudioMuted;
        _musicAudioSourceFirst.mute = IsAudioMuted;
        _musicAudioSourceSecond.mute = IsAudioMuted;
        return IsAudioMuted;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="music">Null can be passed, means silence</param>
    /// <param name="fadeSeconds">Fade in seconds between current and new track</param>
    public void PlayMusic(AudioClip music, float fadeSeconds = 0.0f, bool looping = true)
    {
        Debug.Log("PlayMusic");
        if(fadeSeconds == 0.0f)
        {
            _musicAudioSourceSecond.clip = music;
            _musicAudioSourceSecond.volume = 1.0f;
            _musicAudioSourceSecond.loop = looping;
            _musicAudioSourceSecond.Play();
            _musicAudioSourceFirst.volume = 0.0f;
            _musicAudioSourceFirst.Stop();

            AudioSource tmp = _musicAudioSourceSecond;
            _musicAudioSourceSecond = _musicAudioSourceFirst;
            _musicAudioSourceFirst = tmp;
        }
        else if(_musicChangeQueue.Count != 0)
        {
            _musicChangeQueue.Enqueue(new MusicQueueItem(music, fadeSeconds));
        }
        else
        {
            StartCoroutine(MusicChangeCoroutine(music, fadeSeconds));
        }
    }

    private IEnumerator MusicChangeCoroutine(AudioClip music, float fadeTime)
    {
        float currentTime = Time.time;
        _musicAudioSourceSecond.clip = music;
        _musicAudioSourceSecond.Play();
        _musicAudioSourceSecond.volume = 0.0f;

        while (Time.time - currentTime <= fadeTime)
        {
            float lerp = (Time.time - currentTime) / fadeTime;
            _musicAudioSourceSecond.volume = lerp;
            _musicAudioSourceFirst.volume = 1.0f - lerp;

            yield return null;
        }
        _musicAudioSourceSecond.volume = 1.0f;
        _musicAudioSourceFirst.Stop();

        AudioSource tmp = _musicAudioSourceSecond;
        _musicAudioSourceSecond = _musicAudioSourceFirst;
        _musicAudioSourceFirst = tmp;

        if(_musicChangeQueue.Count != 0)
        {
            MusicQueueItem it = _musicChangeQueue.Dequeue();
            PlayMusic(it.clip, it.fadeTime);
        }

        yield return null;
    }

    #endregion
}
