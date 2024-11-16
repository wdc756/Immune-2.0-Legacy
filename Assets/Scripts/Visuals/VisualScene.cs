using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualScene : MonoBehaviour
{
    /*
     This controls the color shifting and holds some data like pathing links and civilian cell positions
     */

    //Pathing data for linking scenes
    //This VisualScene's index in VisualManager
    public int sceneIndex;
    //Used by VisualManager to direct players to other VisualScenes
    private List<VisualScene> pathingLinks = new List<VisualScene>();
    //Vector3 positions to stop blockage
    private List<Vector3> pathingPositions = new List<Vector3>();



    [Header("Cell stuff")]
    //Civilian Locations, will be generated by VisualManager on SetUp()
    //List of anchors to generate cells around
    private List<Vector3> anchors = new List<Vector3>();
    //List of civlian cell positions
    private List<Vector3> cellPositions = new List<Vector3>();
    [Tooltip("The tendency of cells to be closer to their anchors; larger number = less anchored; by anchors just think of how cells group together into clumps")]
    public float cellAnchorMultiplier = 0.25f;
    //The actual number used
    private float cAnchorMultiplier;
    [Tooltip("The minimum distance a cell can be away from any other cell when generating")]
    public float cellMinDistance = 1.0f;

    [Tooltip("The maximum number of civilian cells this VisualScene can hold")]
    public int maxCivilians;
    public int maxMacrophages;
    public int maxNeutrophiles;
    public int maxBacteria;



    [Header("Screen space stuff")]
    [Tooltip("The scale of the current screen, which scales from 1.0f(v = 5, h = 9); will be set on SetUp()")]
    public float sceneScale = 1.0f;
    //Screen bounds, to be set on SetUp() using sceneScale
    private float verticalMax = 5;
    private float horizontalMax = 9;



    [Header("Color shifting")]
    //The spriteRenderer on the same gameObject, to be set on Start()
    public SpriteRenderer spriteRenderer;
    [SerializeField, Tooltip("The normal color when no external pressures exist")]
    private Color normalColor;
    [SerializeField, Tooltip("Determines the influence the color shifts have on the base colors as a whole")]
    private float hueShiftSaturation = 0.2f;

    [SerializeField, Tooltip("The color shift hue when this section is the source of a lot of stress")]
    private Color stressedHue;
    [SerializeField, Tooltip("The color shift hue when this section is very infected")]
    private Color infectedHue;



    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    
    //Call to reset and generate new cell positions
    public void SetUp(int index, float scale, List<VisualScene> links, int numAnchors, int numCivilians, int numMacrophages, int numNeutrophiles, int numBacteria)
    {
        sceneIndex = index;

        sceneScale = scale;
        SetScreenScale();

        ResetPositions();

        GeneratePathingPositions(links);
        GenerateAnchors(numAnchors);
        GenerateCellPositions((int)(numCivilians * 1.1f));

        maxCivilians = numCivilians;
        maxMacrophages = numMacrophages;
        maxNeutrophiles = numNeutrophiles;
        maxBacteria = numBacteria;
    }
    //Sets the new screen bounds by using scale, starting from v = 5, h = 9 and scales the screen starting at v = 10, h = 18
    private void SetScreenScale()
    {
        verticalMax = 5 * sceneScale;
        horizontalMax = 9 * sceneScale;

        //cAnchorMultiplier = cellAnchorMultiplier * ((sceneScale * sceneScale) - (sceneScale * 2f));
        //Debug.Log(cAnchorMultiplier);
        cAnchorMultiplier = 1.45f + (Mathf.Log(sceneScale, 20));

        Vector3 newScale = new Vector3();
        newScale.x = 18 * sceneScale;
        newScale.y = 10 * sceneScale;
        newScale.z = 0;
        gameObject.transform.localScale = newScale;
    }
    //clears all the lists
    private void ResetPositions()
    {
        pathingPositions.Clear();
        pathingLinks.Clear();
        anchors.Clear();
        cellPositions.Clear();
    }



    //Three pretty similar functions to generate pathing, anchor, and cell positions
    private void GeneratePathingPositions(List<VisualScene> links)
    {
        pathingPositions = new List<Vector3>();

        //used as a distance check
        //Distance formula(max-min) / (numAnchors / 2)
        float minDistance = Mathf.Abs(Mathf.Sqrt(((-horizontalMax - horizontalMax) * (-horizontalMax - horizontalMax))
            + ((-verticalMax - verticalMax) * (-verticalMax - verticalMax))) / (links.Count / 1.1f));

        //temp ticker
        int tick = 0;

        while (pathingPositions.Count < links.Count && tick < links.Count * 10.5f)
        {
            bool add = true;
            Vector3 v = GetRandomEdgePosition();

            for (int j = 0; j < pathingPositions.Count; j++)
            {
                //if the random position is not too close to the pathing points
                if (Vector3.Distance(v, pathingPositions[j]) < minDistance)
                {
                    add = false;
                    break;
                }
            }

            if (add)
            {
                pathingPositions.Add(v);
                pathingLinks.Add(links[pathingLinks.Count]);
            }

            tick++;
        }

        if (tick >= links.Count * 10.5f)
        {
            Debug.Log("Too many attempts: pathing positions");
        }
    }
    private void GenerateAnchors(int numAnchors)
    {
        anchors = new List<Vector3>();

        //used as a distance check
        //Distance formula(max-min) / numAnchors
        float minDistance = Mathf.Abs(Mathf.Sqrt(((-horizontalMax - horizontalMax) * (-horizontalMax - horizontalMax)) 
            + ((-verticalMax - verticalMax) * (-verticalMax - verticalMax))) / (numAnchors * 2f));

        //temp ticker
        int tick = 0;

        while (anchors.Count < numAnchors && tick < numAnchors * 20)
        {
            bool add = true;
            Vector3 v = GetRandomNearEdgePosition();
            
            for (int j = 0; j < pathingPositions.Count; j++)
            {
                //if the random position is not too close to the pathing points
                if (Vector3.Distance(v, pathingPositions[j]) < minDistance)
                {
                    add = false;
                    break;
                }
            }

            for (int j = 0; j < anchors.Count; j++)
            {
                //if the random position is not too close to any others
                if (Vector3.Distance(v, anchors[j]) < minDistance)
                {
                    add = false;
                    break;
                }
            }

            if (add)
            {
                anchors.Add(v);
            }

            tick++;
        }

        if (tick >= numAnchors * 20)
        {
            Debug.Log("Too many attempts: anchors");
        }
    }
    private void GenerateCellPositions(int numCells)
    {
        cellPositions = new List<Vector3>();

        int tick = 0;
        int maxTick = numCells * 20;

        while (cellPositions.Count < numCells && tick < maxTick)
        {
            bool add = true;
            Vector3 v = GetRandomNearAnchorPosition(anchors[Random.Range(0, anchors.Count)], cAnchorMultiplier);

            for (int j = 0; j < cellPositions.Count; j++)
            {
                //if the random position is not too close to any others
                if (Vector3.Distance(v, cellPositions[j]) < cellMinDistance)
                {
                    add = false;
                    break;
                }
            }

            if (add)
            {
                cellPositions.Add(v);
            }

            tick++;
        }

        if (tick >= maxTick)
        {
            Debug.Log("Too many attempts: cells; Only able to generate: " + cellPositions.Count + "/" + numCells + " regularly, generating random positions...");

            tick = 0;
            while (cellPositions.Count < numCells && tick < maxTick)
            {
                bool add = true;
                Vector3 v = GetRandomPosition();

                for (int j = 0; j < cellPositions.Count; j++)
                {
                    if (Vector3.Distance(cellPositions[j], v) < cellMinDistance)
                    {
                        add = false;
                        break;
                    }
                }

                if (add)
                {
                    cellPositions.Add(v);
                }
            }
        }
    }



    //Returns the cell positions list, to be sent to CellManager
    public List<Vector3> GetCellPositions()
    {
        return cellPositions;
    }
    //returns the path positions
    public List<Vector3> GetPathPositions()
    {
        return pathingPositions;
    }
    //Returns the pathing links/script references
    public List<VisualScene> GetPathLinks()
    {
        return pathingLinks;
    }
    //Returns the number of anchors
    public int GetNumAnchors()
    {
        return anchors.Count;
    }



    //Returns a vector3 that is somewhere in the bounds
    private Vector3 GetRandomPosition()
    {
        Vector3 newPosition = new Vector3();

        newPosition.x = Random.Range(-horizontalMax, horizontalMax);
        newPosition.y = Random.Range(-verticalMax, verticalMax);

        return newPosition;
    }
    //Returns a vector3 that is past the edge bounds
    private Vector3 GetRandomEdgePosition()
    {
        // Randomly pick a side: 0 = top, 1 = bottom, 2 = left, 3 = right
        int side = Random.Range(0, 4);
        //Create a new number to add to the edge position
        float outsideAddition = Random.Range(0.5f, 1.25f);

        float x = 0;
        float y = 0;

        switch (side)
        {
            case 0: // Top side
                x = Random.Range(-horizontalMax, horizontalMax);
                y = verticalMax + outsideAddition; // Just outside top
                break;
            case 1: // Bottom side
                x = Random.Range(-horizontalMax, horizontalMax);
                y = -verticalMax - outsideAddition; // Just outside bottom
                break;
            case 2: // Left side
                x = -horizontalMax - outsideAddition; // Just outside left
                y = Random.Range(-verticalMax, verticalMax);
                break;
            case 3: // Right side
                x = horizontalMax + outsideAddition; // Just outside right
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
    //returns a Vector3 close to an anchor; closenessMultiplier determines how close cells will try to be
    private Vector3 GetRandomNearAnchorPosition(Vector3 anchor, float closenessMultiplier)
    {
        Vector3 newPos = new Vector3();

        newPos.z = 0;
        newPos.x = ClampToEdges(anchor.x + CloseToAnchorFunction(Random.Range(-1f * closenessMultiplier, 1f * closenessMultiplier), closenessMultiplier), horizontalMax);
        newPos.y = ClampToEdges(anchor.y + CloseToAnchorFunction(Random.Range(-1f * closenessMultiplier, 1f * closenessMultiplier), closenessMultiplier), verticalMax);

        return newPos;
    }
    //Function to generate distances closer to the anchor
    private float CloseToAnchorFunction(float x, float multiplier)
    {
        //x^3 + x * multiplier
        return (x * x * x) + (x * multiplier);

        //This had to be a cubic function so it wouldn't just generate in the positive coordinates
    }
    private float ClampToEdges(float x, float max)
    {
        float tolerance = Random.Range(-0.5f, 0.5f);
        return Mathf.Clamp(x, -max - tolerance, max + tolerance);
    }



    
    //changes the amount that the colors change; input must be 0 <= in <= 1
    public void SetHueShiftSaturation(float newSaturation)
    {
        if (0 <= newSaturation && newSaturation <= 1)
        {
            hueShiftSaturation = newSaturation;
        }
        else
        {
            Debug.LogWarning("Attempted to set hueShiftSaturation to an invalid value on " + gameObject.name);
        }
    }
    //Updates the image color according to the new stress and infection levels
    public void ShiftColor(float stressLevel, float infectionLevel)
    {
        // Interpolate between normalHue and stressedHue based on stressLevel
        Color stressColor = Color.Lerp(normalColor, stressedHue, stressLevel);

        // Interpolate between the result and infectedHue based on infectionLevel
        Color infectionColor = Color.Lerp(normalColor, infectedHue, infectionLevel);

        //Combine the two shifted colors
        Color finalColor = Color.Lerp(stressColor, infectionColor, 0.5f);

        // Apply the hueShiftSaturation to modify the intensity
        finalColor = Color.Lerp(normalColor, finalColor, hueShiftSaturation);

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }
        // Set the final color to the SpriteRenderer
        spriteRenderer.color = finalColor;
    }


}
