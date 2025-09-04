#region Info

// Created by Horlov Andrii (andreygorlovv@gmail.com)
// https://andriihorlov.github.io/

#endregion

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace com.horaxr.assetsvalidator.Editor
{
    public static class ScenesGetter
    {
        public static IEnumerable<Scene> OpenSceneOneByOne()
        {
            IEnumerable<string> scenesPath = AssetDatabase
                .FindAssets("t:Scene", new[] {"Assets"})
                .Select(AssetDatabase.GUIDToAssetPath);

            foreach (string scenePath in scenesPath)
            {
                Scene scene = SceneManager.GetSceneByPath(scenePath);
                if (!scene.isLoaded)
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }

                yield return scene;
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}