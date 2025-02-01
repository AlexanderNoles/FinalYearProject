using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryData : DataModule
{
	public class HistoryCell
	{

	}

	public Dictionary<RealSpacePosition, HistoryCell> previouslyOwnedTerritories = new Dictionary<RealSpacePosition, HistoryCell>();

	public class Period
    {
		public enum Type
		{
			None,
			Conflict,
			DominantPower
		}

		public Type type = Type.None;

        public int periodID;
		public int startTick;

		public string name;
		public Color color = Color.white;

		public int dominantPowerID = -1;
		public Vector2 originalPoliticalRep;
	}

	private List<Period> periods = new List<Period>();

	public Period GetCurrentPeriod()
	{
		return periods[^1];
	}

	public void AddPeriod(Period period)
	{
		period.periodID = periods.Count;
		period.startTick = SimulationManagement.currentTickID;
		periods.Add(period);
	}
}
