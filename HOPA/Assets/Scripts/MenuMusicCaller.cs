using UnityEngine;
using System.Collections;

public class MenuMusicCaller : MonoBehaviour
{
    public AudioClip MenuMusic;

	// Use this for initialization
	void Start ()
    {
        AudioManager.Instance.PlayMusic(MenuMusic, 0.0f);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
