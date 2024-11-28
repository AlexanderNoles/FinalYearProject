using MonitorBreak;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class SimulationManagement : MonoBehaviour
{
    private static float simulatioSpeedModifier = 1.0f;

    public static float GetSimulationSpeed()
    {
        return simulatioSpeedModifier;
    }

    private static int simulationSeed;
    public static System.Random random;

    private static float tickStartTime;

    public static float GetCurrentSimulationTime()
    {
        return tickStartTime;
    }

    private float nextTickTime;
    private const float TICK_MAX_LENGTH = 3;
    private float tickInitFrame;
    private float minimumFrameLength = 0;

    public static float GetSimulationDaysAsTime(int dayNumber)
    {
        return dayNumber * TICK_MAX_LENGTH;
    }

    private static float currentDay;
    private const float DAY_TO_MONTH = 30;
    private static float currentMonth;
    private const float MONTH_TO_YEAR = 13;
    private static float currentYear;

    private static void IncrementDay()
    {
        currentDay++;

        if (currentDay > DAY_TO_MONTH)
        {
            currentDay = 1;
            currentMonth++;
        }

        if (currentMonth > MONTH_TO_YEAR)
        {
            currentMonth = 1;
            currentYear++;
        }
    }

    public static string GetDateString()
    {
        return $"{currentDay}/{currentMonth}/{currentYear}";
    }

    private static SimulationManagement instance;
    private Task tickTask;
    [HideInInspector]
    public List<RoutineBase> constantRoutines = new List<RoutineBase>();
    [HideInInspector]
    public List<InitRoutineBase> initRoutines = new List<InitRoutineBase>();
    [HideInInspector]
    public List<RoutineBase> debugRoutines = new List<RoutineBase>();
    [HideInInspector]
    public Dictionary<string, RoutineBase> absentRoutines = new Dictionary<string, RoutineBase>();
	public const int attackRoutineStandardPrio = -25; 
	public const int defendRoutineStandardPrio = -30;

	public static void RunAbsentRoutine(string routineIdentifier)
    {
        if (instance.absentRoutines.ContainsKey(routineIdentifier))
        {
            instance.absentRoutines[routineIdentifier].Run();
        }
    }

    private Dictionary<int, Faction> idToFaction = new Dictionary<int, Faction>();
    private Dictionary<Faction.Tags, List<Faction>> factions = new Dictionary<Faction.Tags, List<Faction>>();
    private Dictionary<Faction.Tags, List<Faction>> updatedTags = new Dictionary<Faction.Tags, List<Faction>>();

    public static Faction GetFactionByID(int id)
    {
        if (instance.idToFaction.ContainsKey(id))
        {
            return instance.idToFaction[id];
        }

        return null;
    }

	public static void AddFactionToIDDict(Faction faction)
	{
		instance.idToFaction.Add(faction.id, faction);
	}

    public static void AddFactionOfTag(Faction.Tags tag, Faction faction)
    {
        if (!instance.factions.ContainsKey(tag))
        {
            //Init sub list if it doesn't exist
            instance.factions.Add(tag, new List<Faction>());
        }

        if (!instance.factions[tag].Contains(faction))
        {
            //Add faction to tag if it has not already been added
            instance.factions[tag].Add(faction);

            //Add to updated tags so init functions can run
            if (!instance.updatedTags.ContainsKey(tag))
            {
                instance.updatedTags.Add(tag, new List<Faction>());
            }

            if (!instance.updatedTags[tag].Contains(faction))
            {
                instance.updatedTags[tag].Add(faction);
            }
        }
    }

    public static void RemoveFactionOfTag(Faction.Tags tag, Faction faction)
    {
        if (instance.factions.ContainsKey(tag))
        {
            //Remove faction if faction tag exists and faction belongs to that tag
            instance.factions[tag].Remove(faction);
        }
    }

    public static void RemoveFactionFully(Faction faction)
    {
        foreach (Faction.Tags tag in Enum.GetValues(typeof(Faction.Tags)))
        {
            RemoveFactionOfTag(tag, faction);
        }

        instance.idToFaction.Remove(faction.id);
    }

    public static List<Faction> GetAllFactionsWithTag(Faction.Tags tag)
    {
        if (instance.factions.ContainsKey(tag))
        {
            return instance.factions[tag];
        }

        //Return empty list by default
        return new List<Faction>();
    }

	public static Dictionary<int, T> GetDataForFactionsList<T> (List<Faction> factions, string identifier) where T : DataBase
	{
		Dictionary<int, T> idToData = new Dictionary<int, T>();

		foreach (Faction faction in factions)
		{
			if (faction.GetData(identifier, out T data))
			{
				idToData.Add(faction.id, data);
			}
		}

		return idToData;
	}

	public static bool CellIsLazy(RealSpacePostion cellCenter)
	{
		//A cell is lazy if the player does not exist there
		//Typically most things in the simulation are lazy as the simulation's processing does not
		//directly take into account the player
		//This does not mean the player cannot interact with lazy systems just that the lazy systems can process without
		//taking into account player input
		//Some systems are optionally/mostly lazy but becoming active when a player "arrives".
		//The key example is the current battle system (28/11/2024) that is lazy until a player arrives in a battle
		//that battle is then no longer controlled by the simulation and instead by the normal game loop.

		return !PlayerLocationManagement.IsPlayerLocation(cellCenter);
	}

	public static bool LocationIsLazy(VisitableLocation location)
	{
		//See above function "CellIsLazy"

		return !PlayerLocationManagement.IsPlayerLocation(location);
	}


    //As per the inital design constriction this script always executes after every other (non unity) script.
    //This does not mean it is the final code executed in the frame, we have no control over the execution order outside of scripts
    private void Awake()
    {
        //Reset some stuff
        Planet.availablePlanetPositions.Clear();

        currentDay = 15;
        currentMonth = 3;
        currentYear = 3004;
        //

        simulationSeed = UnityEngine.Random.Range(-100000, 100000);
        random = new System.Random(simulationSeed);

        instance = this;

        const int testCount = 4;
        for (int i = 0; i < testCount; i++)
        {
            //Add test factions
            new Nation().Simulate();
        }

		//Add game world faction
		new GameWorld().Simulate();

        //Add all routine instances
        (constantRoutines, initRoutines, debugRoutines, absentRoutines) = SimulationRoutineExecution.Main(gameObject);
    }

    private void Start()
    {
        if (SimulationSettings.ShouldRunHistory())
        {
            //Run history ticks
            //Simulation is run for a period of years before player arrives to get more dynamic results        //It is important this is run in Start so OnEnable can run on objects before this goes off
            int tickCount = (int)(DAY_TO_MONTH * MONTH_TO_YEAR) * SimulationSettings.HistoryLength();

            for (int i = 0; i < tickCount; i++)
            {
                InitSimulationTick(true);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ActiveSimulationRoutine : Attribute 
    {
        public int priority;

        public enum RoutineTypes
        {
            Normal,
            Init,
            Absent,
            Debug
        }

        public RoutineTypes routineType;
        public string identifier;

        /// <summary>
        /// Construct Active Simulation Routine
        /// </summary>
        /// <param name="priority">Routines Priority, higher priority means it is run first each tick. In range -10000 to 10000</param>
        /// <param name="initRoutine">Should this routine only be run once (on the first tick the faction is created)?</param>
        public ActiveSimulationRoutine(int priority, RoutineTypes routineType = RoutineTypes.Normal, string identifier = "")
        {
            this.priority = Mathf.Clamp(priority, -10000, 10000);
            this.routineType = routineType;
            this.identifier = identifier;
        }
    }

    public class SimulationRoutineExecution : MonoBehaviour
    {
        public static (List<RoutineBase>, List<InitRoutineBase>, List<RoutineBase>, Dictionary<string, RoutineBase>) Main(GameObject parent)
        {
            List<RoutineBase> activeRoutines = new List<RoutineBase>();
            List<RoutineBase> debugRoutines = new List<RoutineBase>();
            Dictionary<string, RoutineBase> absentRoutines = new Dictionary<string, RoutineBase>();
            List<InitRoutineBase> initRoutines = new List<InitRoutineBase>();
            List<(ActiveSimulationRoutine, Type)> rountineClasses = new List<(ActiveSimulationRoutine, Type)>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes().Where(x => typeof(MonoBehaviour).IsAssignableFrom(x) && x != typeof(MonoBehaviour)))
                {
                    ActiveSimulationRoutine routine = (ActiveSimulationRoutine)type.GetCustomAttribute(typeof(ActiveSimulationRoutine), false);
                    if (routine != null)
                    {
                        //Add to routine classes based on priority
                        int routinePriority = routine.priority;

                        bool added = false;
                        for (int i = 0; i < rountineClasses.Count; i++)
                        {
                            if (routinePriority > rountineClasses[i].Item1.priority)
                            {
                                rountineClasses.Insert(i, (routine, type));
                                added = true;
                                break;
                            }
                        }

                        if (!added)
                        {
                            //Add to end of list
                            //Will occur if lowest pri or list is empty (which still means it is lowest pri)
                            rountineClasses.Add((routine, type));
                        }
                    }
                }
            }

            foreach ((ActiveSimulationRoutine, Type) type in rountineClasses)
            {
                //Because the types where sorted based on priority they are displayed in priority order on the target gameobject
                Component newRoutine = parent.AddComponent(type.Item2);
                if (type.Item1.routineType == ActiveSimulationRoutine.RoutineTypes.Init)
                {
                    initRoutines.Add(newRoutine as InitRoutineBase);
                }
                else if (type.Item1.routineType == ActiveSimulationRoutine.RoutineTypes.Debug)
                {
                    debugRoutines.Add(newRoutine as RoutineBase);
                }
                else if (type.Item1.routineType == ActiveSimulationRoutine.RoutineTypes.Absent)
                {
                    if (absentRoutines.ContainsKey(type.Item1.identifier))
                    {
                        Debug.LogError("More than one absent routine shares name: " + type.Item1.identifier);
                        continue;
                    }


                    absentRoutines.Add(type.Item1.identifier, newRoutine as RoutineBase);
                }
                else
                {
                    activeRoutines.Add(newRoutine as RoutineBase);
                }
            }

            return (activeRoutines, initRoutines, debugRoutines, absentRoutines);
        }
    }

    //Multithread control
    public static void InitSimulationTick(bool isInstant)
    {
        if (instance != null) 
        {
            tickStartTime = Time.time;
            instance.tickInitFrame = Time.frameCount;

            if (simulatioSpeedModifier < 0)
            {
                instance.minimumFrameLength = 0;
            }
            else
            {
                instance.minimumFrameLength = Time.captureFramerate / (2.0f / simulatioSpeedModifier);
            }

            IncrementDay();

            instance.tickTask = Task.Run(() =>
            {
                instance.SimulationTick(isInstant);
            });

            if (isInstant)
            {
                instance.tickTask.Wait();
            }
        }
    }

    public static void EndSimulationTick()
    {
        if (instance != null)
        {
            if (instance.tickTask != null)
            {
                instance.tickTask.Wait();
                instance.tickTask = null;
            }
        }
    }

    //Simulation Tick
    private void SimulationTick(bool isInstant)
    {
        //Run this before other routines so we can update a faction on the same tick it is initialized
        //This does mean new factions created this tick will have to wait till next tick to be initilized
        //No good generic check exists that can tell if a faction has been initlized for a given tag or not (though non-generic ones do exist (e.g., a non-initlized Nation will always occupy no spaces))
        //This means routines that create new Factions should be always run after routines that would alter those factions data, or we will run into unexpected behaviour
        if (updatedTags.Count > 0)
        {
            HashSet<Faction.Tags> tags = updatedTags.Keys.ToHashSet();

            foreach (InitRoutineBase routine in initRoutines)
            {
                if (routine.TagsUpdatedCheck(tags))
                {
                    routine.Run();
                }
            }

            updatedTags.Clear();
        }

        //We run each rountine on each faction rather than each faction on every routine so later routines can react to other factions previous routines
        foreach (RoutineBase routine in constantRoutines)
        {
            routine.Run();
        }

        if (!isInstant)
        {
            foreach (DebugRoutine routine in debugRoutines)
            {
                routine.Run();
            }
        }
    }

    //End of scripts
    private void LateUpdate()
    {
        //Check enought time has passed before we do the next tick
        //We also have a minimum frame count so a spiked frame won't cause quick ticks
        //(This is an imperfect system but works more than well enough for the use case)
        if (
            Time.time > nextTickTime 
            && (Time.frameCount > tickInitFrame + minimumFrameLength)
            && (tickTask == null || tickTask.IsCompleted))
        {
            if (simulatioSpeedModifier > 0)
            {
                nextTickTime = (Time.time + (TICK_MAX_LENGTH / simulatioSpeedModifier));
            }

			//DEBUG: NON INSTANT TICKS
            InitSimulationTick(false);
        }
    }

    [MonitorBreak.Bebug.ConsoleCMD("SIMTURBO")]
    public static void TurboSimulation()
    {
        simulatioSpeedModifier = 100.0f;
    }

    [MonitorBreak.Bebug.ConsoleCMD("SIMLIGHTSPEED")]
    public static void LightspeedSimulation()
    {
        simulatioSpeedModifier = -1;
    }

    [MonitorBreak.Bebug.ConsoleCMD("SIMSPEED")]
    public static void SimulationSpeed(string newValue)
    {
        simulatioSpeedModifier = Int32.Parse(newValue);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(SimulationManagement))]
[CanEditMultipleObjects]
public class SimulationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SimulationManagement manager = (SimulationManagement)target;

        if (manager != null)
        {
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.fontSize = 13;
            GUILayout.Label("Init Routines");
            GUI.skin.label.fontSize = 11;
            foreach (InitRoutineBase routine in manager.initRoutines)
            {
                RoutineLabel(routine);
            }

            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.fontSize = 13;
            GUILayout.Label("\nConstant Routines");
            GUI.skin.label.fontSize = 11;
            foreach (RoutineBase routine in manager.constantRoutines)
            {
                RoutineLabel(routine);
            }

            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.fontSize = 13;
            GUILayout.Label("\nDebug Routines");
            GUI.skin.label.fontSize = 11;
            foreach (RoutineBase routine in manager.debugRoutines)
            {
                RoutineLabel(routine);
            }
        }
    }

    private void RoutineLabel(RoutineBase routine)
    {
        GUILayout.BeginHorizontal();
        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUILayout.Label("•  " + routine.ToString().Split("(")[1].Replace(")", ""));

        GUILayout.FlexibleSpace();

        GUI.skin.label.fontStyle = FontStyle.Italic;
        GUILayout.Label(
            (routine.GetType().GetCustomAttribute(typeof(SimulationManagement.ActiveSimulationRoutine)) as SimulationManagement.ActiveSimulationRoutine).priority.ToString()
            );
        GUILayout.EndHorizontal();
    }
}
#endif
