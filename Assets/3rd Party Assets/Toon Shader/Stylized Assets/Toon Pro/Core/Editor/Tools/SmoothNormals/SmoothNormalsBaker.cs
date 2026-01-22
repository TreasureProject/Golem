using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace INab.Common
{
    public class SmoothNormalsBaker : EditorWindow
    {

        public enum UVChannel
        {
            UV1 = 0, UV2 = 1, UV3 = 2
        }

        [SerializeField] public UVChannel uvChannel = UVChannel.UV3;
        [SerializeField] public float cospatialVertexDistance = 0.01f;

        public List<Mesh> Meshes = new List<Mesh>();

        private string SavePath;
        private string NameSuffix = "_NormalsBaked";
        private string NamePrefix = "";

        private bool UserChoosenPath = false;

        private SerializedProperty arrayProperty;
        private SerializedObject serializedObject;

        private class CospatialVertex
        {
            public Vector3 position;
            public Vector3 accumulatedNormal;
        }

        [MenuItem("Tools/INab Studio/Smooth Normals Baker")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SmoothNormalsBaker));
        }


        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);

            arrayProperty = serializedObject.FindProperty("Meshes");
        }


        private void OnGUI()
        {
            uvChannel = (UVChannel)EditorGUILayout.EnumPopup("Select UV Channel: ", uvChannel);

            EditorGUILayout.Space();
            if (!UserChoosenPath)
            {
                if (Meshes.Count > 0)
                {
                    string assetPath = AssetDatabase.GetAssetPath(Meshes[0]);
                    int index = assetPath.LastIndexOf("/");
                    if (index >= 0)
                        assetPath = assetPath.Substring(0, index + 1);

                    SavePath = assetPath;
                }
            }



            //var midStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            EditorGUILayout.LabelField("Baked Meshes Location");

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(SavePath, EditorStyles.helpBox);
                if (GUILayout.Button("..."))
                {
                    string chosenFile = EditorUtility.OpenFolderPanel("Choose new path", "", "");
                    if (chosenFile != "" && chosenFile.Length > Application.dataPath.Length)
                    {
                        chosenFile = "Assets" + chosenFile.Substring(Application.dataPath.Length) + "/";
                        UserChoosenPath = true;
                        SavePath = chosenFile;
                    }

                }

                if (GUILayout.Button("Same Folder"))
                {
                    UserChoosenPath = false;
                }

            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(arrayProperty, true);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("Prefix");
            NamePrefix = EditorGUILayout.TextField(NamePrefix);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Suffix");
            NameSuffix = EditorGUILayout.TextField(NameSuffix);
            EditorGUILayout.Space();


            if (GUILayout.Button("Bake meshes"))
            {
                if (Meshes.Count == 0)
                {
                    Debug.Log("There is no mesh to be baked!");
                    return;
                }

                foreach (var mesh in Meshes)
                {
                    AssetDatabase.CreateAsset(CalculateCustomNormals(mesh), SavePath + NamePrefix + mesh.name + NameSuffix + ".asset");
                }

                AssetDatabase.SaveAssets();
            }

            if (Meshes.Count == 0)
            {
                EditorGUILayout.HelpBox("No mesh to bake texture on.", MessageType.Warning);
            }
        }

        public Mesh CalculateCustomNormals(Mesh meshSave)
        {
            Mesh newMesh = Instantiate(meshSave);

            Vector3[] vertices = meshSave.vertices;
            int[] triangles = meshSave.triangles;
            Vector3[] outlineNormals = new Vector3[vertices.Length];

            List<CospatialVertex> cospatialVerticesData = new List<CospatialVertex>();
            int[] cospacialVertexIndices = new int[vertices.Length];
            FindCospatialVertices(vertices, cospacialVertexIndices, cospatialVerticesData);

            int numTriangles = triangles.Length / 3;
            for (int t = 0; t < numTriangles; t++)
            {
                int vertexStart = t * 3;
                int v1Index = triangles[vertexStart];
                int v2Index = triangles[vertexStart + 1];
                int v3Index = triangles[vertexStart + 2];
                ComputeNormalAndWeights(vertices[v1Index], vertices[v2Index], vertices[v3Index], out Vector3 normal, out Vector3 weights);
                AddWeightedNormal(normal * weights.x, v1Index, cospacialVertexIndices, cospatialVerticesData);
                AddWeightedNormal(normal * weights.y, v2Index, cospacialVertexIndices, cospatialVerticesData);
                AddWeightedNormal(normal * weights.z, v3Index, cospacialVertexIndices, cospatialVerticesData);
            }

            for (int v = 0; v < outlineNormals.Length; v++)
            {
                int cvIndex = cospacialVertexIndices[v];
                var cospatial = cospatialVerticesData[cvIndex];
                outlineNormals[v] = cospatial.accumulatedNormal.normalized;
            }

            newMesh.SetUVs(((int)uvChannel) + 1, outlineNormals);
            return newMesh;
        }

        private void FindCospatialVertices(Vector3[] vertices, int[] indices, List<CospatialVertex> registry)
        {
            for (int v = 0; v < vertices.Length; v++)
            {
                if (SearchForPreviouslyRegisteredCV(vertices[v], registry, out int index))
                {
                    indices[v] = index;
                }
                else
                {
                    var cospatialEntry = new CospatialVertex()
                    {
                        position = vertices[v],
                        accumulatedNormal = Vector3.zero,
                    };
                    indices[v] = registry.Count;
                    registry.Add(cospatialEntry);
                }
            }
        }

        private bool SearchForPreviouslyRegisteredCV(Vector3 position, List<CospatialVertex> registry, out int index)
        {
            for (int i = 0; i < registry.Count; i++)
            {
                if (Vector3.Distance(registry[i].position, position) <= cospatialVertexDistance)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        private void ComputeNormalAndWeights(Vector3 a, Vector3 b, Vector3 c, out Vector3 normal, out Vector3 weights)
        {
            normal = Vector3.Cross(b - a, c - a).normalized;
            weights = new Vector3(Vector3.Angle(b - a, c - a), Vector3.Angle(c - b, a - b), Vector3.Angle(a - c, b - c));
        }

        private void AddWeightedNormal(Vector3 weightedNormal, int vertexIndex, int[] cvIndices, List<CospatialVertex> cvRegistry)
        {
            int cvIndex = cvIndices[vertexIndex];
            cvRegistry[cvIndex].accumulatedNormal += weightedNormal;
        }
    }
}