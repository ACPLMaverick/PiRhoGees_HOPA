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
	}
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();
	}

    protected void OnAssigned(AssignableObject asg)
    {
        ++_assigned;
        if (_assigned == _assignableCount)
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
