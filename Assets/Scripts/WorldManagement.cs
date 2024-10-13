using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak;

[IntializeAtRuntime]
public class WorldManagement : MonoBehaviour
{
    private const double debugMultiplier = 1000.0f;
    private const float debugSize = 2.0f;
    private static List<Vector3> debugPositions = new List<Vector3>();
    public static RealSpacePostion worldCenterPosition;   

    public static void SetWorldCenterPosition(RealSpacePostion initialPos)
    {
        worldCenterPosition = initialPos;
    }

    public static void MoveWorldCenter(Vector3 offset)
    {
        worldCenterPosition.Add(offset);
    }


#if UNITY_EDITOR
    private void Update()
    {
        debugPositions.Clear();
    }
#endif

    //OPERATIONS

    public static RealSpacePostion OffsetFromWorldCenter(RealSpacePostion position)
    {
#if UNITY_EDITOR
        debugPositions.Add(position.TruncatedVector3(debugMultiplier));
#endif
        RealSpacePostion toReturn = new RealSpacePostion(position);

        return toReturn.Subtract(worldCenterPosition);
    }

    private void OnDrawGizmos()
    {
        Vector3 playerPos = worldCenterPosition.TruncatedVector3(debugMultiplier);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerPos, debugSize/2);

        Gizmos.color = Color.yellow;
        foreach (Vector3 position in debugPositions)
        {
            Gizmos.DrawWireSphere(position, debugSize);
        }
    }
}

[System.Serializable]
public class RealSpacePostion
{
    //Utilize doubles for extra precision
    //This position cannot be directly applied to a transform (beyond a certain point) as the loss of precision creates obvious errors
    public double x, y, z;

    //CONSTRUCTORS
    public RealSpacePostion(RealSpacePostion rsp)
    {
        x = rsp.x;
        y = rsp.y;
        z = rsp.z;
    }

    public RealSpacePostion(double x, double y, double z)
    {
        this.x = x; 
        this.y = y; 
        this.z = z;
    }

    public RealSpacePostion(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    //OPERATIONS
    public void Add(Vector3 value)
    {
        x += value.x;
        y += value.y;
        z += value.z;
    }

    public void Subtract(Vector3 value)
    {
        x -= value.x;
        y -= value.y;
        z -= value.z;
    }

    public RealSpacePostion Subtract(RealSpacePostion value)
    {
        x -= value.x;
        y -= value.y;
        z -= value.z;

        return this;
    }

    //HELPER FUNCTIONS
    public double Magnitude()
    {
        double value = (x * x) + (y * y) + (z * z);
        return System.Math.Sqrt(value);
    }

    public Vector3 AsVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public RealSpacePostion Normalized()
    {
        double mag = Magnitude();
        return new RealSpacePostion(x / mag, y / mag, z / mag);
    }

    public Vector3 TruncatedVector3(double modifier)
    {
        return new Vector3((float)(x/modifier), (float)(y / modifier), (float)(z / modifier));
    }
}
