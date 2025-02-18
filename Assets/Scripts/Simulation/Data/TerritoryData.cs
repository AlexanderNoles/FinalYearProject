using EntityAndDataDescriptor;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryData : DataModule
{
    public RealSpacePosition origin = null;
	public bool forceClaimInital = false;
	public HashSet<RealSpacePosition> territoryCenters = new HashSet<RealSpacePosition>();
    public HashSet<RealSpacePosition> borders = new HashSet<RealSpacePosition>();
	public float growthRate;
	public float territoryClaimUpperLimit;
	public float hardTerritoryCountLimit = float.MaxValue;

	public override string Read()
	{
		return $"	Territory Count: {territoryCenters.Count}\n" +
			$"	Territory Origin: {origin}\n" +
			$"	Claim Upper Limit: {territoryClaimUpperLimit}\n" +
			$"	Growth Rate: {growthRate}";
	}

	public bool Contains(RealSpacePosition postion)
    {
        return territoryCenters.Contains(postion);
    }

	public void AddTerritory(RealSpacePosition center)
	{
		territoryCenters.Add(center);

		//Check if this territory is a previously owned territory
		//If so remove it from history data
		if (GameWorld.main.GetData(DataTags.Historical, out HistoryData historyData))
		{
			if (historyData.previouslyOwnedTerritories.ContainsKey(center))
			{
				historyData.previouslyOwnedTerritories.Remove(center);
			}
		}

		BorderCheck(center, true);
	}

	public void RemoveTerritory(RealSpacePosition center)
	{
		territoryCenters.Remove(center);

		if (borders.Contains(center))
		{
			borders.Remove(center);
		}

		BorderCheck(center, false);
	}

	private void BorderCheck(RealSpacePosition pos, bool hasCenter)
	{
		List<RealSpacePosition> toCheck = WorldManagement.GetNeighboursInGrid(pos);

		//If we have just removed this territory we don't bother checking whether it can be a border
		//if this check wasn't here kinda nothing would change but it feels cleaner this way
		if (hasCenter)
		{
			toCheck.Add(pos);
		}

		foreach (RealSpacePosition potentialBorder in toCheck)
		{
			//Couple of cases to account for:

			//First do we even own this territory...
			//...If we don't then we don't care
			if (territoryCenters.Contains(potentialBorder))
			{
				//Then we need to check if all neighbours for this are owned
				//If they are then we need to remove this from the border list
				//If they aren't then we need to add this
				//Assuming it isn't already removed or added in the first place

				bool allClaimed = true;
				List<RealSpacePosition> neighboursOfPotentialBorder = WorldManagement.GetNeighboursInGrid(potentialBorder);

				foreach (RealSpacePosition neighbourOfPB in neighboursOfPotentialBorder)
				{
					if (!territoryCenters.Contains(neighbourOfPB))
					{
						allClaimed = false;
						break; //Break check early
					}
				}

				if (!allClaimed)
				{
					//Should be a border
					if (!borders.Contains(potentialBorder))
					{
						borders.Add(potentialBorder);
					}
				}
				else
				{
					//Should not be a border
					//All already claimed
					if (borders.Contains(potentialBorder))
					{
						borders.Remove(potentialBorder);
					}
				}
			}
		}
	}


	// DRAWER METHOD FOR MAP //

	//First we need to pick a current valid start position
	//We will need to pick a starting position multiple times so this is part of the loop
	//If the starting position fails the degen check then all points will be automatically added as a seperate line
	//and the process will repeat
	//We only care about the already traveresed border positions during this start position check (so we don't keep traversing the same thing)
	//

	//First is the orthagonal vector (N, W, S, E) then their diagonal pair (the cell to check to see if we can traverse this direction), then their opposite diagonal
	//pair (the cell we are now traversing along)
	private static readonly (Vector3, Vector3, Vector3)[] CounterClockwiseOffsets = new (Vector3, Vector3, Vector3)[4]
	{
		(new Vector3(0, 0, 1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1)),
		(new Vector3(-1, 0, 0), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1)),
		(new Vector3(0, 0, -1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1)),
		(new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(1, 0, -1))
	};


	public List<List<Vector3>> CalculateMapBorderPositions(out Vector3 iconPosition, out Vector3 iconScale, float shiftModifier)
	{
		iconPosition = Vector3.zero;

		const int lengthLowerBound = 3;
		int currentAverageCount = 0;

		double halfDensity = WorldManagement.GetGridDensityHalf();
		double fullDensity = WorldManagement.GetGridDensity();

		List<List<Vector3>> output = new List<List<Vector3>>();

		HashSet<RealSpacePosition> alreadyVisited = new HashSet<RealSpacePosition>();

		int loopFailsafe = 0;
		while (alreadyVisited.Count != borders.Count && loopFailsafe < 100000)
		{
			//Establish start position
			RealSpacePosition startPos = null;

			foreach (RealSpacePosition pos in borders)
			{
				if (!alreadyVisited.Contains(pos))
				{
					//We've found our potential start position!
					startPos = pos;

					//Establish this is not a degenrate case
					//We do this here so we don't have to iterate through the whole set again
					if (IsAlone(startPos))
					{
						List<RealSpacePosition> points = GetPoints(startPos);
						List<Vector3> toAddToOutput = new List<Vector3>();

						foreach (RealSpacePosition point in points)
						{
							toAddToOutput.Add(GetMapPosition(point));
						}

						//Add to already visited so we don't add this again
						alreadyVisited.Add(pos);

						if (toAddToOutput.Count > lengthLowerBound)
						{
							//Add to output
							output.Add(ApplyModifier(toAddToOutput, shiftModifier, out Vector3 unused));
						}
					}
					else
					{
						break;
					}
				}
			}

			if (startPos != null)
			{
				//Main part of routine
				//We have established we have a valid start pos
				//No we need to find a valid point for this cell
				List<RealSpacePosition> startPosPoints = GetPoints(startPos);

				RealSpacePosition startPoint = null;

				//Perform a basic interior check on all the start pos points
				//This just means getting their points and using them
				//If any aren't "solid" we have our position
				foreach (RealSpacePosition point in startPosPoints)
				{
					List<RealSpacePosition> checkPoints = GetPoints(point);

					bool validPoint = false;
					foreach (RealSpacePosition checkPoint in checkPoints)
					{
						if (!borders.Contains(checkPoint))
						{
							validPoint = true;
							break;
						}
					}

					if (validPoint)
					{
						//We have found a valid start point!
						startPoint = point; 
						break;
					}
				}

				if (startPoint == null)
				{
					throw new Exception("No valid points from this border for map draw, this means it isn't a border cell and shouldn't be in the border set!");
				}

				//Set current
				RealSpacePosition currentCellPos = startPos;
				RealSpacePosition currentPoint = startPoint;

				//Instatiate new output
				List<Vector3> toAddToOutput = new List<Vector3>();

				int loopClamp = 10000;
				do
				{
					//Add current to output
					Vector3 outputPos = GetMapPosition(currentPoint);
					toAddToOutput.Add(outputPos);

					//If it is not already there add cell pos to closed
					if (!alreadyVisited.Contains(currentCellPos))
					{
						alreadyVisited.Add(currentCellPos);
					}

					//Find our next point
					//This means iterate through orthagonal directions until that direction passes a validation check
					//We iterate in a counter clockwise order so the points are ordered counter clockwise.

					foreach ((Vector3, Vector3, Vector3) direction in CounterClockwiseOffsets)
					{
						RealSpacePosition freeCheckPos = new RealSpacePosition(
							currentPoint.x + (direction.Item2.x * halfDensity),
							0,
							currentPoint.z + (direction.Item2.z * halfDensity));

						RealSpacePosition filledCheckPos = new RealSpacePosition(
							currentPoint.x + (direction.Item3.x * halfDensity),
							0,
							currentPoint.z + (direction.Item3.z * halfDensity));

						if (!territoryCenters.Contains(freeCheckPos) && territoryCenters.Contains(filledCheckPos))
						{
							//Validate the new cell to traverse along is not a singular cell and is not only connected by a corner
							if (!IsAlone(filledCheckPos) && !OnlyConnectedByCorner(filledCheckPos, currentCellPos, currentPoint))
							{
								//Setup new variables
								currentCellPos = filledCheckPos;
								currentPoint = new RealSpacePosition(
									currentPoint.x + (direction.Item1.x * fullDensity),
									0,
									currentPoint.z + (direction.Item1.z * fullDensity)
									);

								//We've found our valid position so we can stop and go onto the next iteration
								break;
							}
						}
					}

					loopClamp--;
				}
				while (!currentPoint.Equals(startPoint) && loopClamp > 0); //We run the loop once so we won't crash out immediately

				if (toAddToOutput.Count > lengthLowerBound)
				{
					//Add to output
					output.Add(ApplyModifier(toAddToOutput, shiftModifier, out Vector3 averagePos));

					//We want the largest area to have the icon over it so use raw count
					if (currentAverageCount < toAddToOutput.Count)
					{
						iconPosition = averagePos;
						currentAverageCount = toAddToOutput.Count;
					}

				}
			}
			else
			{
				//No more valid start positions, we are done
				break;
			}

			loopFailsafe++;
		}

		if (iconPosition == Vector3.zero)
		{
			iconScale = Vector3.zero;
		}
		else
		{
			iconScale = Vector3.one * 3;
		}

		return output;
	}

	private List<Vector3> ApplyModifier(List<Vector3> input, float shiftModifier, out Vector3 averagePos)
	{
		averagePos = Vector3.zero;
		List<Vector3> output = new List<Vector3>();

		//For each position in the input
		//Find that points in (d1) and out (d2) direction
		//If those directions are the same then doon't add to output
		//If the corner is convex use p, p - d1 and p + d2 to create a triangle
		//The center of that triangle is the new point
		//Do the same for concave corners but the new point should be flipped in p
		//This ultimately shrinks the border so it won't cause z fighting with other borders

		int listCount = input.Count;

		for (int i = 0; i < listCount; i++)
		{
			//Calculate directions
			//Need to use a specialized form of mod as typical c# mod doesn't work as expected with negative numbers
			Vector3 inDirection = input[i] - input[(((i - 1) % listCount) + listCount) % listCount];
			inDirection.Normalize();

			Vector3 outDirection = input[(i + 1) % listCount] - input[i];
			outDirection.Normalize();

			//Might need to give some leniency for floating point inaccuracy here
			if (inDirection.Equals(outDirection))
			{
				//Straight line
				//Don't add to output
				continue;
			}
			else
			{
				//Calculate the average of the three points, this is equivalent to finding the center point of the triangle
				Vector3 centerPos = (
					input[i] +
					(input[i] + (-inDirection * shiftModifier)) +
					(input[i] + (outDirection * shiftModifier))
					) / 3.0f;

				//Check if this point lies inside the territory
				//If it doesn't then we need to invert
				Vector3 finalPos = centerPos;

				if (!territoryCenters.Contains(WorldManagement.ClampPositionToGrid(ReverseMapPosition(finalPos))))
				{
					finalPos = input[i] - (finalPos - input[i]);
				}

				averagePos += finalPos;
				output.Add(finalPos);
			}
		}

		averagePos /= output.Count;
		return output;
	}
	
	private bool IsAlone(RealSpacePosition input)
	{
		List<RealSpacePosition> neighbours = WorldManagement.GetNeighboursInGrid(input);

		foreach (RealSpacePosition neighbour in neighbours)
		{
			if (territoryCenters.Contains(neighbour))
			{
				return false;
			}
		}

		return true;
	}

	private bool OnlyConnectedByCorner(RealSpacePosition posA, RealSpacePosition posB, RealSpacePosition cornerPos)
	{
		List<RealSpacePosition> cornersPoints = GetPoints(cornerPos);

		foreach (RealSpacePosition checkPos in cornersPoints)
		{
			if (territoryCenters.Contains(checkPos) && !checkPos.Equals(posA) && !checkPos.Equals(posB))
			{
				//Any other connecting cell
				return false;
			}
		}

		//If no other connecting cells
		double fullDistance = Math.Abs(posA.x - posB.x) + Math.Abs(posA.z - posB.z);
		if (fullDistance > WorldManagement.GetGridDensity() * 1.5f)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	private List<RealSpacePosition> GetPoints(RealSpacePosition input)
	{
		List<RealSpacePosition> toReturn = new List<RealSpacePosition>();

		foreach (Vector3 offset in GenerationUtility.diagonalOffsets)
		{
			toReturn.Add(
				new RealSpacePosition(
					input.x + (offset.x * WorldManagement.GetGridDensityHalf()), 
					0,
					input.z + (offset.z * WorldManagement.GetGridDensityHalf())));
		}

		return toReturn;
	}  

	private Vector3 GetMapPosition(RealSpacePosition input)
	{
		return -input.AsTruncatedVector3(MapManagement.mapRelativeScaleModifier);
	}

	private RealSpacePosition ReverseMapPosition(Vector3 input)
	{
		return new RealSpacePosition(
			-input.x * MapManagement.mapRelativeScaleModifier, 
			-input.y * MapManagement.mapRelativeScaleModifier, 
			-input.z * MapManagement.mapRelativeScaleModifier
			);
	}
}
