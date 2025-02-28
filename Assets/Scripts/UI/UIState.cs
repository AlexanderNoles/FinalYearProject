using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIState : MonoBehaviour
{
	[Header("Mouse state during UI state")]
	public bool useCutsomMouseState;

	[System.Serializable]
	public class UIStateMouse : MouseManagement.MouseState
	{
		public override Color TagALongColour(Sprite current)
		{
			Interaction currentInteraction = PlayerInteractionManagement.chosenInteraction;

			if (currentInteraction != null && current.Equals(currentInteraction.GetIcon()))
			{
				return base.TagALongColour(current);
			}

			return base.TagALongColour(current) * 0.5f;
		}
	}

	public UIStateMouse mouseState;

	[Header("Effect Settings")]
	public bool blurDuringState = false;

	[Header("UI State Settings")]
	public bool coenableNeutral = false;
    public bool autoSetup = true;
	protected bool autoSetupDone = false;
    public bool lockout = false;
    public bool toggleable = false;
	public bool pause = false;
    protected float introT;
	protected int framesSinceAnimBegan;
    protected bool outroOneFrameBuffer = false;
    protected bool wantsActive = false;


    protected virtual void Awake()
    {
        if (!autoSetup)
        {
            return;
        }

        //Add state and set inactive
        UIManagement.AddUIState(this);
        SetActive(false);

		autoSetupDone = true;
    }

    public virtual KeyCode GetSetActiveKey()
    {
        return KeyCode.None;
    }

    public void SetActive(bool _bool)
    {
        GetTargetObject().SetActive(_bool);
        OnSetActive(_bool);

		if (blurDuringState)
		{
			BlurEffect.SetEffectIntensity(_bool ? 1.0f : 0.0f);
		}
    }

    protected virtual void OnSetActive(bool _bool)
    {
        //Nothing by default
    }

    protected virtual GameObject GetTargetObject()
    {
        return gameObject;
    }

    public void TryToSetActive()
    {
        wantsActive = true;
    }

    public bool WantsToBeActive()
    {
        return wantsActive;
    }

    public void ResetWantForActivation()
    {
        wantsActive = false;
    }


    //intro t functions

    public float GetCurrentIntroT()
    {
        return introT;
    }

    public virtual void InitIntro()
    {
        outroOneFrameBuffer = false;
		framesSinceAnimBegan = 0;
		SetIntroT(1.0f);
    }

    public virtual void SetIntroT(float input)
    {
        introT = input;
    }

	public virtual float GetIntroDeltaTime()
	{
		return Time.deltaTime;
	}

    public virtual void RunIntro()
    {
        introT = Mathf.Clamp01(introT - (GetIntroDeltaTime() * GetIntroSpeed()));
		framesSinceAnimBegan++;

		if (introT <= 0.0f)
        {
            if (!outroOneFrameBuffer)
            {
                outroOneFrameBuffer = true;
            }
            else
            {
                outroOneFrameBuffer = false;
                EndIntro();
            }
        }
    }

    public virtual float GetIntroSpeed()
    {
        return 1.0f;
    }

    public virtual void EndIntro()
    {

    }

    public virtual bool IntroRunning()
    {
        return introT > 0.0f || outroOneFrameBuffer;
    }

    public virtual bool LastFrameOfIntro()
    {
        return outroOneFrameBuffer;
    }

    public virtual bool FirstFrameOfIntro()
    {
        return introT == 1.0f || framesSinceAnimBegan < 2;
    }
}
