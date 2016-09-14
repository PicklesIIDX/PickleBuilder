using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LitJson;
using PickleTools.Extensions.ArrayExtensions;
using PickleTools.UnityEditor;

namespace PickleTools.PickleBuilder {
	public class BuildConfigWindow : EditorWindow {

		// TODO:
		// [ ] don't save account info
		// [ ] include instructions on how to set up your sdk folders
		// [ ] test on windows
		// [ ] remove specific scenes
		// [ ] get paths from scene files
		// [ ] add developer name
		// [ ] set unity build settings of name, developer, and version from this config
		// [ ] allow steam uploading of most recent build
		// [ ] auto increment patch version
		// [ ] 

		ConfigData data;
		string helpInfo = "";
		Vector2 scrollPosition = Vector2.zero;

		SortableList<EditorBuildSettingsScene> sceneList;

		GUISkin skin;
		private static readonly string BUILD_CONFIG_SKIN = "Assets/PickleTools/PickleBuilder/Editor/build_config_skin.guiskin";
//		private static readonly string SOURCE = "source";

		[MenuItem("Build/Build Config Window")]
		public static void ShowEditor(){
			BuildConfigWindow window = GetWindow<BuildConfigWindow>();
			window.titleContent = new GUIContent("Build Config");
			window.Show();
			window.Initialize();
		}

//		Object[] scenes = new Object[0];

		void Initialize(){
			if(!System.IO.File.Exists(BuildScript.ConfigPath)){
				ConfigData newData = new ConfigData();
				BuildScript.WriteData(newData);
			}
			
			data = JsonMapper.ToObject<ConfigData>(File.ReadAllText(BuildScript.ConfigPath));
			skin = AssetDatabase.LoadAssetAtPath<GUISkin>(BUILD_CONFIG_SKIN);
			if(skin == null){
				skin = CreateInstance<GUISkin>();
			}

			sceneList = new SortableList<EditorBuildSettingsScene>();
		}

		void DrawSceneListEntry(EditorBuildSettingsScene entry, float entryHeight, int entryNumber){
			GUILayout.BeginVertical(skin.box, GUILayout.Height(entryHeight));
			GUILayout.BeginHorizontal();
			entry.enabled = GUILayout.Toggle(entry.enabled, "");
			GUILayout.Label(entry.path.ToString());
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		void DrawSceneListEntryDrag(EditorBuildSettingsScene entry, Rect dragRect){
			GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			GUI.Box(dragRect, entry.path);
			GUI.color = Color.white;
			Repaint();
		}

		void OnGUI(){
			if(data == null){
				if(GUILayout.Button("Reload Config")){
					Initialize();
				}
				if(GUILayout.Button("Open Config File")){
					BuildScript.EditConfig();
				}
			} else {
				if(helpInfo == ""){
					helpInfo = "Click on an object to learn more about it!";
				}
				GUI.SetNextControlName("help box");

				scrollPosition = GUILayout.BeginScrollView(scrollPosition);

				GUILayout.Box(helpInfo, GUILayout.Height(60), GUILayout.Width(position.width - 8));
				GUI.SetNextControlName("build path");
				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(new GUIContent("Build Path", "The location your built game will be saved."));
				int buttonNameLength = 40;
				string buttonName = data.BuildPath.Substring(Mathf.Max(0, data.BuildPath.Length - 
					buttonNameLength), Mathf.Min(buttonNameLength, data.BuildPath.Length));
				if(buttonName.Length == buttonNameLength){
					buttonName = "..." + buttonName;
				}
				if(GUILayout.Button(buttonName)){
					string selectedPath = EditorUtility.OpenFolderPanel("Build Path", data.BuildPath, data.BuildPath);
					if(selectedPath != ""){
						data.BuildPath = selectedPath;
					}
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(new GUIContent("File Name", "The name of the file"), skin.label);
				GUI.SetNextControlName("file name");
				data.FileName = GUILayout.TextField(data.FileName, skin.textArea);
				GUILayout.EndHorizontal();
				GUI.SetNextControlName("version number");
				data.VersionNumberMajor = EditorGUILayout.IntField("Version Number Major", data.VersionNumberMajor);
				data.VersionNumberMinor = EditorGUILayout.IntField("Version Number Minor", data.VersionNumberMinor);
				data.VersionNumberPatch = EditorGUILayout.IntField("Version Number Patch", data.VersionNumberPatch);
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("New Major Version")){
					data.VersionNumberMajor ++;
					data.VersionNumberMinor = 0;
					data.VersionNumberPatch = 0;
				}
				if(GUILayout.Button("New Minor Version")){
					data.VersionNumberMinor ++;
					data.VersionNumberPatch = 0;
				}
				GUILayout.EndHorizontal();
				GUI.SetNextControlName("preview build");
				data.Preview = EditorGUILayout.Toggle("Preview build", data.Preview);

				GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(position.width - 8));
				EditorGUI.indentLevel ++;

				int deletePlatform = -1;
				for(int i = 0; i < data.Platforms.Length; i ++){
					GUILayout.BeginHorizontal();
					GUI.SetNextControlName("platform " + i);
					BuildType platformType = (BuildType)System.Enum.Parse(typeof(BuildType), data.Platforms[i]);
					data.Platforms[i] = EditorGUILayout.EnumPopup(new GUIContent("Platform " + i, "Path relative to the Unity Project that " +
					                                                             "you want to place builds. Suggested to use '/build'"),
					                                              platformType, 
					                                              GUILayout.Width(position.width - 40))
						.ToString();
					if(GUILayout.Button("X", GUILayout.Width(20))){
						deletePlatform = i;
						break;
					}
					GUILayout.EndHorizontal();
				}
				if(deletePlatform > -1){
					data.Platforms = data.Platforms.RemoveAt(deletePlatform);
				}

				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Add a platform")){
					System.Array.Resize<string>(ref data.Platforms, data.Platforms.Length + 1);
					data.Platforms[data.Platforms.Length - 1] = BuildType.win_x86.ToString();
				}
				EditorGUI.indentLevel --;
				GUILayout.EndHorizontal();

				GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(position.width - 8));

				// SCENES
				GUILayout.Label("Scenes");
				EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
				EditorGUI.indentLevel ++;
				scenes = sceneList.Draw(DrawSceneListEntry, DrawSceneListEntryDrag, scenes);
				EditorGUI.indentLevel --;
				EditorBuildSettings.scenes = scenes;
				List<string> enabledScenes = new List<string>();
				for(int s = 0; s < scenes.Length; s ++){
					if(scenes[s].enabled){
						enabledScenes.Add(scenes[s].path);
					}
				}
				data.Scenes = enabledScenes.ToArray();

				GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(position.width - 8));

#region steam
				// steam settings
				GUILayout.BeginHorizontal();
				data.SteamData.BuildToSteam = EditorGUILayout.Toggle(new GUIContent("Push to Steam",
					"Check this button to enable packing a build to Steamworks."), data.SteamData.BuildToSteam, EditorStyles.miniButtonMid);
				GUILayout.EndHorizontal();
				if(data.SteamData.BuildToSteam){
					// we should do the cool expanding thing
					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(new GUIContent("Steam SDK Path", "The location of the ContentBuilder folder in" +
						" the Steam SDK"));
					buttonNameLength = 40;
					buttonName = data.SteamData.SteamCMDPath.Substring(Mathf.Max(0, data.SteamData.SDKPath.Length - 
						buttonNameLength), Mathf.Min(buttonNameLength, data.SteamData.SteamCMDPath.Length));
					if(buttonName.Length == buttonNameLength){
						buttonName = "..." + buttonName;
					}
					if(GUILayout.Button(buttonName)){
						string selectedPath = EditorUtility.OpenFolderPanel("Steam SDK Path", data.SteamData.SDKPath, data.SteamData.SDKPath);
						if(selectedPath != ""){
							data.SteamData.SDKPath = selectedPath;
						}
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(new GUIContent("Steam CMD Path", "The location of the command line interface" +
						" for steam. This is steamcmd.exe on windows and steamcmd.sh on mac."));
					buttonName = data.SteamData.SteamCMDPath.Substring(Mathf.Max(0, data.SteamData.SteamCMDPath.Length - 
					                                                                     buttonNameLength), 
					                                                   Mathf.Min(buttonNameLength, 
					                                                             data.SteamData.SteamCMDPath.Length));
					if(buttonName.Length == buttonNameLength){
						buttonName = "..." + buttonName;
					}
					if(GUILayout.Button(buttonName)){
						string selectedPath = "";
						selectedPath = EditorUtility.OpenFilePanel("steamcmd Path", data.SteamData.SteamCMDPath, "");
						if(selectedPath != ""){
							data.SteamData.SteamCMDPath = selectedPath;
						}
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(new GUIContent("Use Existing Builds", "Check this box to select a folder" +
						" that contains builds not made during this build process to push to Steam."));
					data.SteamData.UseExistingBuilds = GUILayout.Toggle(data.SteamData.UseExistingBuilds, "");
					GUILayout.EndHorizontal();

					if(data.SteamData.UseExistingBuilds){
						GUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel(new GUIContent("Existing Builds Path", "The location of your builds" +
							" folder that contains builds separated by platform."));
						buttonName = data.SteamData.ExistingBuildsPath.Substring(
							Mathf.Max(0, data.SteamData.ExistingBuildsPath.Length - buttonNameLength), 
							Mathf.Min(buttonNameLength, data.SteamData.ExistingBuildsPath.Length));
						if(buttonName.Length == buttonNameLength){
							buttonName = "..." + buttonName;
						}
						if(GUILayout.Button(buttonName)){
							string selectedPath = "";
							selectedPath = EditorUtility.OpenFolderPanel("Existing Builds Path", data.SteamData.ExistingBuildsPath,
								data.SteamData.ExistingBuildsPath);
							if(selectedPath != ""){
								data.SteamData.ExistingBuildsPath = selectedPath;
							}
						}
						GUILayout.EndHorizontal();
					}

					// app build info
					GUILayout.BeginVertical(skin.box);

					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(new GUIContent("App ID", "This game's steamworks App ID"));
					data.SteamData.AppBuild.AppID = GUILayout.TextField(data.SteamData.AppBuild.AppID);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(new GUIContent("Description", "Details to identify this build in steamworks."));
					data.SteamData.AppBuild.Description = GUILayout.TextField(data.SteamData.AppBuild.Description);
					GUILayout.EndHorizontal();

					data.SteamData.AppBuild.Preview = GUILayout.Toggle(data.SteamData.AppBuild.Preview, 
					                                                   new GUIContent("Preview", "Check this if you don't want" +
					                                                   " to actually send the builds up to steam and just want to test" +
					                                                   " if the correct files were packaged."));

					GUILayout.EndVertical();

					// depot list
					int deleteDepot = -1;
					for(int d = 0; d < data.SteamData.Depots.Length; d ++){
						GUILayout.BeginVertical(skin.box);
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if(GUILayout.Button(new GUIContent("Remove Depot", "Removes this depot from the list."), GUILayout.Width(100))){
							deleteDepot = d;
							break;
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel(new GUIContent("Depot ID", "This refers to the Steamworks depot that the chosen build's" +
							" files will be packaged into. Depots are normally used to create packages for players on a specifc platform."));
						data.SteamData.Depots[d].DepotID = GUILayout.TextField(data.SteamData.Depots[d].DepotID);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel(new GUIContent("Build Type", "The platform this depot is for."));
						BuildType depotBuildType = (BuildType)System.Enum.Parse(typeof(BuildType), data.SteamData.Depots[d].BuildType);
						data.SteamData.Depots[d].BuildType = EditorGUILayout.EnumPopup(depotBuildType).ToString();
						GUILayout.EndHorizontal();
						EditorGUILayout.PrefixLabel(new GUIContent("Ignore Files", "Any files created through the build process that you don.'t" +
							" want to upload to steam. You can use wildcards ('/*') to indicate all files in a directory."));
						int deleteIgnore = -1;
						for(int i = 0; i < data.SteamData.Depots[d].IgnoreFiles.Length; i ++){
							GUILayout.BeginHorizontal();
							data.SteamData.Depots[d].IgnoreFiles[i] = GUILayout.TextField(data.SteamData.Depots[d].IgnoreFiles[i]);
							if(GUILayout.Button(new GUIContent("X", "Removes this file from the ignore list."), GUILayout.Width(20))){
								deleteIgnore = i;
								break;
							}
							GUILayout.EndHorizontal();
						}
						if(deleteIgnore > -1){
							data.SteamData.Depots[d].IgnoreFiles = data.SteamData.Depots[d].IgnoreFiles.RemoveAt(deleteIgnore);
						}
						GUILayout.BeginHorizontal();
						if(GUILayout.Button("Add file to ignore")){
							System.Array.Resize<string>(ref data.SteamData.Depots[d].IgnoreFiles, 
							                            data.SteamData.Depots[d].IgnoreFiles.Length + 1);
							data.SteamData.Depots[d].IgnoreFiles[data.SteamData.Depots[d].IgnoreFiles.Length - 1] = "";
						}
						GUILayout.EndHorizontal();
						GUILayout.EndVertical();
					}
					if(deleteDepot > -1){
						data.SteamData.Depots = data.SteamData.Depots.RemoveAt(deleteDepot);
					}
					GUILayout.BeginHorizontal();
					if(GUILayout.Button("Add Depot", GUILayout.Height(30))){
						System.Array.Resize<DepotData>(ref data.SteamData.Depots, 
						                            data.SteamData.Depots.Length + 1);
						data.SteamData.Depots[data.SteamData.Depots.Length - 1] = new DepotData();
					}
					GUILayout.EndHorizontal();

					GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(Screen.width - 12));
				}
#endregion

				GUILayout.EndScrollView();

				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Save Changes", GUILayout.Height(60))){
					BuildScript.WriteData(data);
				}

				if(GUILayout.Button("BUILD!", GUILayout.Height(80))){
					// popup to ask for password
					if(data.SteamData.BuildToSteam){
						SteamLoginWindow.DoEnterLogin("Login to Steam", BuildWithLogin);
					} else {
						BuildScript.PerformBuild();
					}
				}

				// Help info section
				switch(GUI.GetNameOfFocusedControl()){
				case "help box":
					helpInfo = "This is the help info box! Mouse over other things to learn more about them :3";
					break;
				case "build path":
					helpInfo = "Path relative to the Unity Project that you want to place builds. Suggested to use '/build'";
					break;
				case "file name":
					helpInfo = "The name of the application.";
					break;
				case "version number":
					helpInfo = "Which version you are building. It is suggested to go with XX_YY_ZZ where XX is major version," +
						" YY is minor version, and ZZ is patch version. The current time is always appended to the end of the version";
					break;
				case "preview build":
					helpInfo = "Check this if you want to test out the build. It'll log out where builds are going but it won't actually build them.";
					break;
				default:
					helpInfo = "Please select an item below to discover more about it ^.^";
					break;
				} 
				if(GUI.GetNameOfFocusedControl().Contains("platform")){
					helpInfo = "A list of every platform you are building to. Acceptable platforms are:\n" +
						"win_x86, win_x64, mac, mac_x86, max_x64, linux, linux_x86, linux_x64, android, blackberry, ios," +
							"ps3, ps4, vita, 360, xbone, win_phone_8, win_store_app, web";
				}
				if(GUI.GetNameOfFocusedControl().Contains("scene")){
					helpInfo = "A list of unity scenes to build into the project. Order is important here! This will override your build settings.";
				}
			}
		}

		static void BuildWithLogin(string accountName, string password){
			BuildScript.PerformBuild(accountName, password);
		}
	}
}

public delegate void LoginHandler (string accountName, string password);

public class SteamLoginWindow : EditorWindow {

	LoginHandler callback;

	string accountName = "";
	string password = "";

	void OnGUI(){
		GUILayout.BeginHorizontal();
		GUILayout.Label("Steam account name");
		accountName = GUILayout.TextField(accountName);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("password");
		password = GUILayout.PasswordField(password, '*');
		GUILayout.EndHorizontal();
		if(GUILayout.Button("Confirm")){
			this.Close();
		}
	}

	void OnDisable(){
		CloseWindow();
	}

	void CloseWindow(){
		callback(accountName, password);
	}

	public static void DoEnterLogin(string title, LoginHandler callback){
		SteamLoginWindow popup = EditorWindow.GetWindow(typeof(SteamLoginWindow), true, title, true) as SteamLoginWindow;
		popup.callback = callback;
	}
}