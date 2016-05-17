using UnityEngine;
using System.Collections;

public class MapEdgeFade : MonoBehaviour
{
    #region private

    private bool _shown = true;
    private CanvasGroup _grp;
    private Coroutine _crtPtr;

    #endregion

    protected virtual void Awake()
    {
        _grp = GetComponent<CanvasGroup>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Show()
    {
        if(!_shown)
        {
            if(_crtPtr != null)
            {
                StopCoroutine(_crtPtr);
            }
            gameObject.SetActive(true);
            _crtPtr = StartCoroutine(Utility.FadeCoroutineUI(_grp, 0.0f, 1.0f, 0.35f, true));
            _shown = true;
        }
    }

    public void Hide()
    {
        if(_shown)
        {
            if (_crtPtr != null)
            {
                StopCoroutine(_crtPtr);
            }
            StartCoroutine(Utility.FadeCoroutineUI(_grp, 1.0f, 0.0f, 0.35f, false));
            _shown = false;
        }
    }

    public void ShowImmediate()
    {
        _grp.alpha = 1.0f;
        gameObject.SetActive(true);
        _shown = true;
    }

    public void HideImmediate()
    {
        _grp.alpha = 0.0f;
        gameObject.SetActive(false);
        _shown = false;
    }
}
