// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32543,y:32633|diff-75-OUT,emission-64-OUT,alpha-35-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:33669,y:32436,ptlb:node_BasicColor,ptin:_node_BasicColor,tex:53524c395a1540a4fbd3b942fa54eeda,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Fresnel,id:8,x:33391,y:32810;n:type:ShaderForge.SFN_Multiply,id:9,x:33215,y:32651|A-2-RGB,B-8-OUT;n:type:ShaderForge.SFN_Add,id:35,x:32960,y:32937|A-8-OUT,B-48-R;n:type:ShaderForge.SFN_ValueProperty,id:37,x:33215,y:32810,ptlb:node_GlowValue,ptin:_node_GlowValue,glob:False,v1:1.5;n:type:ShaderForge.SFN_Color,id:48,x:33151,y:33021,ptlb:node_AlphaValue,ptin:_node_AlphaValue,glob:False,c1:0.2205882,c2:0.1930147,c3:0.1930147,c4:1;n:type:ShaderForge.SFN_Multiply,id:64,x:33012,y:32696|A-9-OUT,B-37-OUT;n:type:ShaderForge.SFN_Multiply,id:75,x:33476,y:32563|A-2-RGB,B-88-OUT;n:type:ShaderForge.SFN_Tex2d,id:76,x:34112,y:32505,ptlb:node_OverlayColor,ptin:_node_OverlayColor,tex:a6ef3e0449ffc2b499e33c8dce89c92d,ntxv:0,isnm:False|UVIN-77-UVOUT;n:type:ShaderForge.SFN_Panner,id:77,x:34294,y:32505,spu:0,spv:2;n:type:ShaderForge.SFN_Multiply,id:88,x:33711,y:32699|A-122-OUT,B-91-OUT;n:type:ShaderForge.SFN_ValueProperty,id:91,x:33931,y:32965,ptlb:node_OverLayColorValue,ptin:_node_OverLayColorValue,glob:False,v1:1.5;n:type:ShaderForge.SFN_Multiply,id:122,x:33929,y:32617|A-76-RGB,B-125-RGB;n:type:ShaderForge.SFN_Color,id:125,x:34112,y:32696,ptlb:node_OverlaycolorChange,ptin:_node_OverlaycolorChange,glob:False,c1:0.826572,c2:1,c3:0.3382353,c4:1;proporder:2-37-48-76-91-125;pass:END;sub:END;*/

Shader "Shader Forge/JellyFishTail06" {
	Properties {
		_node_BasicColor ("node_BasicColor", 2D) = "white" {}
		_node_GlowValue ("node_GlowValue", Float ) = 1.5
		_node_AlphaValue ("node_AlphaValue", Color) = (0.2205882,0.1930147,0.1930147,1)
		_node_OverlayColor ("node_OverlayColor", 2D) = "white" {}
		_node_OverLayColorValue ("node_OverLayColorValue", Float ) = 1.5
		_node_OverlaycolorChange ("node_OverlaycolorChange", Color) = (0.826572,1,0.3382353,1)
		[HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	}
	SubShader {
		Tags {
			"IgnoreProjector"="True"
			"Queue"="Transparent"
			"RenderType"="Transparent"
		}
		Pass {
			Name "ForwardBase"
			Tags {
				"LightMode"="ForwardBase"
			}
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define UNITY_PASS_FORWARDBASE
			#include "UnityCG.cginc"
			#pragma multi_compile_fwdbase
			#pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
			#pragma target 3.0
			uniform float4 _LightColor0;
			uniform float4 _TimeEditor;
			uniform sampler2D _node_BasicColor; uniform float4 _node_BasicColor_ST;
			uniform float _node_GlowValue;
			uniform float4 _node_AlphaValue;
			uniform sampler2D _node_OverlayColor; uniform float4 _node_OverlayColor_ST;
			uniform float _node_OverLayColorValue;
			uniform float4 _node_OverlaycolorChange;
			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv0 : TEXCOORD0;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 uv0 : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
				float3 normalDirection =  i.normalDir;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
////// Lighting:
				float attenuation = 1;
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = max( 0.0, NdotL) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
				float2 node_162 = i.uv0;
				float4 node_2 = tex2D(_node_BasicColor,TRANSFORM_TEX(node_162.rg, _node_BasicColor));
				float node_8 = (1.0-max(0,dot(normalDirection, viewDirection)));
				float3 emissive = ((node_2.rgb*node_8)*_node_GlowValue);
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float4 node_163 = _Time + _TimeEditor;
				float2 node_77 = (node_162.rg+node_163.g*float2(0,2));
				finalColor += diffuseLight * (node_2.rgb*((tex2D(_node_OverlayColor,TRANSFORM_TEX(node_77, _node_OverlayColor)).rgb*_node_OverlaycolorChange.rgb)*_node_OverLayColorValue));
				finalColor += emissive;
/// Final Color:
				return fixed4(finalColor,(node_8+_node_AlphaValue.r));
			}
			ENDCG
		}
		Pass {
			Name "ForwardAdd"
			Tags {
				"LightMode"="ForwardAdd"
			}
			Blend One One
			ZWrite Off
			
			Fog { Color (0,0,0,0) }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define UNITY_PASS_FORWARDADD
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma multi_compile_fwdadd
			#pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
			#pragma target 3.0
			uniform float4 _LightColor0;
			uniform float4 _TimeEditor;
			uniform sampler2D _node_BasicColor; uniform float4 _node_BasicColor_ST;
			uniform float _node_GlowValue;
			uniform float4 _node_AlphaValue;
			uniform sampler2D _node_OverlayColor; uniform float4 _node_OverlayColor_ST;
			uniform float _node_OverLayColorValue;
			uniform float4 _node_OverlaycolorChange;
			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv0 : TEXCOORD0;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 uv0 : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
				LIGHTING_COORDS(3,4)
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
				float3 normalDirection =  i.normalDir;
				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = max( 0.0, NdotL) * attenColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float2 node_164 = i.uv0;
				float4 node_2 = tex2D(_node_BasicColor,TRANSFORM_TEX(node_164.rg, _node_BasicColor));
				float4 node_165 = _Time + _TimeEditor;
				float2 node_77 = (node_164.rg+node_165.g*float2(0,2));
				finalColor += diffuseLight * (node_2.rgb*((tex2D(_node_OverlayColor,TRANSFORM_TEX(node_77, _node_OverlayColor)).rgb*_node_OverlaycolorChange.rgb)*_node_OverLayColorValue));
				float node_8 = (1.0-max(0,dot(normalDirection, viewDirection)));
/// Final Color:
				return fixed4(finalColor * (node_8+_node_AlphaValue.r),0);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "ShaderForgeMaterialInspector"
}
