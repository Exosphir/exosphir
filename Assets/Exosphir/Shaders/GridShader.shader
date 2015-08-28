Shader "Exosphir/Double Grid Shader" {
	Properties {
		_CellSize("Cell Size", Float) = 1.0
		_MainWidth("Main Line Width", Range(0, 1)) = 0.05
		_MainColor("Main Line Color", Color) = (1,1,1,1)
		_SecondaryWidth("Secondary Line Width", Range(0, 1)) = 0.02
		_SecondaryColor("Secondary Line Color", Color) = (1,1,1,0.5)
		_BackgroundColor("Background Color", Color) = (0,0,0,0)
		_NearFadeDistance("Near Fade Distance", Range(0, 5)) = 1

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Unlit alpha vertex:vert
		#define AntiAliasBlur 2.5


		#define _onLineExpr(f,w) f - w * 0.5
		#define _onLine(b,x) (1.0 - smoothstep(0.0, b, x))
		#define onLine(f,w,b) _onLine(b, _onLineExpr(f,w))
		#define onLineOffset(f,w,o,b) _onLine(b, o - _onLineExpr(f,w))

		#pragma target 3.0

		struct Input {
			float3 worldPos;
		};
		
		uniform float _CellSize;
		uniform float _MainWidth;
		uniform half4 _MainColor;
		uniform float _SecondaryWidth;
		uniform half4 _SecondaryColor;
		uniform half4 _BackgroundColor;
		uniform float _NearFadeDistance;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT	(Input, o);
		}

		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
			return half4(s.Albedo, s.Alpha);
		}

		void surf(Input IN, inout SurfaceOutput o) {
			
			//lines is 0~1 on each axis, tiling every cellsize in object space
			float2 lines = abs(fmod(abs(IN.worldPos.xz), _CellSize) / _CellSize - float2(.5, .5));
			//make sure lines.x is always the largest axis value
			if (lines.y > lines.x) { lines = lines.yx; }

			float dist = distance(_WorldSpaceCameraPos, IN.worldPos);
			//poor man's antialiasing, defines a "blur radius" for the onLine based on depth (distance)
			//empirical: blur = k / (cell * min(res)) * depth
			float blur = AntiAliasBlur / (_CellSize * min(_ScreenParams.x, _ScreenParams.y)) * dist;

			float main = onLineOffset(lines.x, _MainWidth, 0.5, blur);
			float second = onLine(lines.y, _SecondaryWidth, blur);
			
			//mixes background with secondary color then main color.
			float4 color = lerp(lerp(_BackgroundColor, _SecondaryColor, second), _MainColor, main);
			
			float distPercent = min(_NearFadeDistance, dist) / _NearFadeDistance;
			o.Albedo = color.rgb;
			o.Alpha = color.a * distPercent; // opens a "hole" in the grid when camera too near

		}
		ENDCG
	} 
	FallBack "Diffuse"
}
