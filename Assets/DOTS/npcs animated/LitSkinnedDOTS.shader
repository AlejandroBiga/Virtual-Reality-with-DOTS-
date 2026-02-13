Shader "Custom/LitSkinnedDOTS"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            
            // Pragmas para DOTS skinning
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_DEFORM_LINEAR_BLEND_SKINNING
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // Incluir el soporte de deformación de Entities Graphics
            #if defined(DOTS_DEFORM_LINEAR_BLEND_SKINNING)
                #include "Packages/com.unity.entities.graphics/ShaderLibrary/Deformation.hlsl"
            #endif
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                
                #if defined(DOTS_DEFORM_LINEAR_BLEND_SKINNING)
                    uint4 blendIndices : BLENDINDICES;
                    float4 blendWeights : BLENDWEIGHTS;
                #endif
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                
                // Aplicar deformación de skinning
                #if defined(DOTS_DEFORM_LINEAR_BLEND_SKINNING)
                    ApplyDeformation(IN.positionOS, IN.normalOS, IN.blendIndices, IN.blendWeights);
                #endif
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);
                
                OUT.positionHCS = positionInputs.positionCS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return color;
            }
            ENDHLSL
        }
        
        // Pass de sombras también necesita skinning
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma multi_compile _ DOTS_DEFORM_LINEAR_BLEND_SKINNING
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            #if defined(DOTS_DEFORM_LINEAR_BLEND_SKINNING)
                #include "Packages/com.unity.entities.graphics/ShaderLibrary/Deformation.hlsl"
            #endif
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                
                #if defined(DOTS_DEFORM_LINEAR_BLEND_SKINNING)
                    uint4 blendIndices : BLENDINDICES;
                    float4 blendWeights : BLENDWEIGHTS;
                #endif
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings ShadowVert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                
                #if defined(DOTS_DEFORM_LINEAR_BLEND_SKINNING)
                    ApplyDeformation(IN.positionOS, IN.normalOS, IN.blendIndices, IN.blendWeights);
                #endif
                
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }
            
            half4 ShadowFrag(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}