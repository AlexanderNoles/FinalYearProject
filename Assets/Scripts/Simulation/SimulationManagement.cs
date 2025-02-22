using EntityAndDataDescriptor;
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
	private static int seedOverride = int.MaxValue;

	public static int GetSimulationSeed()
	{
		return simulationSeed;
	}

    public static System.Random random;

    private static float tickStartTime;

    public static float GetCurrentSimulationTime()
    {
        return tickStartTime;
    }

	public static int currentTickID;
	private static int typicallyTickBatchCount = 1;
    private float nextTickTime;
    private const float TICK_MAX_LENGTH = 3;
    private float tickInitFrame;
    private float minimumFrameLength = 0;
	private static bool forceTick;

	public static void ForceTick()
	{
		forceTick = true;
	}

    public static float GetSimulationDaysAsTime(int dayNumber)
    {
        return dayNumber * TICK_MAX_LENGTH;
    }

    private static float currentDay;
    private const float DAY_TO_MONTH = 36; //30 instead of 31 as it fits nicely into 360
    private static float currentMonth;
    private const float MONTH_TO_YEAR = 12; //12 for the same reason as above
    private static float currentYear;

	public static float GetCurrentDayPercentage()
	{
		return currentDay / DAY_TO_MONTH;
	}

	public static float GetCurrentMonthPercentage()
	{
		return currentMonth / MONTH_TO_YEAR;
	}

	public static float GetCurrentYear()
	{
		return currentYear;
	}

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

	public static int YearsToTickNumberCount(int years)
	{
		return (int)(DAY_TO_MONTH * MONTH_TO_YEAR) * years;
	}

	public static int MonthToTickNumberCount(int months)
	{
		return (int)DAY_TO_MONTH * months;
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
	public const int attackRoutineStandardPrio = 150; 
	public const int defendRoutineStandardPrio = -30;
	public const int evaluationRoutineStandardPrio = -3010;

	public static void RunAbsentRoutine(string routineIdentifier)
    {
        if (instance.absentRoutines.ContainsKey(routineIdentifier))
        {
            instance.absentRoutines[routineIdentifier].Run();
        }
    }

    //Entity Removal

    public static void RemoveEntityFromSimulation(SimulationEntity entity)
    {
		if (entity.HasTag(EntityStateTags.Unkillable))
		{
			return;
		}

        HashSet<Enum> entityTags = entity.GetEntityTags();

        foreach (Enum tag in entityTags)
        {
            DeRegisterEntityHasTag(tag, entity);
        }

        List<Enum> dataTags = entity.GetDataTags();

        foreach (Enum tag in dataTags)
        {
            entity.RemoveData(tag);
        }

		entity.OnDeath();
        instance.idToEntity.Remove(entity.id);
    }

    private Dictionary<int, SimulationEntity> idToEntity = new Dictionary<int, SimulationEntity>();

    #region Entity ID Filtering
    public static SimulationEntity GetEntityByID(int id)
    {
        if (instance.idToEntity.ContainsKey(id))
        {
            return instance.idToEntity[id];
        }

        return null;
    }

	public static bool EntityExists(int id)
	{
		return instance.idToEntity.ContainsKey(id);
	}

	public static int GetEntityCount()
	{
		return instance.idToEntity.Count;
	}

	public static bool EntityWithIDExists(int id)
	{
		return instance.idToEntity.ContainsKey(id);
	}

	public static void RegisterEntityToIDDict(SimulationEntity entity)
	{
		instance.idToEntity.Add(entity.id, entity);
	}
    #endregion

    private Dictionary<Enum, List<SimulationEntity>> tagToEntities = new Dictionary<Enum, List<SimulationEntity>>();

	public static int GetEntityCount(Enum tag)
	{
		if (instance.tagToEntities.ContainsKey(tag))
		{
			return instance.tagToEntities[tag].Count;
		}

		return 0;
	}

	#region Entity Tag Filtering

	public static void RegisterEntityHasTag(Enum tag, SimulationEntity entity)
    {
        if (!instance.tagToEntities.ContainsKey(tag))
        {
            //Init sub list if it doesn't exist
            instance.tagToEntities.Add(tag, new List<SimulationEntity>());
        }

        if (!instance.tagToEntities[tag].Contains(entity))
        {
            //Add entity to tag if it has not already been added
            instance.tagToEntities[tag].Add(entity);
        }
    }

    public static void DeRegisterEntityHasTag(Enum tag, SimulationEntity entity)
    {
        if (instance.tagToEntities.ContainsKey(tag))
        {
            //Remove entity from tag entry if that tag is registered and the entity is registered with it
            instance.tagToEntities[tag].Remove(entity);
        }
    }

    public static List<SimulationEntity> GetEntitiesViaTag(Enum tag)
    {
        if (instance != null && instance.tagToEntities.ContainsKey(tag))
        {
            return instance.tagToEntities[tag];
        }

        //Return empty list by default
        return new List<SimulationEntity>();
    }
    #endregion

    private Dictionary<Enum, List<DataModule>> tagToData = new Dictionary<Enum, List<DataModule>>();
    private Dictionary<Enum, List<DataModule>> newDataModulesByTag = new Dictionary<Enum, List<DataModule>>();

    #region Data Tag Filtering
    public static void RegisterDataModule(Enum tag, DataModule module)
    {
        if (!instance.tagToData.ContainsKey(tag))
        {
            instance.tagToData.Add(tag, new List<DataModule>());
        }

        if (!instance.tagToData[tag].Contains(module))
        {
            instance.tagToData[tag].Add(module);

            //Add to new data modules by tag so init routines can run on this new data
            if (!instance.newDataModulesByTag.ContainsKey(tag))
            {
                instance.newDataModulesByTag.Add(tag, new List<DataModule>());
            }

            instance.newDataModulesByTag[tag].Add(module);
        }
    }

    public static void DeRegisterDataModule(Enum tag, DataModule module)
    {
		//Remove from regular data
        if (instance.tagToData.ContainsKey(tag))
        {
            instance.tagToData[tag].Remove(module);
        }

		//Remove from to init data
		if (instance.newDataModulesByTag.ContainsKey(tag))
		{
			instance.newDataModulesByTag[tag].Remove(module);
		}
    }

    public static List<DataModule> GetDataViaTag(Enum tag)
    {
        if (instance != null && instance.tagToData.ContainsKey(tag))
        {
            return instance.tagToData[tag];
        }

        //Return empty list by default
        return new List<DataModule>();
    }

	public static List<T> CastFilter<T>(List<DataModule> source) where T : DataModule
	{
		List<T> result = new List<T>();
		foreach (DataModule module in source)
		{
			if (module is T)
			{
				result.Add((T)module);
			}
		}

		return result;
	}

    public static List<DataModule> GetToInitData(Enum tag)
    {
        if (instance != null && instance.newDataModulesByTag.ContainsKey(tag))
        {
            return instance.newDataModulesByTag[tag];
        }

        //Return empty list by default
        return new List<DataModule>();
    }

    public static Dictionary<int, T> GetEntityIDToData<T>(Enum tag) where T : DataModule
    {
        Dictionary<int, T> toReturn = new Dictionary<int, T>();
        List<DataModule> dataTarget = GetDataViaTag(tag);

        foreach (DataModule dataModule in dataTarget)
        {
            toReturn.Add(dataModule.parent.Get().id, (T)dataModule);
        }

        return toReturn;
    }

    public static List<T> TryGetDataIntoClone<T>(Enum tag, List<DataModule> targets) where T : DataModule
    {
        List<T> toReturn = new List<T>();

        foreach (DataModule dataBase in targets)
        {
            toReturn.Add(dataBase.GetLinkedData<T>(tag));
        }

        return toReturn;
    }
    #endregion

    public static bool LocationIsLazy(VisitableLocation location)
    {
        //A location is considered lazy only if it is not being drawn
        //If a location is being drawn it is updated by the main runtime loop and so it is 'active'
        //i.e., not being calculated by the simulation

        return !PlayerLocationManagement.IsDrawnLocation(location);
    }

	public GameObject simulationRoutinesStorage;
    public int historyLength = 17;
	private int historyTicksLeft;
	private int maxHistoryTicks;
	private bool historyJustEnded = false;

	public static float GetHistoryRunPercentage()
	{
		return Mathf.Clamp01(1.0f - (instance.historyTicksLeft / (float)instance.maxHistoryTicks));
	}

    public static bool RunningHistory()
    {
        return GetHistoryRunPercentage() < 1.0f;
    }

	private const bool batchHistory = false;

    [Header("External References")]
    public PlanetsGenerator planetsGenerator;

    //As per the inital design constriction this script always executes after every other (non unity) script.
    //This does not mean it is the final code executed in the frame, we have no control over the execution order outside of scripts
    private void Awake()
    {
        //Reset some stuff
        Planet.availablePlanetPositions.Clear();

        currentDay = 15;
        currentMonth = 3;
        currentYear = 3004;

		TargetableLocationData.targetableLocationLookup.Clear(); //Make sure this is empty!
																 
		if (seedOverride <= 10000)
		{
			simulationSeed = seedOverride;
			seedOverride = int.MaxValue;
		}
		else
		{
			simulationSeed = UnityEngine.Random.Range(-10000, 10000);
		}

        random = new System.Random(simulationSeed);

        planetsGenerator.GeneratePlanets(random, false);

        instance = this;

        const int testCount = 4;
        for (int i = 0; i < testCount; i++)
        {
            //Add test factions
            new Nation().Simulate();
        }

		//Add game world
		new GameWorld().Simulate();

		//Add Warp
		new Warp().Simulate();

		//Add all routine instances
		RefreshRoutines();
	}

	private void Start()
	{
		//It is important this is run in Start so OnEnable can run on objects before this goes off
		int tickCount = YearsToTickNumberCount(historyLength);

		if (!batchHistory)
		{
			//Setup async history run
			//First active History running ui
			HistoryUIManagement.SetHistoryUIActive();

			//Then we need to disable player input till the history burst is over
			InputManagement.InputEnabled = false;

			//Then we need to set a tick burst count
			historyTicksLeft = tickCount;
			maxHistoryTicks = tickCount;
			InitSimulationTick(false, tickCount);
		}
		else
		{
			//Run history ticks
			//Simulation is run for a period of years before player arrives to get more dynamic results

			for (int i = 0; i < tickCount; i++)
			{
				InitSimulationTick(true);
			}
		}
	}

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SimulationRoutine : Attribute 
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
        public SimulationRoutine(int priority, RoutineTypes routineType = RoutineTypes.Normal, string identifier = "")
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
			//Remove all components from parent object
			foreach (Component component in parent.GetComponents<Component>())
			{
				if (component is not Transform)
				{
					if (Application.isPlaying)
					{
						Destroy(component);
					}
					else
					{
						DestroyImmediate(component);
					}

				}
			}
			//

			List<RoutineBase> activeRoutines = new List<RoutineBase>();
            List<RoutineBase> debugRoutines = new List<RoutineBase>();
            Dictionary<string, RoutineBase> absentRoutines = new Dictionary<string, RoutineBase>();
            List<InitRoutineBase> initRoutines = new List<InitRoutineBase>();
            List<(SimulationRoutine, Type)> rountineClasses = new List<(SimulationRoutine, Type)>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes().Where(x => typeof(MonoBehaviour).IsAssignableFrom(x) && x != typeof(MonoBehaviour)))
                {
                    SimulationRoutine routine = (SimulationRoutine)type.GetCustomAttribute(typeof(SimulationRoutine), false);
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

            foreach ((SimulationRoutine, Type) type in rountineClasses)
            {
                //Because the types where sorted based on priority they are displayed in priority order on the target gameobject
                Component newRoutine = parent.AddComponent(type.Item2);
                if (type.Item1.routineType == SimulationRoutine.RoutineTypes.Init)
                {
                    initRoutines.Add(newRoutine as InitRoutineBase);
                }
                else if (type.Item1.routineType == SimulationRoutine.RoutineTypes.Debug)
                {
                    debugRoutines.Add(newRoutine as RoutineBase);
                }
                else if (type.Item1.routineType == SimulationRoutine.RoutineTypes.Absent)
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
    public static void InitSimulationTick(bool isInstant, int count = 1)
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

			instance.tickTask = Task.Run(() =>
			{
				try
				{
					for (int i = 0; i < count; i++)
					{
						currentTickID++;

						IncrementDay();

						instance.SimulationTick(isInstant);

						if (instance.historyTicksLeft > 0)
						{
							instance.historyTicksLeft--;

							if (instance.historyTicksLeft <= 0)
							{
								instance.historyJustEnded = true;
							}
						}
					}
				}
				catch (Exception e)
				{
					MonitorBreak.Bebug.Console.Log(e);
				}
            });

			if (isInstant)
			{
				instance.tickTask.Wait();
			}
		}
    }

    public static void EndSimulationTick()
    {
		if (instance.historyTicksLeft > 0)
		{
			return;
		}

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
        //Run this before other routines so we can update data on the same tick it is initialized
        //This does mean new entities created this tick will have to wait till next tick to have their data initilized
        //No good generic check exists that can tell if data has been initlized for a given tag or not (though non-generic ones do exist (e.g., a non-initlized Nation will always occupy no spaces))
        //This means routines that create new Entites should be always run after routines that would alter those entites data, or we will run into unexpected behaviour
        //(or more likely the changes that tick will be overwritten and execution time will be spent errouneously)
        if (newDataModulesByTag.Count > 0)
        {
            HashSet<Enum> updatedTags = newDataModulesByTag.Keys.ToHashSet();

            foreach (InitRoutineBase routine in initRoutines)
            {
                if (routine.IsDataToInit(updatedTags))
                {
                    routine.Run();
                }
            }

            newDataModulesByTag.Clear();
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
		//Evaluate whether to init a tick this frame
		bool shouldRunTick =
			Time.time > nextTickTime &&                                 //Time between tick inits
			(Time.frameCount > tickInitFrame + minimumFrameLength) &&   //Minimum frame time incase of large fps spikes
			(tickTask == null || tickTask.IsCompleted);                 //Previous tick is done

		shouldRunTick = shouldRunTick && !(historyTicksLeft > 0);		//Don't run ticks if history sim is running
		//

		//Run
		if (shouldRunTick || forceTick)
        {
			//Init player faction if history just ended
			if (historyJustEnded)
			{
				historyJustEnded = false;

				//Re-enable player input
				InputManagement.InputEnabled = true;

				//Set active map in nation selection mode
				MapManagement.SetActiveAsNationSelection();
			}

			//Only run subsequent ticks when the player faction has been created
			//(Unless we are forcing a tick)
			if (PlayerManagement.PlayerEntityExists() || forceTick)
			{
				forceTick = false;

				if (simulatioSpeedModifier > 0)
				{
					nextTickTime = (Time.time + (TICK_MAX_LENGTH / simulatioSpeedModifier));
				}

				InitSimulationTick(false, typicallyTickBatchCount);
			}
        }
    }

	[MonitorBreak.Bebug.ConsoleCMD("LOADSEED", "Load a solar system with specific seed")]
	public static void LoadSeed(string seed)
	{
		int parsedSeed = int.Parse(seed);
		seedOverride = parsedSeed;

		GameManagement.ReloadScene();
	}

	[MonitorBreak.Bebug.ConsoleCMD("SIMSEED", "Get the current simulation's seed")]
	public static void OutputSimSeedToConsoleCMD()
	{
		MonitorBreak.Bebug.Console.Log(GetSimulationSeed());
	}

	[MonitorBreak.Bebug.ConsoleCMD("SIMTURBO")]
    public static void TurboSimulationCMD()
    {
        simulatioSpeedModifier = 100.0f;
    }

    [MonitorBreak.Bebug.ConsoleCMD("SIMLIGHTSPEED")]
    public static void LightspeedSimulationCMD()
    {
        simulatioSpeedModifier = -1; 
	}

	[MonitorBreak.Bebug.ConsoleCMD("SIMABSURDSPEED")]
	public static void AbsurdspeedSimulationCMD()
	{
		simulatioSpeedModifier = -1;
		typicallyTickBatchCount = 30;
	}

    public static void SimulationSpeed(int newValue)
    {
        simulatioSpeedModifier = newValue;
    }

    [MonitorBreak.Bebug.ConsoleCMD("SIMSPEED")]
    public static void SimulationSpeedCMD(string newValue)
    {
        simulatioSpeedModifier = Int32.Parse(newValue);
    }

	[MonitorBreak.Bebug.ConsoleCMD("SIMBATCH")]
	public static void SimulationBatchCMD(string newValue)
	{
		typicallyTickBatchCount = Int32.Parse(newValue);
	}

	[MonitorBreak.Bebug.ConsoleCMD("SIMBURST")]
	public static void SimulationBurstCMD(string tickCount)
	{
		int count = Int32.Parse(tickCount);

		for (int i = 0; i < count; i++)
		{
			InitSimulationTick(true);
		}
	}

	[MonitorBreak.Bebug.ConsoleCMD("READ", "Read data from entity")]
	public static void ReadEntity(string id)
	{
		int ID = int.Parse(id);

		MonitorBreak.Bebug.Console.Log($"Entity {id}:", 0, false);
		MonitorBreak.Bebug.Console.Log("----", 0, false);

		if (instance.idToEntity.ContainsKey(ID))
		{
			SimulationEntity entity = instance.idToEntity[ID];

			List<DataModule> modules = entity.GetAllDataModules();

			foreach (DataModule module in modules) 
			{
				MonitorBreak.Bebug.Console.Log(module.ToString(), 0, false);

				if (module.ReadImplemented())
				{
					MonitorBreak.Bebug.Console.Log(module.Read(), 0, false);
				}
			}
		}
		MonitorBreak.Bebug.Console.Log("----", 0, false);
	}

	[MonitorBreak.Bebug.ConsoleCMD("ENTITIES", "List All entities and ids")]
	public static void OutputAllEntitiesAndIDs()
	{
		MonitorBreak.Bebug.Console.Log("----", 0, false);
		const int countPerRow = 1; //Disabled essentially
		int currentCount = countPerRow;
		string outputString = "";
		foreach (KeyValuePair<int, SimulationEntity> entry in instance.idToEntity)
		{
			if (currentCount <= 0)
			{
				MonitorBreak.Bebug.Console.Log(outputString, 0, false);
				currentCount = countPerRow;
				outputString = "";
			}

			outputString += $"{entry.Value}: {entry.Key}";
			currentCount--;
		}

		if (currentCount != countPerRow)
		{
			//Half way through a row
			MonitorBreak.Bebug.Console.Log(outputString, 0, false);
		}

		MonitorBreak.Bebug.Console.Log("----", 0, false);
	}

	[MonitorBreak.Bebug.ConsoleCMD("ENTITYCOUNT")]
	public static void OutputEntityCount()
	{
		MonitorBreak.Bebug.Console.Log(instance.idToEntity.Count);
	}

	[ContextMenu("Refresh Routines")]
	private void RefreshRoutines()
	{
		(constantRoutines, initRoutines, debugRoutines, absentRoutines) = SimulationRoutineExecution.Main(simulationRoutinesStorage);
	}
}


#if UNITY_EDITOR
[CustomEditor(typeof(SimulationManagement))]
[CanEditMultipleObjects]
public class SimulationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
		DrawDefaultInspector();

        SimulationManagement manager = (SimulationManagement)target;

		GUILayout.Label("---");

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
        GUILayout.Label("�  " + routine.ToString().Split("(")[1].Replace(")", ""));

        GUILayout.FlexibleSpace();

        GUI.skin.label.fontStyle = FontStyle.Italic;
        GUILayout.Label(
            (routine.GetType().GetCustomAttribute(typeof(SimulationManagement.SimulationRoutine)) as SimulationManagement.SimulationRoutine).priority.ToString()
            );
        GUILayout.EndHorizontal();
    }
}
#endif
