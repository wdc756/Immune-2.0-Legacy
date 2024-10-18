using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySimulation : Simulated
{
    /*
     *  Contains important variables for the simulation,
     *  including a list of all the body parts
     *  Also contains the resource logic
     */
    [Header("Other Scripts")]
    public List<BodySectionSimulation> Sections;
    public AutoImmuneSystem AIS;

    [Header("Global Section Settings")]
    public float AlarmFlagPercent = 30f; // Minimum amount of pathogen presence required for an alarm to work
    public float AlarmResponsePercent = 50f; // Percent the alarm goes to for a 
    public float AlarmMaxResponseTime = 0.5f; // Response time from min response to alarm response
    public float ResponseDefaultLevelPercent = 10f; // The default response level of the immune system (uses 0 resources)
    public float ReponseChangeTime = 3f;     // The amount of time it takes to deescalate/escalate a response in seconds
    public float ReponseChangeDelta = 10f;     // The amount 1 escalation/deescalation changes the response percent
    public float PostscanResponsePercent = 30f; //  if a scan was successful we want to set the response to this level (should be neutral)
    public float AllStressFactor;

    //public BodySectionSimulation firstSection;

    [Header("Resource Usage/Production")]
    public float ResourceDemandPercent;
    public float ResourceProductionPercent;

    void Start()
    {
        Sections = new List<BodySectionSimulation>(GetComponentsInChildren<BodySectionSimulation>());
        //firstSection = Sections[0];

        foreach (BodySectionSimulation section in Sections) section.StressFactor = AllStressFactor;
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


    public override void Tick()
    {
        //Debug.Log("Tick");
        ResourceDemandPercent = 0;
        foreach (BodySectionSimulation section in Sections)
        {
            section.Tick();
            ResourceDemandPercent += section.ResourceDemand;
        }
    }

    public void IncreaseProduction()
    {

    }
}
