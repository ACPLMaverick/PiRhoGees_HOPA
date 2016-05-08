using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

public class Map : PickableUsableObject
{
    #region enum

    public enum MapMovementDirection
    {
        RIGHT,
        LEFT
    };

    #endregion

    #region public

    public CanvasGroup MapObject;
    public AudioClip SoundUnfold;
    public AudioClip SoundFold;
    public List<Room> RoomsOnMap;
    public GameObject MapElementPrefab;

    #endregion

    #region private

    // Buttons
    private List<MapPart> _mapParts;
    private CanvasGroup _mapButtonsGroup;
    private Image _mapTitle;
    private MapDirectionButton _directionButtonLeft;
    private MapDirectionButton _directionButtonRight;
    //private Button _exitButton;
    private Vector2 _movementOneClick;
    private bool _isEnabled = false;
    private int _currentMapPosition = 0;
    private int _totalMapLength;

    #endregion

    #region functions

    // Use this for initialization
    override protected void Start ()
    {
        base.Start();

        _totalMapLength = RoomsOnMap.Count + 1;

        _mapParts = new List<MapPart>();

        Button[] buttons = MapObject.gameObject.GetComponentsInChildren<Button>();
        int count = buttons.Length;

        _mapButtonsGroup = MapObject.gameObject.GetComponentsInChildren<CanvasGroup>()[1];
        _mapTitle = _mapButtonsGroup.gameObject.GetComponentsInChildren<Image>()[1];

        for(int i = 0; i < count; ++i)
        {
            if(buttons[i].name.Contains("MapButtonBack"))
            {
                //_exitButton = buttons[i];
                buttons[i].onClick.AddListener(new UnityAction(HideMap));
            }
            else if(buttons[i].name.Contains("MapDirectionButtonLeft"))
            {
                _directionButtonLeft = buttons[i].GetComponent<MapDirectionButton>();
                _directionButtonLeft.ClickedEvent.AddListener(new UnityAction<MapDirectionButton>(OnDirectionButtonClick));
            }
            else if (buttons[i].name.Contains("MapDirectionButtonRight"))
            {
                _directionButtonRight = buttons[i].GetComponent<MapDirectionButton>();
                _directionButtonRight.ClickedEvent.AddListener(new UnityAction<MapDirectionButton>(OnDirectionButtonClick));
            }
        }

        // calculate movementOneClick and scale factor
        RectTransform r = _mapTitle.GetComponent<RectTransform>();
        _movementOneClick = new Vector2(r.rect.width, 0.0f);

        // instantiate map parts according to part list
        Vector2 d = _movementOneClick;

        foreach(Room room in RoomsOnMap)
        {
            GameObject container = (GameObject)Instantiate(MapElementPrefab, Vector3.zero, Quaternion.identity);
            container.transform.SetParent(_mapButtonsGroup.transform, false);

            RectTransform tr = container.GetComponent<RectTransform>();
            tr.localScale = Vector3.one;
            tr.sizeDelta = r.sizeDelta;
            tr.localPosition += new Vector3(d.x, d.y, 0.0f);

            MapPart mp = container.GetComponent<MapPart>();
            mp.AssociatedRoom = room;
            mp.ClickedEvent.AddListener(new UnityAction<MapPart>(OnMapButtonClick));
            if(room.Locked)
            {
                mp.Lock();
            }
            else
            {
                mp.Unlock();
            }

            _mapParts.Add(mp);
            d.x += _movementOneClick.x;
        }

        _directionButtonLeft.GetComponent<Button>().interactable = false;
        _directionButtonRight.GetComponent<Button>().interactable = true;

        MapObject.gameObject.SetActive(false);
        MapObject.alpha = 0.0f;
    }

    protected override void PickUp(Vector2 position, Collider2D col)
    {
        if (col != null && col.gameObject == this.gameObject && EquipmentManager.Instance.EquipmentFreeContainersAvailable && !_picked)
        {
            TutorialManager.Instance.GoStepFurther();
        }

        base.PickUp(position, col);
    }

    // Update is called once per frame
    override protected void Update ()
    {
        base.Update();
	}

    public void OnDirectionButtonClick(MapDirectionButton directionEnum)
    {
        if(!_isEnabled)
        {
            return;
        }

        Vector2 direction = Vector2.zero;
        int nextPosition = _currentMapPosition;

        switch (directionEnum.AssociatedDirection)
        {
            case MapMovementDirection.LEFT:
                --nextPosition;
                direction = Vector2.left;
                break;
            case MapMovementDirection.RIGHT:
                ++nextPosition;
                direction = Vector2.right;
                break;
        }

        if (nextPosition >= 0 && nextPosition < _totalMapLength)
        {
            if(nextPosition == 0)
            {
                _directionButtonLeft.GetComponent<Button>().interactable = false;
                _directionButtonRight.GetComponent<Button>().interactable = true;
            }
            else if(nextPosition == _totalMapLength - 1)
            {
                _directionButtonLeft.GetComponent<Button>().interactable = true;
                _directionButtonRight.GetComponent<Button>().interactable = false;
            }
            else
            {
                _directionButtonLeft.GetComponent<Button>().interactable = true;
                _directionButtonRight.GetComponent<Button>().interactable = true;
            }

            _currentMapPosition = nextPosition;

            Vector2 finalMovement;
            finalMovement.x = direction.x * -_movementOneClick.x * _mapTitle.canvas.scaleFactor;
            finalMovement.y = direction.y * -_movementOneClick.y;

            RectTransform r = _mapButtonsGroup.GetComponent<RectTransform>();
            finalMovement = new Vector2(r.position.x, r.position.y) + finalMovement;
            StartCoroutine(MapMovementCoroutine(r, r.position, finalMovement, 0.5f));
        }
    }

    protected void OnMapButtonClick(MapPart mp)
    {
        //Debug.Log("Invoked for button " + mp.gameObject.name + " on room " + mp.AssociatedRoom.name);

        if(_isEnabled)
        {
            TutorialManager.Instance.HideCurrentMessage();

            //Inside transition coroutine hides approaching of another tutorial message
            GameManager.Instance.TransitionToRoom(mp.AssociatedRoom);
            HideMap();
        }
    }

    //protected void UpdateMapPositionOnDirectionButtons()
    //{
    //    Int2 delta = new Int2(0, 0);
    //    foreach(MapDirectionButton b in _directionButtons)
    //    {
    //        switch (b.AssociatedDirection)
    //        {
    //            case MapMovementDirection.DOWN:
    //                delta = new Int2(0, 1);
    //                break;
    //            case MapMovementDirection.LEFT:
    //                delta = new Int2(-1, 0);
    //                break;
    //            case MapMovementDirection.RIGHT:
    //                delta = new Int2(1, 0);
    //                break;
    //            case MapMovementDirection.UP:
    //                delta = new Int2(0, -1);
    //                break;
    //        }

    //        delta += _currentPositionInArray;
    //        if(_positionArray[delta.x, delta.y] != 2)
    //        {
    //            b.GetComponent<Button>().interactable = true;
    //        }
    //        else
    //        {
    //            b.GetComponent<Button>().interactable = false;
    //        }
    //    }
    //}

    public void ShowMap()
    {
        if (GameManager.Instance.CurrentRoom.ParentRoom != null)
        {
            EquipmentManager.Instance.DisplayBackButton(false, EquipmentManager.Instance.ButtonBack.interactable);
        }
        MapObject.gameObject.SetActive(true);
        StartCoroutine(MapVisibilityCoroutine(1.0f, 1.0f, true));
        AudioManager.Instance.PlayClip(SoundUnfold, 0.0f);
        InputManager.Instance.InputAllEventsEnabled = false;

        if(TutorialManager.Instance.IsEnabled)
        {
            TutorialManager.Instance.GoStepFurther();
        }
    }

    public void HideMap()
    {
        if (GameManager.Instance.CurrentRoom.ParentRoom != null)
        {
            EquipmentManager.Instance.DisplayBackButton(true, EquipmentManager.Instance.ButtonBack.interactable);
        }
        StartCoroutine(MapVisibilityCoroutine(1.0f, 0.0f, false));
        _isEnabled = false;
        AudioManager.Instance.PlayClip(SoundFold, 0.0f);
    }

    protected IEnumerator MapMovementCoroutine(RectTransform rt, Vector2 startPos, Vector2 targetPos, float timeSeconds)
    {
        float cTime = Time.time;
        rt.position = startPos;
        _isEnabled = false;

        while (Time.time - cTime <= timeSeconds)
        {
            float lerpValue = (Time.time - cTime) / timeSeconds;
            Vector2 finalPos = Vector2.Lerp(startPos, targetPos, lerpValue);
            rt.position = finalPos;
            yield return null;
        }
        rt.position = targetPos;
        _isEnabled = true;

        yield return null;
    }

    protected IEnumerator MapVisibilityCoroutine(float timeSeconds, float targetOpacity, bool isUsableOnFinal)
    {
        float cTime = Time.time;
        float startOpacity = MapObject.alpha;

        while (Time.time - cTime <= timeSeconds)
        {
            float lerpValue = (Time.time - cTime) / timeSeconds;
            float finalOpacity = Mathf.Lerp(startOpacity, targetOpacity, lerpValue);
            MapObject.alpha = finalOpacity;
            yield return null;
        }
        MapObject.alpha = targetOpacity;
        MapObject.gameObject.SetActive(isUsableOnFinal);
        _isEnabled = isUsableOnFinal;

        if(!_isEnabled)
        {
            InputManager.Instance.InputAllEventsEnabled = true;
        }

        yield return null;
    }

    protected override void PerformActionOnClick(GameObject other)
    {
        if(other == null || other == gameObject)
        {
            if(_isEnabled)
            {
                HideMap();
            }
            else
            {
                ShowMap();
            }
        }
        else
        {
            Debug.Log(other.name);
        }
    }

    #endregion
}
