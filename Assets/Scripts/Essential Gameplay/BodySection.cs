using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySection : MonoBehaviour
{
    /*
    This script manages a body area, and acts as a way to manage the cells/pathogens inside

    The optimization/balancing calculation will be run here
     */

    public bool IsFocused = false;
    public GameObject FocusedObject;
    public GameObject MapObject;


    void Start()
    {
        Unfocus();
    }
    void Update()
    {
        
    }

    public void Focus()
    {
        IsFocused = true;
        FocusedObject.SetActive(true);
        MapObject.SetActive(false);
    }

    public void Unfocus()
    {
        IsFocused = false;
        FocusedObject.SetActive(false);
        MapObject.SetActive(true);
    }
}
