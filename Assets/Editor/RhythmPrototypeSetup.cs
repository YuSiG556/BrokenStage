using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class RhythmPrototypeSetup
{
    private const string ScenePath = "Assets/Main.unity";

    static RhythmPrototypeSetup()
    {
        EditorApplication.delayCall += CreateSceneOnFirstOpen;
    }

    private static void CreateSceneOnFirstOpen()
    {
        if (!File.Exists(ScenePath))
        {
            CreatePrototypeScene();
        }
    }

    [MenuItem("Tools/Rhythm Prototype/Create or Reset Main Scene")]
    public static void CreatePrototypeScene()
    {
        const string inputAssetPath = "Assets/Input/RhythmControls.inputactions";
        AssetDatabase.ImportAsset(inputAssetPath, ImportAssetOptions.ForceUpdate);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("Rhythm Game Prototype");
        RhythmGamePrototype prototype = root.AddComponent<RhythmGamePrototype>();
        prototype.inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
            inputAssetPath);
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        Selection.activeGameObject = root;
        Debug.Log("Rhythm prototype scene created. Press the Play button.");
    }
}
