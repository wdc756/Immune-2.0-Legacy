using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        //other setup stuff here

        //if (!isLevelReady())
        //{
        //    gameManager.Error("Error encountered during VisualManager setup");
        //}
    }
    //Generates scenes using the visualScenePrefab
    private void InstantiateVisualScenes()
    {
        visualSceneList.Clear();
        GameObject newSceneObject = Instantiate(visualScenePrefab, visualSceneParent.transform);
        visualSceneList.Add(newSceneObject.GetComponent<VisualScene>());
        newSceneObject.name = "Thymus";

        //instantiate rest of scenes
        //make sure to set other scenes as inactive
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



    void Update()
    {

    }
}
