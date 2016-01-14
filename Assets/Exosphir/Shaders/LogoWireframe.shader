Shader "Exosphir/LogoWireframe" {
	Properties {
        _FColor ("Foreground Color", Color) = (1,1,1,1)
        _BColor ("Background Color", Color) = (0,0,0,1)
        _BStart ("Background Start", Range(0.0,1.0)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
       
        Pass {
            Tags { "LightMode" = "Always" }
           
            Fog { Mode Off }
            ZWrite On
            ZTest LEqual
            Cull Back
            Lighting Off
   
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members screenPos)
#pragma exclude_renderers d3d11 xbox360
            	#include "UnityCG.cginc"
            	
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
               
                fixed4 _FColor;
                fixed4 _BColor;
                float _BStart;
               
                struct appdata {
                    float4 vertex : POSITION;
                };
               
                struct v2f {
                    float4 vertex : POSITION;
                    float4 screenPos;
                };
               
                v2f vert (appdata v) {
                    v2f o;
                    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                    o.screenPos = ComputeScreenPos(o.vertex);
        			return o;
                }
                
                float UnityDistanceFromPlane (float3 pos, float4 plane) {
					float d = dot (float4(pos,1.0f), plane);
					return d;
				}
               
                fixed4 frag (v2f i) : COLOR {
                	float farPlane = UnityDistanceFromPlane(i.screenPos, unity_CameraWorldClipPlanes[5]);
                	float nearPlane = UnityDistanceFromPlane(i.screenPos, unity_CameraWorldClipPlanes[4]);
                	
                	float planeDiff = farPlane - nearPlane;
                	float screenPos = (i.screenPos.z * planeDiff) + nearPlane;
                
                	float BMultiplier = screenPos / _BStart;
                	float FMultiplier = 1.0 - (screenPos / _BStart);
                	
                	float4 finalColor = (_FColor * FMultiplier) + (_BColor * BMultiplier);
                	//float4 finalColor = (_FColor * abs(_BStart - 1.0)) + (_BColor * _BStart);
                    return finalColor;
                }
            ENDCG
   
        }
    }
}
