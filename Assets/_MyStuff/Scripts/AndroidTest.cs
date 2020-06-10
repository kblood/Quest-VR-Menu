using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AndroidTest : MonoBehaviour
{
    public TextMeshPro text;

    public GameObject[] gameCubes;

    public GameObject gamePrefab;
    public GameObject scrollView;
    public GameObject buttonPrefab;
    public TextMeshPro scrollText;

    public List<AppObject> appObjects = new List<AppObject>();

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        scrollText.text = "YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY";
        //text.SetText("Test text");
        text.text = "Test text";

        timer = 10;
        try
        {
#if !UNITY_EDITOR

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
                    text.text = text.text+", "+  names[i];
                    icons[i] = pm.Call<AndroidJavaObject>("getApplicationIcon", currentObject);
                    Debug.Log("(" + ii + ") " + i + " " + names[i]);
                    string packageName = names[i];
                    appObjects.Add(new AppObject() { Name = names[i], Link = links[i], Icon = icons[i] });

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
#endif

#if UNITY_EDITOR

            for (int y = 0; y < 250; y++)
            {
                var app = new AppObject() { Name = "Test " + y, Link = new AndroidJavaObject("Test " + y, new string[] { "Test " + y }) };

                if (appObjects == null)
                    appObjects = new List<AppObject>();

                appObjects.Add(app);
            }

#endif
            //Debug.Log("app info length: " + names.Length);
            //Debug.Log("app info: " + names.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y));
            //text.SetText(names.Select(x => x.ToString())?.Aggregate((x, y) => x + "," + y));

            System.Random random = new System.Random(DateTime.Now.Second);
            //System.Random random = new System.Random(3);

            int retries = 7;

            for (int y = 0; y < gameCubes.Count() - 1; y++)
            {
                text.text += appObjects.Count() + " app objects " + gameCubes.Count();
                GameObject cube = gameCubes[y];
                var gameCubeApp = cube.GetComponent<GameCubeApp>();
                var app = appObjects[random.Next(appObjects.Count - 1)];
                text.text += ". Rand " + appObjects.IndexOf(app) + " ;";
                if (gameCubeApp != null && app != null && app.Name != null && app.Link != null)
                {
                    text.text += " Setting " + app.Name + " onto " + cube.name;
                    gameCubeApp.SetAppObject(app);
                }
                else
                {
                    //text.text = text.text + ". No gamecubeapp script found on´ " + cube.name + " or something";
                    retries--;
                    if (retries > 0)
                        y--;
                }
            }


        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            text.text += e.Message.Substring(0, 7);
        }
        /*
        //#else

        //text.GraphicUpdateComplete();

        */

        foreach (var a in appObjects)
        {
            //ItemGameObject is my prefab pointer that i previous made a public property  
            //and  assigned a prefab to it

            //GameObject gameCube = Instantiate(gamePrefab) as GameObject;
            //var gameCubeApp = gameCube.GetComponent<GameCubeApp>();
            //gameCubeApp.SetAppObject(a);

            if (scrollView != null) // && gameCubeApp.appObject != null)
            {
                //ScrollViewGameObject container object
                //gameCube.transform.SetParent(scrollView.transform, false);

                var button = CreateButton(buttonPrefab.GetComponent<Button>(), scrollView);
                var txtmshpro = button.GetComponentInChildren<TMP_Text>();
                if (txtmshpro != null)
                    txtmshpro.text = a.Name;
                else
                {
                    var txt = button.GetComponentInChildren<Text>();
                    if (txt != null)
                        txt.text = a.Name;
                }
                button.onClick.AddListener(() => { launchApp(a); });
            }
            else
            {
                //DestroyImmediate(gameCube);
            }
        }
    }

    private void Update()
    {
        if (timer < Time.time)
        {
            timer = Time.time + 10;
            try
            {
                scrollText.text = "Listing objects";
                ListSceneObjects();

            }
            catch (Exception e)
            {
                text.text = e.Message;
                //scrollText.text = e.Message;
            }
        }
    }

    private void ListSceneObjects()
    {
        var gltf = GameObject.FindGameObjectsWithTag("Gltf");
        if(gltf.Any())
        {
            scrollText.text = gltf.Select(g => g.name).Aggregate((x, y) => x + ", " + y);

            gltf = gltf.SelectMany(g => g.GetComponentsInChildren<GameObject>())?.ToArray();
            if (gltf.Any())
                scrollText.text = gltf.Select(g => g.name).Aggregate((x, y) => x + ", " + y);
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

    public static Button CreateButton(Button buttonPrefab, GameObject parent)
    {
        var button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as Button;
        var rectTransform = button.GetComponent<RectTransform>();
        rectTransform.SetParent(parent.transform, false);
        rectTransform.anchorMax = new Vector2(0,1);
        rectTransform.anchorMin = new Vector2(0,1);
        
        //rectTransform.anchorMax = cornerTopRight;
        //rectTransform.anchorMin = cornerBottomLeft;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.localScale = new Vector3(1,1,1);
        rectTransform.sizeDelta = new Vector2(60,60);
        return button;
    }

    public static Button CreateButton(Button buttonPrefab, Canvas canvas, Vector2 cornerTopRight, Vector2 cornerBottomLeft)
    {
        var button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as Button;
        var rectTransform = button.GetComponent<RectTransform>();
        rectTransform.SetParent(canvas.transform, false);
        rectTransform.anchorMax = cornerTopRight;
        rectTransform.anchorMin = cornerBottomLeft;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        return button;
    }

    
}

public class AppObject
{
    public AndroidJavaObject Link { get; set; }
    public string Name { get; set; }
    public string PackageName { get; set; }
    public AndroidJavaObject Icon { get; set; }
}
