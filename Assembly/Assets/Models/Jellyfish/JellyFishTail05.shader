// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32612,y:32645|diff-2-RGB,spec-140-RGB,emission-142-OUT,alpha-2-A;n:type:ShaderForge.SFN_Tex2d,id:2,x:33708,y:32606,ptlb:node_MainColor,ptin:_node_MainColor,tex:46abc0325e94e63488f9a9345b0e634e,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:105,x:33708,y:32787,ptlb:node_Color,ptin:_node_Color,glob:False,c1:0.09807009,c2:0.5735294,c3:0.05903978,c4:1;n:type:ShaderForge.SFN_Multiply,id:106,x:33471,y:32723|A-2-RGB,B-105-RGB;n:type:ShaderForge.SFN_Multiply,id:127,x:33004,y:32947|A-140-RGB,B-128-OUT;n:type:ShaderForge.SFN_ValueProperty,id:128,x:33202,y:33058,ptlb:node_ColorValue,ptin:_node_ColorValue,glob:False,v1:1;n:type:ShaderForge.SFN_Tex2d,id:140,x:33402,y:33156,ptlb:node_140,ptin:_node_140,tex:47a916abe9a60834c81eaa248716fb54,ntxv:0,isnm:False|UVIN-141-UVOUT;n:type:ShaderForge.SFN_Panner,id:141,x:33605,y:33140,spu:0,spv:-0.2;n:type:ShaderForge.SFN_Multiply,id:142,x:32966,y:32816|A-406-RGB,B-127-OUT;n:type:ShaderForge.SFN_Tex2d,id:406,x:33402,y:32950,ptlb:node_406,ptin:_node_406,tex:20f7cab44eaf6e849a8b7131f390055a,ntxv:0,isnm:False|UVIN-428-UVOUT;n:type:ShaderForge.SFN_Panner,id:428,x:33619,y:32937,spu:0,spv:0.2;proporder:2-105-128-140-406;pass:END;sub:END;*/

Shader "Shader Forge/JellyFishTail05" {
	Properties {
		_node_MainColor ("node_MainColor", 2D) = "white" {}
		_node_Color ("node_Color", Color) = (0.09807009,0.5735294,0.05903978,1)
		_node_ColorValue ("node_ColorValue", Float ) = 1
		_node_140 ("node_140", 2D) = "white" {}
		_node_406 ("node_406", 2D) = "white" {}
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
			uniform sampler2D _node_MainColor; uniform float4 _node_MainColor_ST;
			uniform float _node_ColorValue;
			uniform sampler2D _node_140; uniform float4 _node_140_ST;
			uniform sampler2D _node_406; uniform float4 _node_406_ST;
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
				o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
				o.posWorld = mul(_Object2World, v.vertex);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
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
				float4 node_462 = _Time + _TimeEditor;
				float2 node_461 = i.uv0;
				float2 node_428 = (node_461.rg+node_462.g*float2(0,0.2));
				float2 node_141 = (node_461.rg+node_462.g*float2(0,-0.2));
				float4 node_140 = tex2D(_node_140,TRANSFORM_TEX(node_141, _node_140));
				float3 emissive = (tex2D(_node_406,TRANSFORM_TEX(node_428, _node_406)).rgb*(node_140.rgb*_node_ColorValue));
///////// Gloss:
				float gloss = exp2(0.5*10.0+1.0);
////// Specular:
				NdotL = max(0.0, NdotL);
				float3 specularColor = node_140.rgb;
				float3 specular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float4 node_2 = tex2D(_node_MainColor,TRANSFORM_TEX(node_461.rg, _node_MainColor));
				finalColor += diffuseLight * node_2.rgb;
				finalColor += specular;
				finalColor += emissive;
/// Final Color:
				return fixed4(finalColor,node_2.a);
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
			uniform sampler2D _node_MainColor; uniform float4 _node_MainColor_ST;
			uniform float _node_ColorValue;
			uniform sampler2D _node_140; uniform float4 _node_140_ST;
			uniform sampler2D _node_406; uniform float4 _node_406_ST;
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
				o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
				o.posWorld = mul(_Object2World, v.vertex);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
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
				float4 node_464 = _Time + _TimeEditor;
				float2 node_463 = i.uv0;
				float2 node_141 = (node_463.rg+node_464.g*float2(0,-0.2));
				float4 node_140 = tex2D(_node_140,TRANSFORM_TEX(node_141, _node_140));
				float3 specularColor = node_140.rgb;
				float3 specular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float4 node_2 = tex2D(_node_MainColor,TRANSFORM_TEX(node_463.rg, _node_MainColor));
				finalColor += diffuseLight * node_2.rgb;
				finalColor += specular;
/// Final Color:
				return fixed4(finalColor * node_2.a,0);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "ShaderForgeMaterialInspector"
}
