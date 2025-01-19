using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTargetsUI : MonoBehaviour
{
	private static CurrentTargetsUI instance;
	public MultiObjectPool targetObjectsPool;
	private Camera mainCamera;
	public CanvasScaler targetCanvasScalar;

	private static List<BattleBehaviour> displayedTargets = new List<BattleBehaviour>();
	private Dictionary<RectTransform, Image> rectTransformToHealthIndicator = new Dictionary<RectTransform, Image>();

	private void Awake()
	{
		instance = this;
		displayedTargets.Clear();
	}

	private void Start()
	{
		mainCamera = CameraManagement.GetMainCamera();
	}

	public static void AddTarget(BattleBehaviour target)
	{
		if (displayedTargets.Contains(target))
		{
			return;
		}

		//Insert new target
		displayedTargets.Add(target);
	}

	public static void RemoveTarget(BattleBehaviour target)
	{
		displayedTargets.Remove(target);

		if (displayedTargets.Count == 0)
		{
			//Hide all
			instance.targetObjectsPool.HideAllObjects(0);
		}
	}

	private void Update()
	{
		//Hide all objects, this function is probably misnamed for it's use case
		//it should go before objects updating for the inital "any active to hide" check to work
		//without "force" being set to true.
		//MulitObjectPool is a ship of thesus type system at this point and should be refactored
		//(12/01/2025)
		targetObjectsPool.PruneObjectsNotUpdatedThisFrame(0);

		//Each frame iterate over targets and update
		foreach (BattleBehaviour target in displayedTargets)
		{
			Vector3 screenSpacePos = mainCamera.WorldToScreenPoint(target.transform.position) * (1.0f / targetCanvasScalar.scaleFactor);

			if (screenSpacePos.z > 0)
			{
				//Set
				RectTransform targetRect = targetObjectsPool.UpdateNextObjectPosition(0, Vector3.zero).transform as RectTransform;

				if (!rectTransformToHealthIndicator.ContainsKey(targetRect))
				{
					rectTransformToHealthIndicator.Add(targetRect, targetRect.GetChild(1).GetComponent<Image>());
				}

				rectTransformToHealthIndicator[targetRect].fillAmount = target.GetHealthPercentage();

				//Zero z component so ui lines up correctly with ui camera
				targetRect.anchoredPosition3D = new Vector3(screenSpacePos.x, screenSpacePos.y, 0.0f);
			}
		}
	}
}
