using UnityEngine;
using System.Collections;

public class MovableObject : MonoBehaviour
{
    #region enum

    public enum MovementMode
    {
        LINE,
        BOX
    };

    #endregion

    #region public

    public MovementMode CurrentMode = MovementMode.LINE;
    public float MovementRate = 1.0f;

    #endregion

    #region protected

    protected Transform _tMin = null;
    protected Transform _tMax = null;
    protected GameObject _item = null;

    #endregion

    #region functions

    // Use this for initialization
    protected virtual void Start ()
    {
        // finding child objects which are NECESSARY to run the script
        _tMin = transform.FindChild("TransformMin");
        _tMax = transform.FindChild("TransformMax");
        _item = transform.FindChild("Item").gameObject;

        if(_tMin == null || _tMax == null || _item == null)
        {
            throw new System.Exception("Error: one or more necessary child objects are not present.");
        }

        InputManager.Instance.OnInputMoveExclusive.AddListener(MoveObject);
	}

    // Update is called once per frame
    protected virtual void Update ()
    {
	
	}

    private void MoveObject(Vector2 screenPos, Vector2 direction, Collider2D hitCollider)
    {
        if(hitCollider != null && hitCollider.gameObject == _item)
        {
            Vector3 wp1 = Camera.main.ScreenToWorldPoint(screenPos);
            Vector3 wp2 = Camera.main.ScreenToWorldPoint(screenPos + direction);
            Vector3 deltaP = wp2 - wp1;

            // if movement mode is LINE it is necessary to project deltaP onto direction vector that is difference between _tMin and _tMax
            if(CurrentMode == MovementMode.LINE)
            {
                Vector3 dir = Vector3.Normalize(_tMax.position - _tMin.position);
                deltaP = Vector3.Project(deltaP, dir);
            }

            Vector3 newMin = Vector3.Min(_tMin.position, _tMax.position);
            Vector3 newMax = Vector3.Max(_tMin.position, _tMax.position);

            Vector3 newPosition = _item.transform.position + deltaP * MovementRate;
            newPosition = Vector3.Max(newPosition, newMin);
            newPosition = Vector3.Min(newPosition, newMax);

            //Vector3 cMin = _item.GetComponent<BoxCollider>().bounds.min;
            //Vector3 cMax = _item.GetComponent<BoxCollider>().bounds.max;
            //cMin.z = cMax.z = 0.0f;

            //Debug.Log(cMin);
            //Debug.Log(cMax);

            //Vector3 actualPosMin = new Vector3(_tMin.position.x, _tMin.position.y, 0.0f);
            //Vector3 actualPosMax = new Vector3(_tMax.position.x, _tMax.position.y, 0.0f);

            //cMin = actualPosMin - Vector3.Min(cMin, actualPosMin);
            //cMax = -(Vector3.Max(cMax, actualPosMax) - actualPosMax);

            _item.transform.position = newPosition;
        }
    }

    #endregion
}
