using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxDescribedBattleBehaviour : BattleBehaviour
{
	public Bounds targetBox;

	public Vector3 PointWithinBounds()
	{
		return new Vector3(
			Random.Range(-targetBox.extents.x, targetBox.extents.x),
			Random.Range(-targetBox.extents.y, targetBox.extents.y),
			Random.Range(-targetBox.extents.z, targetBox.extents.z)
			) + targetBox.center + transform.position;
	}

	protected override Vector3 GetTargetablePosition()
	{
		return PointWithinBounds();
	}

	protected override Vector3 GetFireFromPosition()
	{
		return PointWithinBounds();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;

		Gizmos.DrawWireCube(targetBox.center + GetComponent<Transform>().position, targetBox.size);
	}
}
