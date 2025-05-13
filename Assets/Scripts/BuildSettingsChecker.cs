using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
public class BuildSettingsChecker : MonoBehaviour
{
    [MenuItem("Tools/Check Build Settings")]
    public static void CheckBuildSettings()
    {
        List<string> requiredScenes = new List<string>
        {
            "MainMenu",
            "FarmScene",
            "HouseScene"
        };
        
        List<string> missingScenes = new List<string>();
        List<string> scenesInBuildSettings = new List<string>();
        
        // Get all scenes in the build settings
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            string scenePath = EditorBuildSettings.scenes[i].path;
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            scenesInBuildSettings.Add(sceneName);
            Debug.Log($"Scene {i} in build settings: {sceneName}");
        }
        
        // Check if required scenes are in build the settings
        foreach (string requiredScene in requiredScenes)
        {
            if (!scenesInBuildSettings.Contains(requiredScene))
            {
                missingScenes.Add(requiredScene);
            }
        }
        
        // Displaying the results
        if (missingScenes.Count > 0)
        {
            string message = "The following scenes are missing from the build settings:\n";
            foreach (string missingScene in missingScenes)
            {
                message += $"- {missingScene}\n";
            }
            message += "\nWould you like to open the Build Settings window?";
            
            if (EditorUtility.DisplayDialog("Missing Scenes", message, "Open Build Settings", "Cancel"))
            {
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Build Settings Check", "All required scenes are in the build settings!", "OK");
        }
    }
    
    [MenuItem("Tools/Fix MainMenu Buttons")]
    public static void FixMainMenuButtons()
    {
        // Find the MainMenu script in the active scene
        MainMenu mainMenu = GameObject.FindObjectOfType<MainMenu>();
        if (mainMenu == null)
        {
            EditorUtility.DisplayDialog("Fix MainMenu Buttons", "MainMenu script not found in current scene. Please open the MainMenu scene first.", "OK");
            return;
        }
        
        // Find buttons in the scene
        Button[] allButtons = GameObject.FindObjectsOfType<Button>();
        
        // Try to match buttons by name
        Button startButton = null;
        Button optionsButton = null;
        Button quitButton = null;
        
        foreach (Button button in allButtons)
        {
            string buttonName = button.gameObject.name.ToLower();
            
            if (buttonName.Contains("start") || buttonName.Contains("play"))
            {
                startButton = button;
            }
            else if (buttonName.Contains("option") || buttonName.Contains("setting"))
            {
                optionsButton = button;
            }
            else if (buttonName.Contains("quit") || buttonName.Contains("exit"))
            {
                quitButton = button;
            }
        }
        
        // Assign found buttons
        mainMenu.startGameButton = startButton;
        mainMenu.optionsButton = optionsButton;
        mainMenu.quitButton = quitButton;
        
        // Display results
        string message = "Button assignments:\n";
        message += $"Start Game: {(startButton != null ? startButton.gameObject.name : "Not Found")}\n";
        message += $"Options: {(optionsButton != null ? optionsButton.gameObject.name : "Not Found")}\n";
        message += $"Quit: {(quitButton != null ? quitButton.gameObject.name : "Not Found")}\n";
        
        EditorUtility.DisplayDialog("Fix MainMenu Buttons", message, "OK");
    }
}
#endif 