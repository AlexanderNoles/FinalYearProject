using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedRenderer
{
	public Mesh mesh;
	public Material material;

	private Matrix4x4[] baseData;
	private Matrix4x4[] updatedData;

	public void SetInitalData(Matrix4x4[] array, bool initalUpdate = false)
	{
		baseData = array;
		updatedData = new Matrix4x4[baseData.Length];

		if (initalUpdate)
		{
			Update(new Matrix4x4());
		}
	}

	public void Update(Matrix4x4 perFrameTransform)
	{
		for (int i = 0; i < updatedData.Length; i++)
		{
			updatedData[i] = baseData[i] * perFrameTransform;
		}
	}

	public void Render()
	{
		RenderParams rp = new RenderParams(material);
		rp.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

		Graphics.RenderMeshInstanced(rp, mesh, 0, updatedData);
	}
}
