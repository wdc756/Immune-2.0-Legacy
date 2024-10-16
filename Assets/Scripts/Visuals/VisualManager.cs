using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualManager : MonoBehaviour
{
    /*
     Think of this as a sort of int main() but for the visuals. Without this nothing else will really happen

     While this script may not do much directly, it's responsible for coordinating the various systems in place

     This should be attached to the main camera
     */

    [SerializeField, Tooltip("Reference to the GameManager to tell it if there was some sort of error")]
    private GameManager gameManager;

    [SerializeField, Tooltip("Prefab used to generate Visual Scenes")]
    private GameObject visualScenePrefab;
    [SerializeField, Tooltip("Used to reference all the VisualScenes")]
    private List<VisualScene> visualSceneList = new List<VisualScene>();

    [SerializeField, Tooltip("Prefab used to generate Object Pooling Helpers")]
    private ObjectPoolingHelper objectPoolingHelperPrefab;
    //This is a list because then we can add/subtract stuff as needed during development or for other gamemodes
    [SerializeField, Tooltip("Stores all the ObjectPoolingHelpers used throughout the game")]
    private List<ObjectPoolingHelper> objectPoolingHelperList = new List<ObjectPoolingHelper>();

    //[SerializeField, Tooltip("Reference to ObjectPoolingHelper for civilianCells")]
    //private ObjectPoolingHelper civilianPool;
    //[SerializeField, Tooltip("Reference to ObjectPoolingHelper for macrophages")]
    //private ObjectPoolingHelper macrophagePool;
    //[SerializeField, Tooltip("Reference to ObjectPoolingHelper for neutrophiles")]
    //private ObjectPoolingHelper neutrophilePool;
    //[SerializeField, Tooltip("Reference to ObjectPoolingHelper for TCells")]
    //private ObjectPoolingHelper TCellPool;
    //[SerializeField, Tooltip("Reference to ObjectPoolingHelper for BCells")]
    //private ObjectPoolingHelper BCellPool;
    //[SerializeField, Tooltip("Reference to ObjectPoolingHelper for bacteria")]
    //private ObjectPoolingHelper bacteriaPool;
    //[SerializeField, Tooltip("Reference to ObjectPoolingHelper for infected Cells")]
    //private ObjectPoolingHelper infectedCellPool;

    [SerializeField, Tooltip("Reference to the SimulationManager for data; DO NOT SET ON COMPILE TIME, this will be set during runtime")]
    private SimulationManager simulationManager;

    [SerializeField, Tooltip("Determines if the script should check variables before running the game")]
    private bool checkVariables = true;



    //Sets up the visual side of things
    void SetUpLevel()
    {
        InstantiateVisualScenes();
        InstantiateObjectPoolingHelpers();

        //other setup stuff here

        if (!isLevelReady())
        {
            gameManager.Error("Error encountered during VisualManager setup");
        }
    }
    //Generates scenes using the visualScenePrefab
    private void InstantiateVisualScenes()
    {

    }
    //Generates objectpools using the objectPoolingHelperPrefab
    private void InstantiateObjectPoolingHelpers()
    {

    }
    //returns whether or not the level is ready, and will output why the level is not ready
    bool isLevelReady()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("gameManager is null on VisualManager");
            return false;
        }

        if (simulationManager == null)
        {
            Debug.LogWarning("simulationManager is null on VisualManager");
            return false;
        }

        foreach (VisualScene scene in visualSceneList)
        {
            if (scene == null)
            {
                Debug.LogWarning("VisualScene is null on VisualManager");
                return false;
            }
        }

        foreach(ObjectPoolingHelper helper in objectPoolingHelperList)
        {
            if (helper == null)
            {
                Debug.LogWarning("ObjectPoolingHelper is null on VisualManager");
                return false;
            }
        }

        return checkVariables;
    }



    void Update()
    {
        
    }


}
