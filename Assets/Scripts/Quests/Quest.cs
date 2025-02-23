using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : IDisplay
{
	public VisitableLocation questOrigin;

	//Display methods
	public virtual string GetDescription()
	{
		throw new System.NotImplementedException();
	}

	public virtual string GetExtraInformation()
	{
		throw new System.NotImplementedException();
	}

	public virtual Sprite GetIcon()
	{
		throw new System.NotImplementedException();
	}

	public virtual string GetTitle()
	{
		throw new System.NotImplementedException();
	}
}
