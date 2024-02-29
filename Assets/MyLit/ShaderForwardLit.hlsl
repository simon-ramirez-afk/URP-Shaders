// This file contains the vertex and fragment functions for the forward lit pass
// This is the shader pass that computes visible colors for a material
// by reading material, light, shadow, etc. data

// Pulls the URP Library functions and our own common functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//Textures
TEXTURE2D(_colorMap); SAMPLER(sampler_colorMap); //RGB = Albedo, A= Alpha

float4 _colorMap_ST; //This needs to be set as _textureName_ST
float4 _colorTint; // Color tint is the color of the texture
float _smoothness; // Smoothness property

// This struct captures the data of 1 vertex at a time, hence why it's a float3. The pipeline will run this multiple times in parallel
// Receives data about the mesh we're currently rendering
// Data is auto-placed in field according to their semantic
struct Attributes {
    float3 positionOS : POSITION; // Position in object space
    float3 normalOS : NORMAL; // Normal vertices in object space
    float2 uv : TEXCOORD0; // Material Texture UVs
};

// This struct is output by the vertex function and input to the fragment function
// The fields will be transformed by the rasterizer
struct Interpolators {
    // This value should contain the position in CS (Similar to screen position)
    // when output from the vertex function. It'll be transform into pixel position of the current fragment on the screen when read from the frag functions
    float4 positionCS : SV_POSITION;

    //This value contains the normals in world space, we will run them in the Vertex function as it runs less often than the fragment stage which makes it more optimal
    float3 normalWS : TEXCOORD1;

    // The following variables will retain their values from the vertex stage, except the rasterizer will interpolate them between vertices
    float2 uv : TEXCOORD0; // Material Texture UVs

    // We'll use this variable to get the world space position for the fragment function
    float3 positionWS : TEXTCOORD2;
};


// Vertex function, this runs for each vertex on the mesh, it must output the position on the screen each vertex should appear at, as well as any data the fragment function will need
Interpolators Vertex(Attributes input) {
    Interpolators output;
    
    // Helper function from the ShaderVariablesFunction.hlsl
    // Transform Object Space values into World and Clip Space
    VertexPositionInputs position_inputs = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs normal_inputs = GetVertexNormalInputs(input.normalOS);

    // Pass the position and orientation data to the fragment function
    output.positionCS = position_inputs.positionCS;
    output.uv = TRANSFORM_TEX(input.uv, _colorMap);
    output.normalWS = normal_inputs.normalWS;
    output.positionWS = position_inputs.positionWS;

    return output;
}


// Fragment function, this runs once per fragment, which is like a pixel on the screen, it outputs the final color of the pixel
float4 Fragment(Interpolators input) : SV_TARGET {

    //Passing the uv value from Interpolators struct
    float2 uv = input.uv;
    //Sample the color map
    float4 colorSample = SAMPLE_TEXTURE2D(_colorMap, sampler_colorMap, uv);

    InputData lightingInput = (InputData)0;

    lightingInput.positionWS = input.positionWS;
    lightingInput.normalWS = normalize(input.normalWS);
    lightingInput.viewDirectionWS = GetWorldSpaceViewDir(input.positionWS);
    lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

    SurfaceData surfaceInput = (SurfaceData)0;

    surfaceInput.albedo = colorSample.rgb * _colorTint.rgb;
    surfaceInput.alpha = colorSample.a * _colorTint.a;
    surfaceInput.specular = 1;
    surfaceInput.smoothness = _smoothness;

    #if UNITY_VERSION >= 202120
        return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
    #else
        return UniversalFragmentBlinnPhong(lightingInput, float4(surfaceInput.specular, 1), surfaceInput.smoothness, 0, surfaceInput.alpha);
    #endif
    
    
}
