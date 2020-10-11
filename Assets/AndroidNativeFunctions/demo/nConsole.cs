using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class nConsole : MonoBehaviour {
	
	public KeyCode toggleKey = KeyCode.Escape;
	
	public Color color1 = new Color(0.145f,0.145f,0.145f,1);
	public Color color2 = new Color(0.145f,0.145f,0.145f,1);
	
	private GUIStyle backgroundStyle = new GUIStyle();
	private GUIStyle style1 = new GUIStyle();
	private GUIStyle style2 = new GUIStyle();
	private GUIStyle buttonStyle = new GUIStyle();
	
	private Vector2 scrollPos;
	private List<string> message = new List<string>();
	private List<string> stackTrace = new List<string>();
	private List<bool> showStackTrace = new List<bool>();
	private string consoleText = "";
	private bool show;
	
	private List<string> nameCommand = new List<string>();
	private List<UnityAction> actionCommand = new List<UnityAction>();
	
	private static nConsole instance;
	
	void Awake(){
		instance = this;
		DontDestroyOnLoad(gameObject);
	}
	
	void Start(){
		backgroundStyle.normal.background =  NewTexture(color1);
		style1.contentOffset = new Vector2(15,0);
		style2.normal.background = NewTexture(color2);
		style2.contentOffset = new Vector2(15,0);
		buttonStyle.normal.background = NewTexture(color2);
		buttonStyle.normal.textColor = Color.white;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
		style1.fontSize = Mathf.RoundToInt(13 * Screen.width/(480*1));
		style2.fontSize = Mathf.RoundToInt(13 * Screen.width/(480*1));
		buttonStyle.fontSize = Mathf.RoundToInt(13 * Screen.width/(480*1));
	}
	
	void OnEnable(){
		Application.RegisterLogCallback(HandleLog);
	}
	
	void OnDisable(){
		Application.RegisterLogCallback(null);
	}
	
	public static void CreateGO(KeyCode toggleKey){
		if(instance == null){
			GameObject go = new GameObject("nConsole");
			go.AddComponent<nConsole>().toggleKey = toggleKey;
		}
	}
	
	public static void DestroyGO(){
		if(instance != null){
			Destroy(instance.gameObject);
		}	
	}
	
	public static void AddCommand(string command,UnityAction action){
		if(!instance)
			return;
		instance.nameCommand.Add(command);
		instance.actionCommand.Add(action);
	}
	
	void Update(){
		if(Input.GetKeyDown(toggleKey)){
			show = !show;
		}
	}
	
	void OnGUI(){
		if(!show)
			return;
		GUI.Box(NewRect(-1,-1,102,102),"",backgroundStyle);
		GUI.Window(28, NewRect(0,2,100,85), ConsoleWindow, "",GUIStyle.none);
		if(GUI.Button(NewRect(3,91,20,8),"Clear",buttonStyle)){
			message.Clear();
			stackTrace.Clear();
			showStackTrace.Clear();
		}
		
		consoleText = GUI.TextField(NewRect(47,92.5f,50,6),consoleText);
		Event e = Event.current;
		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Return){
			if(!string.IsNullOrEmpty(consoleText)){
				for(int i=0;i<nameCommand.Count;i++){
					if(consoleText == nameCommand[i]){
						actionCommand[i].Invoke();
						break;
					}
				}
				consoleText = "";
			}
		}
	}
	
	void ConsoleWindow(int windowID){
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		for(int i=0;i<message.Count;i++){
			GUIStyle style;
			if(i%2==0){
				style = style2;
			}else{
				style = style1;
			}
			if(GUILayout.Button(message[i],style)){
				showStackTrace[i] = !showStackTrace[i];
			}
			if(showStackTrace[i]){
				if(GUILayout.Button(stackTrace[i],style)){
					showStackTrace[i] = !showStackTrace[i];
				}
			}
		}
		GUILayout.EndScrollView();
	}
	
	Rect NewRect(float x,float y,float width,float height){
		x = (Screen.width*x)/100f;
		y = (Screen.height*y)/100f;
		width = (Screen.width*width)/100f;
		height = (Screen.height*height)/100f;
		Rect rect = new Rect(x,y,width,height);
		return rect;
	}
	
	Texture2D NewTexture(Color color,int width = 5,int height = 5){
		Texture2D texture = new Texture2D(width,height);
		for(int i=0;i<width;i++){
			for(int j=0;j<height;j++){
				texture.SetPixel(i,j,color);
			}
		}
		texture.Apply();
		return texture;
	}
	
	void HandleLog (string m, string s, LogType type){
		switch(type){
		case LogType.Exception:
		case LogType.Error:
			message.Add("<color=red>" + m + "</color>");
			stackTrace.Add("<color=red>" + s + "</color>");
			break;
		case LogType.Warning:
			message.Add("<color=yellow>" + m + "</color>");
			stackTrace.Add("<color=yellow>" + s + "</color>");
			break;
		case LogType.Log:
			message.Add("<color=white>" + m + "</color>");
			stackTrace.Add("<color=white>" + s + "</color>");
			break;	
		}
		showStackTrace.Add(false);
	}
}
