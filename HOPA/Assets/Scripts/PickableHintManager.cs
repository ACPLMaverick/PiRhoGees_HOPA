using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class PickableHintManager : Singleton<PickableHintManager>
{
    [SerializeField]
    private Image _flash;
    [SerializeField]
    private Image _arrow;
    [SerializeField]
    private Image _hintArea;
    [SerializeField]
    private float _timeToHintSeconds = 30.0f;

    private float _timer = 0.0f;
    private List<PickableObject> _objs;
    private int _objCount;
    private Rect _flashShowRect;

    private bool _showFlash;
    private bool _showArrow;

    private float _flashSpeed = 1.0f;
    private float _flashPhases = 3.0f;
    private float _flashHelper = 0.0f;

    private PickableObject _closestPickable;

	// Use this for initialization
	void Start ()
    {
        _hintArea.gameObject.SetActive(false);
        _flashShowRect = _hintArea.rectTransform.rect;
        Flush();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(!_showArrow && !_showFlash)
        {
            _timer += Time.deltaTime;
        }

        if(_objCount != 0)
        {
            if (_timer >= _timeToHintSeconds)
            {
                _timer = 0.0f;

                _closestPickable = GetClosestPickableObject();
                if(_closestPickable != null)
                {
                    if(IsPickableInFlashArea(_closestPickable))
                    {
                        _showFlash = true;
                        _flashHelper = 0.0f;
                        _flash.gameObject.SetActive(true);
                    }
                    else
                    {
                        _showArrow = true;
                        _arrow.gameObject.SetActive(true);
                    }
                }
            }

            if(_showFlash)
            {
                float boom = Mathf.Sin(_flashHelper);
                _flashHelper += Time.deltaTime;

                Vector2 pos = Camera.main.WorldToScreenPoint(_closestPickable.transform.position);
                _flash.rectTransform.position = pos;

                _flash.GetComponent<CanvasGroup>().alpha = boom;

                if (_flashHelper > _flashPhases * 2.0f * Mathf.PI)
                {
                    Debug.Log("FLUSH");
                    Flush();
                }
            }

            if (_showArrow)
            {
                Vector2 pWorldPos = _closestPickable.transform.position;
                Vector2 pScreenPos = Camera.main.WorldToScreenPoint(pWorldPos);
                Vector2 middle = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
                Vector2 dir = (pScreenPos - middle);
                dir.Normalize();
                float angle = Mathf.Rad2Deg * (Mathf.Atan2(dir.y, dir.x) - Mathf.Atan2(Vector2.up.y, Vector2.up.x));
                _arrow.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, angle);

                _flashShowRect = _hintArea.rectTransform.rect;

                float xOffset = _arrow.rectTransform.rect.width * 0.5f;
                float yOffset = _arrow.rectTransform.rect.height * 0.5f;
                float chuj = -(float)_hintArea.canvas.pixelRect.width / _hintArea.canvas.scaleFactor * 0.5f;
                float dupa = -(float)_hintArea.canvas.pixelRect.height / _hintArea.canvas.scaleFactor * 0.5f;

                pScreenPos.x = Mathf.Min(Mathf.Max(pScreenPos.x, (_flashShowRect.xMin - chuj) + xOffset), _flashShowRect.xMax - chuj - xOffset);
                pScreenPos.y = Mathf.Min(Mathf.Max(pScreenPos.y, (_flashShowRect.yMin - dupa) + yOffset), _flashShowRect.yMax - dupa - yOffset);

                _arrow.rectTransform.position = pScreenPos;

                if (IsPickableInFlashArea(_closestPickable))
                {
                    _showArrow = false;
                    _arrow.gameObject.SetActive(false);
                    _showFlash = true;
                    _flashHelper = 0.0f;
                    _flash.gameObject.SetActive(true);
                }
            }
        }
	}

    public void Flush()
    {
        _timer = 0.0f;
        _objs = GameManager.Instance.CurrentRoom.PickableObjects;
        _objCount = _objs.Count;
        _arrow.gameObject.SetActive(false);
        _flash.gameObject.SetActive(false);
        _showArrow = false;
        _showFlash = false;
    }

    private PickableObject GetClosestPickableObject()
    {
        int oCount = _objs.Count;
        for(int i = 0; i < oCount; ++i)
        {
            PickableObject temp = _objs[i];
            if(!temp.Picked && temp.transform.root.GetComponent<Room>() == GameManager.Instance.CurrentRoom)
            {
                return temp;
            }
        }

        return null;
    }

    private bool IsPickableInFlashArea(PickableObject po)
    {
        Vector2 pos = Camera.main.WorldToScreenPoint(po.transform.position);
        return RectTransformUtility.RectangleContainsScreenPoint(_hintArea.rectTransform, pos);
    }
}
