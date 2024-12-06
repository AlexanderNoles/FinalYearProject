using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(0, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Absent, "BorderInOrder")]
public class BorderInOrderRoutine : RoutineBase
{
    public override void Run()
    {
        List<Faction> territories = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Territory);

        foreach (Faction faction in territories)
        {
            if (faction.GetData(Faction.Tags.Territory, out TerritoryData data))
            {
                //Create a start node
                RealSpacePostion startPos = null;
                foreach (RealSpacePostion pos in data.borders)
                {
                    startPos = pos;
                    break;
                }

                MapNode startNode = new MapNode(startPos, data);
                startNode.neighbourCountLimit = 1; //Limit the startnode's neighbour cause otherwise the whole border will always at least be traversed twice

                data.borderInOrder = CustomLongestPath.GetCircularPath(startNode, data.borders.Count, 0);
            }
        }
    }
}

public class MapNode : INode
{
    public TerritoryData parentData;
    public RealSpacePostion actualPos;
    private readonly float modifier = UIManagement.mapRelativeScaleModifier;
    public float neighbourCountLimit = -1;

    public List<INode> GetNeighbours()
    {
        List<INode> toReturn = new List<INode>();
        foreach (Vector3 offset in GenerationUtility.omniDirectionalOffsets)
        {
            RealSpacePostion pos = new RealSpacePostion(
                actualPos.x + (offset.x * WorldManagement.GetGridDensity()),
                actualPos.y,
                actualPos.z + (offset.z * WorldManagement.GetGridDensity())
                );

            if (parentData.borders.Contains(pos))
            {
                MapNode newNode = new MapNode(pos, parentData);
                toReturn.Add(newNode);

                if (toReturn.Count >= neighbourCountLimit && neighbourCountLimit != -1)
                {
                    //Return early if neighbour count limit has been met
                    return toReturn;
                }
            }
        }

        return toReturn;
    }

    public Vector3 GetPosition()
    {
        return -actualPos.AsTruncatedVector3(modifier);
    }

    public MapNode(RealSpacePostion pos, TerritoryData parentData)
    {
        actualPos = pos;
        this.parentData = parentData;
    }
}
