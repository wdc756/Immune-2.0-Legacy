using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pathogen", menuName = "ScriptableObjects/Pathogen", order = 1)]
public class Pathogen : ScriptableObject
{
    /*
    This will be an information holder that will store information about how the pathogen, similar to Cell.cs
     */

    public string Name;

    /*
     * ResponseWeakness: the response that the pathogen is weak to, i.e.:
     *      Macrophages/Nutrophil
     *      Killer T cells
     *      Antibodies
     *      Compliment system
     * WeaknessFactor: When at full (100%) immune response, this is multiplied with MaxLossRate
     *      ex. bacteria weak to mac/nutrophil will lose at MaxLossRate * WeaknessFactor - GainRate
     *      Gain rate is always present
     */
    public ImmuneSystemResponse.ResponseType ResponseWeakness;
    public float WeaknessFactor = 3f;

    /*
     * All floats related to progress or stress levels can be thought of as percentages. (not weaknessfactor though)
     * GainRate: The fixed percentage rate at which the pathogen progresses every tick
     * MaxLossRate: The max percentage rate at which progress decreases. is multiplied with the immune level response
     * 
     */
    public float GainRatePercent;
    public float MaxLossRatePercent;

}
