using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

public class CustomInspector : ShaderGUI {
    public enum SurfaceType {
        Opaque,
        Transparent
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        if (newShader.name == "IntentoGioco/MyShader") {
            UpdateSurfaceType(material);
        }
    }

#if UNITY_2022_1_OR_NEWER
    public override void ValidateMaterial(Material material) {
        base.ValidateMaterial(material);
        UpdateSurfaceType(material);
    }
#endif

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        Material material = materialEditor.target as Material;
        var surfaceProp = BaseShaderGUI.FindProperty("_SurfaceType", properties, true);

        EditorGUI.BeginChangeCheck();

#if UNITY_2022_1_OR_NEWER
        MaterialEditor.BeginProperty(surfaceProp);
#endif
        surfaceProp.floatValue =
            (int)(SurfaceType)EditorGUILayout.EnumPopup("Surface type", (SurfaceType)surfaceProp.floatValue);
#if UNITY_2022_1_OR_NEWER
        MaterialEditor.EndProperty();
#endif

// #if UNITY_2022_1_OR_NEWER
//         MaterialEditor.BeginProperty(faceProp);
// #endif
//         faceProp.floatValue = (int)(FaceRenderingMode)EditorGUILayout.EnumPopup("Face rendering mode", (FaceRenderingMode)faceProp.floatValue);
// #if UNITY_2022_1_OR_NEWER
//         MaterialEditor.EndProperty();
// #endif

        if (EditorGUI.EndChangeCheck()) {
            UpdateSurfaceType(material);
        }

        base.OnGUI(materialEditor, properties);
    }

    private void UpdateSurfaceType(Material material) {
        SurfaceType surface = (SurfaceType)material.GetFloat("_SurfaceType");
        switch (surface) {
            case SurfaceType.Opaque:
                material.renderQueue = (int)RenderQueue.Geometry;
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SourceBlend", (int)BlendMode.One);
                material.SetInt("_DestBlend", (int)BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.SetShaderPassEnabled("ShadowCaster", true);
                break;
            case SurfaceType.Transparent:
                material.renderQueue = (int)RenderQueue.Transparent;
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SourceBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DestBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.SetShaderPassEnabled("ShadowCaster", false);
                break;
        }
    }
}