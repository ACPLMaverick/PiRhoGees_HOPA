using UnityEngine;
using System.Collections;

public class TransitionRing : MonoBehaviour
{
    #region public

    public float SpeedRotation = 1.0f;
    public float SpeedScaling = 1.0f;
    public float ScalingMin = 0.7f;
    public float ScalingMax = 1.0f;

    #endregion

    #region private

    private Vector3 _baseScale;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        _baseScale = GetComponent<Transform>().localScale;
	}
	
	// Update is called once per frame
	void Update ()
    {
        Transform tr = GetComponent<Transform>();

        Vector3 er = tr.localRotation.eulerAngles;
        er.z += SpeedRotation * Time.deltaTime;
        if(er.z > 360.0f)
        {
            er.z -= 360.0f;
        }
        tr.localRotation = Quaternion.Euler(er);

        float scaleMplier = ((Mathf.Sin(SpeedScaling * Time.time) + 1.0f) * 0.5f) * (ScalingMax - ScalingMin) + ScalingMin;
        tr.localScale = _baseScale * scaleMplier;
	}

    #endregion
}
