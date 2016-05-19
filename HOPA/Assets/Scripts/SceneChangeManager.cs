using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeManager : Singleton<SceneChangeManager>
{
    #region constants

    private const float FADE_TIME = 0.3f;
    private const float ROTATE_SPEED = 100.0f;
    private const string PP_KEY = "LoadingScreenIn";
    private const float IMPOSSIBLE_ANGLE = 0.0f;

    #endregion

    #region public 

    public string LoadingString;
    public Image LoadingScreen;
    public Canvas CurrentCanvas;

    #endregion

    #region private

    private Image _rotatingImage;
    private Text _loadingText;
    private bool _rotatingFlag;
    private Coroutine _crtTemp;
    private Coroutine _crtTempRot;

    private Image _prevLoadingScreen;

    #endregion

    #region functions

    // Use this for initialization
    void Start()
    {
        _rotatingImage = LoadingScreen.GetComponentsInChildren<Image>()[1];
        _loadingText = LoadingScreen.GetComponentInChildren<Text>();
        _loadingText.text = LoadingString;

        float angle = PlayerPrefs.GetFloat(PP_KEY);
        Debug.Log(angle);
        if (angle != IMPOSSIBLE_ANGLE)
        {
            PlayerPrefs.SetFloat(PP_KEY, IMPOSSIBLE_ANGLE);
            // swapping load screen from old to new
            LoadingScreen.gameObject.SetActive(true);
            LoadingScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
            Vector3 cEuler = _rotatingImage.GetComponent<RectTransform>().localRotation.eulerAngles;
            cEuler.z = angle;
            _rotatingImage.GetComponent<RectTransform>().localRotation = Quaternion.Euler(cEuler);
            ////
            StartCoroutine(FadeCoroutineUI(LoadingScreen.GetComponent<CanvasGroup>(), 1.0f, 0.0f, FADE_TIME, false, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void OnDestroy()
    {
        if(!_rotatingFlag)
        {
            PlayerPrefs.SetFloat(PP_KEY, IMPOSSIBLE_ANGLE);
        }
    }

    public void ChangeScene(int index)
    {
        _prevLoadingScreen = LoadingScreen;
        _loadingText = LoadingScreen.GetComponentInChildren<Text>();

        LoadingScreen.gameObject.SetActive(true);
        _crtTemp = StartCoroutine(FadeCoroutineUI(_prevLoadingScreen.GetComponent<CanvasGroup>(), 0.0f, 1.0f, FADE_TIME, true, index));
        _rotatingFlag = true;
        _crtTempRot = StartCoroutine(RotateCoroutineUI(_rotatingImage, ROTATE_SPEED));
    }

    private void ChangeSceneIndeed(int index)
    {
        int cScene = SceneManager.GetActiveScene().buildIndex;
        AsyncOperation op = SceneManager.LoadSceneAsync(index);
        op.priority = 999;
        op.allowSceneActivation = true;
        //SceneManager.UnloadScene(cScene);
        float cAngle = _rotatingImage.GetComponent<RectTransform>().localRotation.eulerAngles.z;
        if(cAngle == 0.0f)
        {
            cAngle = 0.1f;
        }
        PlayerPrefs.SetFloat(PP_KEY, cAngle);
        //StopCoroutine(_crtTemp);
        //StopCoroutine(_crtTempRot);
    }

    private IEnumerator FadeCoroutineUI(CanvasGroup grp, float fadeStart, float fadeTarget, float timeSec, bool active, int nextSceneIndex)
    {
        float currentTime = Time.time;

        grp.alpha = fadeStart;

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;

            grp.alpha = Mathf.Lerp(fadeStart, fadeTarget, lerp);
            yield return null;
        }
        grp.alpha = fadeTarget;
        grp.gameObject.SetActive(active);

        if(active)
        {
            ChangeSceneIndeed(nextSceneIndex);
        }
        else
        {
            
        }

        yield return null;
    }

    private IEnumerator RotateCoroutineUI(Image img, float rotateSpeed)
    {
        RectTransform rt = img.GetComponent<RectTransform>();
        while(_rotatingFlag)
        {
            Vector3 rot = rt.localRotation.eulerAngles;
            rot.z += rotateSpeed * Time.fixedDeltaTime;
            rt.localRotation = Quaternion.Euler(rot);
            yield return null;
        }

        yield return null;
    }

    #endregion
}
