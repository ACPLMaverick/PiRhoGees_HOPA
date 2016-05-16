using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomPuzzleAssign : Room
{
    #region protected

    protected AssignableObject[] _assignables;
    protected int _assignableCount;
    protected int _assigned = 0;

    #endregion

    #region public

    //Set only for second puzzle room
    public GameObject[] Questions;

    #endregion

    #region functions

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
        _assignables = GetComponentsInChildren<AssignableObject>();
        _assignableCount = _assignables.Length;

        for(int i = 0; i < _assignableCount; ++i)
        {
            _assignables[i].AssignedEvent.AddListener(new UnityEngine.Events.UnityAction<AssignableObject>(OnAssigned));
        }

        if (Questions.Length > 0)
        {
            for (int j = 0; j < Questions.Length; ++j)
            {
                for (int k = 0; k < Questions[j].GetComponentsInChildren<AssignableObject>().Length; ++k)
                {
                    Questions[j].GetComponentsInChildren<AssignableObject>()[k].AssignedEvent = new AssignableObjectUnityEvent();
                    Questions[j].GetComponentsInChildren<AssignableObject>()[k].AssignedEvent.AddListener(new UnityEngine.Events.UnityAction<AssignableObject>(OnAssigned));
                }
            }

            Questions[1].SetActive(false);
            Questions[2].SetActive(false);
            Questions[3].SetActive(false);
        }
    }
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();
	}

    protected void OnAssigned(AssignableObject asg)
    {
        ++_assigned;

        if(Questions.Length != 0 && _assigned < 4)
        {
            int _a_tmp = _assigned - 1;

            Questions[_a_tmp].SetActive(false);
            Questions[_assigned].SetActive(true);

            _assignables = GetComponentsInChildren<AssignableObject>();
        }

        //if (_assigned == _assignableCount)
        if(_assigned == 4)
        {
            FinishedEvent.Invoke(this);
        }
    }

    protected override void OnEntered()
    {
        EquipmentManager.Instance.Enabled = false;
    }

    protected override void OnLeft()
    {
        EquipmentManager.Instance.Enabled = true;
        EquipmentManager.Instance.CurrentMode = EquipmentManager.EquipmentMode.USABLES;
    }

    #endregion
}
