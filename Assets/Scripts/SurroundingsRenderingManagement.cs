using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SurroundingsRenderingManagement : MonoBehaviour
{
	private static Vector3 cameraOffset = Vector3.zero;

	public static void SetCameraOffset(Vector3 input)
	{
		cameraOffset = input;
	}

    public GameObject skybox;

    private static List<SurroundingObject> controlledObjects = new List<SurroundingObject>();
	public static Transform mainTransform;

    public static List<SurroundingObject> GetControlledObjects()
    {
        return controlledObjects;
    }

    private void Awake()
    {
        SetNotInMap(true);
		mainTransform = transform;
    }

    public static void SetNotInMap(bool active)
    {
        Shader.SetGlobalFloat("_InMap", active ? -1 : 1);
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
        if (MapManagement.MapActive())
        {
            if (MapManagement.MapIntroRunning())
			{
				if (MapManagement.FirstFrameMapIntroRunning())
				{
					skybox.SetActive(false);
				}

                float evaluatedIntroT = MapManagement.EvaluatedMapIntroT();

				//Animation temporarily disabled
				Vector3 currentOffset = Vector3.down * Mathf.Lerp(0.0f, CameraManagement.cameraOffsetInMap, 1.0f - evaluatedIntroT);

				foreach (SurroundingObject obj in controlledObjects)
                {
                    obj.transform.localPosition = -obj.postion.AsTruncatedVector3(MapManagement.mapRelativeScaleModifier) + currentOffset;
					obj.SetShellOffset(-1);
					obj.SetObjectVisualScale((obj.scale / MapManagement.mapRelativeScaleModifier));
				}
            }
        }
        else
        {
			if (skybox.activeSelf == false)
			{
				skybox.SetActive(true);
			}

            transform.position = Vector3.zero;

            List<(double, RealSpacePostion, int)> distanceOffsetAndIndex = new List<(double, RealSpacePostion, int)>();

            //Calculate distances, so can sort them by distance
            //We also cache offsets cause there is no reason to calculate them twice
            for (int i = 0; i < controlledObjects.Count; i++)
            {
                //Get offset from world center
                RealSpacePostion offset = WorldManagement.OffsetFromWorldCenter(controlledObjects[i].postion, cameraOffset);
                //Get distance to world center
                double magnitude = offset.Magnitude();

                distanceOffsetAndIndex.Add((magnitude, offset, i));
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

                const float baseScaleToEngineModifier = 0.005f;
                float baseScale = target.scale * baseScaleToEngineModifier * WorldManagement.inEngineWorldScaleMultiplier;
                float newScale = (float)(baseScale / distanceOffsetAndIndex[minimumIndex].Item1) * shellOffset;

                target.SetRawScale((float)(target.scale / distanceOffsetAndIndex[minimumIndex].Item1));
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
