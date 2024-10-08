using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySimulation : Simulated
{
    /*
     *  Contains important variables for the simulation,
     *  including a list of all the body parts
     */

    public List<BodySectionSimulation> Sections;
    public AutoImmuneSystem AIS;

    public BodySectionSimulation firstSection;
    void Start()
    {
        Sections = new List<BodySectionSimulation>(GetComponentsInChildren<BodySectionSimulation>());
        firstSection = Sections[0];
    }

    /*
     *  Currently this BodySimulation is responsible for calling the tick
     *  functions of all of the children simulations
     *  
     */
    public override void Tick()
    {
        Debug.Log("Tick");

        foreach (BodySectionSimulation section in Sections) section.Tick();
        AIS.Tick();
    }

}
