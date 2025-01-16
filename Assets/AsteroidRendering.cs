using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidRendering : MonoBehaviour
{
	public Material material;
	public Mesh mesh;

	GraphicsBuffer commandBuffer;
	GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
	const int commandCount = 1;

	private void Start()
	{
		commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
		commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
	}

	private void OnDestroy()
	{
		commandBuffer?.Release();
		commandBuffer = null;
	}

	private void Update()
	{
		//Copied from unity docs to use for learning
		RenderParams rp = new RenderParams(material);
		rp.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one); // use tighter bounds for better FOV culling
		rp.matProps = new MaterialPropertyBlock();
		rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(transform.position));
		commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
		commandData[0].instanceCount = 1000;
		commandBuffer.SetData(commandData);
		Graphics.RenderMeshIndirect(rp, mesh, commandBuffer, commandCount);
	}
}
