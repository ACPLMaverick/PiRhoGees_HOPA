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

    private bool _isCurrentlyTwoSided = false;
    private bool _turned;

    private Vector2 _upPosition;
    private Vector2 _downPosition;

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
    }

    // Use this for initialization
    void Start ()
    {
        RectTransform rt = _background.GetComponent<RectTransform>();

        InputManager.Instance.OnInputMove.AddListener(Slide);
        InputManager.Instance.OnInputSwipe.AddListener(TurnPage);

        _upPosition = GetComponent<RectTransform>().anchoredPosition;
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

            _headerText.text = header;
            _contextText.text = context;
            _secondSideContextText.text = secondSideContext;
            _isCurrentlyTwoSided = twoSided;

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

            rt.anchoredPosition = newPosition;
        }
    }

    private void TurnPage(InputManager.SwipeDirection dir, float length, Collider2D col)
    {
        if (gameObject.activeSelf)
        {
            float t = 0.3f;

            if (_turned && dir == InputManager.SwipeDirection.RIGHT)
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

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;
            Vector2 pos = Vector2.Lerp(startPos, position, lerp);
            rt.anchoredPosition = pos;

            yield return null;
        }
        rt.anchoredPosition = position;

        yield return null;
    }

    private void GetDownPosition(RectTransform field)
    {
        _downPosition = _upPosition;
        _downPosition.y += field.rect.height + 3.5f * field.anchoredPosition.y;
    }

    #endregion
}
