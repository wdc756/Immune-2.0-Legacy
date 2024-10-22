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

    [SerializeField, Tooltip("Used to group the ObjectPoolingHelpers together for organization")]
    private GameObject objectPoolingHelpersParent;
    [SerializeField, Tooltip("Prefab used to generate Object Pooling Helpers")]
    private ObjectPoolingHelper objectPoolingHelperPrefab;
    //This is a list because then we can add/subtract stuff as needed during development or for other gamemodes
    [SerializeField, Tooltip("Stores all the ObjectPoolingHelpers used throughout the game")]
    private List<ObjectPoolingHelper> objectPoolingHelperList = new List<ObjectPoolingHelper>();

    //these are here to help us instantiate the relevant ObjectPoolingHelpers easier
    [SerializeField, Tooltip("Reference to ObjectPoolingHelper for Prefab civilianCells")]
    private GameObject civilianPoolPre;
    [SerializeField, Tooltip("Reference to ObjectPoolingHelper for Prefab macrophages")]
    private GameObject macrophagePoolPre;
    [SerializeField, Tooltip("Reference to ObjectPoolingHelper for Prefab neutrophiles")]
    private GameObject neutrophilePoolPre;
    [SerializeField, Tooltip("Reference to ObjectPoolingHelper for Prefab TCells")]
    private GameObject TCellPoolPre;
    [SerializeField, Tooltip("Reference to ObjectPoolingHelper for Prefab BCells")]
    private GameObject BCellPoolPre;
    [SerializeField, Tooltip("Reference to ObjectPoolingHelper for Prefab bacteria")]
    private GameObject bacteriaPoolPre;
    [SerializeField, Tooltip("Reference to ObjectPoolingHelper for Prefab infected Cells")]
    private GameObject infectedCellPoolPre;

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
        VisualScene civilianCells = new VisualScene();
        //civilianCells.
    }
    //checks if all the object pooling helper prefabs are not null
    private void CheckObjectPoolingHelperPrefabs()
    {
        if (civilianPoolPre == null)
        {
            Debug.LogWarning("civilianPoolPre is null.");
        }
        if (macrophagePoolPre == null)
        {
            Debug.LogWarning("macrophagePoolPre is null.");
        }
        if (neutrophilePoolPre == null)
        {
            Debug.LogWarning("neutrophilePoolPre is null.");
        }
        if (TCellPoolPre == null)
        {
            Debug.LogWarning("TCellPoolPre is null.");
        }
        if (BCellPoolPre == null)
        {
            Debug.LogWarning("BCellPoolPre is null.");
        }
        if (bacteriaPoolPre == null)
        {
            Debug.LogWarning("bacteriaPoolPre is null.");
        }
        if (infectedCellPoolPre == null)
        {
            Debug.LogWarning("infectedCellPoolPre is null.");
        }
    }
    //Generates objectpools using the objectPoolingHelperPrefab
    private void InstantiateObjectPoolingHelpers()
    {
        //In the future we can only choose to load in what we need by passing ints through the function

        ObjectPoolingHelper civilians = Instantiate(civilianPoolPre, objectPoolingHelpersParent.transform).GetComponent<ObjectPoolingHelper>();
        objectPoolingHelperList.Add(civilians);

        ObjectPoolingHelper macrophages = Instantiate(macrophagePoolPre, objectPoolingHelpersParent.transform).GetComponent<ObjectPoolingHelper>();
        objectPoolingHelperList.Add(macrophages);
        ObjectPoolingHelper neutrophils = Instantiate(neutrophilePoolPre, objectPoolingHelpersParent.transform).GetComponent<ObjectPoolingHelper>();
        objectPoolingHelperList.Add(neutrophils);
        ObjectPoolingHelper TCells = Instantiate(TCellPoolPre, objectPoolingHelpersParent.transform).GetComponent<ObjectPoolingHelper>();
        objectPoolingHelperList.Add(TCells);
        ObjectPoolingHelper BCells = Instantiate(BCellPoolPre, objectPoolingHelpersParent.transform).GetComponent<ObjectPoolingHelper>();
        objectPoolingHelperList.Add(BCells);
        ObjectPoolingHelper bacteria = Instantiate(bacteriaPoolPre, objectPoolingHelpersParent.transform).GetComponent<ObjectPoolingHelper>();
        objectPoolingHelperList.Add(bacteria);
        ObjectPoolingHelper infectedCells = Instantiate(infectedCellPoolPre, objectPoolingHelpersParent.transform).GetComponent<ObjectPoolingHelper>();
        objectPoolingHelperList.Add(infectedCells);
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

        if (objectPoolingHelpersParent == null)
        {
            Debug.LogWarning("objectPoolingHelpersParent is null on VisualManager");
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
