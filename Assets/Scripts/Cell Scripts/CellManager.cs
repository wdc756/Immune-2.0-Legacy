using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour
{
    /*
     * This will manage how cells are loaded in, as well as telling them what they should be doing
     */

    //reference to the VisualManager
    private VisualManager visualManager;
    //reference to the GameManager; mainly for error messages
    private GameManager gameManager;

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

    [SerializeField, Tooltip("Vertical Max(absolute value)")]
    private float verticalMax = 21.0f;
    [SerializeField, Tooltip("Horizontal Max(absolute value)")]
    private float horizontalMax = 36.0f;

    public int MaxCivilianCells = 50;
    public int MaxImmuneCells = 50;
    public int MaxBacteraCells = 50;

    public float responseLevel = 0.1f;
    public float infectionLevel = 0.05f;

    public List<Cell> immuneCells = new List<Cell>();
    public List<Cell> bacteriaCells = new List<Cell>();

    public ObjectPoolingHelper immunePool;
    public ObjectPoolingHelper bacteriaPool;

    public bool SetUp(GameManager gameM)
    {
        gameManager = gameM;

        visualManager = GetComponent<VisualManager>();

        if (CheckObjectPoolingHelperPrefabs())
        {
            InstantiateObjectPoolingHelpers();
            return true;
        }
        else
        {
            return false;
        }
    }

    void Update()
    {
        UpdateCellNumbers();


    }

    public void RecieveSimulationNumbers(float response, float infection)
    {
        responseLevel = response;
        infectionLevel = infection;
    }

    private void UpdateCellNumbers()
    {
        int iCells = Mathf.Clamp(Mathf.RoundToInt(MaxImmuneCells * responseLevel), 0, MaxImmuneCells);

        while (immuneCells.Count > iCells)
        {
            RemoveCell(immuneCells[Random.Range(0, immuneCells.Count)]);
        }

        while (immuneCells.Count < iCells)
        {
            GameObject newCellObject = immunePool.GetNextObject();

            if (newCellObject != null)
            {
                newCellObject.SetActive(true);
                Cell newCell = newCellObject.GetComponent<Cell>();
                newCell.ActivateCell(GetRandomEdgePosition());
                newCell.NewTask(1, GetRandomPosition());
                immuneCells.Add(newCell);
            }
        }

        int bCells = Mathf.Clamp(Mathf.RoundToInt(MaxBacteraCells * infectionLevel), 0, MaxBacteraCells);

        while (bacteriaCells.Count > bCells)
        {
            RemoveCell(bacteriaCells[Random.Range(0, bacteriaCells.Count)]);
        }

        while (bacteriaCells.Count < bCells)
        {
            GameObject newCellObject = bacteriaPool.GetNextObject();

            if (newCellObject != null)
            {
                newCellObject.SetActive(true);
                Cell newCell = newCellObject.GetComponent<Cell>();
                newCell.ActivateCell(GetRandomEdgePosition());
                newCell.NewTask(1, GetRandomPosition());
                bacteriaCells.Add(newCell);
            }
        }
    }

    private void RemoveCell(Cell cell)
    {
        if (cell.canMove)
        {
            //Clear the cell's queue, then tell it to move off the screen, and then to deactivate
            cell.ClearTasks();
            cell.NewTask(1, GetRandomEdgePosition());
            cell.NewTask(4);
        }
        else
        {
            cell.DeactivateCell();
        }

        switch (cell.type)
        {
            case 2:
                immuneCells.Remove(cell);
                break;
            case 3:
                bacteriaCells.Remove(cell);
                break;
        }
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 newPosition = new Vector3();

        newPosition.x = Random.Range(-horizontalMax, horizontalMax);
        newPosition.y = Random.Range(-verticalMax, verticalMax);

        return newPosition;
    }

    private Vector3 GetRandomEdgePosition()
    {
        // Randomly pick a side: 0 = top, 1 = bottom, 2 = left, 3 = right
        int side = Random.Range(0, 4);

        float x = 0;
        float y = 0;

        switch (side)
        {
            case 0: // Top side
                x = Random.Range(-horizontalMax, horizontalMax);
                y = verticalMax + Random.Range(0.1f, 1); // Just outside top
                break;
            case 1: // Bottom side
                x = Random.Range(-horizontalMax, horizontalMax);
                y = -verticalMax - Random.Range(0.1f, 1); // Just outside bottom
                break;
            case 2: // Left side
                x = -horizontalMax - Random.Range(0.1f, 1); // Just outside left
                y = Random.Range(-verticalMax, verticalMax);
                break;
            case 3: // Right side
                x = horizontalMax + Random.Range(0.1f, 1); // Just outside right
                y = Random.Range(-verticalMax, verticalMax);
                break;
        }

        // Set z to 0 if in 2D; adjust as needed for 3D space
        return new Vector3(x, y, 0);
    }

    //checks if all the object pooling helper prefabs are not null
    private bool CheckObjectPoolingHelperPrefabs()
    {
        bool isReady = true;

        if (civilianPoolPre == null)
        {
            gameManager.Error("civilianPoolPre is null.");
            isReady = false;
        }
        if (macrophagePoolPre == null)
        {
            gameManager.Error("macrophagePoolPre is null.");
            isReady = false;
        }
        if (neutrophilePoolPre == null)
        {
            gameManager.Error("neutrophilePoolPre is null.");
            isReady = false;
        }
        if (TCellPoolPre == null)
        {
            gameManager.Error("TCellPoolPre is null.");
            isReady = false;
        }
        if (BCellPoolPre == null)
        {
            gameManager.Error("BCellPoolPre is null.");
            isReady = false;
        }
        if (bacteriaPoolPre == null)
        {
            gameManager.Error("bacteriaPoolPre is null.");
            isReady = false;
        }
        if (infectedCellPoolPre == null)
        {
            gameManager.Error("infectedCellPoolPre is null.");
            isReady = false;
        }

        return isReady;
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
}
