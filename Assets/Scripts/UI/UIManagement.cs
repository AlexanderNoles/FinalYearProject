using MonitorBreak;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManagement : MonoBehaviour
{
    private static UIManagement instance;
	private List<StateOverride> currentStateOverrides = new List<StateOverride>();
    private List<UIState> uiStates = new List<UIState>();
    private UIState activeState;

	public static void AddStateOverride(StateOverride stateOverride)
	{
		if (instance == null)
		{
			return;
		}

		instance.currentStateOverrides.Add(stateOverride);
	}

	public static void RemoveStateOverride(StateOverride stateOverride)
	{
		if (instance == null) 
		{ 
			return; 
		}

		instance.currentStateOverrides.Remove(stateOverride);
	}

    public static void AddUIState(UIState newState)
    {
        if (instance == null)
        {
            return;
        }

        instance.uiStates.Add(newState);
    }

	public static bool IsCurrentState(UIState state)
	{
		return instance.activeState.Equals(state);
	}

	public static void RefreshUIState()
	{
		instance.activeState.SetActive(false);
		SetupUIStateInternal();
	}

    public static bool LoadUIState(UIState newState)
    {
        if (instance.activeState != null && instance.activeState.Equals(newState))
        {
            return false;
        }

        if (!InPureNeutral() && !instance.neutral.Equals(newState))
        {
            //If not in neutral and not trying to return to neutral
            return false;
        }

        //Set active
        //Hide old state
        if (instance.activeState != null)
        {
            instance.activeState.SetActive(false);
        }

		//Set new state active
		instance.activeState = newState;
		SetupUIStateInternal();

		return true;
    }

	private static void SetupUIStateInternal()
	{
		instance.activeState.SetActive(true);
		instance.activeState.InitIntro();
		instance.activeState.ResetWantForActivation();

		if (instance.activeState.pause)
		{
			TimeManagement.AddTimeScale(0.0f, 100, instance);
		}
		else
		{
			TimeManagement.RemoveTimeScale(instance);
		}

		if (instance.activeState.useCutsomMouseState)
		{
			MouseManagement.ApplyMouseState(instance.activeState.mouseState);
		}
		else
		{
			MouseManagement.ResetMouseState();
		}

		if (instance.activeState.coenableNeutral && !instance.activeState.Equals(instance.neutral))
		{
			instance.neutral.SetActive(true);
		}
	}

    public UIState neutral;

    public static bool InPureNeutral()
    {
        return instance.activeState != null && instance.activeState.Equals(instance.neutral);
    }

	public static bool NeutralEnabled()
	{
		return instance.neutral.gameObject.activeSelf;
	}

    public static void ReturnToPureNeutral()
    {
        LoadUIState(instance.neutral);
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //First ui state in list
        AddUIState(neutral);
        LoadUIState(neutral);
    }

    private void Update()
    {
		//Check if a window override wants to be closed
		for (int i = 0; i < currentStateOverrides.Count;)
		{
			StateOverride currentTarget = currentStateOverrides[i];
			if (InputManagement.GetKeyDown(currentTarget.inputKey))
			{
				currentTarget.Process();
				//Ensure no other scripts use this keycode till next frame
				InputManagement.ConsumeKeyCode(currentTarget.inputKey);
			}

			//State override was not removed during process
			if (currentStateOverrides.Contains(currentTarget))
			{
				i++;
			}
		}

        if (activeState != null)
        {
            //This runs before the ui state is set active
            //This means any script can check if this is the first
            //frame of an intro anim before we lower the t below 1
            if (activeState.IntroRunning())
            {
                activeState.RunIntro();
            }

            //If in lockout then don't allow user input to be registered to change state
            if (activeState.lockout)
            {
                return;
            }

            //If active state key is pressed down this frame
            //toggle back to neutral
            if (activeState.toggleable && ValidateInput(activeState.GetSetActiveKey()))
            {
                LoadUIState(neutral);
                return;
            }
        }

        //Iterate through each ui state
        foreach (UIState state in uiStates)
        {
            //If key code input registered this frame
            //OR ui state wants to be active
            //Then try to set that state active
            if (state.WantsToBeActive() || ValidateInput(state.GetSetActiveKey()))
            {
				if (LoadUIState(state))
				{
					break;
				}
            }
        }
	}

	private bool ValidateInput(KeyCode keyCode)
	{
		return InputManagement.GetKeyDown(keyCode) && MonitorBreak.Bebug.Console.GetConsoleState() == MonitorBreak.Bebug.Console.ConsoleState.Closed;
	}
}
