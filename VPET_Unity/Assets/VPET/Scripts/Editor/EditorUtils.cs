/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
using UnityEngine;
using System.Collections;
using UnityEditor;


[InitializeOnLoad]
public class SwitchMode
{
    static SwitchMode()
    {
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneLoaded;
    }

    static void SceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
    {
        BuildTargetGroup[] targetGroups = { BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone };

        foreach (BuildTargetGroup grp in targetGroups)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
            if (scene.name.Contains("Server"))
            {
                if (defines != "")
                    defines += ";";
                defines += "SCENE_HOST";
            }
            else
            {
                if (defines.Contains("SCENE_HOST"))
                {
                    defines = defines.Replace(";SCENE_HOST", "");
                    defines = defines.Replace("SCENE_HOST", "");
                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
        }
    }
}

namespace vpet
{
	public static class ExportVpetPackage
	{
	
	    [MenuItem("VPET/Export VPET package")]
	    public static void VpetExport()
	    {
	        string[] content = new string[] {
	                                        "Assets/Plugins/VPET",
	                                        "Assets/Resources/VPET",
                                            "Assets/Resources/VPET/SceneDumps",
	                                        "Assets/VPET",
	                                        "ProjectSettings/AudioManager.asset",
	                                        "ProjectSettings/ClusterInputManager.asset",
	                                        "ProjectSettings/DynamicsManager.asset",
	                                        "ProjectSettings/EditorBuildSettings.asset",
	                                        "ProjectSettings/EditorSettings.asset",
	                                        "ProjectSettings/GraphicsSettings.asset",
	                                        "ProjectSettings/InputManager.asset",
	                                        "ProjectSettings/NavMeshAreas.asset",
	                                        "ProjectSettings/NetworkManager.asset",
	                                        "ProjectSettings/Physics2DSettings.asset",
	                                        "ProjectSettings/ProjectSettings.asset",
	                                        "ProjectSettings/QualitySettings.asset",
	                                        "ProjectSettings/TagManager.asset",
	                                        "ProjectSettings/UnityAdsSettings.asset",
	                                        "ProjectSettings/UnityConnectSettings.asset"
	        };
            AssetDatabase.ExportPackage(content, "VPET.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse); // | ExportPackageOptions.IncludeDependencies);
	        Debug.Log("Done export VPET.unitypackage.");
	    }
	}
}