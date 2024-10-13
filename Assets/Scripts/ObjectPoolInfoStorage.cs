using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolInfoStorage : MonoBehaviour
{
    [HideInInspector]
    public MultiObjectPool originPool;
    public MultiObjectPool.ObjectFromPool<MonoBehaviour> info;

    public bool automaticallyReturn = false;
    public float timeTillAutoReturn;

    private float startTime;

    private void OnEnable()
    {
        StopAllCoroutines();
        if (automaticallyReturn)
        {
            startTime = Time.time;
            StartCoroutine(nameof(AutoReturn));
        }
    }

    private IEnumerator AutoReturn()
    {
        //Always stay for one frame
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(timeTillAutoReturn);
        ReturnSelf();
    }

    public float TimeRemainingPercentage()
    {
        if (automaticallyReturn)
        {
            return Mathf.Clamp01((Time.time - startTime) / timeTillAutoReturn);
        }

        return 0;
    }

    public void ReturnSelf()
    {
        if (originPool == null)
        {
            return;
        }

        MultiObjectPool tempPool = originPool;
        originPool = null;

        tempPool.ReturnObject(info);
    }
}
