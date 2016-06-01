using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemInfo : MonoBehaviour
{
    #region private

    private CanvasGroup _grp;
    private Image _img;
    private Text _tit;
    private Text _txt;
    private Button _backButton;
    private Vector2 _defSizeDelta;

    private Vector3 _dPos;
    private Vector3 _dScl;
    private Vector3 _ndPos;
    private Vector3 _ndScl;

    private Vector3 _dTitPos;
    private Vector3 _ndTitPos;

    #endregion

    #region functions

    protected virtual void Awake()
    {
        _grp = GetComponent<CanvasGroup>();
        Image[] imgs = GetComponentsInChildren<Image>();
        _img = imgs[1];
        Text[] texts = GetComponentsInChildren<Text>();
        _tit = texts[0];
        _txt = texts[1];
        _backButton = GetComponentInChildren<Button>();
        _backButton.onClick.AddListener(new UnityEngine.Events.UnityAction(OnBackButtonClick));
        _defSizeDelta = _img.rectTransform.sizeDelta;

        _dPos = _img.GetComponent<RectTransform>().position;
        _dScl = _img.GetComponent<RectTransform>().localScale;

        _ndPos = GetComponent<RectTransform>().position;
        _ndScl = _img.GetComponent<RectTransform>().localScale * 1.5f;

        _dTitPos = _tit.rectTransform.position;
        _ndTitPos = _dTitPos;
        _ndTitPos.x = _tit.canvas.pixelRect.width * 0.5f;
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
        StartCoroutine(Utility.FadeCoroutineUI(_grp, 0.0f, 1.0f, 0.65f, true));
        _img.sprite = sprite;
        Vector2 nSizeDelta = _defSizeDelta;
        Vector2 nBounds = sprite.bounds.extents;
        nBounds.Normalize();
        nSizeDelta.x *= nBounds.x;
        nSizeDelta.y *= nBounds.y;

        _img.rectTransform.sizeDelta = nSizeDelta;
        _img.rectTransform.position = _dPos;
        _img.rectTransform.localScale = _dScl;

        _tit.text = title;
        _tit.rectTransform.position = _dTitPos;

        _txt.text = text;
    }

    public void Show(Sprite sprite, string title)
    {
        gameObject.SetActive(true);
        StartCoroutine(Utility.FadeCoroutineUI(_grp, 0.0f, 1.0f, 0.65f, true));
        _img.sprite = sprite;
        Vector2 nSizeDelta = _defSizeDelta;
        Vector2 nBounds = sprite.bounds.extents;
        nBounds.Normalize();
        nSizeDelta.x *= nBounds.x;
        nSizeDelta.y *= nBounds.y;

        _img.rectTransform.sizeDelta = nSizeDelta;
        _img.rectTransform.position = _ndPos;
        _img.rectTransform.localScale = _ndScl;

        _tit.text = title;
        _tit.rectTransform.position = _ndTitPos;

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
