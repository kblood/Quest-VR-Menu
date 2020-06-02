using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AndroidTest : MonoBehaviour
{
    public TextMeshPro text;

    public GameObject[] gameCubes;

    public List<AppObject> appObjects = new List<AppObject>();

    // Start is called before the first frame update
    void Start()
    {
        //text.SetText("Test text");
        text.text = "Test text";

        try
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            int flag = new AndroidJavaClass("android.content.pm.PackageManager").GetStatic<int>("GET_META_DATA");
            AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject packages = pm.Call<AndroidJavaObject>("getInstalledApplications", flag);
            //above is working

            int count = packages.Call<int>("size");
            AndroidJavaObject[] links = new AndroidJavaObject[count];
            string[] names = new string[count];
            var icons = new AndroidJavaObject[count];
            List<byte[]> byteimg = new List<byte[]>();
            int ii = 0;
            text.text = "";
            for (int i = 0; ii < count;)
            {
                //get the object
                AndroidJavaObject currentObject = packages.Call<AndroidJavaObject>("get", ii);
                try
                {
                    //try to add the variables to the next entry
                    links[i] = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", currentObject.Get<AndroidJavaObject>("processName"));
                    names[i] = pm.Call<string>("getApplicationLabel", currentObject);
                    //text.text = text.text+", "+  names[i];
                    //icons[i] = pm.Call<AndroidJavaObject>("getApplicationIcon", currentObject);
                    Debug.Log("(" + ii + ") " + i + " " + names[i]);
                    string packageName = names[i];
                    appObjects.Add(new AppObject() { Name = names[i], Link = links[i] });

                    //try
                    //{
                    //    //packageName = currentObject.Call<string>("getPackageName");
                    //    //packageName = pm.Call<string>("getPackageName", currentObject);
                    //    packageName = currentActivity.Call<string>("getPackageName");
                    //    text.text = packageName;
                    //    if(!string.IsNullOrWhiteSpace(packageName))
                    //        appObjects.Add(new AppObject() { Name = names[i], Link = links[i], PackageName = packageName });
                    //}
                    //catch (Exception e)
                    //{ //text.text = e.Message; 
                    //};

                    //string packageName = names[i];

                    //appObjects.Add(new AppObject() { Name = names[i], Link = links[i], Icon = icons[i], PackageName = packageName});

                    //go to the next app and entry
                    i++;
                    ii++;
                }
                catch
                {
                    //if it fails, just go to the next app and try to add to that same entry.
                    Debug.Log("skipped " + ii);
                    ii++;
                }

            }

            //Debug.Log("app info length: " + names.Length);
            //Debug.Log("app info: " + names.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y));
            //text.SetText(names.Select(x => x.ToString())?.Aggregate((x, y) => x + "," + y));


            System.Random random = new System.Random(DateTime.Now.Second);
            //System.Random random = new System.Random(3);

            int retries = 7;

            for (int y = 0; y < gameCubes.Count() - 1; y++)
            {
                text.text+= appObjects.Count() + " app objects " + gameCubes.Count();
                GameObject cube = gameCubes[y];
                var gameCubeApp = cube.GetComponent<GameCubeApp>();
                var app = appObjects[random.Next(appObjects.Count - 1)];
                text.text += ". Rand " + appObjects.IndexOf(app) + " ;";
                if (gameCubeApp != null && app != null && app.Name != null && app.Link != null)
                {
                    text.text += " Setting " + app.Name +" onto " + cube.name;
                    gameCubeApp.SetAppObject(app);
                }
                else
                {
                    //text.text = text.text + ". No gamecubeapp script found on´ " + cube.name + " or something";
                    retries--;
                    if(retries > 0)
                    y--;
                }
            }


        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            text.text += e.Message.Substring(0,7);
        }

        text.GraphicUpdateComplete();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class AppObject
{
    public AndroidJavaObject Link { get; set; }
    public string Name { get; set; }
    public string PackageName { get; set; }
    public AndroidJavaObject Icon { get; set; }
}
