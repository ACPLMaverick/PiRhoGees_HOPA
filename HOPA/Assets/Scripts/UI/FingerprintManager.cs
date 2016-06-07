using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FingerprintManager : Singleton<FingerprintManager>
{
    #region public

    public Image Fingerprint;
    public bool DisappearIfNotHold = false;

    #endregion

    #region private

    private Vector3 _beginScale;
    private float _scaleMplier = 1.25f;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        int bFingerprint = PlayerPrefs.GetInt("bFingerprint", 0);

        if(bFingerprint != 0)
        {
            Fingerprint.gameObject.SetActive(true);
            gameObject.SetActive(true);

            _beginScale = Fingerprint.rectTransform.localScale;

            InputManager.Instance.OnInputClickUp.AddListener(OnClickUp);
            InputManager.Instance.OnInputClickDown.AddListener(OnClickDown);
            InputManager.Instance.OnInputMove.AddListener(OnMove);

            if(DisappearIfNotHold)
            {
                Fingerprint.gameObject.SetActive(false);
            }
        }
        else
        {
            Fingerprint.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    private void OnClickDown(Vector2 screenPos, Collider2D col)
    {
        if (DisappearIfNotHold)
        {
            Fingerprint.gameObject.SetActive(true);
        }

        Fingerprint.rectTransform.position = screenPos;
        Fingerprint.rectTransform.localScale = _beginScale * _scaleMplier;
    }

    private void OnClickUp(Vector2 screenPos, Collider2D col)
    {
        if (DisappearIfNotHold)
        {
            Fingerprint.gameObject.SetActive(false);
        }

        Fingerprint.rectTransform.position = screenPos;
        Fingerprint.rectTransform.localScale = _beginScale;
    }

    private void OnMove(Vector2 screenPos, Vector2 screenDir, Collider2D col)
    {
        Fingerprint.rectTransform.position = screenPos;
    }

    #endregion
}
