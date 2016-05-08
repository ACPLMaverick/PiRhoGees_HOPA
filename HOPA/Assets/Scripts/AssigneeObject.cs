using UnityEngine;
using System.Collections;

public class AssigneeObject : MonoBehaviour {

    #region public

    public AssignableObject Assignable;

    #endregion

    #region properties

    public Transform AssignableSnapTransform { get; private set; }
    public AssignableObject CurrentAssignable { get; set; }

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        AssignableSnapTransform = GetComponentsInChildren<Transform>()[1];
        CurrentAssignable = null;
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    #endregion
}
