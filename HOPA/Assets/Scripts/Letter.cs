using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Letter : PickableObject
{

    #region constants
    public const string MaleHeader = "Szanowny Panie";
    public const string MaleAccusative = "Pana";
    public const string MaleNominative = "Pan";
    public const string FemaleHeader = "Szanowna Pani";
    public const string FemaleAccusative = "Panią";
    public const string FemaleNominative = "Pani";
    #endregion

    #region public
    public LetterUI LetterObj;
    private string _headerText = "{0}";
    private string _contextText = "Pragnę serdecznie powitać {0} na stanowisku młodszego asystena ds. organizacji wystaw. " +
                                    "To wspaniałe, że wśród młodzieży wciąż znajdują się osoby chętne do pracy w instytucjach kulturowych, " +
                                    "takich jak nasze zasłużone Muzeum Kinematografii. Jestem pewien, że czeka nas owocna współpraca i zarówno ja, jak i {1}, " +
                                    "wyniesiemy z niej coś wartościowego.\n" +
                                    "Na drugiej stronie zamieściłem pierwsze wytyczne.W razie pytań lub wątpliwości, chętnie służę wyjaśnieniem.\n" +
                                    "Pozdrawiam i życzę powodzenia.\n\n" +
                                    "Kustosz Muzeum Kinematografii w Łodzi\n" +
                                    "Stefan Grajgór";
    private string _secondSideContextText = "Proszę o przygotowanie do otwarcia wystawy, pod tytułem \"Od negatywu do kopii\"";
    #endregion

    #region private

    #endregion

    #region functions
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {

    }

    protected override void PickUp(Vector2 position, Collider2D col)
    {
        if (col != null && col.gameObject == this.gameObject && !_picked)
        {
            col.gameObject.transform.SetParent(Camera.main.transform, true);
            Vector3 tgt = Vector3.zero/*, scl = Vector3.zero*/;

            if (EquipmentManager.Instance.CurrentMode == EquipmentManager.EquipmentMode.PICKABLES)
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.PanelPickableList.transform.position);
            }
            else
            {
                tgt = Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.ButtonEquipmentPickableToggle.transform.position);
            }
            tgt.z = transform.position.z;

            InputManager.Instance.OnInputClickDown.RemoveListener(PickUp);
            StartCoroutine(Utility.FadeCoroutine(GetComponent<SpriteRenderer>(), 1.0f, 0.0f, 0.3f, false));
            _picked = true;

            LetterObj.OnPageTurned.AddListener(PageTurned);
            ShowLetter(PlayerPrefs.GetString("Gender"));
            TutorialManager.Instance.GoStepFurther();
        }
    }

    protected void ShowLetter(string gender)
    {
        switch (gender)
        {
            case "K":
                _headerText = string.Format(_headerText, FemaleHeader);
                break;
            case "M":
                _headerText = string.Format(_headerText, MaleHeader);
                break;
        }

        switch (gender)
        {
            case "K":
                _contextText = string.Format(_contextText, FemaleAccusative, FemaleNominative);
                break;
            case "M":
                _contextText = string.Format(_contextText, MaleAccusative, MaleNominative);
                break;
        }

        LetterObj.Show(_headerText, _contextText, _secondSideContextText, true);
    }

    protected void PageTurned()
    {
        LetterObj.OnPageTurned.RemoveListener(PageTurned);
        TutorialManager.Instance.GoStepFurther();
    }
    #endregion
}
