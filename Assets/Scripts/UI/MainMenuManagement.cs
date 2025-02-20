using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuManagement : MonoBehaviour
{
	public string title = "star and salvation";
	public char typingString = '|';
	private int currentCharIndex = 0;
	public TextMeshProUGUI titleLabel;
	public float timeBetweenLetters = 0.1f;
	private float timeTillNextLetter;
	private bool typingEffectDone;
	public FadeOnEnable fadeIn;
	public StartScreenState startScreen;

	private void Awake()
	{
		//Ensure _InMap isn't set to 0, as this messes with the planet rendering lighting
		Shader.SetGlobalFloat("_InMap", 1.0f);

		titleLabel.text = "";
		timeTillNextLetter = timeBetweenLetters;
		currentCharIndex = -1;
		typingEffectDone = false;
	}

	private void Update()
	{
		if (fadeIn.Finished() && !typingEffectDone)
		{
			if (timeTillNextLetter > 0.0f)
			{
				timeTillNextLetter -= Time.deltaTime;
			}
			else
			{
				timeTillNextLetter = timeBetweenLetters;

				//Within string
				//Add typing character
				if (currentCharIndex >= 0)
				{
					//Add actual string
					titleLabel.text = titleLabel.text.Replace(typingString, title[currentCharIndex]);
				}

				if (currentCharIndex < title.Length - 1)
				{
					titleLabel.text += typingString;
				}

				currentCharIndex++;

				if (currentCharIndex >= title.Length)
				{
					typingEffectDone = true;
				}
			}
		}
	}

	public void LoadMainScene()
	{
		//Apply emblem data
		Player.emblemOverride = startScreen.emblemData;

		GameManagement.LoadScene(1);
	}

	public void Quit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}
}
