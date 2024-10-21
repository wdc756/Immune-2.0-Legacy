using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/*
 *  Simulated is an abstract class which all simulated behaviors
 *  derive from. it takes from base monobehavior (so it gets all the unity functions)
 *  and allows the simulation manager to keep track of all behaviors
 */
public abstract class Simulated : MonoBehaviour
{
    public abstract void Tick();
}

/*
*   SimulationManager is the top-level class that calls all of the tick functions for the body systems.
*   This script can also call all of the section tick functions if needed
*   but that responsibility is already delegated to BodySimulation
*/
public class SimulationManager : MonoBehaviour
{
    [Header("Simulation Objects")]
    public List<Simulated> Simulated;
    public List<Simulated> SimulatedChildren;

    [Header("Infrastructure (please don't touch)")]
    // Don't touch this, it's an infrastructure variable :)
    public bool ManuallyRunChildren = false;

    [Header("Simulation Time Settings")]
    public int FixedUpdatesPerTick = 1;

    [Header("(Debug)")]
    public bool Running = false;
    public static float TickDelta;

    private int count;


    private void Start()
    {
        // TickDelta is the seconds per tick used to convert seconds to ticks and vice versa
        TickDelta = Time.fixedDeltaTime * (float)FixedUpdatesPerTick;

        // All of the scripts who have tick functions on this object
        Simulated = new List<Simulated>(GetComponents<Simulated>());

        // Fun times with functional programming
        SimulatedChildren = GetComponentsInChildren<Simulated>().Except(Simulated).ToList<Simulated>();
    }

    private void FixedUpdate()
    {
        if (!Running) return;
        if (count++ % FixedUpdatesPerTick != 0) return;

        foreach (Simulated s in Simulated)
        {
            s.Tick();
        }
        if (ManuallyRunChildren) foreach (Simulated s in SimulatedChildren) s.Tick();

        if (debug && --ticks <= 0) Running = false;
    }

    public void Run()
    {
        Debug.Log("Simulation Running");
        Running = true;
    }

    public void Stop()
    {
        Debug.Log("Simulation Stopped");
        Running = false;
    }

    /*  
     *  Debug function for running a set amount of ticks
     */
    [Header("Debug Ticks")]
    public bool debug = false;
    public int ticks;
    public int tickTotal;
    public void DebugTicks()
    {
        Debug.Log("running");
        Running = true;
        ticks = tickTotal;
    }
}
