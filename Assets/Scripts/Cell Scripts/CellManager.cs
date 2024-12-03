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
    [SerializeField]
    private ObjectPoolingHelper dendriticPool;
    public List<Cell> dendritic;
    public int maxDendritic;
    [SerializeField]
    private ObjectPoolingHelper bacteriaPool;
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
    public int targetCivilians;

    public int maxMacrophages;
    private int mMacrophages;
    private int targetMacrophages;
    public int maxNeutrophiles;
    private int mNeutrophiles;
    private int targetNeutrophiles;
    //public int maxTCells = 15;
    //public int targetTCells;
    //public int maxBCells = 10;
    //public int targetBCells;
    public int maxBacteria;
    private int mBacteria;
    public int targetBacteria;
    [Tooltip("The amount the visual sim cells will fight back")]
    public float immuneResilienceAmount;
    private float immuneResilience;
    [Tooltip("The amount of civilian death by the Immune System")]
    public float immuneDamageMultiplier = 0.7f;

    [Header("Screen Bounds")]
    [Tooltip("The scale of the active scene, used to change the cell bounds")]
    public float sceneScale = 1.0f;
    [SerializeField, Tooltip("Vertical Max(absolute value)")]
    private float verticalMax = 5;
    [SerializeField, Tooltip("Horizontal Max(absolute value)")]
    private float horizontalMax = 9;
    //used as a distance check when setting cell kill targets
    public float maxChaseDistance = 4.0f;
    //The actual number used
    private float mChaseDistance;



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
        civilianPool.SetPoolCount((int)(maxCivilians * 1.2f), false);
        civilians = new List<Cell>();
        macrophagePool.SetPoolCount(maxMacrophages, false);
        macrophages = new List<Cell>();
        neutrophilePool.SetPoolCount(maxNeutrophiles, false);
        neutrophiles = new List<Cell>();
        //BCellPool.SetPoolCount(maxBCells, false);
        //TCellPool.SetPoolCount(maxBCells, false);
        dendriticPool.SetPoolCount(maxDendritic, false);


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
        mChaseDistance = maxChaseDistance * sceneScale;

        //Get max cell counts
        mCivilians = activeScene.maxCivilians;
        mMacrophages = activeScene.maxMacrophages;
        mNeutrophiles = activeScene.maxNeutrophiles;
        mBacteria = activeScene.maxBacteria;

        immuneResilience = immuneResilienceAmount * sceneScale;

        //Set up civilian position lists
        civilianCellPositions = activeScene.GetCellPositions();
        isCivilianSpotUsed.Clear();
        foreach (Vector3 v in civilianCellPositions)
        {
            isCivilianSpotUsed.Add(false);
        }

        canUpdate = true;
    }

    //Tells all the objectPoolingHelpers to deactivate all cells
    private void DeactivateAllCells()
    {
        canUpdate = false;

        civilians.Clear();
        macrophages.Clear();
        neutrophiles.Clear();
        bacteria.Clear();
        dendritic.Clear();

        civilianPool.DeactivateAllObjects();
        macrophagePool.DeactivateAllObjects();
        neutrophilePool.DeactivateAllObjects();
        bacteriaPool.DeactivateAllObjects();
        dendriticPool.DeactivateAllObjects();

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
            //NewSimulationNumbers(responseSlider.value, infectionSlider.value, 0);

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
        foreach (Cell cell in dendritic)
        {
            cell.ResetPersistTime();
        }

        //do this for all the cell types

        foreach (Cell cell in bacteria)
        {
            cell.ResetPersistTime();
        }
    }
    //Checks if there are any active bacteria
    public bool AnyBacteriaLeft()
    {
        if (bacteriaPool.AreAnyObjectsActive())
        {
            return true;
        }
        return false;
    }

    //Recieve new simulation numbers, to affect the visuals, scene dependant
    public void NewSimulationNumbers(float response, float infection, int responseType)
    {
        targetBacteria = Mathf.Clamp((int)(mBacteria * infection), 0, mBacteria);
        if (mBacteria == 0)
        {
            targetBacteria = 0;
        }

        targetCivilians = (int)Mathf.Clamp(mCivilians - (mCivilians * ((float)targetBacteria / (float)mBacteria)), 0, mCivilians);
        targetCivilians -= (int)Mathf.Clamp(mCivilians * (response * immuneDamageMultiplier), 0, mCivilians);
        if (mCivilians == 0)
        {
            targetCivilians = 0;
        }

        //Set macrophages as 60% of response and neutrophiles as 40%, if response is low, when high swap
        if (response > 0.3f)
        {
            targetMacrophages = (int)Mathf.Clamp(mMacrophages * (response * 0.4f), 2, mMacrophages);
            if (mMacrophages == 0)
            {
                targetMacrophages = 0;
            }
            targetNeutrophiles = (int)Mathf.Clamp(mNeutrophiles * (response * 0.6f), 1, mNeutrophiles);
            if (mNeutrophiles == 0)
            {
                targetNeutrophiles = 0;
            }
        }
        else
        {
            targetMacrophages = (int)Mathf.Clamp(mMacrophages * (response * 0.6f), 1, mMacrophages);
            if (mMacrophages == 0)
            {
                targetMacrophages = 0;
            }
            targetNeutrophiles = (int)Mathf.Clamp(mNeutrophiles * (response * 0.4f), 2, mNeutrophiles);
            if (mNeutrophiles == 0)
            {
                targetNeutrophiles = 0;
            }
        }

        if ((bacteria.Count > immuneResilience / 2f && Random.Range(0f, 10f) > 5f) || bacteria.Count > immuneResilience)
        {
            UpdateImmuneResilience();
        }
    }
    //Called by the scan button, generates some dendritic cells
    public void StartScan()
    {
        //int targetDendritic = (int)Mathf.Clamp(maxDendritic * (sceneScale / (5 * sceneScale)), 0, maxDendritic);
        //Debug.Log(sceneScale);
        //Debug.Log(targetDendritic);

        int targetDendritic = maxDendritic;

        //check if there are any active scanning cells
        if (!dendriticPool.AreAnyObjectsActive())
        {
            //spawn all of them
            for (int i = 0; i < targetDendritic; i++)
            {
                GameObject cell = dendriticPool.GetNextObject();
                cell.SetActive(true);
                Cell d = cell.GetComponent<Cell>();

                //if already cells in the scene, then somtimes duplicate, otherwise enter
                float chance = Random.Range(0f, 10f);
                if (chance > 8f && dendritic.Count > 0)
                {
                    d.ActivateCell(dendritic[Random.Range(0, dendritic.Count)].gameObject.transform.position);
                }
                else
                {
                    d.ActivateCell(pathingPositions[Random.Range(0, pathingPositions.Count)]);
                }

                //if there are bacteria, then chase/eat a few, then leave, otherwise randomly walk around
                if (bacteria.Count > 0)
                {
                    Cell b = bacteria[Random.Range(0, bacteria.Count)];
                    d.NewTask(5, b.gameObject);
                    bacteria.Remove(b);
                    d.NewTask(1, pathingPositions[Random.Range(0, pathingPositions.Count)]);
                    d.NewTask(10);
                }
                else
                {
                    d.NewTask(1, GetRandomPosition());
                    d.NewTask(1, GetRandomPosition());
                    d.NewTask(1, GetRandomPosition());
                    d.NewTask(1, pathingPositions[Random.Range(0, pathingPositions.Count)]);
                    d.NewTask(10);
                }
            }
        }
    }



    //Sets cell tasks and activates new cells to reach target numbers
    void UpdateCellNumbers()
    {
        if (canUpdate)
        {
            UpdateCivilians();
            UpdateMacrophages();
            UpdateNeutrophiles();
            UpdateBacteria();
        }
    }
    void UpdateCivilians()
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
                    Debug.Log("Missing cell componet or no spots open spawnUpdate: civilians on " + gameObject.name);
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
                Debug.Log("Too many attempts spawnUpdate: civilians on " + gameObject.name);
            }

            //Reset tick for killing old cells
            tick = 0;

            while (civilians.Count > targetCivilians && tick < maxTick)
            {
                Cell c = civilians[Random.Range(0, civilians.Count)];

                float chance = Random.Range(0f, 10f);
                //if chance and there are bacteria/neutrophiles in the scene, then select a random bacteria/neutrophile to kill the cell
                if (bacteria.Count > 0 || neutrophiles.Count > 0)
                {
                    int smallTick = 0;
                    int smallMaxTick = civilians.Count * 10;

                    if (chance > 7f && neutrophiles.Count > 0)
                    {
                        while (smallTick < smallMaxTick)
                        {
                            Cell n = neutrophiles[Random.Range(0, neutrophiles.Count)];
                            if (Vector3.Distance(n.transform.position, n.transform.position) < mChaseDistance * 2f)
                            {
                                n.NewTask(4, c.gameObject);
                                break;
                            }
                            else
                            {
                                smallTick++;
                            }
                        }
                        if (smallTick >= smallMaxTick)
                        {
                            c.NewTask(10);
                        }
                    }
                    else if (chance > 0.3f && bacteria.Count > 0)
                    {
                        while (smallTick < smallMaxTick)
                        {
                            Cell b = bacteria[Random.Range(0, bacteria.Count)];
                            if (Vector3.Distance(b.transform.position, c.transform.position) < mChaseDistance * 2f)
                            {
                                b.NewTask(4, c.gameObject);
                                break;
                            }
                            else
                            {
                                smallTick++;
                            }
                        }
                        if (smallTick >= smallMaxTick)
                        {
                            c.NewTask(10);
                        }
                    }
                    else
                    {
                        c.NewTask(10);
                    }
                    
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
                Debug.Log("Too many attempts destroyUpdate: civilians on " + gameObject.name);
            }
        }
    }
    void UpdateMacrophages()
    {
        //Set a maximum processing tick, which will put a limit on how many times the loop can run
        int maxTick = mMacrophages * 100;
        //Keep track of how many times the loop runs
        int tick = 0;

        if (targetMacrophages != macrophages.Count)
        {
            while (macrophages.Count < targetMacrophages && tick < maxTick)
            {
                GameObject cell = macrophagePool.GetNextObject();
                if (cell == null)
                {
                    Debug.Log("Missing cell componet spawnUpdate: macrophages on" + gameObject.name);
                    break;
                }

                Cell m = cell.GetComponent<Cell>();
                cell.SetActive(true);

                float chance = Random.Range(0f, 10f);
                //small chance to duplicate, large chance to enter from path, small chance to enter from random position
                if (chance > 7f && macrophages.Count > 0)
                {
                    GameObject spawnCell = macrophages[Random.Range(0, macrophages.Count)].gameObject;
                    m.ActivateCell(spawnCell.transform.position, verticalMax, horizontalMax);
                }
                else if (chance > 2f && pathingPositions.Count > 0)
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
                Debug.Log("Too many attempts spawnUpdate: macrophages on" + gameObject.name);
            }

            tick = 0;

            while (macrophages.Count > targetMacrophages && tick < maxTick)
            {
                Cell m = macrophages[Random.Range(0, macrophages.Count)];

                float chance = Random.Range(0f, 10f);
                //Small chance of aptosis, large chance of moving to path, small chance of moving to random off screen
                if (chance > 9.5f)
                {
                    m.NewTask(10);
                }
                else if (chance > 2f && pathingPositions.Count > 0)
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
                Debug.Log("Too many attempts destroyUpdate: macrophages on" + gameObject.name);
            }
        }
    }
    void UpdateNeutrophiles()
    {
        //Set a maximum processing tick, which will put a limit on how many times the loop can run
        int maxTick = mNeutrophiles * 100;
        //Keep track of how many times the loop runs
        int tick = 0;

        if (targetNeutrophiles != neutrophiles.Count)
        {
            while (neutrophiles.Count < targetNeutrophiles && tick < maxTick)
            {
                GameObject cell = neutrophilePool.GetNextObject();
                if (cell == null)
                {
                    Debug.Log("Missing cell componet spawnUpdate: neutrophiles on" + gameObject.name);
                    break;
                }

                Cell n = cell.GetComponent<Cell>();
                cell.SetActive(true);

                float chance = Random.Range(0f, 10f);
                //small chance to duplicate, large chance to enter from path, small chance to enter from random position
                if (chance > 9f && neutrophiles.Count > 0)
                {
                    GameObject spawnCell = neutrophiles[Random.Range(0, neutrophiles.Count)].gameObject;
                    n.ActivateCell(spawnCell.transform.position, verticalMax, horizontalMax);
                }
                else if (chance > 2f && pathingPositions.Count > 0)
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
                Debug.Log("Too many attempts spawnUpdate: neutrophiles on" + gameObject.name);
            }

            tick = 0;

            while (neutrophiles.Count > targetNeutrophiles && tick < maxTick)
            {
                Cell n = neutrophiles[Random.Range(0, neutrophiles.Count)];

                float chance = Random.Range(0f, 10f);
                //Small chance of aptosis, large chance of moving to path, small chance of moving to random off screen
                if (chance > 9f)
                {
                    n.NewTask(10);
                }
                else if (chance > 2f && pathingPositions.Count > 0)
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
                Debug.Log("Too many attempts destroyUpdate: neutrophiles on" + gameObject.name);
            }
        }
    }
    void UpdateBacteria()
    {
        //Set a maximum processing tick, which will put a limit on how many times the loop can run
        int maxTick = mBacteria * 100;
        //Keep track of how many times the loop runs
        int tick = 0;

        if (targetBacteria != bacteria.Count)
        {
            while (bacteria.Count < targetBacteria && tick < maxTick)
            {
                GameObject cell = bacteriaPool.GetNextObject();
                if (cell == null)
                {
                    Debug.Log("Missing cell componet spawnUpdate: bacteria on" + gameObject.name);
                    break;
                }

                Cell b = cell.GetComponent<Cell>();
                cell.SetActive(true);

                float chance = Random.Range(0f, 10f);
                //large chance to duplicate, small chance to enter from path; if those fail then enter from random edge
                if (chance > 9.5f && pathingPositions.Count > 0)
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
                Debug.Log("Too many attempts spawnUpdate: bacteria on" + gameObject.name);
            }

            tick = 0;

            while (bacteria.Count > targetBacteria && tick < maxTick)
            {
                Cell b = bacteria[Random.Range(0, bacteria.Count)];

                float chance = Random.Range(0f, 10f);
                //large chance to get killed, small chance to leave screen
                if (chance > 0.5f && pathingPositions.Count > 0)
                {
                    Cell killingCell;
                    if ((chance > 5.5f || macrophages.Count == 0) && neutrophiles.Count > 0)
                    {
                        int smallTick = 0;
                        int smallMaxTick = neutrophiles.Count * 10;

                        while (smallTick < smallMaxTick)
                        {
                            killingCell = neutrophiles[Random.Range(0, neutrophiles.Count)];
                            if (Vector3.Distance(killingCell.transform.position, b.gameObject.transform.position) < mChaseDistance)
                            {
                                killingCell.NewTask(4, b.gameObject);
                                //Debug.Log("Killing N");
                                break;
                            }
                            else
                            {
                                smallTick++;
                            }
                        }
                        if (smallTick >= smallMaxTick)
                        {
                            //Debug.Log("Fail kill N");
                            b.ClearTasks();
                            b.NewTask(10);
                        }
                    }
                    else if (macrophages.Count > 0)
                    {
                        int smallTick = 0;
                        int smallMaxTick = macrophages.Count * 10;

                        while (smallTick < smallMaxTick)
                        {
                            killingCell = macrophages[Random.Range(0, macrophages.Count)];
                            if (Vector3.Distance(killingCell.transform.position, b.gameObject.transform.position) < mChaseDistance)
                            {
                                killingCell.NewTask(5, b.gameObject);
                                //Debug.Log("Killing M");
                                break;
                            }
                            else
                            {
                                smallTick++;
                            }
                        }
                        if (smallTick >= smallMaxTick)
                        {
                            //Debug.Log("Fail kill M");
                            b.ClearTasks();
                            b.NewTask(10);
                        }
                    }
                    else
                    {
                        //Debug.Log("Fail kill");
                        b.ClearTasks();
                        b.NewTask(10);
                    }
                }
                else
                {
                    //Debug.Log("Leaving");
                    b.ClearTasks();
                    b.NewTask(1, GetRandomEdgePosition());
                    b.NewTask(11);
                }

                bacteria.Remove(b);

                tick++;
            }
            if (tick >= maxTick)
            {
                Debug.Log("Too many attempts destroyUpdate: bacteria on" + gameObject.name);
            }
        }
    }
    void UpdateImmuneResilience()
    {
        int delta = (int)Mathf.Clamp(immuneResilience * Mathf.Sqrt(Random.Range(0f, 0.2f)), 0f, immuneResilience * 2f);

        int newTargetB = targetBacteria - delta;
        //Debug.Log("New targetB: " + newTargetB + " Delta: " + delta + " Resilience: " + immuneResilience);

        //if (delta > 0)
        //{
        //    Debug.Log("Removing " + delta + " bacteria");
        //}

        //if no change, no bacteria, or Immune Cells, don't do logic
        if (delta <= 0 || bacteria.Count == 0 || newTargetB < 0 || (macrophages.Count == 0 && neutrophiles.Count == 0))
        {
            return;
        }

        //Set a maximum processing tick, which will put a limit on how many times the loop can run
        int maxTick = mBacteria * 100;
        //Keep track of how many times the loop runs
        int tick = 0;

        while (bacteria.Count > newTargetB && tick < maxTick)
        {
            Cell b = bacteria[Random.Range(0, bacteria.Count)];

            float chance = Random.Range(0f, 10f);
            Cell killingCell;
            if (chance > 7.5f && neutrophiles.Count > 0)
            {
                killingCell = neutrophiles[Random.Range(0, neutrophiles.Count)];
                killingCell.NewTask(4, b.gameObject);
            }
            else if (macrophages.Count > 0)
            {
                killingCell = macrophages[Random.Range(0, macrophages.Count)];
                killingCell.NewTask(5, b.gameObject);
            }
            else
            {
                b.ClearTasks();
                b.NewTask(10);
            }

            bacteria.Remove(b);

            tick++;
        }
        if (tick >= maxTick)
        {
            Debug.Log("Too many attempts ImmuneResilience: bacteria on" + gameObject.name);
        }
    }
    //Simply activates new cells and places them in the scene during loading
    public void SetCellNumbers()
    {
        //Set a maximum processing tick, which will put a limit on how many times the loop can run
        int maxTick = mCivilians * 100;
        //Keep track of how many times the loop runs
        int tick = 0;

        //Civilians
        if (targetCivilians != 0)
        {
            //Will run until the civilian count is correct or it reaches the processing limit
            while (civilians.Count < targetCivilians && tick < maxTick)
            {
                //Get the next avaliable cell from the object pool
                GameObject cell = civilianPool.GetNextObject();

                //If the object pool is out, or there are no spots open, then stop activating cells
                if (cell == null || !CivilianSpotsOpen())
                {
                    Debug.Log("Missing cell componet or no spots open Spawn: civilians on" + gameObject.name);
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
                Debug.Log("Too many attempts Spawn: civilians on" + gameObject.name);
            }
        }

        //Macrophages
        if (targetMacrophages != 0)
        {
            maxTick = mMacrophages * 10;
            tick = 0;
            while (macrophages.Count < targetMacrophages && tick < maxTick)
            {
                GameObject cell = macrophagePool.GetNextObject();
                if (cell == null)
                {
                    Debug.Log("Missing cell componet Spawn: macrophages on" + gameObject.name);
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
                Debug.Log("Too many attempts Spawn: macrophages on" + gameObject.name);
            }
        }

        //Neutrophiles
        if (targetNeutrophiles != 0)
        {
            maxTick = mNeutrophiles * 10;
            tick = 0;
            while (neutrophiles.Count < targetNeutrophiles && tick < maxTick)
            {
                GameObject cell = neutrophilePool.GetNextObject();
                if (cell == null)
                {
                    Debug.Log("Missing cell componet Spawn: neutrophiles on" + gameObject.name);
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
                Debug.Log("Too many attempts Spawn: neutrophiles on" + gameObject.name);
            }
        }

        //Bacteria
        if (targetNeutrophiles != 0)
        {
            maxTick = mBacteria * 10;
            tick = 0;
            while (bacteria.Count < targetBacteria && tick < maxTick)
            {
                GameObject cell = bacteriaPool.GetNextObject();
                if (cell == null)
                {
                    Debug.Log("Missing cell componet Spawn: bacteria on" + gameObject.name);
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
                Debug.Log("Too many attempts Spawn: bacteria on" + gameObject.name);
            }
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
