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
    public static class SceneUtilityValidator
    {
        private static List<Scene> OriginalScenes;

        public static void TraverseAllScenes(Action<GameObject, Scene> onGameObject)
        {
            SaveCurrentScenes();
            try
            {
                string[] scenePaths = new List<string>(GetAllScenePaths()).ToArray();
                int totalScenes = scenePaths.Length;
                int sceneIndex = 0;

                foreach (string scenePath in scenePaths)
                {
                    sceneIndex++;
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
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
                                processed / (float)Math.Max(1, totalApprox));
                        }

                        onGameObject?.Invoke(go, scene);

                        foreach (Transform child in go.transform)
                        {
                            queue.Enqueue(child.gameObject);
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                RestoreOriginalScenes();
            }
        }

        private static void SaveCurrentScenes()
        {
            OriginalScenes = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                OriginalScenes.Add(SceneManager.GetSceneAt(i));
            }
        }

        private static IEnumerable<string> GetAllScenePaths()
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (string guid in guids)
            {
                yield return AssetDatabase.GUIDToAssetPath(guid);
            }
        }

        private static void RestoreOriginalScenes()
        {
            if (OriginalScenes == null) return;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!OriginalScenes.Contains(scene))
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            OriginalScenes.Clear();
        }
    }
}