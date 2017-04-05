// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32654,y:32609|diff-166-OUT,spec-144-OUT,emission-66-OUT,alpha-90-OUT;n:type:ShaderForge.SFN_Tex2d,id:3,x:33345,y:32478,ptlb:node_3,ptin:_node_3,tex:1829bad55f87dd341991c552559cc127,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Fresnel,id:9,x:33627,y:33139;n:type:ShaderForge.SFN_Multiply,id:15,x:33454,y:33198|A-9-OUT,B-16-OUT;n:type:ShaderForge.SFN_ValueProperty,id:16,x:33627,y:33288,ptlb:AlphaValue,ptin:_AlphaValue,glob:False,v1:2;n:type:ShaderForge.SFN_Add,id:27,x:33293,y:33301|A-15-OUT,B-44-OUT;n:type:ShaderForge.SFN_Color,id:28,x:33820,y:33372,ptlb:AlphaAdd,ptin:_AlphaAdd,glob:False,c1:0.2400519,c2:0.3329853,c3:0.4411765,c4:1;n:type:ShaderForge.SFN_Multiply,id:44,x:33627,y:33356|A-3-G,B-28-R;n:type:ShaderForge.SFN_Tex2d,id:65,x:33614,y:32762,ptlb:DotsAlpha,ptin:_DotsAlpha,tex:60883891eb6d13f48850170d1b948842,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:66,x:33421,y:32762|A-65-RGB,B-68-RGB;n:type:ShaderForge.SFN_Tex2d,id:68,x:33579,y:32953,ptlb:DotsColor,ptin:_DotsColor,tex:086c017932a88a948a5d7eaccebbcadb,ntxv:0,isnm:False|UVIN-69-UVOUT;n:type:ShaderForge.SFN_Panner,id:69,x:33822,y:32953,spu:0,spv:0.5;n:type:ShaderForge.SFN_Add,id:90,x:33080,y:33249|A-65-R,B-27-OUT;n:type:ShaderForge.SFN_Color,id:101,x:33375,y:32927,ptlb:SpecularColor,ptin:_SpecularColor,glob:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:112,x:33375,y:33094,ptlb:SpecularValue,ptin:_SpecularValue,glob:False,v1:3;n:type:ShaderForge.SFN_Multiply,id:123,x:33186,y:32961|A-101-RGB,B-112-OUT;n:type:ShaderForge.SFN_Multiply,id:144,x:33194,y:32686|A-66-OUT,B-145-OUT;n:type:ShaderForge.SFN_ValueProperty,id:145,x:33401,y:32686,ptlb:EmissVAlve,ptin:_EmissVAlve,glob:False,v1:1;n:type:ShaderForge.SFN_Add,id:166,x:32998,y:32563|A-144-OUT,B-3-RGB;proporder:3-16-28-65-68-101-112-145;pass:END;sub:END;*/

Shader "Shader Forge/JellyFishHead02" {
	Properties {
		_node_3 ("node_3", 2D) = "white" {}
		_AlphaValue ("AlphaValue", Float ) = 2
		_AlphaAdd ("AlphaAdd", Color) = (0.2400519,0.3329853,0.4411765,1)
		_DotsAlpha ("DotsAlpha", 2D) = "white" {}
		_DotsColor ("DotsColor", 2D) = "white" {}
		_SpecularColor ("SpecularColor", Color) = (1,1,1,1)
		_SpecularValue ("SpecularValue", Float ) = 3
		_EmissVAlve ("EmissVAlve", Float ) = 1
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
			uniform sampler2D _node_3; uniform float4 _node_3_ST;
			uniform float _AlphaValue;
			uniform float4 _AlphaAdd;
			uniform sampler2D _DotsAlpha; uniform float4 _DotsAlpha_ST;
			uniform sampler2D _DotsColor; uniform float4 _DotsColor_ST;
			uniform float _EmissVAlve;
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
				float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
				float attenuation = 1;
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = max( 0.0, NdotL) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
				float2 node_1922 = i.uv0;
				float4 node_65 = tex2D(_DotsAlpha,TRANSFORM_TEX(node_1922.rg, _DotsAlpha));
				float4 node_1923 = _Time + _TimeEditor;
				float2 node_69 = (node_1922.rg+node_1923.g*float2(0,0.5));
				float3 node_66 = (node_65.rgb*tex2D(_DotsColor,TRANSFORM_TEX(node_69, _DotsColor)).rgb);
				float3 emissive = node_66;
///////// Gloss:
				float gloss = exp2(0.5*10.0+1.0);
////// Specular:
				NdotL = max(0.0, NdotL);
				float3 node_144 = (node_66*_EmissVAlve);
				float3 specularColor = node_144;
				float3 specular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float4 node_3 = tex2D(_node_3,TRANSFORM_TEX(node_1922.rg, _node_3));
				finalColor += diffuseLight * (node_144+node_3.rgb);
				finalColor += specular;
				finalColor += emissive;
/// Final Color:
				return fixed4(finalColor,(node_65.r+(((1.0-max(0,dot(normalDirection, viewDirection)))*_AlphaValue)+(node_3.g*_AlphaAdd.r))));
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
			uniform sampler2D _node_3; uniform float4 _node_3_ST;
			uniform float _AlphaValue;
			uniform float4 _AlphaAdd;
			uniform sampler2D _DotsAlpha; uniform float4 _DotsAlpha_ST;
			uniform sampler2D _DotsColor; uniform float4 _DotsColor_ST;
			uniform float _EmissVAlve;
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
				float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = max( 0.0, NdotL) * attenColor;
///////// Gloss:
				float gloss = exp2(0.5*10.0+1.0);
////// Specular:
				NdotL = max(0.0, NdotL);
				float2 node_1924 = i.uv0;
				float4 node_65 = tex2D(_DotsAlpha,TRANSFORM_TEX(node_1924.rg, _DotsAlpha));
				float4 node_1925 = _Time + _TimeEditor;
				float2 node_69 = (node_1924.rg+node_1925.g*float2(0,0.5));
				float3 node_66 = (node_65.rgb*tex2D(_DotsColor,TRANSFORM_TEX(node_69, _DotsColor)).rgb);
				float3 node_144 = (node_66*_EmissVAlve);
				float3 specularColor = node_144;
				float3 specular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float4 node_3 = tex2D(_node_3,TRANSFORM_TEX(node_1924.rg, _node_3));
				finalColor += diffuseLight * (node_144+node_3.rgb);
				finalColor += specular;
/// Final Color:
				return fixed4(finalColor * (node_65.r+(((1.0-max(0,dot(normalDirection, viewDirection)))*_AlphaValue)+(node_3.g*_AlphaAdd.r))),0);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "ShaderForgeMaterialInspector"
}
