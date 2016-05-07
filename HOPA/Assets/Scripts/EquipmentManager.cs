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

    private int USABLE_MAX_ITEMS = 5;

    #endregion

    #region public

    public RectTransform PanelPickableList;
    public RectTransform PanelUsableList;
    public GameObject PickableListElementPrefab;
    public GameObject UsableListElementPrefab;
    public Button ButtonEquipmentPickableToggle;
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
            return _usableContainersOccupiedCount < USABLE_MAX_ITEMS;
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
                }
                else
                {
                    PanelPickableList.gameObject.SetActive(false);
                    PanelUsableList.gameObject.SetActive(true);
                }
            }
            else
            {
                PanelPickableList.gameObject.SetActive(false);
                PanelUsableList.gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region private

    private EquipmentMode _currentMode;
    private bool _enabled = true;

    private List<PickableObject> _pickableList;
    private List<PickableUsableObject> _usableList;

    private Dictionary<PickableObject, Text> _allPickablesDict;

    private UsableContainer[] _usableContainers;
    private int _usableContainersOccupiedCount = 0;

    private Map _map;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        _allPickablesDict = new Dictionary<PickableObject, Text>();
        _pickableList = new List<PickableObject>();
        _usableList = new List<PickableUsableObject>();
        _currentMode = EquipmentMode.PICKABLES;
        PanelPickableList.gameObject.SetActive(true);
        PanelUsableList.gameObject.SetActive(false);
        ButtonEquipmentPickableToggle.GetComponent<ButtonEquipmentPanelToggle>().SwitchMode(CurrentMode);

        StartGUIPickables();
        StartGUIUsables();

        SwitchPanel();
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
        if(_usableContainersOccupiedCount == USABLE_MAX_ITEMS)
        {
            return;
        }

        if(obj.GetComponent<Map>() != null)
        {
            _map = obj.GetComponent<Map>();
        }

        StartCoroutine(AddObjectToPoolCoroutine(obj, lagSeconds));

        obj.gameObject.layer = LayerMask.NameToLayer("UI");
    }

    public void OnButtonEquipmentPanelToggle()
    {
        CurrentMode = (EquipmentMode)(((int)CurrentMode + 1) % 2);
    }

    public void FlushOnNextRoom()
    {
        _allPickablesDict.Clear();

        Text[] itemtexts = PanelPickableList.GetComponentsInChildren<Text>();
        int count = itemtexts.Length;

        for(int i = 0; i < count; ++i)
        {
            if(!itemtexts[i].name.Contains("Text"))
            {
                Destroy(itemtexts[i].gameObject);
            }
        }

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
            if(CurrentMode == EquipmentMode.PICKABLES)
            {
                SwitchPanel();
            }
            _map.ShowMap();
        }
    }

    public void DisplayBackButton(bool display, bool interactable)
    {
        ButtonBack.gameObject.SetActive(display);
        ButtonBack.interactable = interactable;
    }

    public void ChangeTextToPicked(Text text)
    {
        text.fontStyle = FontStyle.BoldAndItalic;
        text.GetComponent<Button>().interactable = false;
    }

    private IEnumerator AddObjectToPoolCoroutine(PickableUsableObject obj, float lagSeconds)
    {
        yield return new WaitForSeconds(lagSeconds);

        UsableContainer container = null;
        for(int i = 0; i < USABLE_MAX_ITEMS; ++i)
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

    private IEnumerator AddObjectToListCoroutine(PickableObject obj, float lagSeconds)
    {
        yield return new WaitForSeconds(lagSeconds);

        _pickableList.Add(obj);

        yield return null;
    }

    private void StartGUIUsables()
    {
        _usableContainers = new UsableContainer[USABLE_MAX_ITEMS];

        Vector2 firstPos = PanelUsableList.position;
        firstPos.y = PanelUsableList.position.y + PanelUsableList.rect.height * 0.39f * PanelUsableList.GetComponent<Image>().canvas.scaleFactor;
        GameObject container = (GameObject)Instantiate(UsableListElementPrefab, firstPos, Quaternion.identity);
        float xDelta = (PanelUsableList.rect.height - (container.GetComponent<Image>()).rectTransform.rect.height) * 0.5f;
        float panelWidth = container.GetComponent<RectTransform>().rect.width;
        float objTotalDelta = panelWidth + xDelta;
        float containerWidth = container.GetComponent<Image>().rectTransform.rect.width;
        firstPos.x += (panelWidth - (USABLE_MAX_ITEMS * containerWidth + (USABLE_MAX_ITEMS - 1) * xDelta)) * 0.5f;
        container.transform.position = firstPos;
        container.transform.SetParent(PanelUsableList.transform, true);
        _usableContainers[0] = container.GetComponent<UsableContainer>();

        for (int i = 1; i < USABLE_MAX_ITEMS; ++i)
        {
            firstPos.x += objTotalDelta;
            container = (GameObject)Instantiate(UsableListElementPrefab, firstPos, Quaternion.identity);
            container.transform.SetParent(PanelUsableList.transform, true);
            _usableContainers[i] = container.GetComponent<UsableContainer>();
        }
        
        for (int i = 0; i < USABLE_MAX_ITEMS; ++i)
        {
            _usableContainers[i].UsableField.UsableImage.enabled = false;
            _usableContainers[i].UsableField.UsableCanvasGroup.alpha = 0.0f;
        }
    }

    private void StartGUIPickables()
    {
        List<PickableObject> pickablesOnLevel = GameManager.Instance.CurrentRoom.PickableObjects;

        Vector2 firstPos = PanelPickableList.position;
        firstPos.x -= PanelPickableList.rect.width * 0.4f * PanelPickableList.GetComponent<Image>().canvas.scaleFactor;
        firstPos.y += PanelPickableList.rect.height * 0.65f * PanelPickableList.GetComponent<Image>().canvas.scaleFactor;
        int i = 0;
        Vector2 nextPos = firstPos;
        foreach(PickableObject obj in pickablesOnLevel)
        {
            GameObject newobj = (GameObject)Instantiate(PickableListElementPrefab, nextPos, Quaternion.identity);
            newobj.transform.SetParent(PanelPickableList.transform, true);
            Text text = newobj.GetComponent<Text>();
            text.text = obj.Name;

            // Assigning list element to pickable object here
            obj.AssociatedListElement = newobj.GetComponent<Button>();

            RectTransform rt = newobj.GetComponent<RectTransform>();
            if (i % 4 == 0 && i != 0)
            {
                nextPos.y -= rt.rect.height;
                nextPos.x = firstPos.x;
            }
            else if(i != 0)
            {
                nextPos.x += rt.rect.width;
            }
            newobj.transform.position = nextPos;

            if(GameManager.Instance.CurrentRoom.PickablePickedObjectIDs.Contains(obj.ID))
            {
                ChangeTextToPicked(text);
            }

            _allPickablesDict.Add(obj, text);
            ++i;
        }
    }

    private void SwitchPanel()
    {
        if(Enabled)
        {
            if (CurrentMode == EquipmentMode.PICKABLES)
            {
                PanelPickableList.gameObject.SetActive(true);
                PanelUsableList.gameObject.SetActive(false);
            }
            else if (CurrentMode == EquipmentMode.USABLES)
            {
                PanelPickableList.gameObject.SetActive(false);
                PanelUsableList.gameObject.SetActive(true);
            }
            ButtonEquipmentPickableToggle.GetComponent<ButtonEquipmentPanelToggle>().SwitchMode(CurrentMode);
        }
    }

    #endregion
}
