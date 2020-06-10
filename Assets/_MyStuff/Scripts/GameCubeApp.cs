using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameCubeApp : MonoBehaviour
{
    public List<TextMeshPro> text;
    public AppObject appObject { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponentsInChildren<TextMeshPro>().ToList();
        //text.text = "Started";
    }

    private void Awake()
    {
        text = GetComponentsInChildren<TextMeshPro>().ToList();
        if(appObject != null)
        {
            UpdateText(appObject.Name);
        }

    }

    public void SetAppObject(AppObject appobj)
    {
        try
        {
            if (text == null)
                text = GetComponentsInChildren<TextMeshPro>().ToList();

            UpdateText("Updating");
            appObject = appobj;
            if (!string.IsNullOrWhiteSpace(appObject.Name))
                UpdateText(appObject.Name);
            else
                UpdateText("No name: ");
        }
        catch (Exception e)
        {
            //if (text == null)
            //    text = GetComponentsInChildren<TextMeshPro>().ToList();
            //UpdateText(e.Message);
            //AddText(" xyz ");
        }
        //text.GraphicUpdateComplete();
    }

    private string AddText(string newText)
    {
        if (text == null)
            text = GetComponentsInChildren<TextMeshPro>().ToList();

        if (text == null || !text.Any())
            return newText;

        newText = text.First().text += newText;

        foreach (var t in text)
        {
            t.text = newText;
        }
        return newText;
    }

    private string UpdateText(string newText)
    {
        if (text == null)
            text = GetComponentsInChildren<TextMeshPro>().ToList();

        foreach (var t in text)
        {
            t.text = newText;
        }
        return newText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
