using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFullSimulation : MonoBehaviour
{
    public BodySimulation Body;

    public Pathogen BaseBacteria;
    public Pathogen BaseVirus;

    public Slider ResourceProductionSlider;
    public Slider ResourceDemandSlider;

    private void Start()
    {
        ResourceProductionSlider.maxValue = Body.MaxResourceProductionPercent;
        ResourceDemandSlider.maxValue = Body.MaxResourceProductionPercent;
    }

    private void Update()
    {
        ResourceProductionSlider.value = Body.ResourceProductionPercent;
        ResourceDemandSlider.value = Body.ResourceDemandPercent;
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
