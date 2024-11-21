using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveImmuneSystem : Simulated
{
    /*  
     *  Performs the automatic and manual scans on body parts, either when the stress
     *  flag percentage is reached or by the user's request
     *  (Trent)
     */
    [Header("Other Scripts")]
    public BodySimulation Body;

    [Header("Scan Times")]
    public float AutoScanTime;      // In seconds
    public float ManualScanTime;    // In seconds

    [Header("Automatic Scan Requirements")]
    // When these flags are reach in % automatic scans will trigger
    public float StressFlagPercentage;
    public float ProgressFlagPercentage;
    public float ResponseFlagPercentage;


    [Header("(Debug) Responses")]
    // Contains all of the sections where the AIS is responding to (successfully completed a scan)
    [SerializeField] private List<BodySectionSimulation> responses = new List<BodySectionSimulation>();

    // contains all of the active scanned body sections and how many ticks are left to scan
    private Dictionary<BodySectionSimulation, int> scanDictionary = new Dictionary<BodySectionSimulation, int>();
    public override void Tick()
    {
        foreach (BodySectionSimulation section in Body.Sections)
        {
            bool stressFlag = section.StressLevelPercent >= StressFlagPercentage;
            bool progressFlag = section.InfectionProgressPercent >= ProgressFlagPercentage;
            bool responseFlag = section.Response.LevelPercent >= ResponseFlagPercentage;

            if (stressFlag && progressFlag && responseFlag) BeginScan(section);
        }
        TrackScans();
    }

    public void BeginScan(BodySectionSimulation section, bool auto=true)
    {
        // Don't begin new scans if it's being scanned or has been scanned
        if (scanDictionary.ContainsKey(section) || responses.Contains(section)) return;

        // Convert from scan time in seconds to scan time in ticks, this makes it easy to work with
        float scanTime = auto ? AutoScanTime : ManualScanTime;
        int scanTicks = (int)(scanTime / SimulationManager.TickDelta);
        Debug.Log($"Running scan for {scanTime} which is {scanTicks}");

        // Update the dictionary to contain the section being scanned and the time in ticks
        scanDictionary[section] = scanTicks;
    }

    private List<BodySectionSimulation> sectionsToRemove = new List<BodySectionSimulation>();
    public void TrackScans()
    {
        sectionsToRemove.Clear(); // I don't like generating empty lists every tick but ehhhhhh

        // This loop decrements all the scan times and checks if we are done
        foreach (BodySectionSimulation section in Body.Sections)
        {
            if (scanDictionary.ContainsKey(section))
            {
                scanDictionary[section]--;
                if (scanDictionary[section] == 0)
                {
                    // I have a feeling this edge case could be reached accidentally
                    // So I added this failsafe
                    if (section.Infection == null)
                    {
                        sectionsToRemove.Add(section);
                        return;
                    }

                    ImmuneSystemResponse.ResponseType newResponse = section.Infection.ResponseWeakness;

                    // Only activate a new immune response if it isn't macrophages and neutrophils.
                    // AIS doesn't handle that and it essentialy becomes a huge waste of time
                    // So this is a REALLY bad idea for the player (punishing)
                    // Might even impose a stress penalty
                    if (newResponse != ImmuneSystemResponse.ResponseType.MACNEUTRO)
                    {
                        section.HeavyWeaponsResponse(newResponse);
                        responses.Add(section); // We are actively responding to this threat, don't scan again
                    }
                    else
                    {
                        section.Alarm();
                    }

                    section.statusScan = false;

                    Debug.Log($"Scan complete, new response: {newResponse}");
                    sectionsToRemove.Add(section);
                }
            }
        }

        // Needs to be a separate loop or unity will throw a fit
        foreach (BodySectionSimulation section in sectionsToRemove) scanDictionary.Remove(section);
    }

    public void TrackResponses()
    {
        foreach (BodySectionSimulation section in responses)
            if (section.InfectionProgressPercent == 0) responses.Remove(section);   // We win, allow part to be scanned again
    }
}
