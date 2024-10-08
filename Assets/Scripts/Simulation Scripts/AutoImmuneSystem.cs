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
    public bool StressFlag;

    public float ProgressFlagPercentage;
    public bool ProgressFlag;

    public float ResponseFlagPercentage;
    public bool ResponseFlag;


    public override void Tick()
    {
        StressFlag = Body.firstSection.StressLevelPercent >= StressFlagPercentage;
        ProgressFlag = Body.firstSection.InfectionProgressPercent >= ProgressFlagPercentage;
        ResponseFlag = Body.firstSection.Response.LevelPercent >= ResponseFlagPercentage;
    }
}
