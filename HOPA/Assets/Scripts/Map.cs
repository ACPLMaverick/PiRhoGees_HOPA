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
        UP,
        RIGHT,
        DOWN,
        LEFT
    };

    #endregion

    #region public

    public CanvasGroup MapObject;
    public AudioClip SoundUnfold;
    public AudioClip SoundFold;
    public int MapDivision = 2;

    #endregion

    #region private

    protected struct Int2
    {
        public int x;
        public int y;

        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Int2 operator +(Int2 first, Int2 second)
        {
            return new Int2(first.x + second.x, first.y + second.y);
        }

        public static Int2 operator -(Int2 first, Int2 second)
        {
            return new Int2(first.x - second.x, first.y - second.y);
        }
    };

    // Buttons
    private Dictionary<Button, MapButton> _mapButtons;
    private CanvasGroup _mapButtonsGroup;
    //private Image _mapBackground;
    private List<MapDirectionButton> _directionButtons;
    //private Button _exitButton;
    private Vector2 _movementOneClick;
    private byte[,] _positionArray;
    private Int2 _currentPositionInArray;
    private bool _isEnabled = false;

    #endregion

    #region functions

    // Use this for initialization
    override protected void Start ()
    {
        base.Start();

        _mapButtons = new Dictionary<Button, MapButton>();
        _directionButtons = new List<MapDirectionButton>();

        Button[] buttons = MapObject.gameObject.GetComponentsInChildren<Button>();
        int count = buttons.Length;

        _mapButtonsGroup = MapObject.gameObject.GetComponentsInChildren<CanvasGroup>()[1];
        //_mapBackground = _mapButtonsGroup.gameObject.GetComponent<Image>();

        for(int i = 0; i < count; ++i)
        {
            if(buttons[i].name.Contains("MapButtonLoc"))
            {
                MapButton mp = buttons[i].gameObject.GetComponent<MapButton>();
                _mapButtons.Add(buttons[i], mp);
                mp.ClickedEvent.AddListener(new UnityAction<MapButton>(OnMapButtonClick));
            }
            else if(buttons[i].name.Contains("MapButtonBack"))
            {
                //_exitButton = buttons[i];
                buttons[i].onClick.AddListener(new UnityAction(HideMap));
            }
            else if(buttons[i].name.Contains("MapDirectionButton"))
            {
                _directionButtons.Add(buttons[i].GetComponent<MapDirectionButton>());
                buttons[i].GetComponent<MapDirectionButton>().ClickedEvent.AddListener(new UnityAction<MapDirectionButton>(OnDirectionButtonClick));
            }
        }

        // create position array
        _positionArray = new byte[MapDivision + 2, MapDivision + 2];
        for(int i = 0; i < MapDivision + 2; ++i)
        {
            for(int j = 0; j < MapDivision + 2; ++j)
            {
                if(i == 0 || j == 0 || i == MapDivision + 1 || j == MapDivision + 1)
                {
                    _positionArray[i, j] = 0;
                }
                else
                {
                    _positionArray[i, j] = 1;
                }
            }
        }

        // assign start position, i.e. top left corner of the array
        ValidateMapPosition(new Int2(1, 1));
        UpdateMapPositionOnDirectionButtons();

        // calculate movementOneClick and scale factor
        float scaleFactor = (float)MapDivision;
        RectTransform r = _mapButtonsGroup.GetComponent<RectTransform>();
        _movementOneClick = - new Vector2(r.rect.width * 0.5f, r.rect.height * 0.5f);

        // scale and move buttonsgroup to the proper left top corner
        r.localScale *= scaleFactor;
        r.position = new Vector2(r.position.x - _movementOneClick.x, r.position.y + _movementOneClick.y);

        MapObject.gameObject.SetActive(false);
        MapObject.alpha = 0.0f;
    }
	
	// Update is called once per frame
	override protected void Update ()
    {
        base.Update();
	}

    public void OnDirectionButtonClick(MapDirectionButton directionEnum)
    {
        Vector2 direction = Vector2.zero;
        Int2 delta = new Int2(0, 0);

        switch (directionEnum.AssociatedDirection)
        {
            case MapMovementDirection.DOWN:
                direction = Vector2.down;
                delta = new Int2(0, 1);
                break;
            case MapMovementDirection.LEFT:
                direction = Vector2.left;
                delta = new Int2(-1, 0);
                break;
            case MapMovementDirection.RIGHT:
                direction = Vector2.right;
                delta = new Int2(1, 0);
                break;
            case MapMovementDirection.UP:
                direction = Vector2.up;
                delta = new Int2(0, -1);
                break;
        }

        Int2 newPositionInArray = _currentPositionInArray + delta;

        if(ValidateMapPosition(newPositionInArray))
        {
            UpdateMapPositionOnDirectionButtons();

            Vector2 finalMovement;
            finalMovement.x = direction.x * _movementOneClick.x * 2.0f;
            finalMovement.y = direction.y * _movementOneClick.y * 2.0f;

            RectTransform r = _mapButtonsGroup.GetComponent<RectTransform>();
            finalMovement = new Vector2(r.position.x, r.position.y) + finalMovement;
            StartCoroutine(MapMovementCoroutine(r, r.position, finalMovement, 0.5f));
        }
    }

    protected void OnMapButtonClick(MapButton mp)
    {
        //Debug.Log("Invoked for button " + mp.gameObject.name + " on room " + mp.AssociatedRoom.name);

        if(_isEnabled)
        {
            GameManager.Instance.TransitionToRoom(mp.AssociatedRoom);
            HideMap();
        }
    }

    /// <summary>
    /// 
    /// <returns>
    /// If true returned, current map position has ben changed to a new one.
    /// If false, no changes have been made.
    /// </returns>
    /// </summary>
    protected bool ValidateMapPosition(Int2 newPosition)
    {
        if(_positionArray[newPosition.x, newPosition.y] == 0)
        {
            return false;
        }
        else
        {
            _positionArray[_currentPositionInArray.x, _currentPositionInArray.y] = 1;
            _positionArray[newPosition.x, newPosition.y] = 2;
            _currentPositionInArray = newPosition;
            return true;
        }
    }

    protected void UpdateMapPositionOnDirectionButtons()
    {
        Int2 delta = new Int2(0, 0);
        foreach(MapDirectionButton b in _directionButtons)
        {
            switch (b.AssociatedDirection)
            {
                case MapMovementDirection.DOWN:
                    delta = new Int2(0, 1);
                    break;
                case MapMovementDirection.LEFT:
                    delta = new Int2(-1, 0);
                    break;
                case MapMovementDirection.RIGHT:
                    delta = new Int2(1, 0);
                    break;
                case MapMovementDirection.UP:
                    delta = new Int2(0, -1);
                    break;
            }

            delta += _currentPositionInArray;
            if(_positionArray[delta.x, delta.y] != 2)
            {
                b.GetComponent<Button>().interactable = true;
            }
            else
            {
                b.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void ShowMap()
    {
        MapObject.gameObject.SetActive(true);
        StartCoroutine(MapVisibilityCoroutine(1.0f, 1.0f, true));
        AudioManager.Instance.PlayClip(SoundUnfold, 0.0f);
        InputManager.Instance.InputAllEventsEnabled = false;
    }

    public void HideMap()
    {
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
