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
	public const int attackRoutineStandardPrio = -25; 
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

    private Dictionary<Enum, List<DataBase>> tagToData = new Dictionary<Enum, List<DataBase>>();
    private Dictionary<Enum, List<DataBase>> newDataModulesByTag = new Dictionary<Enum, List<DataBase>>();

    #region Data Tag Filtering
    public static void RegisterDataModule(Enum tag, DataBase module)
    {
        if (!instance.tagToData.ContainsKey(tag))
        {
            instance.tagToData.Add(tag, new List<DataBase>());
        }

        if (!instance.tagToData[tag].Contains(module))
        {
            instance.tagToData[tag].Add(module);

            //Add to new data modules by tag so init routines can run on this new data
            if (!instance.newDataModulesByTag.ContainsKey(tag))
            {
                instance.newDataModulesByTag.Add(tag, new List<DataBase>());
            }

            instance.newDataModulesByTag[tag].Add(module);
        }
    }

    public static void DeRegisterDataModule(Enum tag, DataBase module)
    {
        if (instance.tagToData.ContainsKey(tag))
        {
            instance.tagToData[tag].Remove(module);
        }
    }

    public static List<DataBase> GetDataViaTag(Enum tag)
    {
        if (instance != null && instance.tagToData.ContainsKey(tag))
        {
            return instance.tagToData[tag];
        }

        //Return empty list by default
        return new List<DataBase>();
    }

    public static List<DataBase> GetToInitData(Enum tag)
    {
        if (instance != null && instance.newDataModulesByTag.ContainsKey(tag))
        {
            return instance.newDataModulesByTag[tag];
        }

        //Return empty list by default
        return new List<DataBase>();
    }

    public static Dictionary<int, T> GetEntityIDToData<T>(Enum tag) where T : DataBase
    {
        Dictionary<int, T> toReturn = new Dictionary<int, T>();
        List<DataBase> dataTarget = GetDataViaTag(tag);

        foreach (DataBase dataModule in dataTarget)
        {
            toReturn.Add(dataModule.parent.Get().id, (T)dataModule);
        }

        return toReturn;
    }

    public static List<T> TryGetDataIntoClone<T>(Enum tag, List<DataBase> targets) where T : DataBase
    {
        List<T> toReturn = new List<T>();

        foreach (DataBase dataBase in targets)
        {
            toReturn.Add(dataBase.GetLinkedData<T>(tag));
        }

        return toReturn;
    }
    #endregion


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


		return !PlayerLocationManagement.IsPlayerLocation(cellCenter) && !PlayerCapitalShip.IsTargetPosition(cellCenter);
	}

	public static bool LocationIsLazy(VisitableLocation location)
	{
		//See above function "CellIsLazy"

		//Additionally (as of 17/12/2024) the location lazy check now takes into account the place the player is traveling to
		//as travel times mean if we don't the player could arrive and the location could have changed dramatically.

		return !PlayerLocationManagement.IsPlayerLocation(location) && !PlayerCapitalShip.IsTargetPosition(location.GetPosition());
	}

	public static bool PositionIsLazy(RealSpacePostion position)
	{
		//See above two functions, this is used for battles as they will map to a given position, possible a settlement position

		return !PlayerLocationManagement.IsPlayerLocation(position) && !PlayerCapitalShip.IsTargetPosition(position);
	}

	public GameObject simulationRoutinesStorage;
	private int historyTicksLeft;
	private int maxHistoryTicks;

	public static float GetHistoryRunPercentage()
	{
		return Mathf.Clamp01(1.0f - (instance.historyTicksLeft / (float)instance.maxHistoryTicks));
	}

    public static bool RunningHistory()
    {
        return GetHistoryRunPercentage() < 1.0f;
    }

	private const bool batchHistory = false;

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

        simulationSeed = UnityEngine.Random.Range(-10000, 10000);
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
		RefreshRoutines();
	}

	private void Start()
	{        
		//It is important this is run in Start so OnEnable can run on objects before this goes off
		if (SimulationSettings.ShouldRunHistory())
        {
			int tickCount = YearsToTickNumberCount(SimulationSettings.HistoryLength());

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
		else
		{
			//Not running history so just init player faction
			PlayerManagement.InitPlayerFaction();
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
								//Re-enable player input
								InputManagement.InputEnabled = true;

								//Init player faction
								PlayerManagement.InitPlayerFaction();
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
		if (SimulationSettings.ShouldRunHistory() && instance.historyTicksLeft > 0)
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
        //Check enought time has passed before we do the next tick
        //We also have a minimum frame count so a spiked frame won't cause quick ticks
        //(This is an imperfect system but works more than well enough for the use case)
        if ((
            (Time.time > nextTickTime 
            && (Time.frameCount > tickInitFrame + minimumFrameLength)
            && (tickTask == null || tickTask.IsCompleted))
			
			&&

			!(SimulationSettings.ShouldRunHistory() && historyTicksLeft > 0)
			)

			||

			forceTick
			)
        {
			forceTick = false;

            if (simulatioSpeedModifier > 0)
            {
                nextTickTime = (Time.time + (TICK_MAX_LENGTH / simulatioSpeedModifier));
            }

            InitSimulationTick(false, typicallyTickBatchCount);
        }
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
            (routine.GetType().GetCustomAttribute(typeof(SimulationManagement.ActiveSimulationRoutine)) as SimulationManagement.ActiveSimulationRoutine).priority.ToString()
            );
        GUILayout.EndHorizontal();
    }
}
#endif
