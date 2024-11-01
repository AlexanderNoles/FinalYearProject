using MonitorBreak;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManagement : MonoBehaviour
{
    private static int simulationSeed;
    public static System.Random random;

    private float nextTickTime;
    private const float TICK_MAX_LENGTH = 1;
    private float tickInitFrame;
    private float minimumFrameLength = 0;

    private static SimulationManagement instance;
    private Task tickTask;
    [HideInInspector]
    public List<RoutineBase> constantRoutines = new List<RoutineBase>();
    [HideInInspector]
    public List<InitRoutineBase> initRoutines = new List<InitRoutineBase>();

    private Dictionary<Faction.Tags, List<Faction>> factions = new Dictionary<Faction.Tags, List<Faction>>();
    private Dictionary<Faction.Tags, List<Faction>> updatedTags = new Dictionary<Faction.Tags, List<Faction>>();

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

    //As per the inital design constriction this script always executes after every other (non unity) script.
    //This does not mean it is the final code executed in the frame, we have no control over the execution order outside of scripts
    private void Awake()
    {
        //Reset some stuff
        Planet.availablePlanetPositions.Clear();
        //

        simulationSeed = UnityEngine.Random.Range(-100000, 100000);
        random = new System.Random(simulationSeed);

        instance = this;

        //Add game world faction
        new GameWorld().Simulate();

        const int testCount = 25;
        for (int i = 0; i < testCount; i++)
        {
            //Add test factions
            new Nation().Simulate();
        }

        //Add all routine instances
        (constantRoutines, initRoutines) = SimulationRoutineExecution.Main(gameObject);
    }

    private void Start()
    {
        //Run history ticks
        //Simulation is run for a period of years before player arrives to get more dynamic results

        //It is important this is run in Start so OnEnable can run on objects before this goes off
        int tickCount = 365 * 100; //100 years

        for (int i = 0; i < tickCount; i++)
        {
            InitSimulationTick(true);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ActiveSimulationRoutine : Attribute 
    {
        public int priority;
        public bool initRoutine;

        /// <summary>
        /// Construct Active Simulation Routine
        /// </summary>
        /// <param name="priority">Routines Priority, higher priority means it is run first each tick. In range -10000 to 10000</param>
        /// <param name="initRoutine">Should this routine only be run once (on the first tick the faction is created)?</param>
        public ActiveSimulationRoutine(int priority, bool initRoutine = false)
        {
            this.priority = Mathf.Clamp(priority, -10000, 10000);
            this.initRoutine = initRoutine;
        }
    }

    public class SimulationRoutineExecution : MonoBehaviour
    {
        public static (List<RoutineBase>, List<InitRoutineBase>) Main(GameObject parent)
        {
            List<RoutineBase> activeRoutines = new List<RoutineBase>();
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
                if (type.Item1.initRoutine)
                {
                    initRoutines.Add(newRoutine as InitRoutineBase);
                }
                else
                {
                    activeRoutines.Add(newRoutine as RoutineBase);
                }
            }

            return (activeRoutines, initRoutines);
        }
    }

    //Multithread control
    public static void InitSimulationTick(bool isInstant)
    {
        if (instance != null) 
        {
            instance.tickInitFrame = Time.frameCount;
            instance.minimumFrameLength = Time.captureFramerate / 2.0f;

            instance.tickTask = Task.Run(() =>
            {
                instance.SimulationTick();
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
    private void SimulationTick()
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
            nextTickTime = Time.time + TICK_MAX_LENGTH;
            InitSimulationTick(false);
        }
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
