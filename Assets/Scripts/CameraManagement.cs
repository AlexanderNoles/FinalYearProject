using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;
using UnityEngine.Rendering.Universal;

public class CameraManagement : MonoBehaviour
{
    public const float cameraOffsetInMap = 50.0f;
    private static CameraManagement instance;
    private static List<(int, Transform)> targets = new List<(int, Transform)>();

    public static void AddCameraTarget(int priority, Transform target)
    {
        int priorityBeforeOperation = -1;
        if (targets.Count > 0)
        {
            priorityBeforeOperation = targets[0].Item1;
        }

        //Insert at appriopriate point in list
        for (int i = 0; i < targets.Count; i++)
        {
            //Has a lower priority than new target
            if (targets[i].Item1 < priority)
            {
                targets.Insert(i, (priority, target));
                //Leave early to not execute any of the code below
                return;
            }
        }

        //This happens if there are no other targets or if we are the lowest priority
        targets.Add((priority, target));

        if (priorityBeforeOperation != targets[0].Item1)
        {
            OnTargetChanged();
        }
    }

    public static void RemoveCameraTarget(Transform toRemove)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].Item2.Equals(toRemove))
            {
                targets.RemoveAt(i);

                if (i == 0)
                {
                    OnTargetChanged();
                }

                return;
            }
        }
    }

    public static void OnTargetChanged()
    {
		//Do nothing currently
    }

    public Transform GetCurrentTarget()
    {
        if (targets.Count == 0)
        {
            return null;
        }

        return targets[0].Item2;
    }

    public Vector3 GetTargetPosition()
    {
        Transform target = GetCurrentTarget();

        if (target == null)
        {
            return Vector3.zero;
        }

        return target.position;
    }

	public static Vector3 GetCameraDisplacementFromTarget()
	{
		return instance.transform.position - instance.GetTargetPosition();
	}

	public static void SetCameraPositionExternal(Vector3 pos, bool disableLerp)
	{
		if (disableLerp)
		{
			instance.transform.localPosition = Vector3.back * currentCameraZoomTarget;
			instance.cameraAxis.position = instance.GetTargetPosition() + offsetFromTarget;
		}
		else
		{
			instance.transform.position = pos;
		}
	}

	public static void AddRotation(Vector2 input)
	{
		instance.cameraRot += input;
	}

	public static void AddRotationMainOnly(Vector2 input)
	{
		if (instance.mainCamera.enabled)
		{
			instance.cachedCameraRot += input;
		}
		else
		{
			AddRotation(input);
		}
	}

    //////
    
    public static void SetMainCameraActive(bool active)
    {
		if (instance == null)
		{
			return;
		}

        instance.mainCamera.enabled = active;
    }

	public static Camera GetMainCamera()
	{
		return instance.mainCamera;
	}

    public static Vector3 GetMainCameraPosition()
    {
        return instance.transform.position;
    }

	public static Camera GetBackingCamera()
	{
		return instance.actualBackingCamera;
	}

    public static void SetBackingCameraPosition(Vector3 position)
    {
        instance.backingCamera.position = position;
    }

    public static void SetBackingCameraRotation(Quaternion rot)
    {
        instance.backingCamera.rotation = rot;
    }

    public static Vector3 GetBackingCameraPosition()
    {
        return instance.backingCamera.position;
    }

	/////

	public static Vector3 MakeVectorRelativeToCameraDirection(Vector3 input)
	{
		return
			(input.x * instance.mainCamera.transform.right) +
			(input.y * instance.mainCamera.transform.up) +
			(input.z * instance.mainCamera.transform.forward);
	}

	public Transform cameraAxis;
	public Transform backingCameraAxis;
    private Vector2 cameraRot;
	private Vector2 cachedCameraRot;
    private Camera mainCamera;
    public Transform backingCamera;
	private Camera actualBackingCamera;
    private UniversalAdditionalCameraData actualBackingCameraData;
    private static float currentCameraZoomTarget;
	private float cachedZoomTarget;
    private static Vector3 offsetFromTarget;
	private Vector3 cachedOffsetFromTarget;
    private const float lerpLimit = 0.01f;

    private void Awake()
    {
        instance = this;
        targets.Clear();

        mainCamera = GetComponent<Camera>();
		actualBackingCamera = backingCamera.GetComponent<Camera>();
        actualBackingCameraData = backingCamera.GetComponent<UniversalAdditionalCameraData>();
    }

    private void Update()
    {
		Transform targetCamera = null;
		Transform targetCamearAxis = null;
		float lowerCameraLimit = -85f;

		float zoomLowerLimit = 17.5f;

		if (mainCamera.enabled)
		{
			lowerCameraLimit = PlayerCapitalShip.currentState == PlayerCapitalShip.State.Normal ? 0 : -85f;

			if (actualBackingCameraData.renderPostProcessing)
			{
				backingCameraAxis.position = Vector3.zero;
				backingCameraAxis.rotation = Quaternion.identity;

				backingCamera.localPosition = Vector3.zero;
				actualBackingCameraData.renderPostProcessing = false;
				actualBackingCameraData.SetRenderer(1);

				//Reset to previous camera parameters
				cameraRot = cachedCameraRot;
				offsetFromTarget = cachedOffsetFromTarget;
				currentCameraZoomTarget = cachedZoomTarget;
			}

			targetCamera = transform;
			targetCamearAxis = cameraAxis;
		}
		else if (MapManagement.MapActive())
		{
			lowerCameraLimit = 5;
			zoomLowerLimit = 5;

			if (!actualBackingCameraData.renderPostProcessing)
			{
				//cache variables to reapply later
				cachedZoomTarget = currentCameraZoomTarget;
				cachedCameraRot = cameraRot;
				cachedOffsetFromTarget = offsetFromTarget;

				//Set initial zoom level
				currentCameraZoomTarget = 50;

				//Set rotation target
				cameraRot = new Vector2(45, cachedCameraRot.y + 180.0f);
				//Set specifc position so camera plays rotate animation on map opening
				backingCamera.localPosition = Vector3.back * currentCameraZoomTarget;

				//Set offset from center to zero
				offsetFromTarget = Vector3.zero;

				actualBackingCameraData.renderPostProcessing = true;
				actualBackingCameraData.SetRenderer(0);
			}

			targetCamera = backingCamera;
			targetCamearAxis = backingCameraAxis;
		}

		if (targetCamera == null || targetCamearAxis == null)
		{
			return;
		}

		#region Camera Control

		//Rotate camera
		if (UIManagement.InNeutral() || MapManagement.MapActive())
		{
			//In neutral or map
			Vector2 cameraInput = Vector2.zero;

			//Only allow input to rotation in neutral
			//Map simply applites set start rot

			if (InputManagement.GetMouseButton(InputManagement.cameraMove))
			{
				cameraInput = InputManagement.MouseInput();
			}

			cameraRot.y += cameraInput.y;
			cameraRot.x = Mathf.Clamp(cameraRot.x + cameraInput.x, lowerCameraLimit, 85);

			targetCamearAxis.rotation = Quaternion.Euler(cameraRot.x, cameraRot.y, 0.0f);
			targetCamera.LookAt(targetCamearAxis);

			//Move camera out
			float scrollInput = InputManagement.ScrollWheelInput();

			currentCameraZoomTarget = Mathf.Clamp(currentCameraZoomTarget - scrollInput, zoomLowerLimit, 130);
			targetCamera.localPosition = Vector3.Lerp(targetCamera.localPosition, Vector3.back * currentCameraZoomTarget, Time.deltaTime * 5.0f);
		}

		Vector3 newTargetPosition = GetTargetPosition() + offsetFromTarget;

		float lerpT;
		//Lerp is disabled because the effect looks weird with ship movement
		//Also it feels unnecesary
		const bool lerpEnabled = false;
		if (Vector3.Distance(newTargetPosition, targetCamearAxis.position) > lerpLimit && lerpEnabled)
		{
			lerpT = Time.deltaTime * 9.0f;
		}
		else
		{
			//Snap to destination when within lerp limit
			lerpT = 1.0f;
		}

		targetCamearAxis.position = Vector3.Lerp(targetCamearAxis.position, newTargetPosition, lerpT);

		#endregion

		if (mainCamera.enabled)
		{
			SetBackingCameraRotation(transform.rotation); 
            //# Calculate the camera's offset from the world center position #
            //The player is always (as of 05/01/2025) the world center, the player is the camera's target
            //So the ooffset from target and the offset from the world center are the same
			SurroundingsRenderingManagement.SetCameraOffset(mainCamera.transform.localPosition * WorldManagement.invertedInEngineWorldScaleMultiplier);
		}
    }
}
