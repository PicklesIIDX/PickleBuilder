using UnityEditor;
using System.IO;
using LitJson;
using System.Diagnostics;
using System.Collections;
using PickleTools.UnityEditor;

namespace PickleTools.PickleBuilder {

	public enum BuildType {
		none = 0,
		win_x86 = 1,
		win_x64 = 2,
		mac = 3,
		max_x86 = 4,
		mac_x64 = 5,
		linux = 6,
		linux_x86 = 7,
		linux_x64 = 8,
		android = 9,
		blackberry = 10,
		ios = 11,
		ps3 = 12,
		ps4 = 13,
		vita = 14,
		xbox_360 = 15,
		xbone = 16,
		win_phone_8 = 17,
		win_store_app = 18,
		web = 19,
		samsung_tv = 20,
		tizen = 21,
		tv_os = 22,
		web_gl = 23,
		wiiu = 24,
		new_3ds = 25,
	}

	public class ConfigData{
		public string BuildPath = "";
		public string FileName = "";
		public int VersionNumberMajor = 0;
		public int VersionNumberMinor = 0;
		public int VersionNumberPatch = 0;
		public bool AutoIncrement = true;
		public string[] Platforms = new string[0];
		public string[] Scenes = new string[0];
		public bool Preview = false;
		public SteamData SteamData = new SteamData();
	}

	public class SteamData{
		public bool BuildToSteam = false;
		public string SDKPath = "C:\\steamworks_sdk\\tools\\ContentBuilder\\";
		public string SteamCMDPath = "builder\\steamcmd.exe";
		public DepotData[] Depots = new DepotData[0];
		public AppBuildData AppBuild = new AppBuildData();
		public BatchData BatchData = new BatchData();
		public bool UseExistingBuilds = false;
		public string ExistingBuildsPath = "C:\\";
	}

	public class DepotData {
		public string DepotID = "000001";
		public string BuildType = "win_x86";
		public string[] IgnoreFiles = new string[1]{"*.pdb"};
	}

	public class AppBuildData {
		public string AppID = "000000";
		public string Description = "Description of this build.";
		public bool Preview = true;
	}

	public class BatchData {
		
	}
	
	public class BuildScript {

		EditorCoroutine editorCoroutine;

		public static readonly string CONFIG_FILENAME = "build_config.json";

		static string configPath = "";
		public static string ConfigPath {
			get {
				configPath = UnityEngine.Application.dataPath + 
				                        "/PickleTools/PickleBuilder/Editor/" + 
				                        CONFIG_FILENAME;
				System.Console.WriteLine("Config Path: " + configPath);
				return configPath;
			}
		}

		static public void EditConfig(){
			if(!System.IO.File.Exists(ConfigPath)){
				ConfigData newData = new ConfigData();
				WriteData(newData);
			}

			Process.Start(ConfigPath);
		}

		public static void PerformBuildCommand(){
			string accountName = GetArg("-account");
			string password = GetArg("-password");
			PerformBuild(accountName, password);
		}

		private static string GetArg(string name){
			var args = System.Environment.GetCommandLineArgs();
			for(int i = 0; i < args.Length; i ++){
				if(args[i] == name && args.Length > i + 1){
					return args[i + 1];
				}
			}
			return null;
		}

		[MenuItem("Build/Build From Config")]
		public static void PerformBuild(string accountName = "", string password = ""){
			if(!System.IO.File.Exists(ConfigPath)){
				ConfigData newData = new ConfigData();
				WriteData(newData);
			}

			ConfigData data = JsonMapper.ToObject<ConfigData>(File.ReadAllText(ConfigPath));

			int versionNumberMajor = 0;
			int versionNumberMinor = 0;
			int versionNumberPatch = 1;
			if(data.VersionNumberMajor != 0){
				versionNumberMajor = data.VersionNumberMajor;
			} else {
				data.VersionNumberMajor = versionNumberMajor;
			}
			if(data.VersionNumberMinor != 0){
				versionNumberMinor = data.VersionNumberMinor;
			} else {
				data.VersionNumberMinor = versionNumberMinor;
			}
			if(data.VersionNumberPatch != 1){
				versionNumberPatch = data.VersionNumberPatch;
			} else {
				data.VersionNumberPatch = versionNumberPatch;
			}

			bool autoIncrement = true;
			if(data.AutoIncrement != true){
				autoIncrement = data.AutoIncrement;
			} else {
				data.AutoIncrement = autoIncrement;
			}
			//if(autoIncrement){
			//	data.VersionNumberPatch = versionNumberPatch = versionNumberPatch + 1;
			//}

			string completeVersionString = versionNumberMajor + "_" + versionNumberMinor + "_" + versionNumberPatch + "__" + 
				System.DateTime.UtcNow.ToString().Replace('/', '_').Replace(' ', '_').Replace(':', '_');
			string path = System.Environment.CurrentDirectory + "/build" + "/" + completeVersionString + "/";
			if(data.BuildPath != ""){
				path = data.BuildPath + "/" + completeVersionString + "/";
			} else {
				data.BuildPath = path;
			}
			string fileName = "Game Name";
			if(data.FileName != ""){
				fileName = data.FileName;
			} else {
				data.FileName = fileName;
			}

			string[] platforms = new string[1]{"win_x86"};
			if(data.Platforms.Length > 0){
				platforms = data.Platforms;
			} else {
				data.Platforms = platforms;
			}

			string[] scenes = {
			};
			if(data.Scenes.Length > 0){
				scenes = data.Scenes;
			} else {
				data.Scenes = scenes;
			}


			WriteData(data);

			for(int p = 0; p < platforms.Length; p ++){
				string platformPath = path + platforms[p];
				if(!System.IO.Directory.Exists(platformPath)){
					System.IO.Directory.CreateDirectory(platformPath);
				}
				string buildPath = platformPath + "/" + fileName;
				UnityEngine.Debug.LogWarning("Building at: " + buildPath);
				// skip the actual build if this is just a preview.
				if(data.Preview){
					UnityEngine.Debug.LogWarning("but skipping the build due to this being a preview...");
					continue;
				}
				switch(platforms[p]){
				case "win_x86":
					buildPath += ".exe";
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneWindows, BuildOptions.None);
					break;
				case "win_x64":
					buildPath += ".exe";
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
					break;
				case "mac":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneOSXUniversal, BuildOptions.None);
					break;
				case "mac_x86":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneOSXIntel, BuildOptions.None);
					break;
				case "mac_x64":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneOSXIntel64, BuildOptions.None);
					break;
				case "linux":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneLinuxUniversal, BuildOptions.None);
					break;
				case "linux_x86":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneLinux, BuildOptions.None);
					break;
				case "linux_x64":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneLinux64, BuildOptions.None);
					break;
				case "android":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Android, BuildOptions.None);
					break;
				case "blackberry":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.BlackBerry, BuildOptions.None);
					break;
				case "ios":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.iOS, BuildOptions.None);
					break;
				case "ps3":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.PS3, BuildOptions.None);
					break;
				case "ps4":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.PS4, BuildOptions.None);
					break;
				case "vita":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.PSP2, BuildOptions.None);
					break;
				case "360":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.XBOX360, BuildOptions.None);
					break;
				case "xbone":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.XboxOne, BuildOptions.None);
					break;
				case "win_phone_8":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WP8Player, BuildOptions.None);
					break;
				case "win_store_app":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WSAPlayer, BuildOptions.None);
					break;
				case "web":
					data.FileName = fileName;
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WebPlayer, BuildOptions.None);
					break;
				case "new3ds":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Nintendo3DS, BuildOptions.None);
					break;
				case "samsung_tv":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.SamsungTV, BuildOptions.None);
					break;
				case "tizen":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Tizen, BuildOptions.None);
					break;
				case "tv_os":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.tvOS, BuildOptions.None);
					break;
				case "web_gl":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WebGL, BuildOptions.None);
					break;
				case "wiiu":
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WiiU, BuildOptions.None);
					break;
				default:
					BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneWindows, BuildOptions.None);
					break;
				}
			}

			// TODO: Make sure if we aren't on windows, we just create the appropriate files

			SteamData steamData = new SteamData();
			if(data.SteamData.SDKPath != ""){
				steamData = data.SteamData;
			} else {
				data.SteamData = steamData;
			}

			if(steamData.BuildToSteam){
				PushToSteam(accountName, password, completeVersionString);
			}
		}

		static void PushToSteam(string accountName, string password, string completeVersionString){
			
			if(!System.IO.File.Exists(ConfigPath)){
				ConfigData newData = new ConfigData();
				WriteData(newData);
			}

			ConfigData data = JsonMapper.ToObject<ConfigData>(File.ReadAllText(ConfigPath));
			SteamData steamData = new SteamData();
			if(data.SteamData.SDKPath != ""){
				steamData = data.SteamData;
			} else {
				data.SteamData = steamData;
			}
			// get builds path
			string path = "";
			if(data.SteamData.UseExistingBuilds){
				path = data.SteamData.ExistingBuildsPath;
			} else {
				int versionNumberMajor = 0;
				int versionNumberMinor = 0;
				int versionNumberPatch = 1;
				if(data.VersionNumberMajor != 0){
					versionNumberMajor = data.VersionNumberMajor;
				} else {
					data.VersionNumberMajor = versionNumberMajor;
				}
				if(data.VersionNumberMinor != 0){
					versionNumberMinor = data.VersionNumberMinor;
				} else {
					data.VersionNumberMinor = versionNumberMinor;
				}
				if(data.VersionNumberPatch != 1){
					versionNumberPatch = data.VersionNumberPatch;
				} else {
					data.VersionNumberPatch = versionNumberPatch;
				}
				path = System.Environment.CurrentDirectory + "/build" + "/" + completeVersionString + "/";
				if(data.BuildPath != ""){
					path = data.BuildPath + "/" + completeVersionString + "/";
				} else {
					data.BuildPath = path;
				}
			}

			// create content directories for each build
			string scriptsPath = steamData.SDKPath + "/scripts/";
			if(!System.IO.Directory.Exists(scriptsPath)){
				UnityEngine.Debug.LogError("[BuildScript.cs]: Could not find the scripts folder of the Steamworks SDK. " +
					"Please make sure your SDK path is correct and that this path exists: \n" + scriptsPath);

			} else {
				string textFile = "";
				byte[] textBytes = new byte[0];
				// 1. generate a vdf file for each depot
				for(int d = 0; d < steamData.Depots.Length; d ++){
					string contentPath = ".\\" + steamData.Depots[d].BuildType + "\\*";

					FileStream depotStream = System.IO.File.Create(scriptsPath + "depot_build_" + steamData.Depots[d].DepotID + ".vdf");
					textFile = 
						"\"DepotBuildConfig\"\n" +
						"{\n" +
						"\t\"DepotID\" \"" + steamData.Depots[d].DepotID + "\"\n\n" +

						"\t\"ContentRoot\" \"\"\n\n" +

						"\t\"FileMapping\"\n"+
						"\t{\n" +
						"\t\t\"LocalPath\" \"" + contentPath + "\"\n\n" +

						"\t\t\"DepotPath\" \".\"\n\n" +

						"\t\t\"recursive\" \"1\"\n" +
						"\t}\n\n";

					for(int e = 0; e < steamData.Depots[d].IgnoreFiles.Length; e ++){
						textFile += "\t\"FileExclusion\" \"" + steamData.Depots[d].IgnoreFiles[e] + "\"\n";
					}
					textFile += "}";

					textBytes = System.Text.UTF8Encoding.UTF8.GetBytes(textFile);
					depotStream.Write(textBytes, 0, textBytes.Length);
					depotStream.Flush();
					depotStream.Close();
				}

				//  2. generate a vdf file for the appdata
				int previewValue = 0;
				if(steamData.AppBuild.Preview){
					previewValue = 1;
				}
				FileStream appDataStream = System.IO.File.Create(scriptsPath + "app_build_" + steamData.AppBuild.AppID + ".vdf");
				textFile = 
					"\"appbuild\"\n" +
					"{\n" +
					"\t\"appid\" \"" + steamData.AppBuild.AppID + "\"\n" +
					"\t\"desc\" \"" + steamData.AppBuild.Description + "\"\n" +
					"\t\"buildoutput\" \"..\\output\\\"\n" +
					"\t\"contentroot\" \"" + path + "\"\n" +
					"\t\"setlive\" \"\"\n" +
					"\t\"preview\" \"" + previewValue + "\"\n" +
					"\t\"local\" \"\"\n\n" +

					"\t\"depots\"\n" +
					"\t{\n";
				for(int d = 0; d < steamData.Depots.Length; d ++){
					textFile += "\t\t\"" + steamData.Depots[d].DepotID + "\" \"" + ( "depot_build_" + steamData.Depots[d].DepotID + ".vdf") + "\"\n";
				}
				textFile += "\t}\n";
				textFile += "}";

				textBytes = System.Text.UTF8Encoding.UTF8.GetBytes(textFile);
				appDataStream.Write(textBytes, 0, textBytes.Length);
				appDataStream.Flush();
				appDataStream.Close();

				// 3. run the sdk build function
				UnityEngine.Debug.LogWarning("Starting steam publishing process...");
				string appBuildFilePath = steamData.SDKPath + "/scripts/app_build_" + steamData.AppBuild.AppID + ".vdf";
				ProcessStartInfo startInfo = new ProcessStartInfo()
				{
					FileName = steamData.SteamCMDPath,
					Arguments = "+login " +
						accountName + " " + 
						password + " " + 
						"+run_app_build_http " + appBuildFilePath + " +quit",
					UseShellExecute = false,
					RedirectStandardOutput  = true,
				};
				Process proc = new Process()
				{
					StartInfo = startInfo,
				};
				proc.Start();
				// Editor coroutine is being exited early by unity, so we'll just skip it for now
				//EditorCoroutine.start(DisplaySteamLog(proc, steamData.SDKPath + "/scripts/"));
				while (!proc.StandardOutput.EndOfStream) {
					UnityEngine.Debug.Log(proc.StandardOutput.ReadLine());
				}
				UnityEngine.Debug.Log("[BuildScript.cs]: Steam upload process exited.");
				Process.Start(steamData.SDKPath + "/scripts/");
			}
		}

		static IEnumerator DisplaySteamLog(Process proc, string scriptPath){
			while(!proc.StandardOutput.EndOfStream){
				UnityEngine.Debug.Log (proc.StandardOutput.ReadLine ());
				yield return null;
			}
			UnityEngine.Debug.Log ("[BuildScript.cs]: Steam upload process exited.");
			// 4. open the scripts folder for validation
			Process.Start(scriptPath);
			// 5. open steamworks app page for validation
		}

		public static void WriteData(ConfigData newData){
			FileStream configStream = File.Create(ConfigPath);
			JsonWriter writer = new JsonWriter();
			writer.PrettyPrint = true;
			JsonMapper.ToJson(newData, writer);
			string newDataJson = writer.TextWriter.ToString();
			byte[] jsonBytes = System.Text.UTF8Encoding.UTF8.GetBytes(newDataJson);
			configStream.Write(jsonBytes, 0, jsonBytes.Length);
			configStream.Flush();
			configStream.Close();
			string importPath = ConfigPath.Substring(ConfigPath.IndexOf("Assets/"));
			AssetDatabase.ImportAsset(importPath);
		}
	}

}