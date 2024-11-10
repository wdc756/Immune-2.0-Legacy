using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    //[SerializeField]
    //private ObjectPoolingHelper neutrophilePool;
    //private List<Cell> neutrophiles;
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

    [Header("Cell number variables")]
    [Tooltip("The maximum number of civilian cells, will update the object pool")]
    public int maxCivilians = 100;
    [Tooltip("The number of civilian cells the scene should have")]
    public int targetCivilians;
    public List<Vector3> anchors;
    public GameObject anchorPre;
    public int maxMacrophages = 25;
    public int targetMacrophages;
    //public int maxNeutrophiles = 25;
    //public int targetNeutrophiles;
    //public int maxTCells = 15;
    //public int targetTCells;
    //public int maxBCells = 10;
    //public int targetBCells;
    [Tooltip("This is not the total max for all bacteria types, but for each individual type")]
    public int maxBacteria = 50;
    public int targetBacteria;

    [Header("Screen Bounds, in unity units")]
    [SerializeField, Tooltip("Vertical Max(absolute value)")]
    private float verticalMax = 5;
    [SerializeField, Tooltip("Horizontal Max(absolute value)")]
    private float horizontalMax = 8;



    public bool SetUp(GameManager gameM)
    {
        gameManager = gameM;

        visualManager = GetComponent<VisualManager>();

        SetUpObjectPools();

        SetUpCivilianAnchors();

        return true;
    }
    private void SetUpObjectPools()
    {
        civilianPool.SetPoolCount(maxCivilians, false);
        macrophagePool.SetPoolCount(maxMacrophages, false);
        //neutrophilePool.SetPoolCount(maxNeutrophiles, false);
        //BCellPool.SetPoolCount(maxBCells, false);
        //TCellPool.SetPoolCount(maxBCells, false);

        //temp
        bacteriaPool.SetPoolCount(maxBacteria, false);
    }

    private void SetUpCivilianAnchors()
    {
        for (int i = 0; i < 5; i++)
        {
            anchors.Add(GetRandomNearEdgePosition());
            GameObject anchor = Instantiate(anchorPre);
            anchor.transform.position = anchors[i];
        }
    }


    void Update()
    {

        //This should be run on gameTicks, not every frame, but oh well
        UpdatePersistCells();
    }

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
        foreach (Cell cell in bacteria)
        {
            cell.ResetPersistTime();
        }
    }

    //Recieve new simulation numbers, to affect the visuals, scene dependant
    void NewSimulationNumbers(float response, float infection)
    {
        //temporary implementation
        int tick = 0;

        targetCivilians = (int)Mathf.Clamp(maxCivilians - (maxCivilians * infection), 0, maxCivilians);

        while (civilians.Count > targetCivilians && tick < maxCivilians)
        {
            tick++;

            Cell c = civilians[Random.Range(0, civilians.Count)];
            if (bacteria.Count > 0)
            {
                Cell b = bacteria[Random.Range(0, bacteria.Count)];
                b.NewTask(4, c.gameObject);
                civilians.Remove(c);
            }
            else
            {
                c.ClearTasks();
                c.NewTask(10);
                civilians.Remove(c);
            }
        }
        tick = 0;
        while (civilians.Count < targetCivilians && tick < maxCivilians)
        {
            tick++;

            GameObject cell = civilianPool.GetNextObject();
            if (cell == null)
            {
                break;
            }
            Cell c = cell.GetComponent<Cell>();
            c.gameObject.SetActive(true);
            c.ActivateCell(GetAdjustedPosition(anchors[Random.Range(0, 5)]));
            civilians.Add(c);
        }

        targetMacrophages = (int)Mathf.Clamp(maxMacrophages * response, 0, maxMacrophages);

        tick = 0;
        while (macrophages.Count > targetMacrophages && tick < maxMacrophages)
        {
            tick++;

            Cell m = macrophages[Random.Range(0, macrophages.Count)];
            if (Random.Range(0.0f, 1.0f) < 0.3f)
            {
                m.NewTask(1, GetRandomEdgePosition());
                macrophages.Remove(m);
            }
            else
            {
                m.NewTask(10);
                macrophages.Remove(m);
            }
        }
        tick = 0;
        while (macrophages.Count < targetMacrophages && tick < maxMacrophages)
        {
            GameObject cell = macrophagePool.GetNextObject();
            if (cell == null)
            {
                break;
            }
            Cell m = cell.GetComponent<Cell>();
            if (m == null) { break; }
            m.gameObject.SetActive(true);
            m.ActivateCell(GetRandomEdgePosition(), verticalMax, horizontalMax);
            macrophages.Add(m);
        }

        targetBacteria = (int)Mathf.Clamp(maxBacteria * infection, 0, maxBacteria);

        tick = 0;
        while (bacteria.Count > targetBacteria && tick < maxBacteria)
        {
            Cell b = bacteria[Random.Range(0, bacteria.Count)];
            if (macrophages.Count > 0 && Random.Range(0.0f, 1.0f) < 0.9f)
            {
                Cell m = macrophages[Random.Range(0, macrophages.Count)];
                m.NewTask(5, b.gameObject);
                //m.NewTask(-1);
                bacteria.Remove(b);
            }
            else
            {
                b.NewTask(10);
                bacteria.Remove(b);
            }
        }
        tick = 0;
        while (bacteria.Count < targetBacteria && tick < maxBacteria)
        {
            GameObject cell = bacteriaPool.GetNextObject();
            if (cell == null)
            {
                break;
            }
            Cell b = cell.GetComponent<Cell>();
            if (b == null) { break; }
            b.gameObject.SetActive(true);
            b.ActivateCell(GetRandomEdgePosition(), verticalMax, horizontalMax);
            bacteria.Add(b);
        }

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
    private Vector3 GetRandomNearEdgePosition()
    {
        Vector3 newPos = new Vector3();
        newPos.z = 0;

        // Generate a random angle in radians
        float angle = Random.Range(0f, Mathf.PI * 2);

        // Generate a distance factor biased towards the edges by squaring the distance
        float distance = Mathf.Pow(Random.Range(1.0f, 2.0f), 2) - 1;

        // Calculate position using polar coordinates, scaled to your map size
        newPos.x = Mathf.Clamp(Mathf.Cos(angle) * distance * (horizontalMax / 1.5f), -horizontalMax, horizontalMax);
        newPos.y = Mathf.Clamp(Mathf.Sin(angle) * distance * (verticalMax / 1.5f), -horizontalMax, horizontalMax);

        return newPos;
    }
    private Vector3 GetAdjustedPosition(Vector3 anchor)
    {
        Vector3 newPos = new Vector3();

        newPos.z = 0;
        newPos.x = Mathf.Clamp(anchor.x + CloseToAnchorFunction(Random.Range(-1.5f, 1.5f)), -horizontalMax, horizontalMax);
        newPos.y = Mathf.Clamp(anchor.y + CloseToAnchorFunction(Random.Range(-1.5f, 1.5f)), -verticalMax, verticalMax);

        return newPos;
    }
    private float CloseToAnchorFunction(float x)
    {
        return (x * x * x) + x;
    }

}
