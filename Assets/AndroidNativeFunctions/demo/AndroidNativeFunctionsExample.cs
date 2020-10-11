using UnityEngine;
using System.Collections.Generic;

public class AndroidNativeFunctionsExample : MonoBehaviour {
	
	public Texture2D shareTexture;
	private int window = 1;
	private GUIStyle style = new GUIStyle();
	public static string w;
	
	void Start(){
		style.normal.background =  NewTexture(new Color(0.27f,0.27f,0.27f,1));
		style.normal.textColor = Color.white;
		style.alignment = TextAnchor.MiddleCenter;
		style.fontSize = Mathf.RoundToInt(13 * Screen.width/(480*1));
	}

	void OnGUI(){
		GUI.depth = 100;
		switch(window){
		case 1:
			Window1();
			break;
		case 2:
			Window2();
			break;
		case 3:
			Window3();
			break;	
		}

		GUI.Label(NewRect(0,95,50,10),"Button Escape On/Off Console" + w);
	}
	
	void Window1(){
		if(GUI.Button(NewRect(5,5,40,10),"Start App",style)){
			AndroidNativeFunctions.StartApp("com.google.android.youtube",false);
		}
		
		if(GUI.Button(NewRect(5,20,40,10),"Get Installed Apps",style)){
			List<PackageInfo> packageInfo = AndroidNativeFunctions.GetInstalledApps();
			for(int i=0;i<packageInfo.Count;i++){
				print("firstInstallTime:" + packageInfo[i].firstInstallTime + " , lastUpdateTime: " + packageInfo[i].lastUpdateTime + " , packageName: " +
					packageInfo[i].packageName + " , versionCode: " + packageInfo[i].versionCode + " ,  versionName: " + packageInfo[i].versionName);
			}
		}
		
		if(GUI.Button(NewRect(5,35,40,10),"Get App Info",style)){
			PackageInfo packageInfo = AndroidNativeFunctions.GetAppInfo();
			//PackageInfo packageInfo = AndroidNativeFunctions.GetAppInfo("com.google.android.youtube");
			print("firstInstallTime:" + packageInfo.firstInstallTime + " , lastUpdateTime: " + packageInfo.lastUpdateTime + " , packageName: " +
				packageInfo.packageName + " , versionCode: " + packageInfo.versionCode + " ,  versionName: " + packageInfo.versionName);
		}
		
		if(GUI.Button(NewRect(5,50,40,10),"Get Device Info",style)){
			DeviceInfo deviceInfo = AndroidNativeFunctions.GetDeviceInfo();
			print(" CODENAME: " + deviceInfo.CODENAME + " , INCREMENTAL: " + deviceInfo.INCREMENTAL + " , RELEASE: " + deviceInfo.RELEASE
				 + " , SDK: " + deviceInfo.SDK);
		}
		
		if(GUI.Button(NewRect(5,65,40,10),"Get Android ID",style)){
			print("Android ID: " + AndroidNativeFunctions.GetAndroidID());
		}
		
		if(GUI.Button(NewRect(55,5,40,10),"Share Text",style)){
			AndroidNativeFunctions.ShareText("Hello World","Subject","Share Text");
		}
		
		if(GUI.Button(NewRect(55,20,40,10),"Share Image",style)){
			AndroidNativeFunctions.ShareImage("Hello World","Subject","Share Text",shareTexture);
		}
		
		if(GUI.Button(NewRect(55,35,40,10),"Show Progress Dialog",style)){
			AndroidNativeFunctions.ShowProgressDialog("Wait 2 seconds please");
			Invoke("HideProgressDialog",2);
		}
		
		if(GUI.Button(NewRect(55,50,40,10),"Show Toast",style)){
			AndroidNativeFunctions.ShowToast("Hello World",false);
		}
			
		if(GUI.Button(NewRect(55,65,40,10),"Immersive Mode",style)){
			AndroidNativeFunctions.ImmersiveMode();
		}
		
		if(GUI.Button(NewRect(5,80,40,10),"<<",style)){
			window = 3;
		}
		
		if(GUI.Button(NewRect(55,80,40,10),">>",style)){
			window = 2;
		}
	}
	
	void Window2(){
		if(GUI.Button(NewRect(5,5,40,10),"is Installed App",style)){
			print("is Installed App (Youtube): " + AndroidNativeFunctions.isInstalledApp("com.google.android.youtube"));
		}
		
		if(GUI.Button(NewRect(5,20,40,10),"Open Google Play",style)){
			AndroidNativeFunctions.OpenGooglePlay("com.google.android.youtube");
		}
		
		if(GUI.Button(NewRect(5,35,40,10),"is Rooted",style)){
			print("is Device Rooted: " + AndroidNativeFunctions.isDeviceRooted());
		}
		
		if(GUI.Button(NewRect(5,50,40,10),"is TV Device",style)){
			print("is TV Device: " + AndroidNativeFunctions.isTVDevice());
		}
		
		if(GUI.Button(NewRect(5,65,40,10),"is Wired Headset",style)){
			print("is Wired Headset: " + AndroidNativeFunctions.isWiredHeadset());
		}
		
		if(GUI.Button(NewRect(5,80,40,10),"<<",style)){
			window = 1;
		}
		
		if(GUI.Button(NewRect(55,5,40,10),"Has Vibrate",style)){
			print("Has Vibrate: " + AndroidNativeFunctions.hasVibrator());
		}
		
		if(GUI.Button(NewRect(55,20,40,10),"Vibrate 3 sec",style)){
			AndroidNativeFunctions.Vibrate(3000);
		}
		
		if(GUI.Button(NewRect(55,35,40,10),"Vibrate with pattern",style)){
			AndroidNativeFunctions.Vibrate(new long[]{0,100,200,300,400},-1);
		}
		
		if(GUI.Button(NewRect(55,50,40,10),"Vibrate with pattern (loop)",style)){
			AndroidNativeFunctions.Vibrate(new long[]{0,100,200,300,400},0);
		}
		
		if(GUI.Button(NewRect(55,65,40,10),"Vibrate Cancel",style)){
			AndroidNativeFunctions.VibrateCancel();
		}
		
		if(GUI.Button(NewRect(55,80,40,10),">>",style)){
			window = 3;
		}
	}
	
	void Window3(){
		if(GUI.Button(NewRect(5,5,40,10),"Show Alert",style)){
			AndroidNativeFunctions.ShowAlert("This Message","This Title","Ok","Back","Later",ShowAlertAction);
		}
		
		if(GUI.Button(NewRect(5,20,40,10),"Show Alert Input",style)){
			AndroidNativeFunctions.ShowAlertInput("This Text","This Message","This Title","Ok","Cancel",ShowAlertInputAction);
		}
		
		if(GUI.Button(NewRect(5,35,40,10),"Show Alert List",style)){
			AndroidNativeFunctions.ShowAlertList("This Title",new string[]{"Blue","Red","Green","Yellow"},ShowAlertListAction);
		}
		
		if(GUI.Button(NewRect(5,50,40,10),"Set Max Volume Level",style)){
			AndroidNativeFunctions.SetTotalVolume(15); // 15 Max
		}
		
		if(GUI.Button(NewRect(5,65,40,10),"Set Min Volume Level",style)){
			AndroidNativeFunctions.SetTotalVolume(0); //0 Min
		}
		
		if(GUI.Button(NewRect(55,5,40,10),"Get Volume Level",style)){
			print("Volume Level: " + AndroidNativeFunctions.GetTotalVolume());
		}
		
		if(GUI.Button(NewRect(55,20,40,10),"is Connect Internet",style)){
			print("is Connect Internet: " + AndroidNativeFunctions.isConnectInternet());
		}
		
		if(GUI.Button(NewRect(55,35,40,10),"is Connect WiFi",style)){
			print("is Connect Wifi: " + AndroidNativeFunctions.isConnectWifi());
		}
		
		if(GUI.Button(NewRect(55,50,40,10),"Get Battery Level",style)){
			print("Battery Level: " + AndroidNativeFunctions.GetBatteryLevel());
		}
		
		if(GUI.Button(NewRect(55,65,40,10),"Send Email",style)){
			AndroidNativeFunctions.SendEmail("Android Native Functions Super Unitypackage.","Android Native Functions","tibers28@gmail.com");
		}
		
		if(GUI.Button(NewRect(5,80,40,10),"<<",style)){
			window = 2;
		}
		
		if(GUI.Button(NewRect(55,80,40,10),">>",style)){
			window = 1;
		}
	}
	
	void HideProgressDialog(){
		AndroidNativeFunctions.HideProgressDialog();
	}
	
	void ShowAlertAction(DialogInterface w){
		AndroidNativeFunctions.ShowToast(w.ToString());
	}
			
	void ShowAlertInputAction(DialogInterface w,string t){
		AndroidNativeFunctions.ShowToast(w.ToString() + " " + t);
	}
	
	void ShowAlertListAction(string selectList){
		AndroidNativeFunctions.ShowToast(selectList.ToString());
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
	
	Rect NewRect(float x,float y,float width,float height){
		x = (Screen.width*x)/100f;
		y = (Screen.height*y)/100f;
		width = (Screen.width*width)/100f;
		height = (Screen.height*height)/100f;
		Rect rect = new Rect(x,y,width,height);
		return rect;
	}
}