using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class LetterUI : MonoBehaviour
{

    #region events

    public UnityEvent OnLetterOpened = new UnityEvent();
    public UnityEvent OnPageTurned = new UnityEvent();
    public UnityEvent OnLetterClosed = new UnityEvent();

    #endregion

    #region public

    public AudioClip LetterSound;

    #endregion

    #region private

    private Image _background;
    private Text _headerText;
    private Text _contextText;
    private Text _secondSideContextText;
    private Image _edgeFadeLeft;
    private Image _edgeFadeRight;
    private Button _btnLeft;
    private Button _btnRight;

    private bool _isCurrentlyTwoSided = false;
    private bool _turned;

    private Vector2 _upPosition;
    private Vector2 _downPosition;
    private float _btnBeginY;

    #endregion

    #region functions

    protected virtual void Awake()
    {
        _background = GetComponent<Image>();
        Text[] texts = GetComponentsInChildren<Text>();
        int tCount = texts.Length;
        Image[] images = GetComponentsInChildren<Image>();

        for(int i = 0; i < tCount; ++i)
        {
            if(texts[i].name.Equals("LetterHeader"))
            {
                _headerText = texts[i];
            }
            else if (texts[i].name.Equals("LetterContext"))
            {
                _contextText = texts[i];
            }
            else if (texts[i].name.Equals("LetterSecondSideContext"))
            {
                _secondSideContextText = texts[i];
            }
        }

        tCount = images.Length;
        for (int i = 0; i < tCount; ++i)
        {
            if (images[i].name.Equals("LetterEdgeFadeRight"))
            {
                _edgeFadeRight = images[i];
            }
            else if (images[i].name.Equals("LetterEdgeFadeLeft"))
            {
                _edgeFadeLeft = images[i];
            }
        }

        Button[] btns = GetComponentsInChildren<Button>();
        _btnLeft = btns[0];
        _btnRight = btns[1];
        _btnBeginY = _btnLeft.transform.localPosition.y;
    }

    // Use this for initialization
    void Start ()
    {
        RectTransform rt = _background.GetComponent<RectTransform>();

        InputManager.Instance.OnInputMove.AddListener(Slide);
        InputManager.Instance.OnInputSwipe.AddListener(TurnPage);

        _upPosition = rt.anchoredPosition;
        GetDownPosition(_contextText.GetComponent<RectTransform>());

        gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Show(string header, string context, string secondSideContext, bool twoSided)
    {
        if(!gameObject.activeSelf)
        {
            AudioManager.Instance.PlayClip(LetterSound);

            CameraManager.Instance.Enabled = false;

            gameObject.SetActive(true);
            _secondSideContextText.gameObject.SetActive(false);
            _edgeFadeLeft.gameObject.SetActive(false);
            _btnLeft.gameObject.SetActive(false);

            _headerText.text = header;
            _contextText.text = context;
            _secondSideContextText.text = secondSideContext;
            _isCurrentlyTwoSided = twoSided;

            _headerText.gameObject.GetComponent<CanvasGroup>().alpha = 1;
            _headerText.gameObject.SetActive(true);
            _contextText.gameObject.GetComponent<CanvasGroup>().alpha = 1;
            _contextText.gameObject.SetActive(true);

            _btnLeft.transform.localPosition = new Vector3(_btnLeft.transform.localPosition.x, _btnBeginY, _btnLeft.transform.transform.localPosition.z);
            _btnRight.transform.localPosition = new Vector3(_btnRight.transform.localPosition.x, _btnBeginY, _btnRight.transform.transform.localPosition.z);

            GetComponent<RectTransform>().anchoredPosition = _upPosition;

            GetDownPosition(_contextText.GetComponent<RectTransform>());
            _turned = false;

            OnLetterOpened.Invoke();
            StartCoroutine(Utility.FadeCoroutineUI(_background.GetComponent<CanvasGroup>(), 0.0f, 1.0f, 0.5f, true));
        }
    }

    public void Hide()
    {
        if(gameObject.activeSelf)
        {
            AudioManager.Instance.PlayClip(LetterSound);

            CameraManager.Instance.Enabled = true;

            OnLetterClosed.Invoke();
            StartCoroutine(Utility.FadeCoroutineUI(_background.GetComponent<CanvasGroup>(), 1.0f, 0.0f, 0.5f, false));
        }
    }

    private void Slide(Vector2 origin, Vector2 direction, Collider2D col)
    {
        if (gameObject.activeSelf)
        {
            RectTransform rt = GetComponent<RectTransform>();
            Vector2 newPosition = rt.anchoredPosition;
            newPosition += direction;
            newPosition.y = Mathf.Max(Mathf.Min(newPosition.y, _downPosition.y), _upPosition.y);
            newPosition.x = _upPosition.x;

            float zdziszek = _btnLeft.transform.localPosition.y - (newPosition.y - rt.anchoredPosition.y);

            _btnLeft.transform.localPosition = new Vector3(_btnLeft.transform.localPosition.x, zdziszek, _btnLeft.transform.transform.localPosition.z);
            _btnRight.transform.localPosition = new Vector3(_btnRight.transform.localPosition.x, zdziszek, _btnRight.transform.transform.localPosition.z);

            rt.anchoredPosition = newPosition;
        }
    }

    public void TurnLeft()
    {
        TurnPage(Vector2.zero, InputManager.SwipeDirection.LEFT, 100.0f, null);
    }

    public void TurnRight()
    {
        TurnPage(Vector2.zero, InputManager.SwipeDirection.RIGHT, 100.0f, null);
    }

    private void TurnPage(Vector2 origin, InputManager.SwipeDirection dir, float length, Collider2D col)
    {
        if (gameObject.activeSelf)
        {
            float t = 0.3f;

            if ((_turned && dir == InputManager.SwipeDirection.RIGHT) || 
                (!_turned && dir == InputManager.SwipeDirection.RIGHT && !_isCurrentlyTwoSided))
            {
                Hide();
            }
            else if (!_turned && dir == InputManager.SwipeDirection.RIGHT)
            {
                AudioManager.Instance.PlayClip(LetterSound);

                GetDownPosition(_secondSideContextText.GetComponent<RectTransform>());

                StartCoroutine(Utility.FadeCoroutineUI(_headerText.GetComponent<CanvasGroup>(), 1.0f, 0.0f, t, false));
                StartCoroutine(Utility.FadeCoroutineUI(_contextText.GetComponent<CanvasGroup>(), 1.0f, 0.0f, t, false));
                _secondSideContextText.gameObject.SetActive(true);
                StartCoroutine(Utility.FadeCoroutineUI(_secondSideContextText.GetComponent<CanvasGroup>(), 0.0f, 1.0f, t, true));

                _edgeFadeLeft.gameObject.SetActive(true);
                _btnLeft.gameObject.SetActive(true);
                StartCoroutine(Utility.FadeCoroutineUI(_edgeFadeLeft.GetComponent<CanvasGroup>(), 0.0f, 1.0f, t, true));
                //StartCoroutine(Utility.FadeCoroutineUI(_edgeFadeRight.GetComponent<CanvasGroup>(), 1.0f, 0.0f, t, false));

                StartCoroutine(SlideToPositionCoroutine(_upPosition, 0.5f));

                _turned = true;

                OnPageTurned.Invoke();
            }
            else if (_turned && dir == InputManager.SwipeDirection.LEFT)
            {
                AudioManager.Instance.PlayClip(LetterSound);

                GetDownPosition(_contextText.GetComponent<RectTransform>());

                _headerText.gameObject.SetActive(true);
                _contextText.gameObject.SetActive(true);
                StartCoroutine(Utility.FadeCoroutineUI(_headerText.GetComponent<CanvasGroup>(), 0.0f, 1.0f, t, true));
                StartCoroutine(Utility.FadeCoroutineUI(_contextText.GetComponent<CanvasGroup>(), 0.0f, 1.0f, t, true));
                StartCoroutine(Utility.FadeCoroutineUI(_secondSideContextText.GetComponent<CanvasGroup>(), 1.0f, 0.0f, t, false));

                StartCoroutine(Utility.FadeCoroutineUI(_edgeFadeLeft.GetComponent<CanvasGroup>(), 1.0f, 0.0f, t, false));
                _edgeFadeRight.gameObject.SetActive(true);
                _edgeFadeLeft.gameObject.SetActive(false);
                _btnLeft.gameObject.SetActive(false);
                //StartCoroutine(Utility.FadeCoroutineUI(_edgeFadeRight.GetComponent<CanvasGroup>(), 0.0f, 1.0f, t, true));

                StartCoroutine(SlideToPositionCoroutine(_upPosition, 0.5f));

                _turned = false;
                
                OnPageTurned.Invoke();
            }
        }
    }

    private IEnumerator SlideToPositionCoroutine(Vector2 position, float timeSec)
    {
        float currentTime = Time.time;
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        Vector2 lBtnStartPos = _btnLeft.transform.localPosition;
        Vector2 rBtnStartPos = _btnRight.transform.localPosition;
        Vector2 lBtnFinalPos = lBtnStartPos;
        lBtnFinalPos.y = _btnBeginY;
        Vector2 rBtnFinalPos = rBtnStartPos;
        rBtnFinalPos.y = _btnBeginY;

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;
            Vector2 pos = Vector2.Lerp(startPos, position, lerp);
            _btnLeft.transform.localPosition = Vector2.Lerp(lBtnStartPos, lBtnFinalPos, lerp);
            _btnRight.transform.localPosition = Vector2.Lerp(rBtnStartPos, rBtnFinalPos, lerp);
            rt.anchoredPosition = pos;

            yield return null;
        }
        rt.anchoredPosition = position;
        _btnLeft.transform.localPosition = lBtnFinalPos;
        _btnRight.transform.localPosition = rBtnFinalPos;

        yield return null;
    }

    private void GetDownPosition(RectTransform field)
    {
        _downPosition = _upPosition;
        _downPosition.y += field.rect.height + 3.5f * field.anchoredPosition.y;
    }

    #endregion
}
