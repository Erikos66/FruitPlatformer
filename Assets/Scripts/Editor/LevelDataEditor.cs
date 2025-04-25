using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(LevelDataSO))]
public class LevelDataEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        LevelDataSO levelData = (LevelDataSO)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Update Fruit Counts From Scenes")) {
            UpdateFruitCounts(levelData);
        }
    }

    private void UpdateFruitCounts(LevelDataSO levelData) {
        // Save the current scene so we can return to it later
        Scene currentScene = EditorSceneManager.GetActiveScene();
        bool sceneIsDirty = currentScene.isDirty;

        try {
            // Get all scenes in the build settings
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Store our level data here temporarily
            List<LevelDataSO.LevelInfo> levelInfoList = new List<LevelDataSO.LevelInfo>();

            EditorUtility.DisplayProgressBar("Updating fruit counts", "Scanning scenes...", 0);

            for (int i = 0; i < scenes.Count; i++) {
                EditorBuildSettingsScene scene = scenes[i];

                // Skip disabled scenes
                if (!scene.enabled) continue;

                string sceneName = Path.GetFileNameWithoutExtension(scene.path);

                // Only process level scenes
                if (!sceneName.StartsWith("Level_")) continue;

                EditorUtility.DisplayProgressBar("Updating fruit counts",
                    $"Scanning scene {sceneName}", (float)i / scenes.Count);

                // Load the scene additively to not lose the current scene
                Scene loadedScene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);

                // Find all fruits in the scene
                Fruit[] fruits = Object.FindObjectsByType<Fruit>(FindObjectsSortMode.None);

                // Create or update the level info
                LevelDataSO.LevelInfo levelInfo = new LevelDataSO.LevelInfo {
                    levelName = sceneName,
                    totalFruits = fruits.Length
                };

                levelInfoList.Add(levelInfo);

                // Close the scene we just opened
                EditorSceneManager.CloseScene(loadedScene, true);
            }

            // Use serialized properties to properly update the ScriptableObject
            SerializedProperty levelInfoListProperty = serializedObject.FindProperty("levelInfoList");
            levelInfoListProperty.ClearArray();

            for (int i = 0; i < levelInfoList.Count; i++) {
                levelInfoListProperty.arraySize++;
                SerializedProperty element = levelInfoListProperty.GetArrayElementAtIndex(i);

                SerializedProperty levelNameProp = element.FindPropertyRelative("levelName");
                SerializedProperty totalFruitsProp = element.FindPropertyRelative("totalFruits");

                levelNameProp.stringValue = levelInfoList[i].levelName;
                totalFruitsProp.intValue = levelInfoList[i].totalFruits;
            }

            serializedObject.ApplyModifiedProperties();

            // Save the ScriptableObject
            EditorUtility.SetDirty(levelData);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Update Complete",
                $"Updated fruit counts for {levelInfoList.Count} levels.", "OK");
        }
        finally {
            // Clean up and restore original scene
            EditorUtility.ClearProgressBar();

            // If we were already in a scene and made changes, ensure we don't lose them
            if (currentScene.path != null && !currentScene.path.Equals("")) {
                EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
                if (!sceneIsDirty) {
                    // If the scene wasn't dirty before, we need to reload it to remove the dirty flag
                    EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
                }
            }
        }
    }
}