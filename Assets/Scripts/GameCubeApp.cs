using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameCubeApp : MonoBehaviour
{
    public TextMeshPro text;
    public AppObject appObject { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponentInChildren<TextMeshPro>();
        //text.text = "Started";
    }

    public void SetAppObject(AppObject appobj)
    {
        try
        {
            if (text == null)
                text = GetComponentInChildren<TextMeshPro>();
            
            text.text = "Updating";
            appObject = appobj;
            if (!string.IsNullOrWhiteSpace(appObject.Name))
                text.text = appObject.Name;
            else
                text.text = "No name: ";
        }
        catch (Exception e)
        {
            if (text == null)
                text = GetComponentInChildren<TextMeshPro>();
            text.text = e.Message;
            text.text += " xyz ";
        }
        //text.GraphicUpdateComplete();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
