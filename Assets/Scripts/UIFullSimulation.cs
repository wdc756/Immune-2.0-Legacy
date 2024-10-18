using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFullSimulation : MonoBehaviour
{
    public BodySimulation Body;

    public Pathogen BaseBacteria;
    public Pathogen BaseVirus;


    private void Start()
    {
        
    }

    public void InfectRandomPartBacteria()
    {
        BodySectionSimulation section = Body.Sections[Random.Range(0, Body.Sections.Count)];
        if (section.Infection != null)
        {
            Debug.Log("Chose a section which is already infected. Try again");
            return;
        }

        section.Infection = BaseBacteria;
    }
    
    public void InfectRandomPartVirus()
    {
        BodySectionSimulation section = Body.Sections[Random.Range(0, Body.Sections.Count)];
        if (section.Infection != null)
        {
            Debug.Log("Chose a section which is already infected. Try again");
            return;
        }

        section.Infection = BaseVirus;
    }
}
