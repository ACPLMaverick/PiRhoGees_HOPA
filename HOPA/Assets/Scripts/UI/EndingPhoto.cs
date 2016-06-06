using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndingPhoto : MonoBehaviour
{
    #region const

    const float BEGIN_DELAY_SECONDS = 1.5f;
    const float FLASH_FADEIN_SECONDS = 0.1f;
    const float FLASH_FADEOUT_SECONDS = 0.5f;
    const float WAIT_AFTER_FLASH_SECONDS = 3.0f;
    const float ALL_FADEOUT_SECONDS = 1.0f;

    #endregion

    #region public

    public AudioClip SoundCamera;
    public AudioClip SoundVictory;

    #endregion

    #region private

    private CanvasGroup _mainGroup;
    private Image _endingBackground;
    private CanvasGroup _endingBackgroundGroup;
    private Image _endingFlash;
    private CanvasGroup _endingFlashGroup;
    private Image _endingPhoto;
    private Image _endingTextImage;
    private Animator _endingPhotoAnimator;
    private Animator _endingTextImageAnimator;

    #endregion

    #region functions

    protected virtual void Awake()
    {
        CanvasGroup[] groups = GetComponentsInChildren<CanvasGroup>();
        _mainGroup = groups[0];
        _endingBackgroundGroup = groups[1];
        _endingFlashGroup = groups[2];

        Image[] imgs = GetComponentsInChildren<Image>();
        _endingBackground = imgs[0];
        _endingPhoto = imgs[1];
        _endingTextImage = imgs[2];
        _endingFlash = imgs[3];

        _endingPhotoAnimator = _endingPhoto.GetComponent<Animator>();
        _endingTextImageAnimator = _endingTextImage.GetComponent<Animator>();
    }

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    public void Show(Sprite imgPhoto, Sprite imgText)
    {
        gameObject.SetActive(true);
        _endingPhoto.sprite = imgPhoto;
        _endingTextImage.sprite = imgText;

        _mainGroup.alpha = 1.0f;
        _endingBackgroundGroup.alpha = 0.0f;
        _endingFlashGroup.alpha = 0.0f;

        StartCoroutine(ShowCoroutine());
    }

    public void ShowImmediately(Sprite imgPhoto, Sprite imgText)
    {
        _endingPhoto.sprite = imgPhoto;
        _endingTextImage.sprite = imgText;

        gameObject.SetActive(true);

        _mainGroup.alpha = 1.0f;
        _endingBackgroundGroup.alpha = 1.0f;
        _endingFlashGroup.alpha = 0.0f;
    }

    public void HideImmediately()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator ShowCoroutine()
    {
        yield return new WaitForSeconds(BEGIN_DELAY_SECONDS);

        float time = 0.0f;
        while((time += Time.deltaTime) <= FLASH_FADEIN_SECONDS)
        {
            _endingFlashGroup.alpha = time / FLASH_FADEIN_SECONDS;
            yield return null;
        }

        _endingFlashGroup.alpha = 1.0f;
        yield return null;

        AudioManager.Instance.PlayClip(SoundCamera);
        _endingBackgroundGroup.alpha = 1.0f;
        _endingPhotoAnimator.Play("EndingPhotoIn");
        _endingTextImageAnimator.Play("EndingTextImageIn");

        time = 0.0f;
        while ((time += Time.deltaTime) <= FLASH_FADEOUT_SECONDS)
        {
            _endingFlashGroup.alpha = 1.0f - (time / FLASH_FADEOUT_SECONDS);
            yield return null;
        }

        _endingFlashGroup.alpha = 0.0f;

        AudioManager.Instance.PlayClip(SoundVictory);
        yield return new WaitForSeconds(WAIT_AFTER_FLASH_SECONDS);

        time = 0.0f;
        while ((time += Time.deltaTime) <= ALL_FADEOUT_SECONDS)
        {
            _mainGroup.alpha = 1.0f - (time / ALL_FADEOUT_SECONDS);
            yield return null;
        }

        _mainGroup.alpha = 0.0f;
        gameObject.SetActive(false);

        yield return null;
    }

    #endregion
}
