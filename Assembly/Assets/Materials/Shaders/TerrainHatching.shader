//Shader "Nature/Terrain/Diffuse" {


//CGPROGRAM
//#pragma surface surf Lambert



ENDCG  
}




Shader "Custom/Silhoutted Hatching" {
 
     Properties {
     
     	[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
		// used in fallback on old cards & base map
		[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
 
         _OutlineColor ("Outline Color", Color) = (0,0,0,1)
 
         _Outline ("Outline width", Range (0.0, 0.03)) = .005
 
         _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
 
         _BumpMap ("Bumpmap", 2D) = "bump" {}
 
         _Hatch0 ("Hatch 0", 2D) = "white" {}
 
         _Hatch1 ("Hatch 1", 2D) = "gray" {}
 
         _Hatch2 ("Hatch 2", 2D) = "gray" {}
 
         _Hatch3 ("Hatch 3", 2D) = "black" {}
 
     }
   
 
     SubShader {
 
         Tags {"SplatCount" = "4" "Queue" = "Geometry-100" "RenderType" = "Opaque"}
 
  
 
         Pass {
 
             Name "OUTLINE"
 
             Tags { "LightMode" = "Always" }
 
             Cull Off
 
             ZWrite Off
 
             Blend SrcAlpha OneMinusSrcAlpha
 
  
 
             CGPROGRAM
 
             #pragma vertex vert
 
             #pragma fragment frag
 
             #include "UnityCG.cginc"
 
  
 
             struct Input {
				float2 uv_Control : TEXCOORD0;
				float2 uv_Splat0 : TEXCOORD1;
				float2 uv_Splat1 : TEXCOORD2;
				float2 uv_Splat2 : TEXCOORD3;
				float2 uv_Splat3 : TEXCOORD4;
			};
             
             struct appdata {
 
                 float4 vertex : POSITION;
 
                 float3 normal : NORMAL;
 
             };
 
  
 
             struct v2f {
 
                 float4 pos : POSITION;
 
                 float4 color : COLOR;
 
             };
 
  
 
             uniform float _Outline;
 
             uniform float4 _OutlineColor;
 
  
 
             v2f vert(appdata v) {
 
                 v2f o;
 
                 o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
 
                 float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
 
                 float2 offset = TransformViewToProjection(norm.xy);
 
                 o.pos.xy += offset * o.pos.z * _Outline;
 
                 o.color = _OutlineColor;
 
                 return o;
 
             }
 
  
 
             half4 frag(v2f i) : COLOR {
 
                 return i.color;
 
             }
 
             ENDCG
 
         }
 
  
 
         CGPROGRAM
 
  
 
         #pragma surface surf Hatching noambient
 
         #pragma only_renderers d3d9
 
         #pragma target 3.0
 
  
 
         struct SurfaceOutputHatch {
 
             fixed3 Albedo;
 
             fixed3 Normal;
 
             fixed4 Hatch;
 
             fixed3 Emission;
 
             fixed Specular;
 
             fixed Gloss;
 
             float Alpha;
 
         };
 
  
 
         struct Input
 
         {
 
             float2 uv_Hatch0;
 
             float2 uv_MainTex;
             
             float3 worldNormal;
             
             INTERNAL_DATA
 
         };
 
  
 
         sampler2D _Hatch0, _Hatch1, _Hatch2, _Hatch3;
 
         sampler2D _MainTex;
 
         sampler2D _BumpMap;
 
         sampler2D _SpecularTex;
         
         sampler2D _Control;
         
		 sampler2D _Splat0,_Splat1,_Splat2,_Splat3;

		          
 
         inline half4 LightingHatching (inout SurfaceOutputHatch s, half3 lightDir, half3 viewDir, half atten)
 
         {
 
             float3 h = normalize (lightDir + viewDir);
 
             float NdotL = dot (s.Normal, lightDir) * 0.5 + 0.5;
 
             float nh = max (0, dot (s.Normal, h));
 
                          
 
             float intensity = saturate((NdotL) * atten);
 
             
 
             fixed hatch;
 
             hatch = lerp ( s.Hatch.r, 1.0, saturate((intensity - 0.75) * 4));
 
             hatch = lerp ( s.Hatch.g, hatch, saturate((intensity - 0.5) * 4));
 
             hatch = lerp ( s.Hatch.b, hatch, saturate((intensity - 0.25) * 4));
 
             hatch = lerp ( s.Hatch.a, hatch, saturate((intensity) * 4));
 
             
 
              
             half4 c;
 
             c.rgb = s.Albedo * _LightColor0.rgb * hatch;
 
             c.a = 0.5;
 
             return c;
 
         }
 
  
 
         void surf (Input IN, inout SurfaceOutputHatch o)
 
         {
 
             fixed4 splat_control = tex2D (_Control, IN.uv_Control);
		 	 fixed3 col;
		 	 col  = splat_control.r * tex2D (_Splat0, IN.uv_Splat0).rgb;
		 	 col += splat_control.g * tex2D (_Splat1, IN.uv_Splat1).rgb;
		 	 col += splat_control.b * tex2D (_Splat2, IN.uv_Splat2).rgb;
		 	 col += splat_control.a * tex2D (_Splat3, IN.uv_Splat3).rgb;
		 	 o.Albedo = col;
		 	 o.Alpha = 0.0;
		 	              
             o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
 
             o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
 
             o.Hatch.r = tex2D(_Hatch0, IN.uv_Hatch0);
 
             o.Hatch.g = tex2D(_Hatch1, IN.uv_Hatch0);
 
             o.Hatch.b = tex2D(_Hatch2, IN.uv_Hatch0);
 
             o.Hatch.a = tex2D(_Hatch3, IN.uv_Hatch0);
 
         }
 
         ENDCG
 
     }
     
     Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/Lightmap-AddPass"
	 Dependency "BaseMapShader" = "Diffuse"
	 Dependency "Details0"      = "Hidden/TerrainEngine/Details/Vertexlit"
	 Dependency "Details1"      = "Hidden/TerrainEngine/Details/WavingDoublePass"
	 Dependency "Details2"      = "Hidden/TerrainEngine/Details/BillboardWavingDoublePass"
	 Dependency "Tree0"         = "Hidden/TerrainEngine/BillboardTree"
 
     Fallback "Unlit"
 
 }