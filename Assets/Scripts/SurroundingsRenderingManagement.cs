using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SurroundingsRenderingManagement : MonoBehaviour
{
    public GameObject skybox;

    private static List<SurroundingObject> controlledObjects = new List<SurroundingObject>();
	public static Transform mainTransform;

    public static List<SurroundingObject> GetControlledObjects()
    {
        return controlledObjects;
    }

    private void Awake()
    {
        SetActivePlanetLighting(true);
		mainTransform = transform;
    }

    public static void SetActivePlanetLighting(bool active)
    {
        Shader.SetGlobalFloat("_LightingEnabled", active ? 1 : 0);
    }

    public static void RegisterSurroundingObject(SurroundingObject newObj)
    {
        if (newObj == null || controlledObjects.Contains(newObj))
        {
            return;
        }

        controlledObjects.Add(newObj);
    }

    public static void DeRegisterSurroundingObject(SurroundingObject oldObj)
    {
        if (oldObj == null)
        {
            return;
        }

        controlledObjects.Remove(oldObj);
    }

    private void LateUpdate()
    {
        if (UIManagement.MapActive())
        {
            if (UIManagement.MapIntroRunning())
            {
                if (UIManagement.FirstFrameMapIntroRunning())
                {
                    //Set skybox inactive
                    skybox.SetActive(false);
                }

                Vector3 worldCenterPos = WorldManagement.worldCenterPosition.TruncatedVector3(UIManagement.mapRelativeScaleModifier);

                transform.position = worldCenterPos;

                float evaluatedIntroT = UIManagement.EvaluatedMapIntroT();

                foreach (SurroundingObject obj in controlledObjects)
                {
                    obj.transform.localPosition = -obj.postion.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + ((Vector3.down + Vector3.forward).normalized * Mathf.Lerp(0.0f, CameraManagement.cameraOffsetInMap, evaluatedIntroT));
					obj.SetShellOffset(-1);
					obj.SetObjectVisualScale((obj.scale / UIManagement.mapRelativeScaleModifier));
				}
            }
        }
        else
        {
            if (!skybox.activeSelf)
            {
                skybox.SetActive(true);
            }

            transform.position = CameraManagement.GetBackingCameraPosition();

            List<(double, RealSpacePostion, int)> distanceOffsetAndIndex = new List<(double, RealSpacePostion, int)>();

            //Calculate distances, so can sort them by distance
            //We also cache offsets cause there is no reason to calculate them twice
            for (int i = 0; i < controlledObjects.Count; i++)
            {
                //Get offset from world center
                RealSpacePostion offset = WorldManagement.OffsetFromWorldCenter(controlledObjects[i].postion);
                //Get distance to world center
                double magnitude = offset.Magnitude();

                distanceOffsetAndIndex.Add((magnitude, offset, i));

                ////Set rotation around center and scale based on those values
                //float newScale = (float)(obj.scale / magnitude);
                //obj.SetObjectVisualScale(newScale);

                //obj.transform.localPosition = offset.Normalized().AsVector3() * 10;
            }

            int shellIndex = 1;
            while (distanceOffsetAndIndex.Count > 0)
            {
                double minimumDistance = double.MaxValue;
                int minimumIndex = 0;
                //Find shortest distance
                for (int i = 0; i < distanceOffsetAndIndex.Count; i++)
                {
                    if (distanceOffsetAndIndex[i].Item1 < minimumDistance)
                    {
                        minimumDistance = distanceOffsetAndIndex[i].Item1;
                        minimumIndex = i;
                    }
                }

                //Set it's position and scale
                float shellOffset = 10 * shellIndex;
                SurroundingObject target = controlledObjects[distanceOffsetAndIndex[minimumIndex].Item3];

                target.transform.localPosition = distanceOffsetAndIndex[minimumIndex].Item2.Normalized().AsVector3() * shellOffset;

                float newScale = (float)(target.scale / distanceOffsetAndIndex[minimumIndex].Item1) * shellOffset;
				target.SetShellOffset(shellOffset);
				target.SetObjectVisualScale(newScale);

                //Remove it from the list
                distanceOffsetAndIndex.RemoveAt(minimumIndex);

                //Increment the shell index
                shellIndex++;
            }
        }
    }
}
