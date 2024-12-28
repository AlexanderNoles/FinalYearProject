using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManagement : MonoBehaviour
{
    private static UIManagement instance;
    private List<UIState> uiStates = new List<UIState>();
    private UIState activeState;

    public static void AddUIState(UIState newState)
    {
        if (instance == null)
        {
            return;
        }

        instance.uiStates.Add(newState);
    }

    public static void LoadUIState(UIState newState)
    {
        if (instance.activeState != null && instance.activeState.Equals(newState))
        {
            return;
        }

        if (!InNeutral() && !instance.neutral.Equals(newState))
        {
            //If not in neutral and not trying to return to neutral
            return;
        }

        if (!newState.enabled)
        {
            return;
        }

        //Set active
        //Hide old state
        if (instance.activeState != null)
        {
            instance.activeState.SetActive(false);
        }

        //Set new state active
        instance.activeState = newState;
        instance.activeState.SetActive(true);
        instance.activeState.InitIntro();
        instance.activeState.ResetWantForActivation();
    }

    public UIState neutral;

    public static bool InNeutral()
    {
        return instance.activeState != null && instance.activeState.Equals(instance.neutral);
    }

    public static void ReturnToNeutral()
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
            if (activeState.toggleable && InputManagement.GetKeyDown(activeState.GetSetActiveKey()))
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
            if (state.WantsToBeActive() || InputManagement.GetKeyDown(state.GetSetActiveKey()))
            {
                LoadUIState(state);
                break;
            }
        }
	}
}
