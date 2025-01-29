using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuManagement : MonoBehaviour
{
	public string title = "star and salvation";
	private int currentCharIndex = 0;
	public TextMeshProUGUI titleLabel;
	public float timeBetweenLetters = 0.1f;
	private float timeTillNextLetter;
	public FadeOnEnable fadeIn;

	private void Awake()
	{
		//Ensure _InMap isn't set to 0, as this messes with the planet rendering lighting
		Shader.SetGlobalFloat("_InMap", 1.0f);

		titleLabel.text = "";
		timeTillNextLetter = timeBetweenLetters;
		currentCharIndex = 0;
	}

	private void Update()
	{
		if (fadeIn.Finished() && currentCharIndex != -1)
		{
			if (timeTillNextLetter > 0.0f)
			{
				timeTillNextLetter -= Time.deltaTime;
			}
			else
			{
				timeTillNextLetter = timeBetweenLetters;
				titleLabel.text += title[currentCharIndex];
				currentCharIndex++;

				if (currentCharIndex >= title.Length)
				{
					currentCharIndex = -1;
				}
			}
		}
	}

	public void LoadMainScene()
	{
		GameManagement.LoadScene(1);
	}
}
