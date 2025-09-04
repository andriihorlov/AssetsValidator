#region Info

// Created by Horlov Andrii (andreygorlovv@gmail.com)
// https://andriihorlov.github.io/

#endregion

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.horaxr.assetsvalidator.Editor
{
#region Info

// About window for Assets Validator
// Created by Horlov Andrii
// Place this file in Editor/ (or inside your package Editor/ folder)

#endregion

    public static class AssetsValidator
    {
        private enum MissingType
        {
            Components,
            Prefabs,
            Materials
        }

        private const string MenuBrokenComponents = "Tools/Asset Validator/Find Broken Components";
        private const string MenuBrokenPrefabs = "Tools/Asset Validator/Find Broken Prefabs";
        private const string MenuMissingMaterials = "Tools/Asset Validator/Find Missing Materials";
        private const string MenuAbout = "Tools/Asset Validator/About";

        private const string MsgMissingScript = "Missing scripts on GameObject <color=yellow>'{0}'</color> in scene <b>'{1}'</b> → path: <color=white>{2}</color>";
        private const string MsgMissingPrefab = "Missing prefab source for <color=yellow>'{0}'</color> in scene <b>'{1}'</b> → path: <color=white>{2}</color>";
        private const string MsgMissingMaterial = "Null / missing material on <color=yellow>'{0}'</color> in scene <b>'{1}'</b> → path: <color=white>{2}</color>";
        private const string MsgSearchingMaterials = "Searching for missing materials...";
        private const string MsgSearchingPrefabs = "Searching for broken prefabs...";
        private const string MsgSearchingComponents = "Searching for broken components...";
        private const string MsgSearchDone = "Search complete.";

        private const float WindowSizeX = 420f;
        private const float WindowSizeY = 260f;

        private static ILogger Logger => Debug.unityLogger;

        [MenuItem(MenuBrokenComponents, false, 1)]
        public static void FindBrokenComponentsMenu()
        {
            Logger.Log(LogType.Log, MsgSearchingComponents);
            SearchMissingObjects(MissingType.Components);
        }

        [MenuItem(MenuBrokenPrefabs, false, 2)]
        public static void FindBrokenPrefabsMenu()
        {
            Logger.Log(LogType.Log, MsgSearchingPrefabs);
            SearchMissingObjects(MissingType.Prefabs);
        }

        [MenuItem(MenuMissingMaterials, false, 3)]
        public static void FindMissingMaterialsMenu()
        {
            Logger.Log(LogType.Log, MsgSearchingMaterials);
            SearchMissingObjects(MissingType.Materials);
        }

        [MenuItem(MenuAbout, false, 100)]
        public static void AboutMenu()
        {
            AboutWindow wnd = EditorWindow.GetWindow<AboutWindow>(true, AboutWindow.DisplayName);
            wnd.minSize = new Vector2(WindowSizeX, WindowSizeY);
            wnd.maxSize = new Vector2(WindowSizeX, WindowSizeY);
            wnd.Show();
            wnd.Focus();
        }

        private static void SearchMissingObjects(MissingType missingType)
        {
            try
            {
                string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
                int totalScenes = sceneGuids.Length;
                int sceneIndex = 0;

                foreach (string guid in sceneGuids)
                {
                    sceneIndex++;
                    string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    var allRoots = scene.GetRootGameObjects();
                    int processed = 0;
                    int totalApprox = allRoots.Length;

                    Queue<GameObject> queue = new Queue<GameObject>(allRoots);
                    while (queue.Count > 0)
                    {
                        GameObject go = queue.Dequeue();
                        processed++;

                        if (processed % 100 == 0)
                        {
                            EditorUtility.DisplayProgressBar(
                                "Assets Validator",
                                $"Scanning scene {scene.name} ({sceneIndex}/{totalScenes})",
                                processed / (float) Mathf.Max(1, totalApprox));
                        }

                        switch (missingType)
                        {
                            case MissingType.Components:
                                FindBrokenComponents(go, scene);
                                break;
                            case MissingType.Prefabs:
                                FindBrokenPrefabs(go, scene);
                                break;
                            case MissingType.Materials:
                                FindMissingMaterials(go, scene);
                                break;
                        }

                        foreach (Transform child in go.transform)
                        {
                            queue.Enqueue(child.gameObject);
                        }
                    }

                    EditorUtility.ClearProgressBar();
                }

                Logger.Log(LogType.Log, MsgSearchDone);
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Logger.Log(LogType.Error, $"AssetsValidator encountered an error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void FindBrokenComponents(GameObject gameObject, Scene scene)
        {
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (missingCount > 0)
            {
                string path = GetGameObjectPath(gameObject);
                Logger.Log(LogType.Error, string.Format(MsgMissingScript, gameObject.name, scene.name, path));
                return;
            }
        }

        private static void FindBrokenPrefabs(GameObject gameObject, Scene scene)
        {
            // Check prefab instance whose source asset is missing
            bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(gameObject);
            if (isPrefabInstance)
            {
                UnityEngine.Object source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                if (source == null)
                {
                    string path = GetGameObjectPath(gameObject);
                    Logger.Log(LogType.Error, string.Format(MsgMissingPrefab, gameObject.name, scene.name, path));
                    return;
                }
            }

            // Additionally, check if the GameObject is a prefab asset reference that's missing (rare for scene GOs)
            try
            {
                if (PrefabUtility.IsPrefabAssetMissing(gameObject))
                {
                    string path = GetGameObjectPath(gameObject);
                    Logger.Log(LogType.Error, string.Format(MsgMissingPrefab, gameObject.name, scene.name, path));
                    return;
                }
            }
            catch
            {
                // IsPrefabAssetMissing may throw in some Unity versions for scene objects; ignore safely
            }
        }

        private static void FindMissingMaterials(GameObject gameObject, Scene scene)
        {
            // Use Renderer (covers MeshRenderer + SkinnedMeshRenderer and others)
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var sharedMats = renderer.sharedMaterials;
            if (sharedMats == null)
            {
                // unexpected, but report as missing
                string pathNull = GetGameObjectPath(gameObject);
                Logger.Log(LogType.Error, string.Format(MsgMissingMaterial, gameObject.name, scene.name, pathNull));
                return;
            }

            // If any material slot is null -> missing material
            for (int i = 0; i < sharedMats.Length; i++)
            {
                if (sharedMats[i] == null)
                {
                    string path = GetGameObjectPath(gameObject);
                    Logger.Log(LogType.Error, string.Format(MsgMissingMaterial, gameObject.name, scene.name, path));
                    return;
                }
            }
        }

        private static string GetGameObjectPath(GameObject go)
        {
            if (go == null) return "<null>";
            string path = go.name;
            Transform t = go.transform.parent;
            while (t != null)
            {
                path = t.name + "/" + path;
                t = t.parent;
            }

            return path;
        }
    }
}