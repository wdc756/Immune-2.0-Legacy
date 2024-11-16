using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempUIHandler : MonoBehaviour
{
    public BodySimulation simulation;
    private BodySectionSimulation sectionSim;
    public VisualManager visualManager;

    //calls the active body section's function when a button is pressed
    public void CallBodySectionFunction(int type)
    {
        int activeScene = visualManager.activeScene;
        sectionSim = simulation.Sections[activeScene];

        if (sectionSim == null)
        {
            Debug.Log("Could not find BodySectionSimulation");
            return;
        }

        switch (type)
        {
            case 0:
                sectionSim.Alarm();
                break;
            case 1:
                sectionSim.Escalate();
                break;
            case 2:
                sectionSim.Deescalate();
                break;
            case 3:
                sectionSim.Scan();
                break;
            default:
                Debug.Log("Invalid int argument for CallBodySectionFunction: " +  type);
                break;
        }
    }
}
