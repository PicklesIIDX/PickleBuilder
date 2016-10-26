#PickleBuilder

A Unity tool for building to multiple platforms and uploading to steam.

#Builder Window

![Unity Build Config Tool Wind](https://github.com/PicklesIIDX/PickleBuilder/raw/master/build_config_tool.png "Unity Build Config Tool Window")


This is the build window which you can open by selecting Build > Build Config Window in the Unity Menu Bar. It allows you to define how you want to build your game for different platforms. 

**Build Path:** This is the folder all of your builds will be organized into. When performing a build a new folder will be created inside this folder that is named with the version and date of the build. For example, a folder named 0_1_20__10_26_2016_06_44_27 means that the folder contains version 0.1.20 and was built at 6:44 and 27 seconds GMT on October 26th, 2016.

**File Name:** This is the name of the application file. The .app on OSX and the .exe on Windows.

**Version Number Major:** The first number used to describe the version.

**Version Number Minor:** The second number used to describe the version.

**Version Number Patch:** The third number used to describe the version. This is incremented automatically every time you build.

**New Major Version:** Click this button to increase the major version by 1.

**New Minor Version:** Click this button to increase the minor version by 1.

**Preview build:** If this is checked the game will not actually be built. But the process will run and the build folders will be created. The Patch version will not be incremented.

**Platform:** This is a list of all platforms to build for. Use the dropdown list to select your platform. Some platforms, although in the list, will only be able to be built if you have the right licenses (consoles for example). You can remove a platform to build by pressing the "x" button on the right side. When built, the files for this platform will be placed in the versioned build folder under a sub folder that has the name of the build target.

**Add a platform:** Press this button to add a new platform to build to.

**Scenes:** This is a list of all scenes in your Build Settings. If a scene is checked, it will be included in the build. You can reorder scenes by clicking and dragging the name of the scene.

**Push to Steam:** If this toggle is selected, after the builds are made they will be pushed to your Steam app repository. To use the upload to steam function, you must be a steam developer, have an app ID, have set up your app depots, and have the steam sdk. Look below to see how to get your computer ready to upload to steam.

**Steam SDK Content Builder Path:** Click this button to select the path to the ContentBuilder folder, which is in the steam sdk: /sdk/tools/ContentBuilder/. Follow the instructions below to download the steam SDK

**Steam CMD Path:** Click this button to select the path to the steamCMD application. Follow the instructions below to download the steamCMD.

**Use Existing Builds:** Check this box to use an existing build folder instead of the build folder created for this build.

**Existing Builds Path:** This is only visible if Use Existing Builds is checked. This is the root build folder which contains your individual platform builds.

**App Block:** This is a group which describes the settings for uploading to steam.

**App ID:** The app ID of your game. This is given to you by Valve and can be seen on your steamworks app page.

**Description:** This is a message for this particular upload that you will see in the builds list in your app admin on steamworks.

**Preview:** Check this box to veryify that the steam process works, but prevent actually uploading to your steam repository.

**Depot Block:** This is a group which describes the settings for each of your depots.

**Depot ID:** The ID of the depot which has been setup on your steamworks account.

**Build Type:** The files to include in this depot. It will grab all the files in the specified build folder.

**Ignore Files:** Any files listed in here will not be included from the build folder. Accepts wildcards. You can remove the entry with the "x" button.

**Add file to ignore:** Adds another entry in the Ignore Files list.

**Add Depot:** Select this button to add a new depot.

**Save Changes:** This will save your build settings to a file called build_config.json, which will be located in this project at Assets/PickleTools/PickleBuilder/Editor

**BUILD!:** This will perform the build as you have specified in this window.




#Steam SDK Setup

##OSX

Download the steamCMD

1. open terminal

2. create a go to the directory you want the steam sdk to be located

2. run curl -sqL 'https://steamcdn-a.akamaihd.net/client/installer/steamcmd_osx.tar.gz' | tar zxvf -

Update the steam mac client

3. open steamCMD by running ./osx32/steamcmd in terminal

4. it will open and update steam

5. close the steam client

Download the SDK

6. You will find the latest SDK to download on the right side of this page: https://partner.steamgames.com/home

7. Unip the steam sdk


##Windows

Download the steamCMD

1. download the steamCMD from here: https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip

2. unzip the folder

Update the steam windows client

3. run steamCMD.exe to update steam

4. close the steam client

Download the SDK

5. You will find the latest SDK to download on the right side of this page: https://partner.steamgames.com/home

6. Unzip the steam sdk


#Steamworks setup

Next you will need to prepare you steam repository. This page (https://partner.steamgames.com/documentation/steampipe) contains a full tutorial on how to setup your steam repository. You only really need to follow the first 4 minutes of this tutorial: https://www.youtube.com/watch?v=SoNH-v6aU9Q. Particularly, you need to set up demos for your game's platforms.

Launch Options: Create a launch option for each platform. The executable will be the name set in the File Name property in the Build Config Window. Make sure to use .exe for Windows and .app for OSX.

Depots: You will need to setup an individual depot for each platform. This Build Config Tool uploads a complete version of the game to each depot.

Make sure to publish your changes!

#Build from the Command Line:
The BuildScript is accessible from the command line. This is useful if you want to build from Unity without the hassle of opening Unity up and using a GUI interface. Use this command to build using your current config from the command line:

Windows:

	C:\program files\Unity\Editor\Unity.exe -quit -batchmode -executeMethod BuildScript.PerformBuild steam_login_name steam_login_password

OSX:

	/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -executeMethod BuildScript.PerformBuild steam_login_name steam_login_password

Note that the path to the Unity application is where you have installed Unity. Listed above is the default.
Also note that steam_login_name and steam_login_password are optional parameters that should be replaced with your steam username and password if you want to upload the build to steam. If not, just leave those arguments out.

#Common Issues

Unity locks up when I hit BUILD!:

When uploading to Steam we start an external process and wait for it to complete. Just wait a bit. The process is running. The two things that take the most time are checking for steam updates and actually uploading the game content. After the process is complete you will see a log in your console of how the steam upload went.

I get a depot build error:

you may have added a new depot in the PickleBuilder, but not in steamworks. You need to add the new depot and publish the changes.

I only see Reload Config and Open Config File in the Build Config Window:

The build config window is not serialized so when you recompile your code it will unload the build config. Select the Reload Config button to load the build_config.json that is in your project. Select Open Config File to open that json file with a text editor.
	
