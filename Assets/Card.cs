using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{

    internal int Number;
    internal bool Hidden;
    internal bool isDragging;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        foreach(var text in GetComponentsInChildren<TMPro.TextMeshProUGUI>())
        {
            if (Hidden) text.text = "";
           
            else text.text = Number.ToString();
        }
    }
}
