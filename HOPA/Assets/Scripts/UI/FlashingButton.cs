using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlashingButton : MonoBehaviour
{
    #region public

    public Color ColorHighlight;
    public Color ColorNoHighlight;
    public float FlashSpeed = 1.0f;
    public bool InfluenceAlpha = false;

    #endregion

    #region private

    private Image _img;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        _img = GetComponent<Image>();
        _img.material = Instantiate(_img.material);
	}
	
	// Update is called once per frame
	void Update ()
    {
        float lerp = (Mathf.Sin(Time.time * FlashSpeed) + 1.0f) * 0.5f;
        Color finalColor = Color.Lerp(ColorNoHighlight, ColorHighlight, lerp);

        if(!InfluenceAlpha)
        {
            finalColor.a = 1.0f;
        }

        _img.material.color = finalColor;
    }

    #endregion
}
