using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MultiObjectPool : MonoBehaviour
{
    [Tooltip("Automatically Generate Objects On Awake")]
    public bool generateOnAwake = false;
    private Transform selfTransform;
	private bool initSetupRun = false;

    private void Awake()
    {
		if (initSetupRun)
		{
			return;
		}

		InitSetup();
    }

	public void ForceSetup()
	{
		if (!initSetupRun)
		{
			initSetupRun = true;
			InitSetup();
		}
	}

	private void InitSetup()
	{
		selfTransform = transform;
		foreach (ObjectPool pool in pools)
		{
			pool.Setup(selfTransform, generateOnAwake);
		}
	}

    public void AddNewPools(List<int> numbers, List<GameObject> baseObjects)
    {
        for (int i = 0; i < numbers.Count; i++)
        {
            AddNewPool(numbers[i], baseObjects[i]);
        }
    }

    public int AddNewPool(int numberOfObjects, GameObject baseObject, string name = "")
    {
        if (selfTransform == null)
        {
            throw new System.Exception("Function run too early! Run in Start instead.");
        }

        int currentNumberOfPools = pools.Count;
        if (name == "")
        {
            name = currentNumberOfPools.ToString();
        }

        pools.Add(new ObjectPool(numberOfObjects, baseObject, name, selfTransform));

        return currentNumberOfPools;
    }

    [System.Serializable]
    public class ObjectPool
    {
        public enum Mode
        {
            RemoveAndReturn,
            UpdateNoRemove
        }

        [Header("Pool Settings")]
        public string name;
        public Mode mode = Mode.RemoveAndReturn;
        public GameObject baseObject;
        public int baseNumberOfObjects;
        [Header("Object Settings")]
        [Tooltip("Generate extra objects when the pool is empty but an object is required")]
        public bool generateAsNeeded = false;

        [HideInInspector]
        public Stack<Transform> activeObjectsStack = new Stack<Transform>();

        public struct UpdateObject
        {
            public Transform transform;
            private GameObject gameObject;

            public UpdateObject(Transform transform)
            {
                this.transform = transform;
                gameObject = transform.gameObject;
            }

            public void Update(Vector3 position)
            {
                gameObject.SetActive(true);
                transform.position = position;
            }

            public void Hide()
            {
                gameObject.SetActive(false);
            }
        }

        [HideInInspector]
        public List<UpdateObject> activeObjectsList = new List<UpdateObject>();
        [HideInInspector]
        public int currentUpdatedObjectPointer;

        private Transform actualTransform;
        public bool automaticallySetParent = true;

        public ObjectPool(int baseNumberOfObjects, GameObject baseObject, string name, Transform actualTransform)
        {
            this.baseNumberOfObjects = baseNumberOfObjects;
            this.baseObject = baseObject;
            this.name = name;

            Setup(actualTransform, true);
        }

        public void Setup(Transform actualTransform, bool generateObjects)
        {
            this.actualTransform = actualTransform;

            if (name == "")
            {
                throw new System.Exception("This pool doesn't have a name!");
            }

            if (!generateObjects)
            {
                return;
            }

            activeObjectsStack = new Stack<Transform>();
            activeObjectsList = new List<UpdateObject>();

            for (int i = 0; i < baseNumberOfObjects; i++)
            {
                GenerateObject();
            }
        }

        public Transform GenerateObject()
        {
            GameObject currentObject = Instantiate(baseObject, actualTransform);
            currentObject.SetActive(false);

            Transform transform = currentObject.transform;

            if (mode == Mode.RemoveAndReturn)
            {
                activeObjectsStack.Push(transform);
            }
            else
            {
                activeObjectsList.Add(new UpdateObject(transform));
            }

            return transform;
        }

        public Transform GetObject()
        {
            if (mode != Mode.RemoveAndReturn)
            {
                throw new Exception("No object removal allowed!");
            }

            if (activeObjectsStack.Count == 0)
            {
                if (generateAsNeeded)
                {
                    GenerateObject();
                }
                else
                {
                    return null;
                }
            }

            return activeObjectsStack.Pop();
        }

        public void ReturnObject(Transform _object)
        {
            if (mode != Mode.RemoveAndReturn)
            {
                throw new Exception("No object return allowed!");
            }

            if (automaticallySetParent)
            {
                _object.SetParent(actualTransform);
            }
            _object.gameObject.SetActive(false);

            activeObjectsStack.Push(_object);
        }

        public Transform UpdateNextObjectPosition(Vector3 position)
        {
            if (mode != Mode.UpdateNoRemove)
            {
                throw new Exception("No object update allowed! Remove object from pool and update it instead!");
            }

            if (currentUpdatedObjectPointer == activeObjectsList.Count)
            {
                if (generateAsNeeded)
                {
                    GenerateObject();
                }
                else
                {
                    //Do nothing
                    return null;
                }
            }

            //Update object
            activeObjectsList[currentUpdatedObjectPointer].Update(position);
            currentUpdatedObjectPointer++;

            return activeObjectsList[currentUpdatedObjectPointer - 1].transform;
        }

		public void HideAll()
		{
			for (int i = 0; i < activeObjectsList.Count; i++)
			{
				activeObjectsList[i].Hide();
			}

			//Reset pointer
			currentUpdatedObjectPointer = 0;
		}

        public void PruneObjectsNotUpdatedThisFrame()
        {
            if (currentUpdatedObjectPointer <= 0)
            {
                return;
            }

            //Hide all objects not updated this frame
            for (int i = currentUpdatedObjectPointer; i < activeObjectsList.Count; i++)
            {
                activeObjectsList[i].Hide();
            }

            //Reset pointer
            currentUpdatedObjectPointer = 0;
        }

        public Dictionary<Transform, T> GetComponentsOnActiveObjects<T>()
        {
            Dictionary<Transform, T> toReturn = new Dictionary<Transform, T>();

            if (mode == Mode.RemoveAndReturn)
            {
                Transform[] allObjects = GetActiveObjectsAsArray(); 
                foreach (Transform _obj in allObjects)
                {
                    toReturn.Add(_obj, _obj.GetComponent<T>());
                }
            }
            else
            {
                foreach (UpdateObject _obj in activeObjectsList)
                {
                    toReturn.Add(_obj.transform, _obj.transform.GetComponent<T>());
                }
            }

            return toReturn;
        }

        public Transform[] GetActiveObjectsAsArray()
        {
            return activeObjectsStack.ToArray();
        }
    }

    public List<ObjectPool> pools = new List<ObjectPool>();

    public int GetPoolIndex(string poolName)
    {
        int numOfPools = pools.Count;

        for (int i = 0; i < numOfPools; i++)
        {
            if (pools[i].name == poolName)
            {
                return i;
            }
        }

        throw new System.Exception("No pool with that name in multi pool");
    }

    public ObjectFromPool<MonoBehaviour> GetObject(string poolName)
    {
        return GetObject<MonoBehaviour>(poolName);
    }

    public ObjectFromPool<T> GetObject<T>(string poolName)
    {
        return GetObject<T>(GetPoolIndex(poolName));
    }

    public struct ObjectFromPool<T>
    {
        public int poolIndex;
        public Transform transform;
        public T component;

        public ObjectFromPool(int poolIndex, Transform transform, T component)
        {
            this.poolIndex = poolIndex;
            this.transform = transform;
            this.component = component;
        }
    }

    public ObjectFromPool<MonoBehaviour> GetObject(int poolIndex)
    {
        return GetObject<MonoBehaviour>(poolIndex);
    }

    public ObjectFromPool<T> GetObject<T>(int poolIndex)
    {
        Transform newObject = pools[poolIndex].GetObject();
        if (newObject == null)
        {
            throw new System.Exception("No Objects left in pool!");
        }
        else
        {
            newObject.TryGetComponent(out T component);

            return new ObjectFromPool<T>(poolIndex, newObject, component);
        }
    }

    public Transform UpdateNextObjectPosition(int poolIndex, Vector3 position)
    {
        return pools[poolIndex].UpdateNextObjectPosition(position);
    }

    public void PruneObjectsNotUpdatedThisFrame(int poolIndex)
    {
        pools[poolIndex].PruneObjectsNotUpdatedThisFrame();
    }

	public void HideAllObjects(int poolIndex)
	{
		pools[poolIndex].HideAll();
	}

	/// <summary>
	/// Unlike GetObject this will automatically update the information on the object and set it active
	/// </summary>
	public ObjectFromPool<T> SpawnObject<T>(int poolIndex, Vector3 position)
    {
        //Return a blank if there is no object
        ObjectFromPool<T> toReturn = new ObjectFromPool<T>();

        Transform newObject = pools[poolIndex].GetObject();
        if (newObject != null)
        {
            newObject.position = position;
            newObject.gameObject.SetActive(true);

            //Setup toReturn
            toReturn.poolIndex = poolIndex;
            toReturn.transform = newObject;

            if (typeof(T) != typeof(MonoBehaviour))
            {
                newObject.TryGetComponent(out T component);
                toReturn.component = component;
            }

            try
            {
                ObjectPoolInfoStorage objectPoolInfoStorage = newObject.GetComponent<ObjectPoolInfoStorage>();
                objectPoolInfoStorage.info = new ObjectFromPool<MonoBehaviour>(poolIndex, newObject, null);
                objectPoolInfoStorage.originPool = this;
            }
            catch (System.NullReferenceException)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"WARNING: There is no {nameof(ObjectPoolInfoStorage)} attached to the new attack. This means it cannot return itself to the object pool!");
#endif
            }
        }

        return toReturn;
    }

    public ObjectFromPool<T> SpawnObject<T>(Enum enumEntry, Vector3 position)
    {
        return SpawnObject<T>(Convert.ToInt32(enumEntry), position);
    }

    public ObjectFromPool<MonoBehaviour> SpawnObject(int poolIndex, Vector3 position)
    {
        return SpawnObject<MonoBehaviour>(poolIndex, position);
    }

    public ObjectFromPool<MonoBehaviour> SpawnObject(Enum enumEntry, Vector3 position)
    {
        return SpawnObject<MonoBehaviour>(enumEntry, position);
    }

    public void ReturnObject(string poolName, Transform _object)
    {
        ReturnObject(GetPoolIndex(poolName), _object);
    }

    public void ReturnObject(ObjectFromPool<MonoBehaviour> objectFromPool)
    {
        pools[objectFromPool.poolIndex].ReturnObject(objectFromPool.transform);
    }

    public void ReturnObject(int poolIndex, Transform _object)
    {
        pools[poolIndex].ReturnObject(_object);
    }

    public void IncreasePoolCapacity(string poolName, int amount = 1)
    {
        IncreasePoolCapacity(GetPoolIndex(poolName), amount);
    }

    public void IncreasePoolCapacity(int poolIndex, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            pools[poolIndex].GenerateObject();
        }
    }

    public Dictionary<Transform, T> GetComponentsOnAllActiveObjects<T>(int poolIndex)
    {
        return pools[poolIndex].GetComponentsOnActiveObjects<T>();
    }
}
