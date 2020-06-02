using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConsoleCube : MonoBehaviour
{
    public TextMeshPro text;



    // Start is called before the first frame update
    void Start()
    {
        text = GetComponentInChildren<TextMeshPro>();
        text.text = "Some text test;";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        var go = collision.gameObject;
        var app = go.GetComponent<GameCubeApp>();
        if (app != null)
        {
            go.transform.position = Vector3.one * 3;

            if(app.appObject == null)
                text.text = "GameCube had no app object";
            else if (!string.IsNullOrWhiteSpace(app.appObject.PackageName))
                text.text = app.appObject.PackageName;
            else if (!string.IsNullOrWhiteSpace(app.appObject.Name))
                text.text = app.appObject.Name;
            else
                text.text = "GameCube had no package or app name";

            launchApp(app.appObject);
            //launchApp(app.appObject.PackageName);
        }
        else
        {
            text.text = "GameCube had no GameCubeApp component";
        }
    }

    public void launchApp(AppObject appobj)
    {
        bool fail = false;
        //string bundleId = packageName; // your target bundle id
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

        AndroidJavaObject launchIntent = null;
        try
        {
            launchIntent = appobj.Link; //packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleId);
        }
        catch (System.Exception e)
        {
            text.text = e.Message;
            text.text += appobj.Name;
            fail = true;
        }

        if (fail)
        { //open app in store

            //text.text = "Opening Google";
            //Application.OpenURL("https://google.com");
            if (appobj == null)
                text.text = "No app object";
        }
        else //open the app
        {
            text.text = "Launching";
            ca.Call("startActivity", launchIntent);

        }

        up.Dispose();
        ca.Dispose();
        packageManager.Dispose();
        launchIntent.Dispose();
    }

    public void launchApp(String packageName)
    {
        bool fail = false;
        string bundleId = packageName; // your target bundle id
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

        AndroidJavaObject launchIntent = null;
        try
        {
            launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleId);
        }
        catch (System.Exception e)
        {
            fail = true;
        }

        if (fail)
        { //open app in store
            Application.OpenURL("https://google.com");
        }
        else //open the app
            ca.Call("startActivity", launchIntent);

        up.Dispose();
        ca.Dispose();
        packageManager.Dispose();
        launchIntent.Dispose();
    }
}
