using MonitorBreak;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    private float nextTickTime;
    private const float TICK_MAX_LENGTH = 1;
    private float tickInitFrame;
    private float minimumFrameLength = 0;

    private static SimulationManager instance;
    private Task tickTask;
    private List<Faction> factions = new List<Faction>();
    private List<RoutineBase> constantRoutines = new List<RoutineBase>();
    private List<RoutineBase> initRoutines = new List<RoutineBase>();

    //As per the inital design constriction this script always executes after every other (non unity) script.
    //This does not mean it is the final code executed in the frame, we have no control over the execution order outside of scripts
    private void Awake()
    {
        instance = this;

        //Add game world faction
        AddFaction(new GameWorld());

        const int testCount = 2;
        for (int i = 0; i < testCount; i++)
        {
            //Add test factions
            Nation nation = new Nation();
            AddFaction(nation);
        }

        //Add all routine instances
        (constantRoutines, initRoutines) = SimulationRoutineExecution.Main(gameObject);
    }

    private void Start()
    {
        //Run a instant simulation tick to do init ticks for inital factions
        InitSimulationTick(true);
    }

    public static void AddFaction(Faction newFaction)
    {
        instance.factions.Add(newFaction);
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
        public static (List<RoutineBase>, List<RoutineBase>) Main(GameObject parent)
        {
            List<RoutineBase> activeRoutines = new List<RoutineBase>();
            List<RoutineBase> initRoutines = new List<RoutineBase>();
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
                RoutineBase newRoutine = parent.AddComponent(type.Item2) as RoutineBase;
                if (type.Item1.initRoutine)
                {
                    initRoutines.Add(newRoutine);
                }
                else
                {
                    activeRoutines.Add(newRoutine);
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
        bool runInitRoutines = false;

        //We run each rountine on each faction rather than each faction on every routine so later routines can react to other factions previous routines
        foreach (RoutineBase routine in constantRoutines)
        {
            foreach (Faction faction in factions)
            {
                if (faction.hasRunInit)
                {
                    if (routine.Check(faction))
                    {
                        routine.Run(faction);
                    }
                }
                else
                {
                    runInitRoutines = true;
                }
            }
        }

        if (!runInitRoutines)
        {
            return;
        }

        foreach (RoutineBase routine in initRoutines)
        {
            foreach (Faction faction in factions)
            {
                if (!faction.hasRunInit)
                {
                    if (routine.Check(faction))
                    {
                        routine.Run(faction);
                    }
                }
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
            nextTickTime = Time.time + TICK_MAX_LENGTH;
            InitSimulationTick(false);
        }
    }
}
