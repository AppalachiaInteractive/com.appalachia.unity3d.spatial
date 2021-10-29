#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Appalachia.Spatial.SDF
{
    public class GeneratorWindow : EditorWindow
    {
        private static readonly Generator Generator;

        static GeneratorWindow()
        {
            Generator = new Generator();
        }

        private void CreateSDF()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Save As",
                Generator.Mesh.name + "_SDF",
                "asset",
                ""
            );

            if ((path == null) || path.Equals(""))
            {
                return;
            }

            var voxels = Generator.Generate();

            AssetDatabase.CreateAsset(voxels, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Close();

            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }

        private void OnGUI()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                EditorGUILayout.HelpBox(
                    "This tool requires a GPU that supports compute shaders.",
                    MessageType.Error
                );

                if (GUILayout.Button("Close"))
                {
                    Close();
                }

                return;
            }

            Generator.Mesh = EditorGUILayout.ObjectField("Mesh", Generator.Mesh, typeof(Mesh), false) as Mesh;

            if (Generator.Mesh == null)
            {
                if (GUILayout.Button("Close"))
                {
                    Close();
                }

                return;
            }

            if (Generator.Mesh.subMeshCount > 1)
            {
                Generator.SubMeshIndex = (int) Mathf.Max(
                    EditorGUILayout.IntField("Submesh Index", Generator.SubMeshIndex),
                    0f
                );
            }

            Generator.Padding = EditorGUILayout.Slider("Padding", Generator.Padding, 0f, 1f);

            Generator.Resolution = (int) Mathf.Max(
                EditorGUILayout.IntField("Resolution", Generator.Resolution),
                1f
            );

            if (GUILayout.Button("Create"))
            {
                CreateSDF();
            }

            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        [MenuItem(PKG.Menu.Appalachia.Windows.Base + "Generator")]
        private static void Window()
        {
            var window = (GeneratorWindow) GetWindow(typeof(GeneratorWindow), true, "Generate SDF");
            window.ShowUtility();
        }
    }
}

#endif
