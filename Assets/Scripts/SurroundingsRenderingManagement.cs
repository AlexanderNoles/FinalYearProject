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

	public GameObject allSurroundings;
    public GameObject skybox;

    private static List<SurroundingObject> controlledObjects = new List<SurroundingObject>();
	private static SurroundingsRenderingManagement instance;
	public static Transform mainTransform;

    public static List<SurroundingObject> GetControlledObjects()
    {
        return controlledObjects;
    }

    private void Awake()
    {
		instance = this;
        SetNotInMap(true);
		mainTransform = transform;

		SetAllSurroundingsActive(false);
    }

    public static void SetNotInMap(bool active)
    {
        Shader.SetGlobalFloat("_InMap", active ? -1 : 1);
    }

	public static void SetAllSurroundingsActive(bool _bool)
	{
		instance.allSurroundings.SetActive(_bool);
	}

	public static void SetSkyboxActive(bool _bool)
	{
		instance.skybox.SetActive(_bool);
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
		if (!MapManagement.MapActive() && !SimulationManagement.RunningHistory())
        {
			//If in jump travel then disable default surroundings rendering
			if (PlayerCapitalShip.InJumpTravelStage())
			{
				allSurroundings.SetActive(false);
				return;
			}
			else if (!allSurroundings.activeSelf)
			{
				allSurroundings.SetActive(true);
			}
			//

			if (!skybox.activeSelf)
			{
				skybox.SetActive(true);
			}

			transform.position = Vector3.zero;

			List<(double, RealSpacePosition, int)> distanceOffsetAndIndex = new List<(double, RealSpacePosition, int)>();

			//Calculate distances, so can sort them by distance
			//We also cache offsets cause there is no reason to calculate them twice
			for (int i = 0; i < controlledObjects.Count; i++)
			{
				//Get offset from world center
				RealSpacePosition offset = WorldManagement.OffsetFromWorldCenter(controlledObjects[i].position, cameraOffset);
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
