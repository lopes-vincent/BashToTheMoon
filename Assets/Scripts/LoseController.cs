using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoseController : MonoBehaviour
{
    public Text loseText;

    // Start is called before the first frame update
    private void Start()
    {
        loseText.text = StaticClass.LoseText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
