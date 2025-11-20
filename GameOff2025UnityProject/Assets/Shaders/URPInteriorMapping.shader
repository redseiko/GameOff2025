Shader "GameJam/URPInteriorMapping"
{
    Properties
    {
        _BaseColor ("Wall Color", Color) = (0.9, 0.9, 0.9, 1)
        _FloorColor ("Floor Color", Color) = (0.4, 0.3, 0.2, 1)
        _CeilingColor ("Ceiling Color", Color) = (0.8, 0.8, 0.8, 1)
        _EmissionColor ("Light Color", Color) = (1, 0.95, 0.8, 1)
        _EmissionStrength ("Light Strength", Range(0, 5)) = 1.5
        _RoomDepth ("Room Depth", Range(0.1, 2.0)) = 0.8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "InteriorMapping"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDirTS : TEXCOORD1;
                float3 posWorld : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _FloorColor;
                float4 _CeilingColor;
                float4 _EmissionColor;
                float _EmissionStrength;
                float _RoomDepth;
            CBUFFER_END

            float rand(float3 co)
            {
                return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 45.543))) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.posWorld = vertexInput.positionWS;
                output.uv = input.uv;

                float3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                float3 bitangent = cross(normalInput.normalWS, normalInput.tangentWS.xyz) * input.tangentOS.w;
                float3x3 TBN = float3x3(normalInput.tangentWS.xyz, bitangent, normalInput.normalWS);
                output.viewDirTS = mul(transpose(TBN), viewDirWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(input.viewDirTS);
                float2 uvRemapped = (input.uv * 2.0) - 1.0;
                float roomDepth = _RoomDepth;

                // Raycast
                float3 rayDir = viewDir;
                rayDir.z *= -1.0; 
                
                float distBack = (roomDepth - 0.0) / rayDir.z;
                float distX = (sign(rayDir.x) * 1.0 - uvRemapped.x) / rayDir.x;
                float distY = (sign(rayDir.y) * 1.0 - uvRemapped.y) / rayDir.y;

                float3 dists = float3(distBack, distX, distY);
                float minDist = min(distBack, min(distX > 0 ? distX : 999, distY > 0 ? distY : 999));
                float3 hitPos = float3(uvRemapped, 0) + rayDir * minDist;
                
                half4 finalColor = _BaseColor;

                // Determine surface
                if (abs(hitPos.y - 1.0) < 0.01 || abs(hitPos.y + 1.0) < 0.01)
                {
                    if (hitPos.y > 0) finalColor = _CeilingColor;
                    else finalColor = _FloorColor;
                }
                else if (abs(hitPos.z - roomDepth) < 0.01)
                {
                    finalColor = _BaseColor; 
                    if(length(hitPos.xy) < 0.4) finalColor *= 0.5; 
                }
                else
                {
                    finalColor = _BaseColor * 0.8; 
                }

                // --- FIX FOR TILING ISSUE ---
                // Instead of input.posWorld (fragment pos), use the Object Origin from the Matrix.
                // unity_ObjectToWorld[0].w = Position X
                // unity_ObjectToWorld[1].w = Position Y
                // unity_ObjectToWorld[2].w = Position Z
                float3 objectOrigin = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
                
                // Randomize based on Object Origin so the whole window matches
                float seed = rand(objectOrigin); 
                float lightOn = step(0.4, seed); 

                if (lightOn > 0.5)
                {
                    finalColor += (_EmissionColor * _EmissionStrength);
                    float ao = smoothstep(0.0, 0.5, 1.0 - abs(hitPos.x)) * smoothstep(0.0, 0.5, 1.0 - abs(hitPos.y));
                    finalColor *= (0.5 + 0.5 * ao);
                }
                else
                {
                    finalColor *= 0.2; 
                }

                return finalColor;
            }
            ENDHLSL
        }
    }
}
