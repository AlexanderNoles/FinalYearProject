using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralUIState : UIState
{
	public override KeyCode GetSetActiveKey()
    {
        return InputManagement.returnToNeutral;
    }
}
