Shader "IntentoGioco/MyShader" {
    // Properties are options set per material, exposed by the inspector
    Properties {
        [Header(Surface Options)] // Text header
        // [MainTexture] does the same thing [MainColor] does
        [MainTexture] _colorMap("Color", 2D) = "white"{}
        // [MainColor] allows Material.color to use the correct property
        [MainColor] _colorTint("Tint", Color) = (1, 1, 1, 1)
        // Smoothness for the materials - Smoother = smaller, think of a metal object!
        _smoothness("Smoothness", Float) = 0
    }
    // Sub-shaders allow for different behaviours and options for different pipelines & platforms
    SubShader {
        // Tags are shared by all passes in the sub-shader
        Tags {
            "RenderPipeline" = "UniversalPipeline"
        }

        // Shaders can have multiples passes for different material data, each pass has its own vertex function, fragment function, and keywords  
        Pass {
            Name "ForwardLit" // Debugging
            Tags {
                "LightMode" = "UniversalForward"
            } // Pass specific tags // This one will tell Unity "UniversalForward" will be the main lighting pass for this shader

            HLSLPROGRAM
            // HLSL Code
            
            #define _SPECULAR_COLOR // This enables specular highlights since they're disabled by default with UniversalFragmentBlinnPhong
            #if UNITY_VERSION >= 202120
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #else
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS // Creates a variant of the shader one with shadows and one without, the first underscore is a variant without any keywords
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #endif
            
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            #pragma vertex Vertex
            #pragma fragment Fragment

            //HLSL File
            #include "ShaderForwardLit.hlsl"
            ENDHLSL
        }
        
        Pass {
            Name "ShadowCaster" // Debugging
            Tags {
                "LightMode" = "ShadowCaster"
            }
            
            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "ShaderShadowCaster.hlsl"
            ENDHLSL
        }
    }
}