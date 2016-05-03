using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemInfoManager : Singleton<ItemInfoManager>
{
    #region private

    private CanvasGroup _grp;
    private Image _img;
    private Text _tit;
    private Text _txt;
    private Button _backButton;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        _grp = GetComponent<CanvasGroup>();
        Image[] imgs = GetComponentsInChildren<Image>();
        _img = imgs[1];
        Text[] texts = GetComponentsInChildren<Text>();
        _tit = texts[0];
        _txt = texts[1];
        _backButton = GetComponentInChildren<Button>();
        _backButton.onClick.AddListener(new UnityEngine.Events.UnityAction(OnBackButtonClick));
	}

    // Update is called once per frame
    void Update()
    {

    }

    public void Show(Sprite sprite, string title, string text)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeCoroutine(0.0f, 1.0f, 0.65f, true));
        _img.sprite = sprite;
        _tit.text = title;
        _txt.text = text;
    }

    public void Close()
    {
        OnBackButtonClick();
    }

    private void OnBackButtonClick()
    {
        StartCoroutine(FadeCoroutine(1.0f, 0.0f, 0.65f, false));
    }

    private IEnumerator FadeCoroutine(float fadeStart, float fadeTarget, float timeSec, bool active)
    {
        float currentTime = Time.time;

        _grp.alpha = fadeStart;

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;

            _grp.alpha = Mathf.Lerp(fadeStart, fadeTarget, lerp);
            yield return null;
        }
        _grp.alpha = fadeTarget;
        gameObject.SetActive(active);

        yield return null;
    }

    #endregion
}
