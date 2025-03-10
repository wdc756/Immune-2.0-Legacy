using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VisualManager : MonoBehaviour
{
    /*
     Think of this as a sort of int main() but for the visuals. Without this nothing else will really happen

     While this script may not do much directly, it's responsible for coordinating the various systems in place
     */

    //Makes sure that stuff doesn't get called too early
    private bool canRun = false;

    //Reference to the GameManager to tell it if there was some sort of error
    private GameManager gameManager;
    //Reference to the CellManager
    private CellManager cellManager;
    // reference to the body simulation
    private BodySimulation bodySimulation;
    //Used to generate new link buttons
    private UIManager uiManager;


    //The main camera, used to change the camerasize float according to the size of the active VisualScene
    public Camera cam;

    [Tooltip("The active visual scene that the visual system is rendering; change this number to change the starting scene")]
    public int activeScene = 0;
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

    [SerializeField, Tooltip("The Thymus UI")]
    private GameObject thymusUI;


    //Changes the active VisualScene
    public void ChangeScene(int scene)
    {
        //stop update functions
        canRun = false;

        //Disable the active scene
        visualSceneList[activeScene].gameObject.SetActive(false);

        //If the new scene is -1, index through the list of scenes
        if (scene == -1)
        {
            activeScene++;
        }
        else
        {
            activeScene = scene;
        }
        //Check for indexing overflow
        if (activeScene >= visualSceneList.Count)
        {
            activeScene = 0;
        }

        //if on thymus, then turn on thymus UI
        if (activeScene == 0)
        {
            thymusUI.SetActive(true);
        }
        else
        {
            thymusUI.SetActive(false);
        }

        //activate the new current scene
        visualSceneList[activeScene].gameObject.SetActive(true);

        //turn update functions back on
        canRun = true;

        //update the camera size
        cam.orthographicSize = visualSceneList[activeScene].sceneScale * 5f;

        //Reset and create new link buttons
        uiManager.LoadSceneLinkButtons();
        //Update the toolbar
        if (activeScene != 0)
        {
            uiManager.toolbar.SetActive(true);
        }
        else
        {
            uiManager.toolbar.SetActive(false);
        }

        //Setup cellmanager
        cellManager.LoadVisualScene(visualSceneList[activeScene]);
        ReceiveSimulationNumbers();
        cellManager.SetCellNumbers();

        //Debug.Log("Loaded " + visualSceneList[activeScene].gameObject.name);
    }
    public void ResetActiveScene()
    {
        canRun = false;

        VisualScene scene = visualSceneList[activeScene];

        int index = scene.sceneIndex;
        float scale = scene.sceneScale;
        int numAnchors = scene.GetNumAnchors();
        int numCivilians = scene.maxCivilians;
        int numMacrophages = scene.maxMacrophages;
        int numNeutrophiles = scene.maxNeutrophiles;
        int numBacteria = scene.maxBacteria;

        scene.SetUp(index, scale, visualSceneList, numAnchors, numCivilians, numMacrophages, numNeutrophiles, numBacteria);

        //Debug.Log("Reset " + scene.gameObject.name);

        uiManager.DestroyAllLinkButtons();

        ChangeScene(activeScene);
    }
    //returns the active scene from the list
    public VisualScene GetActiveScene()
    {
        return visualSceneList[activeScene];
    }


    //Sets up the visual side of things; to be called by GameManager
    public void SetUp(GameManager gameM)
    {
        canRun = false;

        gameManager = gameM;

        InstantiateVisualScenes();

        cellManager = GetComponent<CellManager>();
        cellManager.SetUp(gameManager);

        bodySimulation = FindObjectOfType<BodySimulation>();

        uiManager = FindObjectOfType<UIManager>();
        uiManager.SetUp(gameManager);

        ChangeScene(activeScene);
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

        newSceneObject = Instantiate(visualScenePrefab, visualSceneParent.transform);
        visualSceneList.Add(newSceneObject.GetComponent<VisualScene>());
        newSceneObject.name = "Lungs";

        newSceneObject = Instantiate(visualScenePrefab, visualSceneParent.transform);
        visualSceneList.Add(newSceneObject.GetComponent<VisualScene>());
        newSceneObject.name = "Gut";

        newSceneObject = Instantiate(visualScenePrefab, visualSceneParent.transform);
        visualSceneList.Add(newSceneObject.GetComponent<VisualScene>());
        newSceneObject.name = "Liver";



        //SetUp(scale, list of links, number of anchors, number of civilians, number of M, num N, num B

        //A new list varaible used to set up a selection of paths
        List<VisualScene> setUpScenes = new List<VisualScene>
        {
            visualSceneList[1],
            visualSceneList[2]
        };
        visualSceneList[0].SetUp(0, 1f, setUpScenes, 0, 0, 0, 0, 0);
        visualSceneList[0].gameObject.SetActive(false);

        setUpScenes = new List<VisualScene>
        {
            visualSceneList[0],
            visualSceneList[2],
            visualSceneList[3]
        };
        visualSceneList[1].SetUp(1, 3.0f, setUpScenes, 10, 275, 100, 75, 150);
        visualSceneList[1].gameObject.SetActive(false);

        setUpScenes = new List<VisualScene>
        {
            visualSceneList[0],
            visualSceneList[1]
        };
        visualSceneList[2].SetUp(2, 5.0f, setUpScenes, 12, 825, 200, 125, 400);
        visualSceneList[2].gameObject.SetActive(false);

        setUpScenes = new List<VisualScene>
        {
            visualSceneList[1],
            visualSceneList[4]
        };
        visualSceneList[3].SetUp(3, 2.5f, setUpScenes, 7, 225, 75, 100, 125);
        visualSceneList[3].gameObject.SetActive(false);

        setUpScenes = new List<VisualScene>
        {
            visualSceneList[3]
        };
        visualSceneList[4].SetUp(4, 3f, setUpScenes, 8, 275, 100, 125, 150);
        visualSceneList[4].gameObject.SetActive(false);
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



    //This will be called by the Simulation Manager to update the current scene numbers, and by this whenever a scene gets loaded in
    public void ReceiveSimulationNumbers()
    {
        if (canRun)
        {
            //use ActiveScene int to index into the BodySections list and pull the correct numbers from there
            //then send those numbers to the relevant scripts in the scene

            //Check if we've lost
            int civilianDeathCount = bodySimulation.CivilianDeathCount;
            if (civilianDeathCount > gameManager.maxCivilianDeathCount)
            {
                gameManager.Lose();
            }



            // Change for full body section if you use a boolean instead
            if (activeScene == 0)
            {
                float resourceDemandPercent = bodySimulation.ResourceDemandPercent;
                float resourceProductionPercent = bodySimulation.ResourceProductionPercent;
                float resourceUsagePercent = (resourceDemandPercent / resourceProductionPercent) * 100f;

                //Debug.Log($"demand {resourceDemandPercent} prod {resourceProductionPercent}");
                //Debug.Log(resourceUsagePercent);

                //Update Thymus UI
                uiManager.UpdateThymusUI(civilianDeathCount, resourceUsagePercent);

                //send info to cellManager, specifically to reset the target values to avoid unnecesarry calculations
                cellManager.NewSimulationNumbers(0f, 0f, 0);

                return;
            }
            else
            {
                // Get the active section, set back by one because the thymus is an inactive scene
                BodySectionSimulation section = bodySimulation.Sections[activeScene - 1];
                //Debug.Log(section);
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
                //Debug.Log($"response {responsePercent} infection {infectionPercent}");
                cellManager.NewSimulationNumbers(responsePercent / 100f, infectionPercent / 100f, responseTypeInt);
                //Debug.Log("new sim numbers");

                visualSceneList[activeScene].ShiftColor(stressPercent / 100f, infectionPercent / 100f);
            }
        }
    }

    //Called for testing
    public void ReceiveTestingNumbers(float response, float infection, int responseType)
    {
        if (canRun)
        {
            cellManager.NewSimulationNumbers(response, infection, responseType);
            visualSceneList[activeScene].ShiftColor(response, infection);
        }
    }
}
