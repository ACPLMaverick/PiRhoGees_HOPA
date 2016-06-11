using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ClickMarkManager : Singleton<ClickMarkManager>
{
    #region enum

    public enum Mode
    {
        NOTHING,
        UI,
        INTERACTION
    }

    #endregion

    #region public

    public Color ColorClickNothing;
    public Color ColorClickUI;
    public Color ColorClickInteraction;

    public float TimeTakesSeconds = 0.6f;

    #endregion

    #region private

    private Color _currentColor;
    private Image _img;
    private CanvasGroup _grp;
    private RectTransform _trn;
    private Coroutine _crt;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        _currentColor = ColorClickNothing;
        _img = GetComponent<Image>();
        _img.color = _currentColor;
        _grp = GetComponent<CanvasGroup>();
        _trn = GetComponent<RectTransform>();

        gameObject.SetActive(false);

        InputManager.Instance.OnInputClickUp.AddListener(OnClickUp);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Click(Vector2 screenPos)
    {
        gameObject.SetActive(true);
        if(_crt != null)
        {
            StopCoroutine(_crt);
        }
        _trn.position = new Vector3(screenPos.x, screenPos.y, _trn.position.z);
        _crt = StartCoroutine(ClickCoroutine());
    }

    public void Click(Vector2 screenPos, Mode md)
    {
        ChangeMode(md);
        Click(screenPos);
    }

    public void ChangeMode(Mode md)
    {
        _currentColor = ModeToColor(md);
        _img.color = _currentColor;
    }

    private void OnClickUp(Vector2 pos, Collider2D col)
    {
        int objectsLayerID = LayerMask.NameToLayer("Objects");

        if(col != null && col.gameObject != null &&
            col.gameObject.layer == objectsLayerID)
        {
            Click(pos, Mode.INTERACTION);
        }
        else if(InputManager.Instance.IsPointerOnGui())
        {
            Click(pos, Mode.UI);
        }
        else
        {
            Click(pos, Mode.NOTHING);
        }
    }

    private Color ModeToColor(Mode md)
    {
        switch (md)
        {
            case Mode.NOTHING:
                {
                    return ColorClickNothing;
                }
            case Mode.INTERACTION:
                {
                    return ColorClickInteraction;
                }
            case Mode.UI:
                {
                    return ColorClickUI;
                }
            default:
                return new Color(0.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    private IEnumerator ClickCoroutine()
    {
        _trn.localScale = Vector3.zero;
        _grp.alpha = 0.0f;
        float time = 0.0f;

        while(time < TimeTakesSeconds)
        {
            time += Time.deltaTime;
            Vector3 scl = Vector3.Lerp(Vector3.zero, Vector3.one, time);
            float alphaMplier = 0.0f;
            float h = time / Mathf.Max(TimeTakesSeconds, 0.0000001f);

            alphaMplier = Mathf.Clamp01(-(2.0f * h - 1.0f) * (2.0f * h - 1.0f) + 1.0f);

            _grp.alpha = alphaMplier;
            _trn.localScale = scl;

            yield return null;
        }

        _trn.localScale = Vector3.one;
        _grp.alpha = 0.0f;

        gameObject.SetActive(false);


        yield return null;
    }

    #endregion
}
