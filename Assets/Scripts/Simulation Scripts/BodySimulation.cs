using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySimulation : MonoBehaviour
{
    public List<BodySectionSimulation> Sections;
    public bool Running = false;
    public int FixedUpdatesPerTick = 1;
    private int count;

    public BodySectionSimulation firstSection;
    void Start()
    {
        Sections = new List<BodySectionSimulation>(GetComponentsInChildren<BodySectionSimulation>());
        firstSection = Sections[0];
    }


    private void FixedUpdate()
    {
        if (!Running) return;
        if (count++ % FixedUpdatesPerTick != 0) return;
        Debug.Log("Tick");
        foreach (BodySectionSimulation section in Sections) section.Tick();
        if (--ticks <= 0) Running = false;
    }

    public int ticks;
    public int tickTotal;
    public void DebugTicks()
    {
        Debug.Log("running");
        Running = true;
        ticks = tickTotal;
    }

}
