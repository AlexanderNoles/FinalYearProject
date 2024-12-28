using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOnEnable : MonoBehaviour
{
    private Color cachedColor;

    public void SetStartColor(Color color) 
    {
        cachedColor = color;
    }

    private float targetAlpha;

    //non-ui
    private SpriteRenderer sr;
    private bool nonUI = false;

    //ui
    private Image _image;
    
    public float speed = 1f;
    public bool fadeIn = false;
    private bool fadeInLastFrame = false;
    public bool adjustForSlowedTime;
    public float targetAlphaOverrideValue;
    public bool overrideTargetAlpha;

    public bool startAutomatically = true;
    private bool canStart = false;

    [Header("Wait Time")]
    public bool waitForTime = false;
    public float time;
    private float timeRemaining;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if(_image == null)
        {
            sr = GetComponent<SpriteRenderer>();
            cachedColor = sr.color;
            nonUI = true;
        }
        else
        {
            cachedColor = _image.color;
        }
    }

	public void Restart()
	{
		gameObject.SetActive(false);
		gameObject.SetActive(true);
	}

    public bool Finished()
    {
        float currentAlpha;
        if (nonUI)
        {
            currentAlpha = sr.color.a;
        }
        else
        {
            currentAlpha = _image.color.a;
        }
        return currentAlpha > targetAlpha * 0.99f;
    }

    public float GetCurrentAlpha()
    {
        if (nonUI)
        {
            return sr.color.a;
        }
        else
        {
            try{
                return _image.color.a;
            }
            catch{};
        }
        return 0;
    }

    private void OnEnable()
    {
        if (nonUI)
        {
            sr.color = cachedColor;
        }
        else
        {
            _image.color = cachedColor;
        }

        if (waitForTime)
        {
            timeRemaining = time;
        }

        canStart = false;
    }

    private void Update()
    {
        if (!startAutomatically && !canStart)
        {
            return;
        }

        if(waitForTime && timeRemaining > 0)
        {
            timeRemaining -= Time.unscaledDeltaTime;
            return;
        }

        if (overrideTargetAlpha)
        {
            targetAlpha = targetAlphaOverrideValue;
        }
        else
        {
            if (fadeInLastFrame)
            {
                targetAlpha = 1f;
            }
            else
            {
                targetAlpha = 0f;
            }
        }

        float speedThisFrame = speed;

        if (adjustForSlowedTime)
        {
            speedThisFrame *= Time.unscaledDeltaTime;
        }
        else
        {
            speedThisFrame *= Time.deltaTime;
        }

        if (nonUI)
        {
            sr.color = new Color(cachedColor.r, cachedColor.g, cachedColor.b, Mathf.Lerp(sr.color.a, targetAlpha, speedThisFrame));
        }
        else
        {
            _image.color = new Color(cachedColor.r, cachedColor.g, cachedColor.b, Mathf.Lerp(_image.color.a, targetAlpha, speedThisFrame));
        }
        fadeInLastFrame = fadeIn;
    }

    public void StartEffect()
    {
        canStart = true;
    }
}
