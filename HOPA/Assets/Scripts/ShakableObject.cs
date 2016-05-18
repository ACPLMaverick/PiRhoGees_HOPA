using UnityEngine;
using System.Collections;

public class ShakableObject : MonoBehaviour
{
    #region public

    #endregion

    #region protected

    protected Animator _animator;

    #endregion

    #region functions

    // Use this for initialization
    void Start ()
    {
        _animator = GetComponent<Animator>();
        InputManager.Instance.OnInputShake.AddListener(Shake);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    protected void Shake(Vector3 shakeVec)
    {
        _animator.SetBool("IsShake", true);

        InputManager.Instance.OnInputShake.RemoveListener(Shake);
    }

    #endregion
}
