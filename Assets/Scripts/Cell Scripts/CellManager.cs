using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CellManager : MonoBehaviour
{
    /*
     * This will manage how cells are loaded in, as well as telling them what they should be doing
     */

    //reference to the VisualManager
    private VisualManager visualManager;
    //reference to the GameManager; mainly for error messages
    private GameManager gameManager;

    //reference to the active scene, just to do some functions and get data
    private VisualScene activeScene;
    //List of vector3 pathing links, for cells to enter/exit scene
    private List<Vector3> pathingPositions;

    //Used when loading stuff in to prevent this from running before stuff is fully set up
    private bool canUpdate = false;

    [Header("Object Pooling Helpers, Must be set on compile time")]
    //These are used to "create" objects as needed
    [SerializeField, Tooltip("Used to place generated object pools in, for organization")]
    private GameObject objectPoolsParent;
    [SerializeField]
    private ObjectPoolingHelper civilianPool;
    public List<Cell> civilians;
    [SerializeField]
    private ObjectPoolingHelper macrophagePool;
    public List<Cell> macrophages;
    [SerializeField]
    private ObjectPoolingHelper neutrophilePool;
    public List<Cell> neutrophiles;
    //[SerializeField]
    //private ObjectPoolingHelper TCellPool;
    //private List<Cell> TCells;
    //[SerializeField]
    //private ObjectPoolingHelper BCellPool;
    //private List<Cell> BCells;
    //[SerializeField, Tooltip("This is a prefab, not something already in the scene")]
    //private ObjectPoolingHelper bacteriaPoolPre;
    //This is in list format because there can be multiple types of bacteria
    //private List<ObjectPoolingHelper> bacteriaPools;
    //private List<List<Cell>> bacteria;

    //this is just temp
    public ObjectPoolingHelper bacteriaPool;
    public List<Cell> bacteria;

    [Header("Cell variables")]
    //List of civilian cell positions to spawn new cells in, retrieved from the active VisualScene
    public List<Vector3> civilianCellPositions;
    //List of bools that keep track of which positions are occupied
    public List<bool> isCivilianSpotUsed = new List<bool>();
    [Tooltip("The maximum number of civilian cells, will update the object pool")]
    public int maxCivilians;
    //This is the actual max, set by the Visual Scene
    private int mCivilians;
    [Tooltip("The number of civilian cells the scene should have")]
    public int targetCivilians;

    public int maxMacrophages;
    private int mMacrophages;
    public int targetMacrophages;
    public int maxNeutrophiles;
    private int mNeutrophiles;
    public int targetNeutrophiles;
    //public int maxTCells = 15;
    //public int targetTCells;
    //public int maxBCells = 10;
    //public int targetBCells;
    [Tooltip("This is not the total max for all bacteria types, but for each individual type")]
    public int maxBacteria;
    private int mBacteria;
    public int targetBacteria;

    [Header("Screen Bounds")]
    [Tooltip("The scale of the active scene, used to change the cell bounds")]
    public float sceneScale = 1.0f;
    [SerializeField, Tooltip("Vertical Max(absolute value)")]
    private float verticalMax = 5;
    [SerializeField, Tooltip("Horizontal Max(absolute value)")]
    private float horizontalMax = 9;

    public Slider responseSlider;
    public Slider infectionSlider;



    public bool SetUp(GameManager gameM)
    {
        canUpdate = false;

        gameManager = gameM;

        visualManager = GetComponent<VisualManager>();

        SetUpObjectPools();

        DeactivateAllCells();

        return true;
    }
    private void SetUpObjectPools()
    {
        //We need more civilians than any other cell type
        civilianPool.SetPoolCount(maxCivilians * 2, false);
        civilians = new List<Cell>();
        macrophagePool.SetPoolCount(maxMacrophages, false);
        macrophages = new List<Cell>();
        neutrophilePool.SetPoolCount(maxNeutrophiles, false);
        neutrophiles = new List<Cell>();
        //BCellPool.SetPoolCount(maxBCells, false);
        //TCellPool.SetPoolCount(maxBCells, false);

        //temp
        bacteriaPool.SetPoolCount(maxBacteria, false);
    }



    //To be called by VisualManager to load in a VisualScene
    public void LoadVisualScene(VisualScene scene)
    {
        canUpdate = false;
        DeactivateAllCells();

        activeScene = scene;

        pathingPositions = activeScene.GetPathPositions();

        //Update screen scaling
        sceneScale = scene.sceneScale;
        verticalMax = 5 * sceneScale;
        horizontalMax = 9 * sceneScale;

        //Get max cell counts
        mCivilians = activeScene.maxCivilians;
        mMacrophages = activeScene.maxMacrophages;
        mNeutrophiles = activeScene.maxNeutrophiles;
        mBacteria = activeScene.maxBacteria;

        //Set up civilian position lists
        civilianCellPositions = activeScene.GetCellPositions();
        isCivilianSpotUsed.Clear();
        foreach (Vector3 v in civilianCellPositions)
        {
            isCivilianSpotUsed.Add(false);
        }

        NewSimulationNumbers(responseSlider.value, infectionSlider.value, 0);
        SetCellNumbers();

        canUpdate = true;
    }

    //Tells all the objectPoolingHelpers to deactivate all cells
    private void DeactivateAllCells()
    {
        civilians.Clear();
        macrophages.Clear();
        neutrophiles.Clear();
        bacteria.Clear();

        civilianPool.DeactivateAllObjects();
        macrophagePool.DeactivateAllObjects();
        neutrophilePool.DeactivateAllObjects();
        bacteriaPool.DeactivateAllObjects();

        isCivilianSpotUsed.Clear();
        foreach (Vector3 v in civilianCellPositions)
        {
            isCivilianSpotUsed.Add(false);
        }
    }



    void Update()
    {
        if (canUpdate)
        {
            NewSimulationNumbers(responseSlider.value, infectionSlider.value, 0);

            //These should be run on gameTicks, not every frame, but oh well
            UpdatePersistCells();
            UpdateCellNumbers();
        }
    }



    //Updates all the active cells by resetting their kill clocks, done to get rid of old cells
    void UpdatePersistCells()
    {
        foreach (Cell cell in civilians)
        {
            cell.ResetPersistTime();
        }
        foreach (Cell cell in macrophages)
        {
            cell.ResetPersistTime();
        }
        foreach (Cell cell in neutrophiles)
        {
            cell.ResetPersistTime();
        }

        //do this for all the cell types

        foreach (Cell cell in bacteria)
        {
            cell.ResetPersistTime();
        }
    }

    //Recieve new simulation numbers, to affect the visuals, scene dependant
    public void NewSimulationNumbers(float response, float infection, int responseType)
    {
        targetCivilians = (int)(mCivilians - (mCivilians * infection));

        //Set macrophages as 60% of response and neutrophiles as 40%
        targetMacrophages = (int)(mMacrophages * (response * 0.6f));
        targetNeutrophiles = (int)(mNeutrophiles * (response * 0.4f));

        targetBacteria = (int)(mBacteria * infection);

        //temp
        if (activeScene != null)
        {
            activeScene.ShiftColor(response, infection);
        }
    }



    //Sets cell tasks and activates new cells to reach target numbers
    void UpdateCellNumbers()
    {
        //Set a maximum processing tick, which will put a limit on how many times the loop can run
        int maxTick = mCivilians * 100;
        //Keep track of how many times the loop runs
        int tick = 0;

        //Civilian cells
        if (targetCivilians != civilians.Count)
        {
            //Will run until the civilian count is correct or it reaches the processing limit
            while (civilians.Count < targetCivilians && tick < maxTick)
            {
                //Get the next avaliable cell from the object pool
                GameObject cell = civilianPool.GetNextObject();

                //If the object pool is out, or there are no spots open, then stop activating cells
                if (cell == null || !CivilianSpotsOpen())
                {
                    Debug.Log("Missing cell componet or no spots open: civilians");
                    break;
                }

                //Get a Cell.cs reference
                Cell c = cell.GetComponent<Cell>();

                //Until the cell is active, or we are out of processing time
                while (!cell.activeInHierarchy && tick < maxTick)
                {
                    //Get a random civilian spot and test if it is not being used
                    int randomSpot = Random.Range(0, isCivilianSpotUsed.Count);
                    if (!isCivilianSpotUsed[randomSpot])
                    {
                        //Set the spot as used
                        isCivilianSpotUsed[randomSpot] = true;

                        //Activate the cell
                        cell.SetActive(true);
                        c.cellSpot = randomSpot;
                        c.ActivateCell(civilianCellPositions[randomSpot]);
                        civilians.Add(c);
                        break;
                    }

                    tick++;
                }

                tick++;
            }
            //If we ran out of processing time, then log that
            if (tick >= maxTick)
            {
                Debug.Log("Too many attempts: civilians");
            }

            //Reset tick for killing old cells
            tick = 0;

            while (civilians.Count > targetCivilians && tick < maxTick)
            {
                Cell c = civilians[Random.Range(0, civilians.Count)];

                //if chance and there are bacteria in the scene, then select a random bacteria to kill the cell
                if (Random.Range(0f, 10f) > 0.3f && bacteria.Count > 0)
                {
                    Cell b = bacteria[Random.Range(0, bacteria.Count)];
                    b.NewTask(4, c.gameObject);
                }
                else
                {
                    c.NewTask(10);
                }

                //set the spot to unused
                isCivilianSpotUsed[c.cellSpot] = false;
                civilians.Remove(c);

                tick++;
            }
            if (tick >= maxTick)
            {
                Debug.Log("Too many attempts: civilians");
            }
        }

        maxTick = mMacrophages * 10;
        tick = 0;
        //Macrophages
        if (targetMacrophages != macrophages.Count)
        {
            while (macrophages.Count < targetMacrophages && tick < maxTick)
            {
                GameObject cell = macrophagePool.GetNextObject();
                if (cell == null)
                {
                    Debug.Log("Missing cell componet: macrophages");
                    break;
                }

                Cell m = cell.GetComponent<Cell>();
                cell.SetActive(true);

                float chance = Random.Range(0f, 10f);
                //small chance to duplicate, large chance to enter from path, small chance to enter from random position
                if (chance > 0.97f && macrophages.Count > 0)
                {
                    GameObject spawnCell = macrophages[Random.Range(0, macrophages.Count)].gameObject;
                    m.ActivateCell(spawnCell.transform.position, verticalMax, horizontalMax);
                }
                else if (chance > 0.2f && pathingPositions.Count > 0)
                {
                    m.ActivateCell(pathingPositions[Random.Range(0, pathingPositions.Count)], verticalMax, horizontalMax);
                }
                else
                {
                    m.ActivateCell(GetRandomEdgePosition(), verticalMax, horizontalMax);
                }

                macrophages.Add(m);

                tick++;
            }
            if (tick >= maxTick)
            {
                Debug.Log("Too many attempts: macrophages");
            }

            tick = 0;

            while (macrophages.Count < targetMacrophages && tick < maxTick)
            {
                Cell m = macrophages[Random.Range(0, macrophages.Count)];

                float chance = Random.Range(0f, 10f);
                //Small chance of aptosis, large chance of moving to path, small chance of moving to random off screen
                if (chance > 0.8f)
                {
                    m.NewTask(10);
                }
                else if (chance > 0.3f && pathingPositions.Count > 0)
                {
                    m.NewTask(1, pathingPositions[Random.Range(0, pathingPositions.Count)]);
                    m.NewTask(11);
                }
                else
                {
                    m.NewTask(1, GetRandomEdgePosition());
                    m.NewTask(11);
                }

                macrophages.Remove(m);

                tick++;
            }
            if (tick >= maxTick)
            {
                Debug.Log("Too many attempts: macrophages");
            }
        }

        maxTick = mNeutrophiles * 10;
        tick = 0;
        //Neutrophiles
        if (targetNeutrophiles != neutrophiles.Count)
        {
            while (neutrophiles.Count < targetNeutrophiles && tick < maxTick)
            {
                GameObject cell = neutrophilePool.GetNextObject();
                if (cell == null)
                {
                    Debug.Log("Missing cell componet: neutrophiles");
                    break;
                }

                Cell n = cell.GetComponent<Cell>();
                cell.SetActive(true);

                float chance = Random.Range(0f, 10f);
                //small chance to duplicate, large chance to enter from path, small chance to enter from random position
                if (chance > 0.98f && neutrophiles.Count > 0)
                {
                    GameObject spawnCell = neutrophiles[Random.Range(0, neutrophiles.Count)].gameObject;
                    n.ActivateCell(spawnCell.transform.position, verticalMax, horizontalMax);
                }
                else if (chance > 0.2f && pathingPositions.Count > 0)
                {
                    n.ActivateCell(pathingPositions[Random.Range(0, pathingPositions.Count)], verticalMax, horizontalMax);
                }
                else
                {
                    n.ActivateCell(GetRandomEdgePosition(), verticalMax, horizontalMax);
                }

                neutrophiles.Add(n);

                tick++;
            }
            if (tick >= maxTick)
            {
                Debug.Log("Too many attempts: neutrophiles");
            }

            tick = 0;

            while (neutrophiles.Count < targetNeutrophiles && tick < maxTick)
            {
                Cell n = neutrophiles[Random.Range(0, neutrophiles.Count)];

                float chance = Random.Range(0f, 10f);
                //Small chance of aptosis, large chance of moving to path, small chance of moving to random off screen
                if (chance > 0.8f)
                {
                    n.NewTask(10);
                }
                else if (chance > 0.3f && pathingPositions.Count > 0)
                {
                    n.NewTask(1, pathingPositions[Random.Range(0, pathingPositions.Count)]);
                    n.NewTask(11);
                }
                else
                {
                    n.NewTask(1, GetRandomEdgePosition());
                    n.NewTask(11);
                }

                neutrophiles.Remove(n);

                tick++;
            }
            if (tick >= maxTick)
            {
                Debug.Log("Too many attempts: neutrophiles");
            }
        }

        maxTick = mBacteria * 10;
        tick = 0;
        //Bacteria
        if (targetBacteria != bacteria.Count)
        {
            while (bacteria.Count < targetBacteria && tick < maxTick)
            {
                GameObject cell = bacteriaPool.GetNextObject();
                if (cell == null)
                {
                    Debug.Log("Missing cell componet: bacteria");
                    break;
                }

                Cell b = cell.GetComponent<Cell>();
                cell.SetActive(true);

                float chance = Random.Range(0f, 10f);
                //large chance to duplicate, small chance to enter from path; if those fail then enter from random edge
                if (chance > 0.9f && pathingPositions.Count > 0)
                {
                    b.ActivateCell(pathingPositions[Random.Range(0, pathingPositions.Count)], verticalMax, horizontalMax);
                    
                }
                else if (bacteria.Count > 0)
                {
                    GameObject spawnCell = bacteria[Random.Range(0, bacteria.Count)].gameObject;
                    b.ActivateCell(spawnCell.transform.position, verticalMax, horizontalMax);
                }
                else
                {
                    b.ActivateCell(GetRandomEdgePosition(), verticalMax, horizontalMax);
                }

                bacteria.Add(b);

                tick++;
            }
            if (tick >= maxTick)
            {
                Debug.Log("Too many attempts: bacteria");
            }

            tick = 0;

            while (bacteria.Count > targetBacteria && tick < maxTick)
            {
                Cell b = bacteria[Random.Range(0, bacteria.Count)];

                float chance = Random.Range(0f, 10f);
                //large chance to get killed, small chance to leave screen
                if (chance > 0.9f && pathingPositions.Count > 0)
                {
                    Cell killingCell;
                    if (chance > 0.7f && neutrophiles.Count > 0)
                    {
                        killingCell = neutrophiles[Random.Range(0, neutrophiles.Count)];
                        killingCell.NewTask(4, b.gameObject);
                    }
                    else if (macrophages.Count > 0)
                    {
                        killingCell = macrophages[Random.Range(0, macrophages.Count)];
                        killingCell.NewTask(4, b.gameObject);
                    }
                    else
                    {
                        b.NewTask(10);
                    }
                }
                else
                {
                    b.NewTask(1, GetRandomEdgePosition());
                    b.NewTask(11);
                }

                bacteria.Remove(b);

                tick++;
            }
            if (tick >= maxTick)
            {
                Debug.Log("Too many attempts: bacteria");
            }
        }

    }
    //Simply activates new cells and places them in the scene during loading
    void SetCellNumbers()
    {
        //Civilians

        //Set a maximum processing tick, which will put a limit on how many times the loop can run
        int maxTick = mCivilians * 100;
        //Keep track of how many times the loop runs
        int tick = 0;

        //Will run until the civilian count is correct or it reaches the processing limit
        while (civilians.Count < targetCivilians && tick < maxTick)
        {
            //Get the next avaliable cell from the object pool
            GameObject cell = civilianPool.GetNextObject();

            //If the object pool is out, or there are no spots open, then stop activating cells
            if (cell == null || !CivilianSpotsOpen())
            {
                Debug.Log("Missing cell componet or no spots open: civilians");
                break;
            }
            
            //Get a Cell.cs reference
            Cell c = cell.GetComponent<Cell>();

            //Until the cell is active, or we are out of processing time
            while (!cell.activeInHierarchy && tick < maxTick)
            {
                //Get a random civilian spot and test if it is not being used
                int randomSpot = Random.Range(0, isCivilianSpotUsed.Count);
                if (!isCivilianSpotUsed[randomSpot])
                {
                    //Set the spot as used
                    isCivilianSpotUsed[randomSpot] = true;

                    //Activate the cell
                    cell.SetActive(true);
                    c.cellSpot = randomSpot;
                    c.ActivateCell(civilianCellPositions[randomSpot]);
                    civilians.Add(c);
                    break;
                }

                tick++;
            }

            tick++;
        }
        //If we ran out of processing time, then log that
        if (tick >= maxTick)
        {
            Debug.Log("Too many attempts: civilians");
        }

        //Macrophages
        maxTick = mMacrophages * 10;
        tick = 0;
        while (macrophages.Count < targetMacrophages && tick < maxTick)
        {
            GameObject cell = macrophagePool.GetNextObject();
            if (cell == null)
            {
                Debug.Log("Missing cell componet: macrophages");
                break;
            }

            Cell m = cell.GetComponent<Cell>();

            cell.SetActive(true);
            m.ActivateCell(GetRandomPosition(), verticalMax, horizontalMax);
            macrophages.Add(m);

            tick++;
        }
        if (tick >= maxTick)
        {
            Debug.Log("Too many attempts: macrophages");
        }

        //Neutrophiles
        maxTick = mNeutrophiles * 10;
        tick = 0;
        while (neutrophiles.Count < targetNeutrophiles && tick < maxTick)
        {
            GameObject cell = neutrophilePool.GetNextObject();
            if (cell == null)
            {
                Debug.Log("Missing cell componet: neutrophiles");
                break;
            }

            Cell n = cell.GetComponent<Cell>();

            cell.SetActive(true);
            n.ActivateCell(GetRandomPosition(), verticalMax, horizontalMax);
            neutrophiles.Add(n);

            tick++;
        }
        if (tick >= maxTick)
        {
            Debug.Log("Too many attempts: neutrophiles");
        }

        //Bacteria
        maxTick = mBacteria * 10;
        tick = 0;
        while (bacteria.Count < targetBacteria && tick < maxTick)
        {
            GameObject cell = bacteriaPool.GetNextObject();
            if (cell == null)
            {
                Debug.Log("Missing cell componet: bacteria");
                break;
            }

            Cell b = cell.GetComponent<Cell>();

            cell.SetActive(true);
            b.ActivateCell(GetRandomPosition(), verticalMax, horizontalMax);
            bacteria.Add(b);

            tick++;
        }
        if (tick >= maxTick)
        {
            Debug.Log("Too many attempts: bacteria");
        }
    }
    //Checks if there are any civilian spots left
    bool CivilianSpotsOpen()
    {
        bool spotsOpen = false;
        foreach (bool spot in isCivilianSpotUsed)
        {
            if (!spot)
            {
                spotsOpen = true;
                break;
            }
        }

        if (!spotsOpen)
        {
            Debug.Log("No spots open");
        }
        return spotsOpen;
    }



    ////Sets up bacteria stuff when a new one instantiates
    //void NewBacteria()
    //{

    //}
    ////removes a specific bacteria type from all scenes
    //void EradicateBacteria()
    //{

    //}



    //Helper functions when placing cells
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

        return new Vector3(x, y, 0);
    }
    //Returns a vector3 that is near the edge bounds
    private Vector3 GetRandomNearEdgePosition()
    {
        Vector3 newPos = new Vector3();
        newPos.z = 0;

        // Generate a random angle in radians
        float angle = Random.Range(0f, Mathf.PI * 2);

        // Generate a distance factor biased towards the edges by squaring the distance
        float distance = Mathf.Pow(Random.Range(1.0f, 2.0f), 2) - 1;

        // Calculate position using polar coordinates, scaled to the map size
        newPos.x = Mathf.Clamp(Mathf.Cos(angle) * distance * (horizontalMax / 1.5f), -horizontalMax, horizontalMax);
        newPos.y = Mathf.Clamp(Mathf.Sin(angle) * distance * (verticalMax / 1.5f), -verticalMax, verticalMax);

        return newPos;
    }
}
