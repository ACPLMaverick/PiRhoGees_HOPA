using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Letter : PickableObject {

    #region constants
    public const string MaleHeader = "Szanowny Panie";
    public const string MaleAccusative = "Pana";
    public const string MaleNominative = "Pan";
    public const string FemaleHeader = "Szanowna Pani";
    public const string FemaleAccusative = "Panią";
    public const string FemaleNominative = "Pani";
    #endregion

    #region public
    public Button LetterBackground;
    public Text LetterHeaderText;
    public Text LetterContextText;
    public Text LetterSecondSideContextText;
    #endregion

    #region private
    private bool _isLetterTurned;
    #endregion

    #region functions
    // Use this for initialization
    protected override void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update () {

	}

    protected override void PickUp(Vector2 position, Collider2D col)
    {
        if (col != null && col.gameObject == this.gameObject && !_picked)
        {
            col.gameObject.transform.SetParent(Camera.main.transform, true);
            Vector3 tgt = Vector3.zero, scl = Vector3.zero;

            if (EquipmentManager.Instance.CurrentMode == EquipmentManager.EquipmentMode.PICKABLES)
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.PanelPickableList.transform.position);
            }
            else
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.ButtonEquipmentPickableToggle.transform.position);
            }
            tgt.z = transform.position.z;

            //StartCoroutine(FlyToTarget(tgt, scl, FADE_OUT_TIME_SEC));

            InputManager.OnInputClickDown -= PickUp;
            _picked = true;

            ShowLetter(PlayerPrefs.GetString("Gender"));
        }
    }

    public void ShowLetter(string gender)
    {
        string tmp = "";

        //TurnOffForm();
        LetterBackground.gameObject.SetActive(true);

        LetterHeaderText.gameObject.SetActive(true);
        switch (gender)
        {
            case "K":
                tmp = string.Format(LetterHeaderText.text, FemaleHeader);
                break;
            case "M":
                tmp = string.Format(LetterHeaderText.text, MaleHeader);
                break;
        }
        LetterHeaderText.text = tmp;

        LetterContextText.gameObject.SetActive(true);
        switch (gender)
        {
            case "K":
                tmp = string.Format(LetterContextText.text, FemaleAccusative, FemaleNominative);
                break;
            case "M":
                tmp = string.Format(LetterContextText.text, MaleAccusative, MaleNominative);
                break;
        }
        LetterContextText.text = tmp;
    }

    public void OnLetterBackgroundClick()
    {
        if (!_isLetterTurned)
        {
            LetterHeaderText.gameObject.SetActive(false);
            LetterContextText.gameObject.SetActive(false);
            LetterBackground.transform.localScale = new Vector3(-1, 1, 1);
            LetterSecondSideContextText.gameObject.SetActive(true);
            _isLetterTurned = true;
        }
        else
        {
            LetterBackground.gameObject.SetActive(false);
            LetterSecondSideContextText.gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
    #endregion
}
