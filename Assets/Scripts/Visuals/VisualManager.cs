using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VisualManager : MonoBehaviour
{
    /*
     Think of this as a sort of int main() but for the visuals. Without this nothing else will really happen

     While this script may not do much directly, it's responsible for coordinating the various systems in place
     */

    //Reference to the GameManager to tell it if there was some sort of error
    private GameManager gameManager;
    //Reference to the CellManager
    private CellManager cellManager;
    private BodySimulation bodySimulation;

    [Tooltip("The active visual scene that the visual system is rendering; change this number to change the starting scene")]
    public int activeScene = -1;
    [SerializeField, Tooltip("Prefab used to generate Visual Scenes")]
    private GameObject visualScenePrefab;
    [SerializeField, Tooltip("Holds all the VisualScenes")]
    private GameObject visualSceneParent;
    [SerializeField, Tooltip("Used to reference all the VisualScenes")]
    private List<VisualScene> visualSceneList = new List<VisualScene>();

    [SerializeField, Tooltip("Reference to the SimulationManager for data")]
    private SimulationManager simulationManager;

    [SerializeField, Tooltip("Determines if the script should check variables before running the game")]
    private bool checkVariables = true;


    //Changes the active VisualScene
    public void ChangeScene(int scene)
    {
        activeScene++;
        if (activeScene > SceneManager.sceneCount)
        {
            activeScene = 0;
        }
        cellManager.LoadVisualScene(visualSceneList[activeScene]);
    }


    //Sets up the visual side of things; to be called by GameManager
    public void SetUp(GameManager gameM)
    {
        gameManager = gameM;

        InstantiateVisualScenes();

        cellManager = GetComponent<CellManager>();
        if (cellManager != null)
        {
            cellManager.SetUp(gameManager);
        }
        else
        {
            gameManager.Error("CellManager is not set on VisualManager");
        }

        bodySimulation = FindObjectOfType<BodySimulation>();
    }
    //Generates scenes using the visualScenePrefab
    private void InstantiateVisualScenes()
    {
        visualSceneList.Clear();

        GameObject newSceneObject = Instantiate(visualScenePrefab, visualSceneParent.transform);
        visualSceneList.Add(newSceneObject.GetComponent<VisualScene>());
        newSceneObject.name = "Thymus";

        newSceneObject = Instantiate(visualScenePrefab, visualSceneParent.transform);
        visualSceneList.Add(newSceneObject.GetComponent<VisualScene>());
        newSceneObject.name = "Heart";

        //instantiate rest of scenes
        //make sure to set other scenes as inactive


        foreach (VisualScene scene in visualSceneList)
        {
            scene.SetUp(2.5f, visualSceneList, 3, 100);
        }
    }
    
    //returns whether or not the level is ready, and will output why the level is not ready
    bool isLevelReady()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("gameManager is null on VisualManager");
            return false;
        }

        //if (simulationManager == null)
        //{
        //    Debug.LogWarning("simulationManager is null on VisualManager");
        //    return false;
        //}

        foreach (VisualScene scene in visualSceneList)
        {
            if (scene == null)
            {
                Debug.LogWarning("VisualScene is null on VisualManager");
                return false;
            }
        }

        return checkVariables;
    }



    //This will be called by the Simulation Manager to update the current scene numbers
    public void ReceiveSimulationNumbers()
    {
        //use ActiveScene int to index into the BodySections list and pull the correct numbers from there
        //then send those numbers to the relevant scripts in the scene

        // Change for full body section if you use a boolean instead
        if (activeScene == -1)
        {
            int civilianDeathCount = bodySimulation.CivilianDeathCount;
            float resourceDemandPercent = bodySimulation.ResourceDemandPercent;
            float resourceProductionPercent = bodySimulation.ResourceProductionPercent;
            float resourceUsagePercent = (resourceDemandPercent / resourceProductionPercent) * 100f;
            return;
        }

        // Get the active section
        BodySectionSimulation section = bodySimulation.Sections[activeScene];

        // Response 0-100, Response type
        float responsePercent = section.Response.LevelPercent;

        ImmuneSystemResponse.ResponseType responseType = section.Response.Type;
        //convert ResponseType to int
        int responseTypeInt = 0;

        // Infection 0-100
        float infectionPercent = section.InfectionProgressPercent;

        // Stress 0-100
        float stressPercent = section.StressLevelPercent;

        //send info to cellManager
        cellManager.NewSimulationNumbers(responsePercent, infectionPercent, responseTypeInt);

        //send info to visualScene, for color changes
        if (activeScene != -1)
        {
            visualSceneList[activeScene].ShiftColor(stressPercent, infectionPercent);
        }
    }
}
