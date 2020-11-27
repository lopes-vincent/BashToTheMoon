using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Message")]
public class Message : ScriptableObject
{
    [TextArea] 
    public string text;

    public int timeToWait;
}
