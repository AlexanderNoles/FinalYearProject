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

	public static void SetRealWorldPos(Vector3 pos)
	{
		instance.transform.position = pos;
	}

	public static void ModelLookAt(Vector3 pos)
	{
		pos.y = instance.transform.position.y;

		instance.transform.LookAt(pos);

		//Adjustment for current model
		instance.transform.Rotate(Vector3.down * 90, Space.Self);
	}
}
