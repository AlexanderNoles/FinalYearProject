using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDisplay
{
	public string GetTitle();
	public string GetDescription();
	public Sprite GetIcon();
	public string GetExtraInformation();
}
