using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

public class Map : MonoBehaviour
{
    #region const

    private const float MAP_PART_PIECE_OVERLAY_MPLIER = 0.8f;

    #endregion

    #region public

    public CanvasGroup MapObject;
    public AudioClip SoundUnfold;
    public AudioClip SoundFold;
    public List<Room> RoomsOnMap;
    public GameObject MapElementPrefab;
    public AudioClip PickUpSound;

    #endregion

    #region private

    // Buttons
    private List<MapPart> _mapParts;
    private CanvasGroup _mapButtonsGroup;
    private Image _mapTitle;
    private MapEdgeFade _fadeLeft;
    private MapEdgeFade _fadeRight;
    //private Button _exitButton;
    private Vector2 _movementOneClick;
    private Vector2 _movementOneClickFromStart;
    private Vector2 _visiblePos;
    private Vector2 _invisiblePos;
    private bool _isEnabled = false;
    private int _currentMapPosition = 0;
    private int _totalMapLength;

    #endregion

    #region functions

    // Use this for initialization
    protected void Start ()
    {
        InputManager.Instance.OnInputClickUp.AddListener(PickUp);

        _totalMapLength = RoomsOnMap.Count + 1;

        _mapParts = new List<MapPart>();

        Button[] buttons = MapObject.gameObject.GetComponentsInChildren<Button>();
        int count = buttons.Length;

        for (int i = 0; i < count; ++i)
        {
            if(buttons[i].name.Contains("MapButtonBack"))
            {
                //_exitButton = buttons[i];
                buttons[i].onClick.AddListener(new UnityAction(HideMap));
            }
        }

        _mapButtonsGroup = MapObject.gameObject.GetComponentsInChildren<CanvasGroup>()[1];
        _mapTitle = _mapButtonsGroup.gameObject.GetComponentsInChildren<Image>()[1];

        MapEdgeFade[] fades = MapObject.gameObject.GetComponentsInChildren<MapEdgeFade>();
        _fadeLeft = fades[0];
        _fadeRight = fades[1];

        _visiblePos = MapObject.GetComponent<RectTransform>().localPosition;
        _invisiblePos = new Vector2(_visiblePos.x + MapObject.GetComponent<RectTransform>().rect.width, _visiblePos.y);

        // calculate movementOneClick and scale factor
        RectTransform r = _mapTitle.GetComponent<RectTransform>();
        _movementOneClick = new Vector2(r.rect.width/* * MAP_PART_PIECE_OVERLAY_MPLIER*/, 0.0f);
        _movementOneClickFromStart = new Vector2(r.rect.width * MAP_PART_PIECE_OVERLAY_MPLIER, 0.0f);

        // instantiate map parts according to part list
        Vector2 d = _movementOneClick;

        foreach(Room room in RoomsOnMap)
        {
            GameObject container = (GameObject)Instantiate(MapElementPrefab, Vector3.zero, Quaternion.identity);
            container.transform.SetParent(_mapButtonsGroup.transform, false);

            RectTransform tr = container.GetComponent<RectTransform>();
            tr.sizeDelta = r.sizeDelta;
            //tr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MAP_PART_PIECE_OVERLAY_MPLIER);
            tr.localPosition += new Vector3(d.x, d.y, 0.0f);
            tr.localScale = Vector3.one;

            MapPart mp = container.GetComponent<MapPart>();
            mp.AssociatedRoom = room;
            room.AssociatedMapPart = mp;
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

            if(mp.AssociatedRoom.Locked)
            {
                mp.GetComponentInChildren<FlashingButton>().enabled = false;
            }

            d.x += _movementOneClick.x;
        }

        InputManager.Instance.OnInputSwipe.AddListener(MoveInDirection);

        _fadeLeft.HideImmediate();

        MapObject.gameObject.SetActive(false);
        MapObject.alpha = 0.0f;
    }

    protected void PickUp(Vector2 position, Collider2D col)
    {
        if (col != null && col.gameObject == this.gameObject)
        {
            InputManager.Instance.OnInputClickUp.RemoveListener(PickUp);
            EquipmentManager.Instance.HasMap = true;
            TutorialManager.Instance.GoStepFurther();
            AudioManager.Instance.PlayClip(PickUpSound);
            StartCoroutine(Utility.FadeCoroutine(GetComponent<SpriteRenderer>(), 1.0f, 0.0f, 1.0f, true));
            StartCoroutine(FlyToTarget(Camera.main.ScreenToWorldPoint(EquipmentManager.Instance.ButtonMap.transform.position), Vector3.zero, 1.0f));
        }
    }

    // Update is called once per frame
    protected void Update ()
    {
	}

    protected void MoveInDirection(Vector2 origin, InputManager.SwipeDirection dirEnum, float length, Collider2D col)
    {
        if(!_isEnabled || dirEnum == InputManager.SwipeDirection.UP || dirEnum == InputManager.SwipeDirection.DOWN)
        {
            return;
        }

        Vector2 direction = Vector2.zero;
        int nextPosition = _currentMapPosition;

        switch (dirEnum)
        {
            case InputManager.SwipeDirection.LEFT:
                --nextPosition;
                direction = Vector2.left;
                break;
            case InputManager.SwipeDirection.RIGHT:
                ++nextPosition;
                direction = Vector2.right;
                break;
        }

        TutorialManager.Instance.GoStepFurther(6);

        if (nextPosition >= 0 && nextPosition < _totalMapLength)
        {
            SetEdgeFadeOnPosition(nextPosition);

            Vector2 oneClick = _movementOneClick;

            if ((_currentMapPosition == 0 && dirEnum == InputManager.SwipeDirection.RIGHT) || 
                (_currentMapPosition == 1 && dirEnum == InputManager.SwipeDirection.LEFT))
            {
                oneClick = _movementOneClickFromStart;
            }

            _currentMapPosition = nextPosition;

            Vector2 finalMovement;
            finalMovement.x = direction.x * -oneClick.x * _mapTitle.canvas.scaleFactor;
            finalMovement.y = direction.y * -oneClick.y;

            RectTransform r = _mapButtonsGroup.GetComponent<RectTransform>();
            finalMovement = new Vector2(r.position.x, r.position.y) + finalMovement;
            StartCoroutine(MapMovementCoroutine(r, r.position, finalMovement, 0.5f));
        }
    }

    protected void SetEdgeFadeOnPosition(int position)
    {
        if (position == 0)
        {
            _fadeLeft.Hide();
            _fadeRight.Show();
        }
        else if (position == _totalMapLength - 1)
        {
            _fadeLeft.Show();
            _fadeRight.Hide();
        }
        else
        {
            _fadeLeft.Show();
            _fadeRight.Show();
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

    public void ShowMap()
    {
        //To enable toggle button (On room 0 it's useless and provokes bugs)
        EquipmentManager.Instance.ButtonEquipmentPickableToggle.interactable = true;

        if (GameManager.Instance.CurrentRoom.ParentRoom != null)
        {
            EquipmentManager.Instance.DisplayMapButton(false, EquipmentManager.Instance.ButtonBack.interactable);
        }
        MapObject.gameObject.SetActive(true);
        SetEdgeFadeOnPosition(_currentMapPosition);
        StartCoroutine(MapVisibilityCoroutine(1.0f, 1.0f, false, true));
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
            EquipmentManager.Instance.DisplayMapButton(true, EquipmentManager.Instance.ButtonBack.interactable);
        }
        StartCoroutine(MapVisibilityCoroutine(1.0f, 0.0f, true, false));
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

    protected IEnumerator MapVisibilityCoroutine(float timeSeconds, float targetOpacity, bool influenceActivity, bool isUsableOnFinal)
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
        if(influenceActivity)
        {
            MapObject.gameObject.SetActive(isUsableOnFinal);
        }
        _isEnabled = isUsableOnFinal;

        if(!_isEnabled)
        {
            InputManager.Instance.InputAllEventsEnabled = true;
        }

        yield return null;
    }

    protected IEnumerator MapVisibilityCoroutine(float timeSeconds, Vector2 startPos, Vector2 targetPos, bool influenceActivity, bool isUsableOnFinal)
    {
        float cTime = Time.time;
        MapObject.GetComponent<RectTransform>().localPosition = startPos;

        while (Time.time - cTime <= timeSeconds)
        {
            float lerpValue = (Time.time - cTime) / timeSeconds;
            Vector2 pos = Vector2.Lerp(startPos, targetPos, lerpValue);
            MapObject.GetComponent<RectTransform>().localPosition = pos;
            yield return null;
        }
        MapObject.GetComponent<RectTransform>().localPosition = targetPos;

        if (influenceActivity)
        {
            MapObject.gameObject.SetActive(isUsableOnFinal);
        }
        _isEnabled = isUsableOnFinal;

        if (!_isEnabled)
        {
            InputManager.Instance.InputAllEventsEnabled = true;
        }

        yield return null;
    }

    protected IEnumerator FlyToTarget(Vector3 targetPos, Vector3 targetScale, float time)
    {
        float currentTime = Time.time;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        while (Time.time - currentTime <= time)
        {
            float lerp = (Time.time - currentTime) / time;

            transform.position = Vector3.Lerp(startPos, targetPos, lerp);
            transform.localScale = Vector3.Lerp(startScale, targetScale, lerp);
            yield return null;
        }

        transform.parent = EquipmentManager.Instance.transform;
        yield return null;
    }

    #endregion
}
