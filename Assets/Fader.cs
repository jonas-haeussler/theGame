using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    [SerializeField] private float duration;
    private float startTime;

    private bool fading;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!fading) return;
        var color = GetComponent<Image>().color;
        color.a = 1 - (Time.realtimeSinceStartup - startTime) / duration;
        Debug.Log(color.a);
        GetComponent<Image>().color = color;
        if (color.a <= 0) gameObject.SetActive(false);
    }

    public void StartFading()
    {
        startTime = Time.realtimeSinceStartup;
        fading = true;
    }
}
