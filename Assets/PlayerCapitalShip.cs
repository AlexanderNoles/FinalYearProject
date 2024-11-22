using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCapitalShip : MonoBehaviour
{
	private static PlayerCapitalShip instance;
	private RealSpacePostion pcsRSP;

	private void Awake()
	{
		instance = this;
	}

	public static void UpdatePCSPosition(RealSpacePostion pos)
	{
		instance.pcsRSP = pos;

		Shader.SetGlobalVector("_PCSPosition", instance.pcsRSP.TruncatedVector3(20000));
	}
}
