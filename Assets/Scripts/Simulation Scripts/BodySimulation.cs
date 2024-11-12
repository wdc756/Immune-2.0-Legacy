using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySimulation : Simulated
{
    /*
     *  Contains important variables for the simulation,
     *  including a list of all the body parts
     *  Also contains the resource logic
     *  (Trent)
     */
    [Header("Other Scripts")]
    public List<BodySectionSimulation> Sections;
    public AdaptiveImmuneSystem AIS;

    // Oh boy lookie here there's a lot of yapping
    [Header("Global Section Settings")]
    public float AlarmFlagPercent = 30f;            // Minimum amount of pathogen presence required for an alarm to work
    public float AlarmResponsePercent = 50f;        // Percent the alarm goes to for a 
    public float AlarmMaxResponseTime = 0.5f;       // Response time from min response to alarm response
    public float ResponseDefaultLevelPercent = 10f; // The default response level of the immune system (uses 0 resources)
    public float ReponseChangeTime = 3f;            // The amount of time it takes to deescalate/escalate a response in seconds
    public float ReponseChangeDelta = 10f;          // The amount 1 escalation/deescalation changes the response percent
    public float PostscanResponsePercent = 30f;     //  if a scan was successful we want to set the response to this level (should be neutral)
    public float AllStressFactor;

    //public BodySectionSimulation firstSection;

    [Header("Resource Usage/Production")]
    public float ResourceDemandPercent;
    public float ResourceProductionPercent;
    public float MinResourceProductionPercent;
    public float MaxResourceProductionPercent;
    public float DefaultResourceProductionPercent;
    public float ResourceChangeTime;

    [Header("Civilian Cell Death Count")]
    public int CivilianDeathCount;
    public float PercentPerDeath;

    private bool resourceChanging;
    private float resourceChangeTicks;
    private float resourceChangeDelta;
    private float resourceTarget;   // for floating point correction

    void Start()
    {
        Sections = new List<BodySectionSimulation>(GetComponentsInChildren<BodySectionSimulation>());
        //firstSection = Sections[0];

        ResourceProductionPercent = DefaultResourceProductionPercent;

        foreach (BodySectionSimulation section in Sections) section.StressFactor = AllStressFactor;

        resourceChangeTicks = (int) (ResourceChangeTime / SimulationManager.TickDelta);
        resourceChangeDelta = 10f / (float) resourceChangeTicks;

        CivilianDeathCount = 0;
    }

    /*
     *  Currently this BodySimulation is responsible for calling the tick
     *  functions of all of the children simulations
     *  
     */

    public BodySectionSimulation GetBodySection(int index)
    {
        return Sections[index];
    }

    float sum;
    public override void Tick()
    {
        //Debug.Log("Tick");
        sum = 0;
        foreach (BodySectionSimulation section in Sections)
        {
            section.Tick();
            sum += section.ResourceDemand;

            CivilianDeathCount += (int)(section.StressLevelPercent + section.InfectionProgressPercent / PercentPerDeath);
            int resourceDeathCount = (int)(ResourceProductionPercent - DefaultResourceProductionPercent / PercentPerDeath);
        }
        ResourceDemandPercent = sum;

        // Min cap the resource demand percent
        ResourceDemandPercent = Mathf.Max(0f, ResourceProductionPercent);

        HandleResources();
        if (ResourceProductionPercent < ResourceDemandPercent) Kickback();
    }

    public void IncreaseProduction()
    {
        if (resourceChanging) return;

        resourceTarget = ResourceProductionPercent + 10f;
        resourceChangeDelta = Mathf.Abs(resourceChangeDelta);
        if (resourceTarget > MaxResourceProductionPercent) return;
        resourceChanging = true;
    }

    public void DecreaseProduction()
    {
        if (resourceChanging) return;

        resourceTarget = ResourceProductionPercent - 10f;
        resourceChangeDelta = -Mathf.Abs(resourceChangeDelta);
        if (resourceTarget < MinResourceProductionPercent) return;
        resourceChanging = true;
    }

    private void HandleResources()
    {
        if (!resourceChanging) return;

        ResourceProductionPercent += resourceChangeDelta;
        resourceChangeTicks--;

        // Reset everything
        if (resourceChangeTicks == 0)
        {
            ResourceProductionPercent = resourceTarget;

            resourceChangeTicks = (int)(ResourceChangeTime / SimulationManager.TickDelta);
            resourceChangeDelta = resourceChangeDelta = 10f / (float)resourceChangeTicks;
            resourceChanging = false;
        }
    }

    private void Kickback()
    {
        Debug.Log("Exeeded resource demand, kicking back immune system");
        foreach (BodySectionSimulation section in Sections) section.Kickback();
    }
}
