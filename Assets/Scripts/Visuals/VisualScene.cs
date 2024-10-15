using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualScene : MonoBehaviour
{
    /*
     This just controls the color shifting and status images for the background(s) and also contains path references used by VisualManager to direct the player
    to different VisualScenes. Think like how the Thymus has connections to the lungs, the heart, and also two an interchange, then that interchange
    has connections back to the Thymus, but also to two other parts of the body
    
    Should be applied to an image
     */

    [SerializeField, Tooltip("Used by VisualManager to direct players to other VisualScenes")]
    private List<VisualScene> pathingLinks = new List<VisualScene>();


    [SerializeField, Tooltip("The normal color shift hue when no external pressures exist")]
    private Color normalHue;
    [SerializeField, Tooltip("Determines the influence the color shifts have on the base colors as a whole")]
    private float hueShiftSaturation = 0.2f;

    [SerializeField, Tooltip("The color shift hue when this section is the source of a lot of stress")]
    private Color stressedHue;
    [SerializeField, Tooltip("The color shift hue when this section is very infected")]
    private Color infectedHue;



    void Start()
    {
        
    }


    
    //Updates the visuals according to the new stress and infection levels
    public void UpdateColorTints(float stress, float infected)
    {

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



    //checks if the int index path is set to a valid link in pathingLinks
    public bool PathExists(int path)
    {
        if (0 <= path && path < pathingLinks.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //returns the path of the current int path index from pathingLinks
    public VisualScene GetPath(int path)
    {
        if (PathExists(path))
        {
            return pathingLinks[path];
        }
        else
        {
            Debug.LogWarning("Attempted to pull a VisualScene path from an invalid index on " + gameObject.name);
            return null;
        }
    }
}
