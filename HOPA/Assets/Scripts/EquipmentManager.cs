using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EquipmentManager : Singleton<EquipmentManager>
{
    #region enums

    public enum EquipmentMode
    {
        PICKABLES,
        USABLES
    };


    #endregion

    #region constants
    #endregion

    #region public

    public RectTransform PanelPickableList;
    public RectTransform PanelUsableList;
    public GameObject PickableListElementPrefab;
    public GameObject UsableListElementPrefab;
    public Button ButtonEquipmentPickableToggle;
    public Button ButtonMap;
    public Button ButtonBack;

    #endregion

    #region properties

    public EquipmentMode CurrentMode
    {
        get
        {
            return _currentMode;
        }
        set
        {
            EquipmentMode temp = _currentMode;
            _currentMode = value;
            if (temp != value)
            {
                SwitchPanel();
            }
        }
    }
    public bool EquipmentFreeContainersAvailable
    {
        get
        {
            return _usableContainersOccupiedCount < _usableContainers.Length;
        }
    }
    public bool Enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            _enabled = value;

            ButtonEquipmentPickableToggle.gameObject.SetActive(value);

            if(value)
            {
                if(CurrentMode == EquipmentMode.PICKABLES)
                {
                    PanelPickableList.gameObject.SetActive(true);
                    PanelUsableList.gameObject.SetActive(false);
                    _currentPanel = PanelPickableList;
                }
                else
                {
                    PanelPickableList.gameObject.SetActive(false);
                    PanelUsableList.gameObject.SetActive(true);
                    _currentPanel = PanelUsableList;
                }
            }
            else
            {
                PanelPickableList.gameObject.SetActive(false);
                PanelUsableList.gameObject.SetActive(false);
                _currentPanel = null;
            }
        }
    }

    public bool HasMap
    {
        get
        {
            return _hasMap;
        }
        set
        {
            _hasMap = value;
            ButtonMap.interactable = _hasMap;
        }
    }

    #endregion

    #region private

    private EquipmentMode _currentMode;
    private RectTransform _currentPanel;
    private bool _enabled = true;
    private bool _hasMap = false;

    private List<PickableObject> _pickableList;
    private List<PickableUsableObject> _usableList;

    private Dictionary<PickableObject, Text> _allPickablesDict;

    private UsableContainer[] _usableContainers;
    private int _usableContainersOccupiedCount = 0;

    [SerializeField]
    private Map _map;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        _allPickablesDict = new Dictionary<PickableObject, Text>();
        _pickableList = new List<PickableObject>();
        _usableList = new List<PickableUsableObject>();
        PanelPickableList.gameObject.SetActive(true);
        PanelUsableList.gameObject.SetActive(false);
        ButtonEquipmentPickableToggle.GetComponent<ButtonEquipmentPanelToggle>().SwitchMode(CurrentMode);

        StartGUIPickables();
        StartGUIUsables();

        CurrentMode = EquipmentMode.USABLES;
        PanelUsableList.GetComponent<PanelGeneric>().Hide(true);
        PanelPickableList.GetComponent<PanelGeneric>().Hide(true);

        HasMap = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void AddObjectToList(PickableObject obj, float lagSeconds)
    {
        StartCoroutine(AddObjectToListCoroutine(obj, lagSeconds));
    }

    public void AddObjectToPool(PickableUsableObject obj, float lagSeconds)
    {
        if(_usableContainersOccupiedCount == _usableContainers.Length)
        {
            return;
        }

        ++_usableContainersOccupiedCount;
        StartCoroutine(AddObjectToPoolCoroutine(obj, lagSeconds));
        if(CurrentMode == EquipmentMode.USABLES)
        {
            _currentPanel.GetComponent<PanelGeneric>().Show(false);
        }

        obj.gameObject.layer = LayerMask.NameToLayer("UI");
    }

    public void RemoveObjectFromPool(PickableUsableObject obj, float lagSeconds)
    {
        --_usableContainersOccupiedCount;
        StartCoroutine(RemoveObjectFromPoolCoroutine(obj, lagSeconds));
    }

    public void OnButtonEquipmentPanelToggle()
    {
        if(_currentPanel.GetComponent<PanelGeneric>().Hidden)
        {
            _currentPanel.GetComponent<PanelGeneric>().Show(false);
        }
        else
        {
            CurrentMode = (EquipmentMode)(((int)CurrentMode + 1) % 2);
        }
    }

    public void FlushOnNextRoom()
    {
        _allPickablesDict.Clear();

        StartGUIPickables();

        if((GameManager.Instance.CurrentRoom.PickableAllowed && CurrentMode == EquipmentMode.USABLES) ||
            (!GameManager.Instance.CurrentRoom.PickableAllowed && CurrentMode == EquipmentMode.PICKABLES)
            )
        {
            CurrentMode = (EquipmentMode)(((int)CurrentMode + 1) % 2);
        }
    }

    public void OpenMapArbitrarily()
    {
        if(_map != null)
        {
            //if(CurrentMode == EquipmentMode.PICKABLES)
            //{
            //    SwitchPanel();
            //}
            _map.ShowMap();
        }
    }

    public void DisplayMapButton(bool display, bool interactable)
    {
        ButtonBack.gameObject.SetActive(display);
        ButtonBack.interactable = interactable;
    }

    public void ChangeTextPickedStatus(Text text, bool status)
    {
        text.fontStyle = !status ? FontStyle.Bold : FontStyle.BoldAndItalic;
        text.GetComponent<Button>().interactable = !status;
    }

    private IEnumerator AddObjectToPoolCoroutine(PickableUsableObject obj, float lagSeconds)
    {
        yield return new WaitForSeconds(lagSeconds);

        UsableContainer container = null;
        for(int i = 0; i < _usableContainers.Length; ++i)
        {
            container = _usableContainers[i];
            if(container.IsFree)
            {
                break;
            }
        }

        obj.AssignEquipmentContainer(container);
        container.AssignEquipmentUsable(obj);

        _usableList.Add(obj);

        yield return null;
    }

    private IEnumerator RemoveObjectFromPoolCoroutine(PickableUsableObject obj, float lagSeconds)
    {
        yield return new WaitForSeconds(lagSeconds);

        obj.AssociatedContainer.UnassignEquipmentUsable();
        obj.AssignEquipmentContainer(null);

        yield return null;
    }

    private IEnumerator AddObjectToListCoroutine(PickableObject obj, float lagSeconds)
    {
        yield return new WaitForSeconds(lagSeconds);

        _pickableList.Add(obj);

        yield return null;
    }

    private void StartGUIUsables()
    {
        _usableContainers = PanelUsableList.GetComponentsInChildren<UsableContainer>();
        
        for (int i = 0; i < _usableContainers.Length; ++i)
        {
            _usableContainers[i].UsableField.UsableImage.enabled = false;
            _usableContainers[i].UsableField.UsableCanvasGroup.alpha = 0.0f;
        }
    }

    private void StartGUIPickables()
    {
        List<PickableObject> pickablesOnLevel = GameManager.Instance.CurrentRoom.PickableObjects;
        CanvasGroup cg = PanelPickableList.GetComponentInChildren<CanvasGroup>();
        Text[] fields = cg.GetComponentsInChildren<Text>(true);


        int i = 0;
        foreach(PickableObject obj in pickablesOnLevel)
        {
            fields[i].gameObject.SetActive(true);
            Button button = fields[i].GetComponent<Button>();
            fields[i].text = obj.Name;

            // Assigning list element to pickable object here
            obj.AssociatedListElement = button;
            button.enabled = true;

            if(GameManager.Instance.CurrentRoom.PickablePickedObjectIDs.Contains(obj.ID))
            {
                ChangeTextPickedStatus(fields[i], true);
            }
            else
            {
                ChangeTextPickedStatus(fields[i], false);
            }

            _allPickablesDict.Add(obj, fields[i]);
            ++i;
        }


        // shutting down remaining fields
        for(int j = i; j < fields.Length; ++j)
        {
            fields[j].gameObject.SetActive(false);
            fields[j].text = "";
            fields[j].GetComponent<Button>().enabled = false;
        }
    }

    private void SwitchPanel()
    {
        if(Enabled)
        {
            RectTransform prevPanel = _currentPanel;
            if (CurrentMode == EquipmentMode.PICKABLES)
            {
                PanelPickableList.gameObject.SetActive(true);
                PanelUsableList.gameObject.SetActive(false);
                _currentPanel = PanelPickableList;
            }
            else if (CurrentMode == EquipmentMode.USABLES)
            {
                PanelPickableList.gameObject.SetActive(false);
                PanelUsableList.gameObject.SetActive(true);
                _currentPanel = PanelUsableList;
                if(!TutorialManager.Instance.EquipmentWasShown)
                    TutorialManager.Instance.ShowMessageOutOfQueue(12);
            }
            ButtonEquipmentPickableToggle.GetComponent<ButtonEquipmentPanelToggle>().SwitchMode(CurrentMode);
            if (_currentPanel.GetComponent<PanelGeneric>().Hidden && prevPanel != null)
            {
                _currentPanel.GetComponent<PanelGeneric>().Show(!prevPanel.GetComponent<PanelGeneric>().Hidden);
            }
        }
    }

    #endregion
}
