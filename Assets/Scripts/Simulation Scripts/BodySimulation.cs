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
    public float ResponseDefaultLevelPercent = 10f;
    public float ReponseChangeTime = 3f;     // The amount of time it takes to deescalate/escalate a response in seconds
    public float ReponseChangeDelta = 10f;     // The amount 1 escalation/deescalation changes the response percent
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
