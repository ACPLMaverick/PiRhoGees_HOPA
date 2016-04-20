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
    public GameObject PickableListElementPrefab;
    public RectTransform PanelUsableList;
    public Button ButtonEquipmentPickableToggle;

    #endregion

    #region properties

    public EquipmentMode CurrentMode { get; private set; }

    #endregion

    #region private

    private List<PickableObject> pickableList;
    private List<PickableUsableObject> usableList;

    private Dictionary<PickableObject, Text> allPickablesDict;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        allPickablesDict = new Dictionary<PickableObject, Text>();
        pickableList = new List<PickableObject>();
        usableList = new List<PickableUsableObject>();
        CurrentMode = EquipmentMode.PICKABLES;

        StartGUI();

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
        StartCoroutine(AddObjectToPoolCoroutine(obj, lagSeconds));

        obj.gameObject.layer = LayerMask.NameToLayer("UI");
    }

    public void OnButtonEquipmentPanelToggle()
    {
        CurrentMode = (EquipmentMode)(((int)CurrentMode + 1) % 2);
        SwitchPanel();
    }

    public void FlushOnNextRoom()
    {
        allPickablesDict.Clear();

        Text[] itemtexts = PanelPickableList.GetComponentsInChildren<Text>();
        int count = itemtexts.Length;

        for(int i = 0; i < count; ++i)
        {
            GameObject.Destroy(itemtexts[i].gameObject);
        }

        StartGUI();
    }

    private IEnumerator AddObjectToPoolCoroutine(PickableUsableObject obj, float lagSeconds)
    {
        yield return new WaitForSeconds(lagSeconds);

        obj.gameObject.transform.SetParent(Camera.main.transform, true);
        Vector2 firstPos = PanelUsableList.position;
        firstPos.x -= PanelUsableList.rect.width * 0.5f - obj.GetComponent<SpriteRenderer>().sprite.rect.width * 0.8f;
        Vector3 firstPosWorld = Camera.main.ScreenToWorldPoint(new Vector3(firstPos.x, firstPos.y, 0.0f));
        firstPosWorld.z = obj.transform.position.z;
        obj.transform.position = firstPosWorld;
        obj.transform.localScale = Vector3.one * 0.7f;

        usableList.Add(obj);

        yield return null;
    }

    private IEnumerator AddObjectToListCoroutine(PickableObject obj, float lagSeconds)
    {
        yield return new WaitForSeconds(lagSeconds);

        pickableList.Add(obj);

        Text associated = allPickablesDict[obj];
        ChangeTextToPicked(associated);

        yield return null;
    }

    private void ChangeTextToPicked(Text text)
    {
        text.fontStyle = FontStyle.Bold;
    }

    private void StartGUI()
    {
        List<PickableObject> pickablesOnLevel = GameManager.Instance.CurrentRoom.PickableObjects;

        Vector2 firstPos = PanelPickableList.position;
        firstPos.x -= PanelPickableList.rect.width * 0.5f;
        firstPos.y += PanelPickableList.rect.height * 0.5f - 10.0f;
        int i = 0;
        Vector2 nextPos = firstPos;
        foreach(PickableObject obj in pickablesOnLevel)
        {
            GameObject newobj = (GameObject)Instantiate(PickableListElementPrefab, nextPos, Quaternion.identity);
            newobj.transform.SetParent(PanelPickableList.transform, true);
            Text text = newobj.GetComponent<Text>();
            text.text = obj.Name;
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

            if(GameManager.Instance.CurrentRoom.PickablePickedObjects.Contains(obj))
            {
                ChangeTextToPicked(text);
            }

            allPickablesDict.Add(obj, text);
            ++i;
        }
    }

    private void SwitchPanel()
    {
        if(CurrentMode == EquipmentMode.PICKABLES)
        {
            PanelPickableList.gameObject.SetActive(true);
            PanelUsableList.gameObject.SetActive(false);

            foreach (PickableObject obj in usableList)
            {
                obj.gameObject.SetActive(false);
            }
        }
        else if(CurrentMode == EquipmentMode.USABLES)
        {
            PanelPickableList.gameObject.SetActive(false);
            PanelUsableList.gameObject.SetActive(true);

            foreach (PickableObject obj in usableList)
            {
                obj.gameObject.SetActive(true);
            }
        }
    }

    #endregion
}
