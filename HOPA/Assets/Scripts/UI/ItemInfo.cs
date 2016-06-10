using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemInfo : MonoBehaviour
{
    #region private

    private CanvasGroup _grp;
    private Image _img;
    private Image _imgBig;
    private Text _tit;
    private Text _titBig;
    private Text _txt;
    private Button _backButton;
    private Vector2 _defSizeDelta;

    #endregion

    #region functions

    protected virtual void Awake()
    {
        _grp = GetComponent<CanvasGroup>();
        Image[] imgs = GetComponentsInChildren<Image>();
        _img = imgs[1];
        _imgBig = imgs[3];
        Text[] texts = GetComponentsInChildren<Text>();
        _tit = texts[0];
        _txt = texts[1];
        _titBig = texts[2];
        _backButton = GetComponentInChildren<Button>();
        _backButton.onClick.AddListener(new UnityEngine.Events.UnityAction(OnBackButtonClick));
        _defSizeDelta = _img.rectTransform.sizeDelta;
    }

    // Use this for initialization
    void Start ()
    {

	}

    // Update is called once per frame
    void Update()
    {

    }

    public void Show(Sprite sprite, string title, string text)
    {
        gameObject.SetActive(true);
        _tit.gameObject.SetActive(true);
        _txt.gameObject.SetActive(true);
        _img.gameObject.SetActive(true);
        _titBig.gameObject.SetActive(false);
        _imgBig.gameObject.SetActive(false);

        StartCoroutine(Utility.FadeCoroutineUI(_grp, 0.0f, 1.0f, 0.65f, true));
        _img.sprite = sprite;
        Vector2 nSizeDelta = _defSizeDelta;
        Vector2 nBounds = sprite.bounds.extents;
        nBounds.Normalize();
        nSizeDelta.x *= nBounds.x;
        nSizeDelta.y *= nBounds.y;

        _img.rectTransform.sizeDelta = nSizeDelta;

        _tit.text = title;

        _txt.text = text;
    }

    public void Show(Sprite sprite, string title)
    {
        gameObject.SetActive(true);
        _titBig.gameObject.SetActive(true);
        _imgBig.gameObject.SetActive(true);
        _txt.gameObject.SetActive(false);
        _tit.gameObject.SetActive(false);
        _img.gameObject.SetActive(false);

        StartCoroutine(Utility.FadeCoroutineUI(_grp, 0.0f, 1.0f, 0.65f, true));
        _imgBig.sprite = sprite;
        Vector2 nSizeDelta = _defSizeDelta;
        Vector2 nBounds = sprite.bounds.extents;
        nBounds.Normalize();
        nSizeDelta.x *= nBounds.x;
        nSizeDelta.y *= nBounds.y;

        _imgBig.rectTransform.sizeDelta = nSizeDelta;

        _titBig.text = title;

        _txt.text = "";
    }

    public void Close()
    {
        OnBackButtonClick();
    }

    private void OnBackButtonClick()
    {
        StartCoroutine(Utility.FadeCoroutineUI(_grp,1.0f, 0.0f, 0.65f, false));
    }

    #endregion
}
