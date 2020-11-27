using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinController : MonoBehaviour
{
    public Text winText;

    // Start is called before the first frame update
    private void Start()
    {
        string text = "Oh yeah ! You succeeded this mission with SUCCESS !!";
        text += "\n \n \n";
        text += "And you do it in <b>";
        float hours = ((StaticClass.TimeElapsed / 60)/60);
        if (hours > .5f)
        {
            string suffix = hours > 1 ? "s " : " ";
            text += hours.ToString("00") + " hour" + suffix;
        }
        float minutes = ((StaticClass.TimeElapsed / 60)%60);
        if (minutes > .5f)
        {
            string suffix = minutes > 1 ? "s " : " ";
            text += minutes.ToString("00") + " minute" + suffix;
        }

        text += "! </b>";
        
        winText.text = text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
