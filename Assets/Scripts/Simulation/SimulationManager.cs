using MonitorBreak;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    private float nextTickTime;
    private const float TICK_MAX_LENGTH = 1;
    private float tickInitFrame;
    private float minimumFrameLength = 0;

    private static SimulationManager instance;
    private Task tickTask;
    private List<Faction> factions = new List<Faction>();
    private List<RoutineBase> routines = new List<RoutineBase>();

    //As per the inital design constriction this script always executes after every other (non unity) script.
    //This does not mean it is the final code executed in the frame, we have no control over the execution order outside of scripts
    private void Awake()
    {
        instance = this;
        //Add test faction
        factions.Add(new Faction());
        //Add all routine instances
        routines = SimulationRoutineExecution.Main(gameObject);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ActiveSimulationRoutine : Attribute 
    {
        public int priority;

        public ActiveSimulationRoutine(int priority)
        {
            this.priority = priority;
        }
    }

    public class SimulationRoutineExecution : MonoBehaviour
    {
        public static List<RoutineBase> Main(GameObject parent)
        {
            List<RoutineBase> toReturn = new List<RoutineBase>();
            List<(int, Type)> rountineClasses = new List<(int, Type)>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes().Where(x => typeof(MonoBehaviour).IsAssignableFrom(x) && x != typeof(MonoBehaviour)))
                {
                    ActiveSimulationRoutine routine = (ActiveSimulationRoutine)type.GetCustomAttribute(typeof(ActiveSimulationRoutine), false);
                    if (routine != null)
                    {
                        //Add to routine classes based on priority
                        if (rountineClasses.Count == 0)
                        {
                            rountineClasses.Add((routine.priority, type));
                        }
                        else
                        {
                            for (int i = 0; i < rountineClasses.Count; i++)
                            {
                                if (routine.priority > rountineClasses[i].Item1)
                                {
                                    rountineClasses.Insert(i, (routine.priority, type));
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            foreach ((int, Type) type in rountineClasses)
            {
                //Because the types where sorted based on priority they are displayed in priority order on the target gameobject
                toReturn.Add(parent.AddComponent(type.Item2) as RoutineBase);
            }

            return toReturn;
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
        //We run each rountine on each faction rather than each faction on every routine so later routines can react to other factions previous routines
        foreach (RoutineBase routine in routines)
        {
            foreach (Faction faction in factions)
            {
                if (routine.Check(null))
                {
                    routine.Run(null);
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
