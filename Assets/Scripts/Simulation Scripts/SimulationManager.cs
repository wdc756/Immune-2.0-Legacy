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

public class SimulationManager : MonoBehaviour
{
    public List<Simulated> Simulated;
    public List<Simulated> SimulatedChildren;

    // Don't touch this, it's an infrastructure variable :)
    public bool ManuallyRunChildren = false;

    public bool Running = false;
    public int FixedUpdatesPerTick = 1;
    private int count;

    private void Start()
    {
        Simulated = new List<Simulated>(GetComponents<Simulated>());
        // Fun times with functional programming
        SimulatedChildren = GetComponentsInChildren<Simulated>().Except(Simulated).ToList<Simulated>();
    }

    private void FixedUpdate()
    {
        if (!Running) return;
        if (count++ % FixedUpdatesPerTick != 0) return;

        foreach (Simulated s in Simulated) s.Tick();
        if (ManuallyRunChildren) foreach (Simulated s in SimulatedChildren) s.Tick();

        if (--ticks <= 0) Running = false;
    }

    /*  
     *  Debug function for running a set amount of ticks
     */
    public int ticks;
    public int tickTotal;
    public void DebugTicks()
    {
        Debug.Log("running");
        Running = true;
        ticks = tickTotal;
    }
}
