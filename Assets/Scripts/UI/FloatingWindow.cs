using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingWindow : MonoBehaviour
{
	[HideInInspector]
	public new GameObject gameObject
	{
		get
		{
			if (_gameObject == null)
			{
				_gameObject = base.gameObject;
			}

			return _gameObject;
		}
	}
	private GameObject _gameObject;

	[HideInInspector]
	public new Transform transform
	{
		get
		{
			if (_transform == null)
			{
				_transform = base.transform;
			}

			return _transform;
		}
	}
	private Transform _transform;

	public void MoveToFront()
	{
		transform.SetAsLastSibling();
	}
}
