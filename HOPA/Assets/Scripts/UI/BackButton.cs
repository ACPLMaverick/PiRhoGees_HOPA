using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackButton : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(OnClick));
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    protected void OnClick()
    {
        if(GameManager.Instance.CurrentRoom.PrevRoom != null)
        {
            GameManager.Instance.TransitionToRoom(GameManager.Instance.CurrentRoom.PrevRoom);
        }
    }
}
