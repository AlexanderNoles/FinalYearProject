using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentCollider : MonoBehaviour
{
	//Could convert this to a lookup table BUT
	//currently only realistically 2-4 colliders in any given scene
	//Lookup table would probably end up being more expensive than just iterationg
	public static List<EnvironmentCollider> currentColliders = new List<EnvironmentCollider>();

	public static Vector3 GetDisplacementFromColliders(Vector3 pos)
	{
		Vector3 toReturn = Vector3.zero;
		foreach (EnvironmentCollider collider in currentColliders)
		{
			toReturn += collider.GetDisplacementVectorAtPosition(pos);
		}

		return toReturn;
	}

	[HideInInspector]
	public new Transform transform;
	public float radius = 10.0f;
	public float fallOffDistance = 1.0f;

	[Header("Extras")]
	public bool assumeFlatPlane = true;
	public float maxForce = 10;
	public Color debugColor = Color.cyan;

	private void OnEnable()
	{
		currentColliders.Add(this);
	}

	private void OnDisable()
	{
		currentColliders.Remove(this);
	}

	private void Awake()
	{
		transform = base.transform;
	}

	public Vector3 GetDisplacementVectorAtPosition(Vector3 pos)
	{
		Vector3 ourPos = transform.position;

		if (assumeFlatPlane)
		{
			//Match ys
			pos.y = ourPos.y;
		}

		//Return random maxed displacement vector
		if (pos.Equals(ourPos))
		{
			Vector3 toReturn = Random.onUnitSphere;
			toReturn.y = 0.0f;
			toReturn.Normalize();

			return toReturn * maxForce;
		}
		else
		{
			//Calculate displacement direction from center
			Vector3 displacement = ourPos - pos;

			//As we get closer to the radius edge increase the intensity up to max
			float distancePercentage = Mathf.Clamp01((displacement.magnitude - radius) / fallOffDistance);
			float intensity = Mathf.Lerp(0.0f, maxForce, 1.0f - distancePercentage);

			//Return the negative displacement (so we move further away) multiplied by the intensity
			return -displacement.normalized * intensity;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = debugColor;
		Gizmos.DrawWireSphere(base.transform.position, radius);

		Gizmos.color *= 0.9f;
		Gizmos.DrawWireSphere(base.transform.position, radius + fallOffDistance);
	}
}
