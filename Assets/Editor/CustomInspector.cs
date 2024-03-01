using UnityEditor; 
using UnityEngine; 
using UnityEngine.PlayerLoop; 
using UnityEngine.Rendering; 

// CustomInspector inherits from ShaderGUI, allowing customization of shader properties in the inspector.
public class CustomInspector : ShaderGUI {
    // Defines an enumeration for different surface types that a material can have.
    public enum SurfaceType {
        Opaque,
        TransparentBlend,
        TransparentCutout
    }

    // Method called when a new shader is assigned to a material. It allows for custom initialization.
    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
        base.AssignNewShaderToMaterial(material, oldShader, newShader); // Call the base method implementation.

        // Checks if the new shader is the specific one we're interested in.
        if (newShader.name == "IntentoGioco/MyShader") {
            UpdateSurfaceType(material); // Update material properties based on its surface type.
        }
    }

#if UNITY_2022_1_OR_NEWER
    // Method to validate and update material properties. Only defined for Unity 2022.1 or newer.
    public override void ValidateMaterial(Material material) {
        base.ValidateMaterial(material); // Call the base method implementation.
        UpdateSurfaceType(material); // Update material properties based on its surface type.
    }
#endif

    // Overrides the GUI for the material inspector, allowing custom UI elements.
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        Material material = materialEditor.target as Material; // Cast the target of the material editor to Material.
        // Find the property for surface type in the shader.
        var surfaceProp = BaseShaderGUI.FindProperty("_SurfaceType", properties, true);

        EditorGUI.BeginChangeCheck(); // Start checking for changes in the inspector.

#if UNITY_2022_1_OR_NEWER
        MaterialEditor.BeginProperty(surfaceProp); // Indicate the start of a property GUI block.
#endif
        // Display an enum popup to select the surface type and update the material's property based on the selection.
        surfaceProp.floatValue =
            (int)(SurfaceType)EditorGUILayout.EnumPopup("Surface type", (SurfaceType)surfaceProp.floatValue);
#if UNITY_2022_1_OR_NEWER
        MaterialEditor.EndProperty(); // Indicate the end of a property GUI block.
#endif

        // If any changes were detected in the inspector, update the material's surface type settings.
        if (EditorGUI.EndChangeCheck()) {
            UpdateSurfaceType(material);
        }

        base.OnGUI(materialEditor, properties); // Call the base OnGUI method.
    }

    // Method to update material settings based on its surface type.
    private void UpdateSurfaceType(Material material) {
        SurfaceType surface = (SurfaceType)material.GetFloat("_SurfaceType"); // Retrieve the current surface type from the material.
        switch (surface) {
            case SurfaceType.Opaque:
                material.renderQueue = (int)RenderQueue.Geometry; // Set render queue for opaque objects.
                material.SetOverrideTag("RenderType", "Opaque"); // Tag the material as opaque.
                break;
            case SurfaceType.TransparentCutout:
                material.renderQueue = (int)RenderQueue.AlphaTest; // Set render queue for cutout transparency.
                material.SetOverrideTag("RenderType", "TransparentCutout"); // Tag the material as transparent cutout.
                break;
            case SurfaceType.TransparentBlend:
                material.renderQueue = (int)RenderQueue.Transparent; // Set render queue for blended transparency.
                material.SetOverrideTag("RenderType", "Transparent"); // Tag the material as transparent.
                break;
        }

        // Update blending and depth writing settings based on surface type.
        switch (surface) {
            case SurfaceType.Opaque:
            case SurfaceType.TransparentCutout:
                material.SetInt("_SourceBlend", (int)BlendMode.One);
                material.SetInt("_DestBlend", (int)BlendMode.Zero);
                material.SetInt("_ZWrite", 1); // Enable depth writing.
                break;
            case SurfaceType.TransparentBlend:
                material.SetInt("_SourceBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DestBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0); // Disable depth writing for transparent surfaces.
                break;
        }
        
        // Enable or disable the shadow caster pass based on whether the surface is not transparent blend.
        material.SetShaderPassEnabled("ShadowCaster", surface != SurfaceType.TransparentBlend);
    }
}
