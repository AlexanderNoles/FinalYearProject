using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManagement : MonoBehaviour
{
	private void Awake()
	{
		Shader.SetGlobalFloat("_InMap", 1.0f);
	}

	public void LoadMainScene()
	{
		GameManagement.LoadScene(1);
	}
}
