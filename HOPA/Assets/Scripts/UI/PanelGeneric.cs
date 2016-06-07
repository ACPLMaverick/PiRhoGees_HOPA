using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PanelGeneric : MonoBehaviour
{
    #region const

    public const float Y_HIDDEN = -209.0f; 

    #endregion

    #region public

    public List<PanelGeneric> ConnectedPanels;
    public Image Arrow;

    #endregion

    #region properties

    public bool Hidden
    {
        get
        {
            return _isHidden;
        }
    }

    #endregion

    #region protected

    protected Button _showButton;
    RectTransform _rt;
    protected bool _isHidden = false;
    protected float _yStart;
    protected Coroutine _crt;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        _rt = GetComponent<RectTransform>();
        _showButton = GetComponentInChildren<Button>();
        _showButton.onClick.AddListener(TogglePanel);
        InputManager.Instance.OnInputSwipe.AddListener(HidePanelSwipe);
        InputManager.Instance.OnInputSwipe.AddListener(ShowPanelSwipe);
        _yStart = GetComponent<RectTransform>().anchoredPosition.y;
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void TogglePanel()
    {
        if(gameObject.activeSelf)
        {
            if(_isHidden)
            {
                Show(false);

                foreach (PanelGeneric p in ConnectedPanels)
                {
                    p.Show(!p.gameObject.activeSelf);
                }
            }
            else
            {
                Hide(false);

                foreach (PanelGeneric p in ConnectedPanels)
                {
                    p.Hide(!p.gameObject.activeSelf);
                }
            }
        }
    }

    public void Hide(bool immediate)
    {
        if(!_isHidden)
        {
            if (immediate)
            {
                if(_crt != null)
                    StopCoroutine(_crt);
                _rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, Y_HIDDEN);
            }
            else
            {
                _crt = StartCoroutine(Utility.TransformCoroutineUI(
                    _rt,
                    new Vector2(_rt.anchoredPosition.x, _yStart),
                    _rt.localRotation,
                    _rt.localScale,
                    new Vector2(_rt.anchoredPosition.x, Y_HIDDEN),
                    _rt.localRotation,
                    _rt.localScale,
                    0.5f,
                    true
                    ));
            }

            Arrow.transform.rotation = Quaternion.Euler(Vector3.zero);
            _isHidden = true;
        }
    }

    public void Show(bool immediate)
    {
        if(_isHidden)
        {
            if (immediate)
            {
                if (_crt != null)
                    StopCoroutine(_crt);
                _rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, _yStart);
            }
            else
            {
                _crt = StartCoroutine(Utility.TransformCoroutineUI(
                    _rt,
                    new Vector2(_rt.anchoredPosition.x, Y_HIDDEN),
                    _rt.localRotation,
                    _rt.localScale,
                    new Vector2(_rt.anchoredPosition.x, _yStart),
                    _rt.localRotation,
                    _rt.localScale,
                    0.5f,
                    true
                    ));
            }

            Arrow.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f));
            _isHidden = false;
        }
    }

    protected void HidePanelSwipe(Vector2 origin, InputManager.SwipeDirection dir, float length, Collider2D col)
    {
        if(gameObject.activeSelf && !_isHidden && dir == InputManager.SwipeDirection.DOWN && Utility.IsCursorInUIBounds(_rt, origin))
        {
            TogglePanel();
        }
    }

    protected void ShowPanelSwipe(Vector2 origin, InputManager.SwipeDirection dir, float length, Collider2D col)
    {
        if (gameObject.activeSelf && _isHidden && dir == InputManager.SwipeDirection.UP && Utility.IsCursorInUIBounds(_rt, origin))
        {
            TogglePanel();
        }
    }

    #endregion
}
