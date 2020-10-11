using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Security.Cryptography;

//http://developer.android.com/reference/android/os/Build.VERSION.html
public class DeviceInfo  {
	public string CODENAME;
	public string INCREMENTAL;
	public string RELEASE;
	public int SDK;
}

//http://developer.android.com/reference/android/content/pm/PackageInfo.html
public class PackageInfo{
	public long firstInstallTime;
	public long lastUpdateTime;
	public string packageName;
	public int versionCode;
	public string versionName;
}

public enum DialogInterface{
	Positive = -1,
	Negative = -2,
	Neutral = -3
}

public class AndroidNativeFunctions : MonoBehaviour {
	
	
	private static bool immersiveMode;
	private static AndroidNativeFunctions instance;
	
	private static AndroidJavaObject vibrator;
	private static AndroidJavaObject progressDialog;
	private static AndroidJavaObject currentActivity{
		
		get{ 
			return new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
		}
		
	}
	
	private static void CreateGO(){
		if(instance != null)
			return;
		GameObject go = new GameObject("AndroidNativeFunctions");
		instance = go.AddComponent<AndroidNativeFunctions>();
	}
	
	void OnApplicationFocus(bool focusStatus){
		if(immersiveMode && focusStatus){
			ImmersiveMode();
		}
	}
	
	//=====================================================================================================
	
	public static void StartApp(string packageName,bool isExitThisApp){
		if(Application.platform != RuntimePlatform.Android)
			return;
		AndroidJavaObject launch = currentActivity.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getLaunchIntentForPackage",packageName);
 		currentActivity.Call("startActivity",launch);
		if(isExitThisApp){
			Application.Quit();
		}
	}

	public static void UninstallApp(string packageName)
    {
		//String appPackage = "com.your.app.package";

		AndroidJavaObject sharingIntent = new AndroidJavaObject("android.content.Intent");
		sharingIntent.Call<AndroidJavaObject>("setAction", "android.intent.action.ACTION_DELETE");

		//Uri uri = Uri.EscapeUriString("package", packageName, null);
		//Intent it = newIntent(Intent.ACTION_DELETE, uri);

		AndroidJavaObject launch = currentActivity.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("F").Call<AndroidJavaObject>("uninstall", packageName);

		//Intent intent = new Intent(getActivity(), getActivity().getClass());
		//PendingIntent sender = PendingIntent.getActivity(getActivity(), 0, intent, 0);
		//PackageInstaller mPackageInstaller = getActivity().getPackageManager().getPackageInstaller();
		//mPackageInstaller.uninstall(appPackage, sender.getIntentSender());
	}
	
	//=====================================================================================================

	public static List<PackageInfo> GetInstalledApps(){
		if(Application.platform != RuntimePlatform.Android)
			return null;
		AndroidJavaObject packages = currentActivity.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getInstalledPackages",0);
		int size = packages.Call<int>("size");
		List<PackageInfo> list = new List<PackageInfo>();
 		for(int i=0;i<size;i++){
			AndroidJavaObject info = packages.Call<AndroidJavaObject>("get",i);
			PackageInfo packageInfo = new PackageInfo();
			packageInfo.firstInstallTime = info.Get<long>("firstInstallTime");
			packageInfo.packageName = info.Get<string>("packageName");
			packageInfo.lastUpdateTime = info.Get<long>("lastUpdateTime");
			packageInfo.versionCode = info.Get<int>("versionCode");
			packageInfo.versionName = info.Get<string>("versionName");
			list.Add(packageInfo);
		}
		return list;
	}
	
	//=====================================================================================================
	
	public static PackageInfo GetAppInfo(){
		return GetAppInfo(currentActivity.Call<string>("getPackageName"));
	}
	
	public static PackageInfo GetAppInfo(string packageName){
		if(Application.platform != RuntimePlatform.Android)
			return null;
		AndroidJavaObject info = currentActivity.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getPackageInfo",packageName,0);
		PackageInfo packageInfo = new PackageInfo();
		packageInfo.firstInstallTime = info.Get<long>("firstInstallTime");
		packageInfo.packageName = info.Get<string>("packageName");
		packageInfo.lastUpdateTime = info.Get<long>("lastUpdateTime");
		packageInfo.versionCode = info.Get<int>("versionCode");
		packageInfo.versionName = info.Get<string>("versionName");
		return packageInfo;
	}
	
	//=====================================================================================================
	
	public static DeviceInfo GetDeviceInfo(){
		if(Application.platform != RuntimePlatform.Android)
			return null;
		AndroidJavaClass build = new AndroidJavaClass("android.os.Build$VERSION");
		DeviceInfo deviceInfo = new DeviceInfo();
		deviceInfo.CODENAME = build.GetStatic<string>("CODENAME");
		deviceInfo.INCREMENTAL = build.GetStatic<string>("INCREMENTAL");
		deviceInfo.RELEASE = build.GetStatic<string>("RELEASE");
		deviceInfo.SDK = build.GetStatic<int>("SDK_INT");
		return deviceInfo;
	}
	
	//=====================================================================================================
	
	public static string GetAndroidID(){
		if(Application.platform != RuntimePlatform.Android)
			return "";
		AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject> ("getContentResolver");
 		return new AndroidJavaClass ("android.provider.Settings$Secure").CallStatic<string> ("getString", contentResolver, "android_id");
	}
	
	//=====================================================================================================
	
	public static void ShareText(string text,string subject,string chooser){
		if(Application.platform != RuntimePlatform.Android)
			return;
		AndroidJavaObject sharingIntent  = new AndroidJavaObject("android.content.Intent");
		sharingIntent.Call<AndroidJavaObject>("setAction","android.intent.action.SEND");
		sharingIntent.Call<AndroidJavaObject>("setType","text/plain");
		sharingIntent.Call<AndroidJavaObject>("putExtra","android.intent.extra.TEXT",text);
		sharingIntent.Call<AndroidJavaObject>("putExtra","android.intent.extra.SUBJECT",subject);
		AndroidJavaObject createChooser = sharingIntent.CallStatic<AndroidJavaObject>("createChooser",sharingIntent,chooser);
		currentActivity.Call("startActivity",createChooser);
	}
	
	public static void ShareImage(string text,string subject,string chooser,Texture2D picture){	
		byte[] bytes = new AndroidJavaObject("android.util.Base64").CallStatic<byte[]>("decode",System.Convert.ToBase64String (picture.EncodeToPNG()),0);
		AndroidJavaObject bitmap =  new AndroidJavaObject("android.graphics.BitmapFactory").CallStatic<AndroidJavaObject>("decodeByteArray",bytes,0,bytes.Length);
		AndroidJavaObject compress = new AndroidJavaClass("android.graphics.Bitmap$CompressFormat").GetStatic<AndroidJavaObject>("JPEG");
		bitmap.Call<bool>("compress",compress,100,new AndroidJavaObject("java.io.ByteArrayOutputStream"));
		string path = new AndroidJavaClass("android.provider.MediaStore$Images$Media").CallStatic<string>("insertImage",currentActivity.Call<AndroidJavaObject>("getContentResolver"),bitmap,picture.name,"");
		AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse",path);
		AndroidJavaObject sharingIntent  = new AndroidJavaObject("android.content.Intent");
		sharingIntent.Call<AndroidJavaObject>("setAction","android.intent.action.SEND");
		sharingIntent.Call<AndroidJavaObject>("setType","image/*");
		sharingIntent.Call<AndroidJavaObject>("putExtra","android.intent.extra.STREAM",uri);
		sharingIntent.Call<AndroidJavaObject>("putExtra","android.intent.extra.TEXT",text);
		sharingIntent.Call<AndroidJavaObject>("putExtra","android.intent.extra.SUBJECT",subject);
		AndroidJavaObject createChooser = sharingIntent.CallStatic<AndroidJavaObject>("createChooser",sharingIntent,chooser);
		currentActivity.Call("startActivity",createChooser);
	}
	
	//=====================================================================================================
	
	public static void ShowAlert(string message,string title,string positiveButton,string negativeButton,string neutralButton,UnityAction<DialogInterface> action){
		if(Application.platform != RuntimePlatform.Android)
			return;
		currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			AndroidJavaObject builder = new AndroidJavaObject("android/app/AlertDialog$Builder",currentActivity);
			builder.Call<AndroidJavaObject>("setMessage",message);
			builder.Call<AndroidJavaObject>("setCancelable",false);
			if(!string.IsNullOrEmpty(title)){
				builder.Call<AndroidJavaObject>("setTitle",title);
			}
			builder.Call<AndroidJavaObject>("setPositiveButton",positiveButton,new ShowAlertListener(action));
			if(!string.IsNullOrEmpty(negativeButton)){
				builder.Call<AndroidJavaObject>("setNegativeButton",negativeButton,new ShowAlertListener(action));
			}
			if(!string.IsNullOrEmpty(neutralButton)){
				builder.Call<AndroidJavaObject>("setNeutralButton",neutralButton,new ShowAlertListener(action));
			}
			AndroidJavaObject dialog = builder.Call<AndroidJavaObject>("create");
			dialog.Call("show");
		}));
	}
	
 
	private class ShowAlertListener: AndroidJavaProxy{
		private UnityAction<DialogInterface> action;
		public ShowAlertListener(UnityAction<DialogInterface> a): base("android.content.DialogInterface$OnClickListener"){
			action = a;
		}
		public void onClick(AndroidJavaObject obj,int which){
			action((DialogInterface)which);
		}
	}
 
	
	//=====================================================================================================
	
	public static void ShowAlertInput(string text,string message,string title,string positiveButton,string negativeButton,UnityAction<DialogInterface,string> action){
		if(Application.platform != RuntimePlatform.Android)
			return;
		currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			AndroidJavaObject builder = new AndroidJavaObject("android/app/AlertDialog$Builder",currentActivity);
			AndroidJavaObject editText = new AndroidJavaObject("android.widget.EditText",currentActivity);
			if(!string.IsNullOrEmpty(text)){
				editText.Call("setText",text);
			}
			builder.Call<AndroidJavaObject>("setView",editText);
			if(!string.IsNullOrEmpty(message)){
				builder.Call<AndroidJavaObject>("setMessage",message);
			}
			builder.Call<AndroidJavaObject>("setCancelable",false);
			if(!string.IsNullOrEmpty(title)){
				builder.Call<AndroidJavaObject>("setTitle",title);
			}
			builder.Call<AndroidJavaObject>("setPositiveButton",positiveButton,new ShowAlertInputListener(action,editText));
			if(!string.IsNullOrEmpty(negativeButton)){
				builder.Call<AndroidJavaObject>("setNegativeButton",negativeButton,new ShowAlertInputListener(action,editText));
			}
			AndroidJavaObject dialog = builder.Call<AndroidJavaObject>("create");
			dialog.Call("show");
		}));
 
	}
	
 
	private class ShowAlertInputListener: AndroidJavaProxy{
		private UnityAction<DialogInterface,string> action;
		private AndroidJavaObject editText;
		public ShowAlertInputListener(UnityAction<DialogInterface,string> a,AndroidJavaObject et): base("android.content.DialogInterface$OnClickListener"){
			action = a;
			editText = et;
		}
		public void onClick(AndroidJavaObject obj,int which){
			action((DialogInterface)which,editText.Call<AndroidJavaObject>("getText").Call<string>("toString"));
		}
	}
 
	
	//=====================================================================================================
	
	public static void ShowAlertList(string title,string[] list,UnityAction<string> action){
		if(Application.platform != RuntimePlatform.Android)
			return;
		currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			AndroidJavaObject builder = new AndroidJavaObject("android/app/AlertDialog$Builder",currentActivity);
			builder.Call<AndroidJavaObject>("setCancelable",false);
			if(!string.IsNullOrEmpty(title)){
				builder.Call<AndroidJavaObject>("setTitle",title);
			}
			builder.Call<AndroidJavaObject>("setItems",list,new ShowAlertListListener(action,list));
			AndroidJavaObject dialog = builder.Call<AndroidJavaObject>("create");
			dialog.Call("show");
		}));
	}
	
 
	private class ShowAlertListListener: AndroidJavaProxy{
		private string[] list;
		private UnityAction<string> action;
		public ShowAlertListListener(UnityAction<string> w,string[] a): base("android.content.DialogInterface$OnClickListener"){
			action = w;
			list = a;
		}
		public void onClick(AndroidJavaObject obj,int which){
			action(list[which]);
		}
	}
 
	
	//=====================================================================================================
	
	public static void ShowToast(string message){
		ShowToast(message,true);
	}
	
	public static void ShowToast(string message,bool shortDuration){
		if(Application.platform != RuntimePlatform.Android)
			return;	
		currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			AndroidJavaObject toast = new AndroidJavaObject("android.widget.Toast",currentActivity);
			toast.CallStatic<AndroidJavaObject>("makeText",currentActivity,message,(shortDuration?0:1)).Call("show");
		}));
	}
	
	//=====================================================================================================
	
	public static void ShowProgressDialog(string message){
		ShowProgressDialog(message,"");
	}
	
	public static void ShowProgressDialog(string message,string title){
		if(Application.platform != RuntimePlatform.Android)
			return;
		if(progressDialog != null)
			HideProgressDialog();
		currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			progressDialog = new AndroidJavaObject("android.app.ProgressDialog",currentActivity);
			progressDialog.Call("setProgressStyle",0);
			progressDialog.Call("setIndeterminate",true);
			progressDialog.Call("setCancelable",false);
			progressDialog.Call("setMessage",message);
			if(!string.IsNullOrEmpty(title)){
				progressDialog.Call("setTitle",title);
			}
			progressDialog.Call("show");
		}));
	}
	
	public static void HideProgressDialog(){
		if(progressDialog == null)
			return;
		currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			progressDialog.Call("hide");
			progressDialog.Call("dismiss");
			progressDialog = null;
		}));
	}
	
	//=====================================================================================================
	
	public static void ImmersiveMode(){
		if(Application.platform != RuntimePlatform.Android)
			return;
		int sdk = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
		if(sdk < 19)
			return;
		CreateGO();
		immersiveMode = true;
		currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			AndroidJavaClass cView = new AndroidJavaClass("android.view.View");
			AndroidJavaObject id = currentActivity.Call<AndroidJavaObject>("findViewById",new AndroidJavaClass("android.R$id").GetStatic<int>("content"));
			id.Call("setSystemUiVisibility",cView.GetStatic<int>("SYSTEM_UI_FLAG_LAYOUT_STABLE") |
	        cView.GetStatic<int>("SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION") |
	        cView.GetStatic<int>("SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN") |
	        cView.GetStatic<int>("SYSTEM_UI_FLAG_HIDE_NAVIGATION") |
	        cView.GetStatic<int>("SYSTEM_UI_FLAG_FULLSCREEN") |
	        cView.GetStatic<int>("SYSTEM_UI_FLAG_IMMERSIVE_STICKY"));
		}));
	}
	
	//=====================================================================================================
	
	public static void OpenGooglePlay(string packageName){
		if(Application.platform == RuntimePlatform.Android){
			Application.OpenURL("market://details?id="+packageName);
		}else{
			Application.OpenURL("https://play.google.com/store/apps/details?id="+packageName);
		}
	}
	
	//=====================================================================================================
	
	public static bool isDeviceRooted(){
		if(Application.platform != RuntimePlatform.Android)
			return false;	
		string[] paths = { "/system/app/Superuser.apk", "/sbin/su", "/system/bin/su", "/system/xbin/su", "/data/local/xbin/su", "/data/local/bin/su", "/system/sd/xbin/su","/system/bin/failsafe/su", "/data/local/su" };
		foreach(string path in paths){
			if(File.Exists(path)){
				return true;
			}
		}
		string buildTags = new AndroidJavaClass("android.os.Build").GetStatic<string>("TAGS");
		if((buildTags != null && buildTags.Contains("test-keys"))){
			return true;
		}
		return false;
	}
	
	//=====================================================================================================
	
	public static bool isInstalledApp(string packageName){
		if(Application.platform != RuntimePlatform.Android)
			return false;	
		try{
			currentActivity.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getPackageInfo",packageName,0);
			return true;
		}
		catch{
			return false;
		}
	}
	
	//=====================================================================================================

	public static void Vibrate(long milliseconds){
		if(Application.platform != RuntimePlatform.Android)
			return;
		try{
			if(vibrator == null)
				vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService","vibrator");
			vibrator.Call("vibrate",milliseconds);
		}
		catch{
			Handheld.Vibrate();
		}	
	}
	
	public static void Vibrate(long[] pattern, int repeat){
		if(Application.platform != RuntimePlatform.Android)
			return;	
		try{
			if(vibrator == null)
				vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService","vibrator");
			vibrator.Call("vibrate",pattern,repeat);
		}
		catch{
			Handheld.Vibrate();
		}
	}
	
	public static void VibrateCancel(){	
		if(vibrator == null)
			return;
		vibrator.Call("cancel");
	}
	
	public static bool hasVibrator(){	
		if(vibrator == null)
				vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService","vibrator");
		return vibrator.Call<bool>("hasVibrator");
	}
 
	
	//=====================================================================================================
	
	public static bool isTVDevice(){
		if(Application.platform != RuntimePlatform.Android)
			return false; 	
		int tvDevice = currentActivity.Call<AndroidJavaObject>("getSystemService","uimode").Call<int>("getCurrentModeType");
		if(tvDevice == 4)
			return true;
		return false;
 
	}
	
	//=====================================================================================================
	
	public static bool isWiredHeadset(){
		if(Application.platform != RuntimePlatform.Android)
			return false;
		return currentActivity.Call<AndroidJavaObject>("getSystemService","audio").Call<bool>("isWiredHeadsetOn");
	}
	
	//=====================================================================================================
	
	public static void SetTotalVolume(int volumeLevel){
		if(Application.platform != RuntimePlatform.Android)
			return;
		volumeLevel = Mathf.Clamp(volumeLevel,0,15);
		currentActivity.Call<AndroidJavaObject>("getSystemService","audio").Call("setStreamVolume",3,volumeLevel,0);
	}
		
	//=====================================================================================================
	
	public static int GetTotalVolume(){
		if(Application.platform != RuntimePlatform.Android)
			return 0;
		return currentActivity.Call<AndroidJavaObject>("getSystemService","audio").Call<int>("getStreamVolume",3);
	}
	
	//=====================================================================================================
	
	public static bool isConnectInternet(){
		if(Application.platform != RuntimePlatform.Android)
			return false;	
		try{
			AndroidJavaObject netInfo = currentActivity.Call<AndroidJavaObject>("getSystemService","connectivity").Call<AndroidJavaObject>("getActiveNetworkInfo");
			if(netInfo == null)
				return false;
			return netInfo.Call<bool>("isConnectedOrConnecting");
		}
		catch{
			return false;
		}
	}
	
	//=====================================================================================================
	
	public static bool isConnectWifi(){
		if(Application.platform != RuntimePlatform.Android)
			return false;	
		try{
			AndroidJavaObject netInfo = currentActivity.Call<AndroidJavaObject>("getSystemService","connectivity").Call<AndroidJavaObject>("getNetworkInfo",1);
			if(netInfo == null)
				return false;
			return netInfo.Call<bool>("isConnectedOrConnecting");
		}
		catch{
			return false;
		}
	}
	
	//=====================================================================================================
	
	public static int GetBatteryLevel(){
		if(Application.platform != RuntimePlatform.Android)
			return 0;
		AndroidJavaObject batteryIntent = currentActivity.Call<AndroidJavaObject>("getApplicationContext").Call<AndroidJavaObject>("registerReceiver",null,new AndroidJavaObject("android.content.IntentFilter","android.intent.action.BATTERY_CHANGED"));
		int level = batteryIntent.Call<int>("getIntExtra","level",-1);
		int scale = batteryIntent.Call<int>("getIntExtra","scale",-1);
		if(level == -1 || scale == -1){
			return 0;	
		}
		return (int)(((float)level/(float)scale)*100f);
	}
	
	//=====================================================================================================
	
	public static void SendEmail(string text,string subject,string email){
		if(Application.platform != RuntimePlatform.Android)
			return;
		AndroidJavaObject intent  = new AndroidJavaObject("android.content.Intent");
		intent.Call<AndroidJavaObject>("setAction","android.intent.action.SENDTO");
		intent.Call<AndroidJavaObject>("setType","text/plain");
		intent.Call<AndroidJavaObject>("putExtra","android.intent.extra.TEXT",text);
		intent.Call<AndroidJavaObject>("putExtra","android.intent.extra.SUBJECT",subject);
		intent.Call<AndroidJavaObject>("setData",new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse","mailto:"+email));
		currentActivity.Call("startActivity",intent);
	}
	
	//=====================================================================================================
	
	public static bool VerifyGooglePlayPurchase(string purchaseJson, string base64Signature, string publicKey){
		using (var provider = new RSACryptoServiceProvider()){
                try{
                    provider.FromXmlString(publicKey);
                    var signature = Convert.FromBase64String(base64Signature);
                    var sha = new SHA1Managed();
                    var data = System.Text.Encoding.UTF8.GetBytes(purchaseJson);
                    return provider.VerifyData(data, sha, signature);
                }
                catch (Exception e){
                    print(e);
                }
                return false;
            }
	}
	
	//=====================================================================================================
				
}
