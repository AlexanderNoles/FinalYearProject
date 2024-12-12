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

    public enum Mode
    {
        Normal,
        Debug
    }

    public static Mode currentMode = Mode.Normal;

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
		if (UIManagement.MapActive())
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
        instance.mainCamera.enabled = active;
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
    private const float moveSpeed = 5.0f;
    private const float sprintSpeed = 15.0f;
    private const float lerpLimit = 0.01f;
    private const float moveLimit = 250.0f;

	public float surroundingsMoveMultiplier = 1.0f;

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
        if (currentMode == Mode.Normal)
        {
			Transform targetCamera = null;
			Transform targetCamearAxis = null;

			if (mainCamera.enabled)
			{
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
			else if (UIManagement.MapActive())
			{
				if (UIManagement.FirstFrameMapIntroRunning())
				{
					//cache variables to reapply later
					cachedZoomTarget = currentCameraZoomTarget;
					cachedCameraRot = cameraRot;
					cachedOffsetFromTarget = offsetFromTarget;

					//Set initial zoom level
					currentCameraZoomTarget = 50;

					//Set rotation target
					cameraRot = new Vector2(45, 0);
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
			Vector2 cameraInput = Vector2.zero;

            if (InputManagement.GetMouseButton(InputManagement.cameraMove))
            {
                cameraInput = InputManagement.MouseInput();
            }

			cameraRot.y += cameraInput.y;
			cameraRot.x = Mathf.Clamp(cameraRot.x + cameraInput.x, -85, 85);

			targetCamearAxis.rotation = Quaternion.Euler(cameraRot.x, cameraRot.y, 0.0f);
			targetCamera.LookAt(targetCamearAxis);

			//Move camera out
			float scrollInput = InputManagement.ScrollWheelInput();

            currentCameraZoomTarget = Mathf.Clamp(currentCameraZoomTarget - scrollInput, 10, 150);
			targetCamera.localPosition = Vector3.Lerp(targetCamera.localPosition, Vector3.back * currentCameraZoomTarget, Time.deltaTime * 5.0f);

			//Move camera about
			if (!UIManagement.MapActive())
			{
				//No movement allowed in the map
				if (InputManagement.GetKeyDown(KeyCode.Space))
				{
					offsetFromTarget = Vector3.zero;
				}
				else
				{
					Vector3 wasdInput = InputManagement.WASDInput();

					//Get wasdInput relative to the direction the camera is facing
					Vector3 cameraForward = targetCamera.forward;
					cameraForward.y = 0;
					cameraForward.Normalize();

					Vector3 cameraRight = targetCamera.right;
					cameraRight.y = 0;
					cameraRight.Normalize();

					Vector3 relativeWasdInput = ((wasdInput.z * cameraRight) + (wasdInput.x * cameraForward)).normalized;

					//Apply to offset
					offsetFromTarget += relativeWasdInput * Time.deltaTime * (InputManagement.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);

					offsetFromTarget = Vector3.ClampMagnitude(offsetFromTarget, moveLimit);
				}
			}

            Vector3 newTargetPosition = GetTargetPosition() + offsetFromTarget;

            float lerpT;
            if (Vector3.Distance(newTargetPosition, targetCamearAxis.position) > lerpLimit)
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
		}
		else if (currentMode == Mode.Debug)
        {
            transform.localPosition = Vector3.zero;

            //Camera rotation
            Vector2 cameraInput = InputManagement.MouseInput();

            cameraRot.y += cameraInput.y;
            cameraRot.x = Mathf.Clamp(cameraRot.x + cameraInput.x, -85, 85);

            transform.rotation = Quaternion.Euler(cameraRot.x, cameraRot.y, 0.0f);

            //Move movent
            Vector3 cameraForward = transform.forward;
            Vector3 cameraRight = transform.right;
            Vector3 wasdInput = InputManagement.WASDInput();
            Vector3 relativeWasdInput = ((wasdInput.z * cameraRight) + (wasdInput.x * cameraForward)).normalized;

            cameraAxis.position += relativeWasdInput * Time.deltaTime * (InputManagement.GetKey(KeyCode.LeftShift) ? 10000 : 50);
        }

		if (mainCamera.enabled)
		{
			SetBackingCameraRotation(transform.rotation); 
			SurroundingsRenderingManagement.SetCameraOffset(transform.position * surroundingsMoveMultiplier);
		}
    }

    [ConsoleCMD("DCamera")]
    public static void ActivateDebugCamera()
    {
        currentMode = Mode.Debug;
    }

    [ConsoleCMD("NCamera")]
    public static void ActivateNormalCamera()
    {
        currentMode = Mode.Normal;
    }
}
