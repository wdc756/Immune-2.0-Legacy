using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoImmuneSystem : Simulated
{
    /*  
     *  Performs the automatic and manual scans on body parts, either when the stress
     *  flag percentage is reached or by the user's request
     */

    public BodySimulation Body;

    public float AutoScanTime;
    public float ManualScanTime;

    public float StressFlagPercentage;

    

    public override void Tick()
    {

    }
}
