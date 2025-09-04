using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.horaxr.assetsvalidator.Editor
{
    public class AboutWindow : EditorWindow
    {
        public const string DisplayName = "Assets Validator";
        private const string PackageId = "com.horaxr.assetsvalidator";
        private const string Version = "1.0.0";
        private const string ShortDescription = "Quickly find and fix broken scripts, missing prefabs and misplaced materials.";

        private const string HoraXRName = "Hora XR";
        private const string HoraXRWebsite = "https://horaxr.com/";
        private const string GitHubUrl = "https://github.com/andriihorlov/AssetsValidator";
        private const string AssetStoreUrl = "https://assetstore.unity.com/publishers/94841";
        private const string SupportEmail = "hello@horaxr.com";

        private const string LogoPath = "Assets/com.horaxr.assetsvalidator/Editor/Content/logo.png";

        private Texture2D _logoTex;

        private void OnEnable()
        {
            LoadLogo();
        }

        private void LoadLogo()
        {
            if (File.Exists(Path.Combine(Application.dataPath, "../", LogoPath)))
            {
                _logoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoPath);
            }
            else
            {
                _logoTex = null;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (_logoTex != null)
            {
                GUILayout.Label(_logoTex, GUILayout.Width(92), GUILayout.Height(92));
            }
            else
            {
                GUILayout.Box("Logo", GUILayout.Width(92), GUILayout.Height(92));
            }

            EditorGUILayout.BeginVertical();
            GUILayout.Space(6);
            EditorGUILayout.LabelField(DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Version: {Version}");
            DrawClickablePackageId();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(ShortDescription, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Asset Store")) OpenUrl(AssetStoreUrl);
            if (GUILayout.Button("Website")) OpenUrl(HoraXRWebsite);
            if (GUILayout.Button("GitHub")) OpenUrl(GitHubUrl);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // About text
            EditorGUILayout.LabelField($"About {HoraXRName}", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"{HoraXRName} — XR studio focused on VR/AR/MR solutions.\nAuthor: Andrii Horlov\nWebsite: {HoraXRWebsite}", MessageType.Info);

            EditorGUILayout.Space();

            // Footer
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Send Email")) OpenUrl($"mailto:{SupportEmail}?subject={DisplayName} support");
            EditorGUILayout.LabelField("© " + System.DateTime.Now.Year + " Hora XR", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawClickablePackageId()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Package ID:", GUILayout.Width(80));

            GUIStyle linkStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                normal = {textColor = EditorGUIUtility.isProSkin ? new Color(0.4f, 0.7f, 1f) : new Color(0f, 0.3f, 0.8f)},
                stretchWidth = true
            };

            if (GUILayout.Button(PackageId, linkStyle))
            {
                EditorGUIUtility.systemCopyBuffer = PackageId;
                ShowNotification(new GUIContent("Package ID copied to clipboard"), 1f);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            Application.OpenURL(url);
        }
    }
}