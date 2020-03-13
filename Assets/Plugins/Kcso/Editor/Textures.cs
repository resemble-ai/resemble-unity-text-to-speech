using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/// <summary> Keep Compressed Stuff Only </summary>
namespace Kcso
{
    public static class Textures
    {

        public struct CompressionData
        {
            public Vector2Int resolution;
            public bool highQuality;
            public bool mipmaps;
            public bool materialReplace;
            public bool deleteOriginal;
            public KeyValuePair<Material, string>[] mats;

            public CompressionData(Vector2Int resolution, KeyValuePair<Material, string>[] mats)
            {
                this.resolution = resolution;
                this.mats = mats;
                highQuality = true;
                mipmaps = true;
                materialReplace = true;
                deleteOriginal = false;
            }

            public CompressionData(Vector2Int resolution, bool delete, KeyValuePair<Material, string>[] mats)
            {
                this.resolution = resolution;
                this.mats = mats;
                deleteOriginal = delete;
                highQuality = true;
                mipmaps = true;
                materialReplace = true;
            }

        }

        [MenuItem("Assets/Make Compressed Copy")]
        static void MakeCompressedCopy()
        {
            //Get Data
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);


            //Show modal window
            bool deleteOriginal = EditorPrefs.GetBool("Kcso_DeleteOriginal", false);
            CompressionData data = new CompressionData(new Vector2Int(tex.width, tex.height), deleteOriginal, GetReferenceMats(tex));

            TextureCompressionWindow.Show(data, (CompressionData d) =>
            {
                EditorPrefs.SetBool("Kcso_DeleteOriginal", d.deleteOriginal);
                CreateCompressedTex(path, tex, d);
            });
        }

        private static void CreateCompressedTex(string path, Texture2D tex, CompressionData data)
        {
            //Create and blit new texture
            Texture2D newTex = new Texture2D(tex.width, tex.height);
            RenderTexture rendTex = RenderTexture.GetTemporary(tex.width, tex.height);
            Graphics.Blit(tex, rendTex);
            RenderTexture.active = rendTex;
            newTex.ReadPixels(new Rect(0, 0, rendTex.width, rendTex.height), 0, 0);
            newTex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rendTex);


            //Set asset importsettings
            newTex.Compress(true);


            //Write new asset
            path = path.Remove(path.Length - 3) + "asset";
            AssetDatabase.CreateAsset(newTex, path);
            AssetDatabase.ImportAsset(path);


            //Remplace in material
            if (data.materialReplace)
            {
                for (int i = 0; i < data.mats.Length; i++)
                    data.mats[i].Key.SetTexture(data.mats[i].Value, newTex);
            }

            //Delete original
            if (data.deleteOriginal)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tex));
            }
        }


        [MenuItem("Assets/Make Compressed Copy", true)]
        static bool MakeCompressedCopyValidate()
        {
            return Selection.activeObject is Texture2D;
        }

        public static KeyValuePair<Material, string>[] GetReferenceMats(Texture tex)
        {
            Material[] mats = Resources.FindObjectsOfTypeAll<Material>();
            List<KeyValuePair<Material, string>> result = new List<KeyValuePair<Material, string>>();

            for (int i = 0; i < mats.Length; i++)
            {
                string name = mats[i].name;
                if (name.StartsWith("Hidden/") || name.StartsWith("PostProcess"))
                    continue;

                if (mats[i].shader == null)
                    continue;

                Shader shader = mats[i].shader;
                int count = ShaderUtil.GetPropertyCount(shader);
                for (int j = 0; j < count; j++)
                {
                    if (ShaderUtil.GetPropertyType(shader, j) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string texName = ShaderUtil.GetPropertyName(shader, j);
                        Texture propertyTex = mats[i].GetTexture(texName);

                        if (propertyTex == tex)
                            result.Add(new KeyValuePair<Material, string>(mats[i], texName));
                    }
                }
            }
            return result.ToArray();
        }
    }

    class TextureCompressionWindow : EditorWindow
    {
        private static TextureCompressionWindow window;
        private static Textures.CompressionData data;
        private static Callback callback;
        public delegate void Callback(Textures.CompressionData data);
        private static Vector2 scroll;

        public static void Show(Textures.CompressionData data, Callback callback)
        {
            TextureCompressionWindow.data = data;
            TextureCompressionWindow.callback = callback;
            window = CreateInstance<TextureCompressionWindow>();
            window.titleContent = new GUIContent("Texture compression settings");
            window.ShowModalUtility();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 200, 500);
        }

        public void OnGUI()
        {
            Event e = Event.current;
            if (e.shift)
            {
                callback.Invoke(data);
                Close();
            }


            data.resolution = EditorGUILayout.Vector2IntField("Resolution", data.resolution);
            data.highQuality = EditorGUILayout.Toggle("High quality", data.highQuality);
            data.mipmaps = EditorGUILayout.Toggle("Generate Mip Maps", data.mipmaps);

            GUILayout.Space(5);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(Screen.width, 1), Color.grey);
            GUILayout.Space(5);

            GUILayout.Label("These materials contain references to the texture :");
            scroll = GUILayout.BeginScrollView(scroll, GUI.skin.box);
            for (int i = 0; i < data.mats.Length; i++)
                GUILayout.Label(data.mats[i].Key.name);
            GUILayout.EndScrollView();

            data.materialReplace = EditorGUILayout.Toggle("Replace", data.materialReplace);
            data.deleteOriginal = EditorGUILayout.Toggle("Delete original", data.deleteOriginal);

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close"))
            {
                Close();
            }

            if (GUILayout.Button("Apply"))
            {
                callback.Invoke(data);
                Close();
            }
            GUILayout.EndHorizontal();
        }

    }

}
