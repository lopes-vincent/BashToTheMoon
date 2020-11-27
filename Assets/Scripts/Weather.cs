using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weather")]
public class Weather : ScriptableObject
{
    [TextArea]
    public string description;
    
    [TextArea]
    public string accidentDescription;
    
    public int minimumSuccessPercentage;
    
    public int maximumSuccessPercentage;
}
