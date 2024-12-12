using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagement : MonoBehaviour
{
    private static UIManagement instance;

	[Header("Map")]
    public GameObject map;
    public const float mapRelativeScaleModifier = 1000.0f;
    public AnimationCurve mapIntroCurve;
    private static GameObject mapObject;
    private static float mapIntroT;
    private static bool oneFrameBuffer = false;

	private bool mapButtonPressed = false;

	[Header("History")]
	public HistoryUIManagement historyUI;

	[Header("Inventory")]
	public FloatingWindow inventoryUI;

	[Header("Location")]
	public GameObject locationUI;

    public static bool MapIntroRunning()
    {
        return mapIntroT > 0.0f || oneFrameBuffer;
    }

    public static bool FirstFrameMapIntroRunning()
    {
        //UI managment always runs first with our mandated script execution order
        return mapIntroT == 1.0f;
    }

    public static bool LastFrameMapIntroRunning()
    {
        return oneFrameBuffer;
    }


    public static float EvaluatedMapIntroT()
    {
        return instance.mapIntroCurve.Evaluate(1.0f - mapIntroT);
    }

    public static bool MapActive()
    {
        return mapObject != null && mapObject.activeSelf; 
    }

	public static void SetHistoryUIActive(bool _bool)
	{
		if (_bool)
		{
			instance.historyUI.Activate();
		}
		else
		{
			instance.historyUI.Deactivate();
		}
	}

    private void Awake()
    {
        instance = this;

        mapObject = map.gameObject;
        mapObject.SetActive(false);
        mapIntroT = 0.0f;
    }

    private void Update()
    {
        if (map != null)
        {
            if ((InputManagement.GetKeyDown(KeyCode.M) || mapButtonPressed) && MonitorBreak.Bebug.Console.GetConsoleState() != MonitorBreak.Bebug.Console.ConsoleState.FullScreen)
            {
				mapButtonPressed = false;

				bool active = !mapObject.activeSelf;

				mapObject.SetActive(active);

				CameraManagement.SetMainCameraActive(!active);
				SurroundingsRenderingManagement.SetActivePlanetLighting(!active);

				mapIntroT = 1.0f;
			}
            else
            {
                if (MapActive())
                {
                    //Can't use MapIntroRunning() as we don't want to account for the one frame buffer
                    if (mapIntroT > 0.0f)
                    {
                        mapIntroT -= Time.deltaTime * 1.5f;

                        if (mapIntroT <= 0.0f)
                        {
                            //Ensure one frame of intro anim runs in all other scripts when map intro goes below 0.0f
                            oneFrameBuffer = true;
                        }
                    }
                    else
                    {
                        if (oneFrameBuffer)
                        {
                            oneFrameBuffer = false;
                        }
                    }
                }
            }
        }

		if (InputManagement.GetKeyDown(KeyCode.Tab))
		{
			ToggleInventory();
		}

		if (InputManagement.GetKeyDown(KeyCode.E))
		{
			ToggleLocationInformation();
		}
	}

	public void ToggleMapButton()
	{
		//This has to be done (instead of simply using this function for toggling the map and calling that when m is pressed)
		//because the first frame of map animation system relies on UIManagement running before anything that uses it in the frame
		mapButtonPressed = true;
	}

	public void ToggleInventory()
	{
		inventoryUI.gameObject.SetActive(!inventoryUI.gameObject.activeSelf);
		inventoryUI.MoveToFront();
	}

	public void ToggleLocationInformation()
	{
		locationUI.SetActive(!locationUI.activeSelf);
	}
}
