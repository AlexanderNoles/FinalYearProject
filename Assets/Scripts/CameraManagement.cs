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
        currentCameraZoomTarget = 10.0f;
        offsetFromTarget = Vector3.zero;
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

    //////
    
    public static void SetMainCameraActive(bool active)
    {
        instance.mainCamera.enabled = active;
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
    private Vector2 cameraRot;
    private Vector2 inMapRot;
    private Camera mainCamera;
    public Transform backingCamera;
    private UniversalAdditionalCameraData actualBackingCameraData;
    private static float currentCameraZoomTarget;
    private static Vector3 offsetFromTarget;
    private const float moveSpeed = 5.0f;
    private const float sprintSpeed = 15.0f;
    private const float lerpLimit = 0.01f;
    private const float moveLimit = 250.0f;

    private Vector3 positionLastFrame;

    private void Awake()
    {
        instance = this;
        targets.Clear();

        mainCamera = GetComponent<Camera>();
        actualBackingCameraData = backingCamera.GetComponent<UniversalAdditionalCameraData>();

        positionLastFrame = transform.position;
    }

    private void Update()
    {
        //If most things aren't being rendered
        if (!mainCamera.enabled)
        {
            //Do backing camera solo stuff
            if (UIManagement.MapActive())
            {
                if (UIManagement.MapIntroRunning())
                {
                    if (UIManagement.FirstFrameMapIntroRunning())
                    {
                        inMapRot = new Vector2(45, 0);
                        actualBackingCameraData.renderPostProcessing = true;
                        actualBackingCameraData.SetRenderer(0);
                    }
                }
                else
                {
                    //Rotation
                    if (InputManagement.GetMouseButton(InputManagement.cameraMove))
                    {
                        Vector2 cameraInput = InputManagement.MouseInput();

                        inMapRot.y += cameraInput.y;
                        inMapRot.x = Mathf.Clamp(inMapRot.x + cameraInput.x, -85, 85);
                    }

                    //Move Camera around
                    Vector3 wasdInput = InputManagement.WASDInput();

                    Vector3 cameraForward = backingCamera.forward;
                    cameraForward.Normalize();

                    Vector3 cameraRight = backingCamera.right;
                    cameraRight.Normalize();

                    Vector3 relativeWasdInput = ((wasdInput.z * cameraRight) + (wasdInput.x * cameraForward)).normalized;

                    backingCamera.position += relativeWasdInput * (Time.deltaTime * (InputManagement.GetKey(KeyCode.LeftShift) ? 75 : 50));

                    Vector3 anchorPos = MapManagement.GetDisplayOffset();

                    Vector3 offsetFromAnchor = backingCamera.position - anchorPos;

                    float mag = Mathf.Clamp(offsetFromAnchor.magnitude, 5, 150);

                    offsetFromAnchor = offsetFromAnchor.normalized * mag;
                    backingCamera.position = anchorPos + offsetFromAnchor;
                }

                backingCamera.rotation = Quaternion.Euler(inMapRot.x, inMapRot.y, 0);
            }

            return;
        }
        else
        {
            if (actualBackingCameraData.renderPostProcessing)
            {
                backingCamera.position = Vector3.zero;
                actualBackingCameraData.renderPostProcessing = false;
                actualBackingCameraData.SetRenderer(1);
            }
        }

        if (currentMode == Mode.Normal)
        {
            //Rotate camera
            if (InputManagement.GetMouseButton(InputManagement.cameraMove))
            {
                Vector2 cameraInput = InputManagement.MouseInput();

                cameraRot.y += cameraInput.y;
                cameraRot.x = Mathf.Clamp(cameraRot.x + cameraInput.x, -85, 85);

                cameraAxis.rotation = Quaternion.Euler(cameraRot.x, cameraRot.y, 0.0f);

                mainCamera.transform.LookAt(cameraAxis);
            }

            //Move camera out
            float scrollInput = InputManagement.ScrollWheelInput();

            currentCameraZoomTarget = Mathf.Clamp(currentCameraZoomTarget - scrollInput, 10, 250);
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.back * currentCameraZoomTarget, Time.deltaTime * 5.0f);

            //Move camera about
            if (InputManagement.GetKeyDown(KeyCode.Space))
            {
                offsetFromTarget = Vector3.zero;
            }
            else
            {
                Vector3 wasdInput = InputManagement.WASDInput();

                //Get wasdInput relative to the direction the camera is facing
                Vector3 cameraForward = transform.forward;
                cameraForward.y = 0;
                cameraForward.Normalize();

                Vector3 cameraRight = transform.right;
                cameraRight.y = 0;
                cameraRight.Normalize();

                Vector3 relativeWasdInput = ((wasdInput.z * cameraRight) + (wasdInput.x * cameraForward)).normalized;

                //Apply to offset
                offsetFromTarget += relativeWasdInput * Time.deltaTime * (InputManagement.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);

                offsetFromTarget = Vector3.ClampMagnitude(offsetFromTarget, moveLimit);
            }

            Vector3 newTargetPosition = GetTargetPosition() + offsetFromTarget;

            float lerpT;
            if (Vector3.Distance(newTargetPosition, cameraAxis.position) > lerpLimit)
            {
                lerpT = Time.deltaTime * 9.0f;
            }
            else
            {
                //Snap to destination when within lerp limit
                lerpT = 1.0f;
            }

            cameraAxis.position = Vector3.Lerp(cameraAxis.position, newTargetPosition, lerpT);
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

            cameraAxis.position += relativeWasdInput * Time.deltaTime * (InputManagement.GetKey(KeyCode.LeftShift) ? 1000 : 50);
        }

        SetBackingCameraRotation(transform.rotation);

        Vector3 offset = transform.position - positionLastFrame;
        WorldManagement.MoveWorldCenter(offset);

        positionLastFrame = transform.position;
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
