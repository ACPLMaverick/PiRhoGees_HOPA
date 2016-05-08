using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackButton : MonoBehaviour
{
    #region public

    public Sprite SpritePuzzle;
    public Sprite SpriteBack;

    #endregion

    #region private

    private Image _img;
    private bool _isPuzzle;

    #endregion

    #region functions

    protected virtual void Awake()
    {
        _img = GetComponent<Image>();
    }

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
        if(_isPuzzle)
        {
            if (GameManager.Instance.CurrentRoom.PuzzleRoom != null)
            {
                GameManager.Instance.TransitionToRoom(GameManager.Instance.CurrentRoom.PuzzleRoom);
            }
        }
        else
        {
            if (GameManager.Instance.CurrentRoom.ParentRoom != null)
            {
                GameManager.Instance.TransitionToRoom(GameManager.Instance.CurrentRoom.ParentRoom);
            }
        }
    }

    public void ShowAsPuzzle(bool flag)
    {
        _isPuzzle = flag;
        if(flag)
        {
            _img.sprite = SpritePuzzle;
        }
        else
        {
            _img.sprite = SpriteBack;
        }
    }

    #endregion
}
