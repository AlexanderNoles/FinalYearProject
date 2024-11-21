using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic pathfinding namespace, project agnostic. Utilizes an interface to be intergrated with existing code. Currently only implements AStar.
/// </summary>
namespace Pathfinding
{
    /// <summary>
    /// Interface implemented by classes that the pathfinding will treat as nodes.
    /// </summary>
    public interface INode
    {
        public Vector3 GetPosition();
        public List<INode> GetNeighbours();
    }

    /// <summary>
    /// Static class implementation of AStar Pathfinding. Main access point is FindPath.
    /// </summary>
    public static class AStar
    {
        //implements A* path finding to find a valid path from one node to another
        private class AStarNode
        {
            public INode node;
            public AStarNode previousNode;
            public float g, h, f;

            public AStarNode(INode node, AStarNode previousNode, float g, INode endNode)
            {
                this.node = node;
                this.g = g;
                h = Distance(node, endNode);
                f = h + g;

                this.previousNode = previousNode;
            }
        }

        private static float Distance(INode a, INode b)
        {
            return Mathf.Abs(a.GetPosition().x - b.GetPosition().x) + Mathf.Abs(a.GetPosition().z - b.GetPosition().z);
        }

        public static List<INode> FindPath(INode startNode, INode endNode, bool circular)
        {
            List<INode> toReturn = new List<INode>();

            {
                //Reverse the end and start nodes so we don't have to reverse the final path when we move back from the end
                INode temp = startNode;
                startNode = endNode;
                endNode = temp;
            }
                List<AStarNode> closed = new List<AStarNode>();
                List<AStarNode> frontier = new List<AStarNode>(){
                new AStarNode(startNode, null, 0, endNode)
                };
            

            while (frontier.Count > 0)
            {
                AStarNode current = GetNextNode(frontier);

                if (current.node.GetPosition() == endNode.GetPosition()) 
                {
                    //We have found our path, we can just get a reverse path
                    while (current != null)
                    {
                        toReturn.Add(current.node);
                        current = current.previousNode;
                    }

                    break;
                }

                frontier.Remove(current);
                closed.Add(current);

                foreach (INode neighbour in current.node.GetNeighbours())
                {
                    //Add all the neighbours to the frontier
                    if (Contains(closed, neighbour))
                    {
                        continue;
                    }

                    AStarNode newNode = new AStarNode(
                        neighbour,
                        current,
                        current.g + Distance(current.node, neighbour),
                        endNode);

                    if (Contains(frontier, neighbour, out AStarNode outNode))
                    {
                        if (newNode.g > outNode.g)
                        {
                            continue;
                        }
                    }
                    if(!(circular && closed.Count == 1 && newNode.node.GetPosition() == endNode.GetPosition()))
                    {
                        frontier.Add(newNode);
                    }
                }
            }

            return toReturn;
        }

        private static AStarNode GetNextNode(List<AStarNode> nodes)
        {
            AStarNode toReturn = nodes[0];

            foreach (AStarNode node in nodes)
            {
                if (toReturn.f > node.f)
                {
                    toReturn = node;
                }
            }

            return toReturn;
        }

        private static bool Contains(List<AStarNode> nodes, INode checkNode, out AStarNode outNode)
        {
            outNode = null;

            foreach (AStarNode node in nodes)
            {
                if (node.node.GetPosition() == checkNode.GetPosition())
                {
                    outNode = node;
                    return true;
                }
            }

            return false;
        }

        private static bool Contains(List<AStarNode> nodes, INode checkNode)
        {
            foreach (AStarNode node in nodes)
            {
                if (node.node.GetPosition() == checkNode.GetPosition())
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class CustomLongestPath
    {
        public class LongestPathNode
        {
            public INode baseNode;
            //Used to form the final path by traversing backwards
            public LongestPathNode previousNode;

            public LongestPathNode(INode baseNode, LongestPathNode previousNode)
            {
                this.baseNode = baseNode;
                this.previousNode = previousNode;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                LongestPathNode node = (LongestPathNode)obj;

                //Do the positions match?
                return node.baseNode.GetPosition() == baseNode.GetPosition();
            }

            public override int GetHashCode()
            {
                return baseNode.GetPosition().GetHashCode();
            }
        }

        public class LongestPathStrand
        {
            public LongestPathStrand parent = null;
            public LongestPathNode origin;
            public HashSet<LongestPathNode> closed = new HashSet<LongestPathNode>();
            public LongestPathNode frontier = null;

            public bool split = false;

            public LongestPathStrand(LongestPathNode strandOrigin, LongestPathStrand parent)
            {
                this.parent = parent;
                origin = strandOrigin;
                frontier = strandOrigin;
            }

            public bool Contains(LongestPathNode node)
            {
                //Check all parents to see if they contain the node as well=
                if (closed.Contains(node))
                {
                    return true;
                }

                if (parent == null)
                {
                    return false;
                }

                return parent.Contains(node);
            }

            public int TotalClosedLength()
            {
                if (parent == null)
                {
                    return closed.Count;
                }

                return closed.Count + parent.TotalClosedLength();
            }
        }

        public static List<Vector3> GetCircularPath(INode startNode, int desiredLength, float leniency = 5, float leniencyIncrease = 5)
        {
            //Create the base strand
            LongestPathNode actualStartNode = new LongestPathNode(startNode, null);
            List<LongestPathStrand> currentStrands = new List<LongestPathStrand>
            {
                new LongestPathStrand(actualStartNode, null)
            };

            //Algorithim specification:
            //We begin an initial strand from a given start node
            //We then begin to traverse to the next avaliable neighbour
            //  If there is only one option (most cases) we simply add that to the strand and move on (akin any normal pathfinding algorithm)
            //  If there are multiple options we open a strand for each, we need to now resolve these strands
            //          We want to move through all the strands until a couple things happen
            //          First, we want to stop a strand if it meets the startnode and does not include enough nodes to meet the desired length
            //          Realizing now this is the only thing we need, we just need a strand that reaches the desired length, nothing else!

            while (currentStrands.Count > 0)
            {
                //Iterate through all current strands
                int splitCount = 0;
                for (int i = 0; i < currentStrands.Count;)
                {
                    //Get strand
                    LongestPathStrand currentStrand = currentStrands[i];

                    if (currentStrand.split)
                    {
                        //If this strand has been split we don't want to move it forward
                        i++;
                        splitCount++;
                        continue;
                    }

                    //Get neighbours of current frontier node
                    //This is a single node and not a list because if there is multiple options we create new strands
                    List<INode> rawNeighbours = currentStrand.frontier.baseNode.GetNeighbours();
                    List<LongestPathNode> prunedNeighbours = new List<LongestPathNode>();

                    //Prune neighbours of already enncountered nodes
                    for (int j = 0; j < rawNeighbours.Count; j++)
                    {
                        LongestPathNode newNode = new LongestPathNode(rawNeighbours[j], currentStrand.frontier);

                        if (!currentStrand.Contains(newNode))
                        {
                            //Valid neighbour
                            prunedNeighbours.Add(newNode);
                        }
                    }

                    //If no neighbours, we might have a completed path!
                    if (prunedNeighbours.Count == 0)
                    {
                        int totalLength = currentStrand.TotalClosedLength() + 1; //+1 for the current frontier as it is not added to the closed sets
                        bool returningResult = totalLength >= desiredLength - leniency;

                        //Check if this path is considered long enough
                        if (returningResult)
                        {
                            //We have found a long enough path!
                            //Backwards construct the path!

                            //While backwards constructing the path we check to see if it crosses over itself anywhere
                            //If so break and remove this strand instead
                            List<Vector3> finalPath = new List<Vector3>();
                            LongestPathNode currentNode = currentStrand.frontier;

                            try
                            {
                                do
                                {
                                    finalPath.Add(currentNode.baseNode.GetPosition());

                                    //Do self intersecting check
                                    if (GenerationUtility.IsShapeSelfIntersecting(finalPath, 10))
                                    {
                                        returningResult = false;
                                        break;
                                    }
                                    else
                                    {
                                        currentNode = currentNode.previousNode;
                                    }
                                }
                                while (currentNode.previousNode != null);
                            }
                            catch (NullReferenceException)
                            {
                                return null;
                            }
                           

                            if (returningResult)
                            {
                                return finalPath;
                            }
                        }

                        if (!returningResult)
                        {
                            //This strand has failed!
                            //Remove at i
                            //Because we don't auto increment i this mean i will now point to the next strand!

                            currentStrands.RemoveAt(i);
                        }
                    }
                    else
                    {
                        //Add the frontier to the closed set
                        currentStrand.closed.Add(currentStrand.frontier);

                        if (prunedNeighbours.Count == 1)
                        {
                            //If only one neighbour we simply set the frontier to now be that neighbour
                            currentStrand.frontier = prunedNeighbours[0];
                        }
                        else
                        {
                            //We have more than one neighbour! We need to create more strands
                            //First say this strand has been split so we don't follow it anymore
                            currentStrand.split = true;

                            foreach (LongestPathNode neighbour in prunedNeighbours)
                            {
                                LongestPathStrand newStrand = new LongestPathStrand(neighbour, currentStrand);
                                currentStrands.Add(newStrand);
                            }
                        }

                        i++;
                    }
                }

                if (splitCount == currentStrands.Count)
                {
                    //No more active strands
                    //Run again with a lower leniency

                    return GetCircularPath(startNode, desiredLength, leniency + leniencyIncrease, leniencyIncrease);
                }
            }

            return null;
        }
    }
}
