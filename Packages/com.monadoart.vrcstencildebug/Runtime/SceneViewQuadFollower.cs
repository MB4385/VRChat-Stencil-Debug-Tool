using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SceneViewQuadFollower : MonoBehaviour
{
    public static readonly float distance = 2f; // Distance in front of the Scene view camera
    private static readonly Vector2 size = new Vector2(10f, 10f); // Hardcoded size of the quad
    private Material quadMaterial;
    private GameObject quad;

    [Header("Material Controls")]
    [Range(0f, 1f)] public float tilingSlider = 0.7f; // Logarithmic slider, 0=min, 1=max, 0.7=default (32)
    [Range(0f, 1f)] public float opacity = 0.35f;
    [Range(-180f, 180f)] public float rotation = 22.5f;
    [HideInInspector] public bool showNumbers = true;

    void OnEnable()
    {
        CreateOrFindQuad();
        SceneView.duringSceneGui += UpdateQuadTransform;
        UpdateMaterialProperties();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= UpdateQuadTransform;
        if (quad != null)
        {
            DestroyImmediate(quad);
        }
    }

    void CreateOrFindQuad()
    {
        if (quad == null)
        {
            quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "SceneViewStencilDebugQuad";
            quad.hideFlags = HideFlags.HideAndDontSave;
            if (quadMaterial == null)
            {
                quadMaterial = new Material(Shader.Find("Unlit/StencilDebugShader"));
            }
            quad.GetComponent<Renderer>().sharedMaterial = quadMaterial;
        }
    }

    void UpdateQuadTransform(SceneView sceneView)
    {
        if (quad == null) CreateOrFindQuad();
        Camera cam = sceneView.camera;
        if (cam == null) return;
        quad.transform.position = cam.transform.position + cam.transform.forward * distance;
        quad.transform.rotation = cam.transform.rotation;
        quad.transform.localScale = new Vector3(size.x, size.y, 1f);
    }

    void OnValidate()
    {
        if (quad != null && quadMaterial != null)
        {
            quad.GetComponent<Renderer>().sharedMaterial = quadMaterial;
        }
        UpdateMaterialProperties();
    }

    void UpdateMaterialProperties()
    {
        // Logarithmic mapping: 2 to 512
        float minTiling = 2f;
        float maxTiling = 512f;
        float logMin = Mathf.Log(minTiling, 2f);
        float logMax = Mathf.Log(maxTiling, 2f);
        float exponent = Mathf.Lerp(logMin, logMax, tilingSlider);
        float tilingAmount = Mathf.Round(Mathf.Pow(2f, exponent));
        if (quadMaterial != null)
        {
            quadMaterial.SetFloat("_TileCount", tilingAmount);
            quadMaterial.SetFloat("_BgOpacity", opacity);
            quadMaterial.SetFloat("_NumberRotation", rotation);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SceneViewQuadFollower))]
    public class SceneViewQuadFollowerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SceneViewQuadFollower follower = (SceneViewQuadFollower)target;
            DrawDefaultInspector();

            // Boolean toggle for numbers
            bool prevShowNumbers = follower.showNumbers;
            follower.showNumbers = EditorGUILayout.Toggle("Show Numbers", follower.showNumbers);
            if (follower.showNumbers != prevShowNumbers)
            {
                Undo.RecordObject(follower, "Toggle Show Numbers");
                if (follower.quadMaterial != null)
                {
                    follower.quadMaterial.SetFloat("_ShowNumbers", follower.showNumbers ? 1f : 0f);
                }
                EditorUtility.SetDirty(follower);
            }

            // Opacity control buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Opacity 0%"))
            {
                Undo.RecordObject(follower, "Set Opacity 0%");
                follower.opacity = 0f;
                follower.UpdateMaterialProperties();
                EditorUtility.SetDirty(follower);
            }
            if (GUILayout.Button("Opacity Default"))
            {
                Undo.RecordObject(follower, "Reset Opacity");
                follower.opacity = 0.35f;
                follower.UpdateMaterialProperties();
                EditorUtility.SetDirty(follower);
            }
            if (GUILayout.Button("Opacity 100%"))
            {
                Undo.RecordObject(follower, "Set Opacity 100%");
                follower.opacity = 1f;
                follower.UpdateMaterialProperties();
                EditorUtility.SetDirty(follower);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⟲ Rotate -22.5°"))
            {
                Undo.RecordObject(follower, "Rotate Text Left");
                follower.rotation -= 22.5f;
                follower.UpdateMaterialProperties();
                EditorUtility.SetDirty(follower);
            }
            if (GUILayout.Button("⟳ Rotate +22.5°"))
            {
                Undo.RecordObject(follower, "Rotate Text Right");
                follower.rotation += 22.5f;
                follower.UpdateMaterialProperties();
                EditorUtility.SetDirty(follower);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Reset Material Controls"))
            {
                Undo.RecordObject(follower, "Reset Material Controls");
                follower.opacity = 0.35f;
                follower.tilingSlider = 0.7f;
                follower.rotation = 22.5f;
                follower.showNumbers = true;
                follower.UpdateMaterialProperties();
                if (follower.quadMaterial != null)
                {
                    follower.quadMaterial.SetFloat("_ShowNumbers", 1f);
                }
                EditorUtility.SetDirty(follower);
            }
        }
    }
#endif
}
