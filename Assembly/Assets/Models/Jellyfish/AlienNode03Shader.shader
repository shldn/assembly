// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32719,y:32712|diff-650-OUT,diffpow-2537-OUT,spec-619-OUT,gloss-620-R,emission-3-RGB,amdfl-635-RGB,amspl-375-RGB,alpha-423-OUT,voffset-665-OUT;n:type:ShaderForge.SFN_Tex2d,id:3,x:33754,y:32486,ptlb:Color,ptin:_Color,tex:a6ef3e0449ffc2b499e33c8dce89c92d,ntxv:2,isnm:False|UVIN-598-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:347,x:33256,y:32975,ptlb:Alpha,ptin:_Alpha,tex:60883891eb6d13f48850170d1b948842,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:375,x:33325,y:32546,ptlb:Spec,ptin:_Spec,tex:47a916abe9a60834c81eaa248716fb54,ntxv:0,isnm:False|UVIN-479-UVOUT;n:type:ShaderForge.SFN_Color,id:388,x:33256,y:33168,ptlb:AlphaValue,ptin:_AlphaValue,glob:False,c1:0.1397059,c2:0.1397059,c3:0.1397059,c4:1;n:type:ShaderForge.SFN_Add,id:423,x:33066,y:33058|A-347-R,B-388-R;n:type:ShaderForge.SFN_Panner,id:479,x:33512,y:32767,spu:0.1,spv:0.1;n:type:ShaderForge.SFN_Panner,id:598,x:33988,y:32433,spu:-0.5,spv:-0.5;n:type:ShaderForge.SFN_Multiply,id:619,x:33083,y:32461|A-620-R,B-375-R;n:type:ShaderForge.SFN_Color,id:620,x:33286,y:32331,ptlb:SpecularValue,ptin:_SpecularValue,glob:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Cubemap,id:635,x:33489,y:32951,ptlb:CubeMap,ptin:_CubeMap,cube:a596436b21c6d484bb9b3b6385e3e666,pvfc:2;n:type:ShaderForge.SFN_Multiply,id:650,x:33128,y:32708|A-1847-OUT,B-654-OUT;n:type:ShaderForge.SFN_Vector1,id:654,x:33332,y:32854,v1:10;n:type:ShaderForge.SFN_Multiply,id:665,x:33107,y:33339|A-672-OUT,B-669-OUT,C-707-OUT;n:type:ShaderForge.SFN_Vector1,id:669,x:33341,y:33491,v1:0.03;n:type:ShaderForge.SFN_NormalVector,id:672,x:33397,y:33297,pt:False;n:type:ShaderForge.SFN_ComponentMask,id:701,x:34207,y:33357,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-1219-UVOUT;n:type:ShaderForge.SFN_Frac,id:703,x:34020,y:33357|IN-701-OUT;n:type:ShaderForge.SFN_Subtract,id:704,x:33811,y:33370|A-703-OUT,B-705-OUT;n:type:ShaderForge.SFN_Vector1,id:705,x:34020,y:33493,v1:0.5;n:type:ShaderForge.SFN_Abs,id:706,x:33635,y:33380|IN-704-OUT;n:type:ShaderForge.SFN_Power,id:707,x:33327,y:33611|VAL-709-OUT,EXP-710-OUT;n:type:ShaderForge.SFN_Vector1,id:708,x:33681,y:33555,v1:10;n:type:ShaderForge.SFN_Multiply,id:709,x:33518,y:33511|A-706-OUT,B-708-OUT;n:type:ShaderForge.SFN_Vector1,id:710,x:33518,y:33709,v1:1;n:type:ShaderForge.SFN_Panner,id:1219,x:34442,y:33363,spu:0,spv:-0.7;n:type:ShaderForge.SFN_Vector3,id:1846,x:33798,y:32699,v1:0,v2:0.04827571,v3:1;n:type:ShaderForge.SFN_Multiply,id:1847,x:33531,y:32578|A-3-RGB,B-1846-OUT;n:type:ShaderForge.SFN_Color,id:2319,x:33489,y:33114,ptlb:node_2319,ptin:_node_2319,glob:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Fresnel,id:2490,x:34043,y:32878;n:type:ShaderForge.SFN_Color,id:2493,x:34070,y:33073,ptlb:node_2493,ptin:_node_2493,glob:False,c1:0.3897059,c2:0.8232253,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:2537,x:33769,y:32993|A-2490-OUT,B-2538-OUT,C-2493-RGB;n:type:ShaderForge.SFN_ValueProperty,id:2538,x:33864,y:33187,ptlb:node_2538,ptin:_node_2538,glob:False,v1:10;proporder:3-347-375-388-620-635-2319-2493-2538;pass:END;sub:END;*/

Shader "Shader Forge/AlienNode03Shader" {
	Properties {
		_Color ("Color", 2D) = "black" {}
		_Alpha ("Alpha", 2D) = "white" {}
		_Spec ("Spec", 2D) = "white" {}
		_AlphaValue ("AlphaValue", Color) = (0.1397059,0.1397059,0.1397059,1)
		_SpecularValue ("SpecularValue", Color) = (1,1,1,1)
		_CubeMap ("CubeMap", Cube) = "_Skybox" {}
		_node_2319 ("node_2319", Color) = (1,1,1,1)
		_node_2493 ("node_2493", Color) = (0.3897059,0.8232253,1,1)
		_node_2538 ("node_2538", Float ) = 10
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
			uniform sampler2D _Color; uniform float4 _Color_ST;
			uniform sampler2D _Alpha; uniform float4 _Alpha_ST;
			uniform sampler2D _Spec; uniform float4 _Spec_ST;
			uniform float4 _AlphaValue;
			uniform float4 _SpecularValue;
			uniform samplerCUBE _CubeMap;
			uniform float4 _node_2493;
			uniform float _node_2538;
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
				float4 node_2566 = _Time + _TimeEditor;
				float2 node_2565 = o.uv0;
				v.vertex.xyz += (v.normal*0.03*pow((abs((frac((node_2565.rg+node_2566.g*float2(0,-0.7)).g)-0.5))*10.0),1.0));
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
				float3 normalDirection =  i.normalDir;
				float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
				float attenuation = 1;
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = pow(max( 0.0, NdotL), ((1.0-max(0,dot(normalDirection, viewDirection)))*_node_2538*_node_2493.rgb)) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
				float4 node_2566 = _Time + _TimeEditor;
				float2 node_2565 = i.uv0;
				float2 node_598 = (node_2565.rg+node_2566.g*float2(-0.5,-0.5));
				float4 node_3 = tex2D(_Color,TRANSFORM_TEX(node_598, _Color));
				float3 emissive = node_3.rgb;
///////// Gloss:
				float4 node_620 = _SpecularValue;
				float gloss = exp2(node_620.r*10.0+1.0);
////// Specular:
				NdotL = max(0.0, NdotL);
				float2 node_479 = (node_2565.rg+node_2566.g*float2(0.1,0.1));
				float4 node_375 = tex2D(_Spec,TRANSFORM_TEX(node_479, _Spec));
				float node_619 = (node_620.r*node_375.r);
				float3 specularColor = float3(node_619,node_619,node_619);
				float3 specularAmb = node_375.rgb * specularColor;
				float3 specular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor + specularAmb;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				diffuseLight += texCUBE(_CubeMap,viewReflectDirection).rgb; // Diffuse Ambient Light
				finalColor += diffuseLight * ((node_3.rgb*float3(0,0.04827571,1))*10.0);
				finalColor += specular;
				finalColor += emissive;
/// Final Color:
				return fixed4(finalColor,(tex2D(_Alpha,TRANSFORM_TEX(node_2565.rg, _Alpha)).r+_AlphaValue.r));
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
			uniform sampler2D _Color; uniform float4 _Color_ST;
			uniform sampler2D _Alpha; uniform float4 _Alpha_ST;
			uniform sampler2D _Spec; uniform float4 _Spec_ST;
			uniform float4 _AlphaValue;
			uniform float4 _SpecularValue;
			uniform float4 _node_2493;
			uniform float _node_2538;
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
				float4 node_2568 = _Time + _TimeEditor;
				float2 node_2567 = o.uv0;
				v.vertex.xyz += (v.normal*0.03*pow((abs((frac((node_2567.rg+node_2568.g*float2(0,-0.7)).g)-0.5))*10.0),1.0));
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
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
				float3 diffuse = pow(max( 0.0, NdotL), ((1.0-max(0,dot(normalDirection, viewDirection)))*_node_2538*_node_2493.rgb)) * attenColor;
///////// Gloss:
				float4 node_620 = _SpecularValue;
				float gloss = exp2(node_620.r*10.0+1.0);
////// Specular:
				NdotL = max(0.0, NdotL);
				float4 node_2568 = _Time + _TimeEditor;
				float2 node_2567 = i.uv0;
				float2 node_479 = (node_2567.rg+node_2568.g*float2(0.1,0.1));
				float4 node_375 = tex2D(_Spec,TRANSFORM_TEX(node_479, _Spec));
				float node_619 = (node_620.r*node_375.r);
				float3 specularColor = float3(node_619,node_619,node_619);
				float3 specular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float2 node_598 = (node_2567.rg+node_2568.g*float2(-0.5,-0.5));
				float4 node_3 = tex2D(_Color,TRANSFORM_TEX(node_598, _Color));
				finalColor += diffuseLight * ((node_3.rgb*float3(0,0.04827571,1))*10.0);
				finalColor += specular;
/// Final Color:
				return fixed4(finalColor * (tex2D(_Alpha,TRANSFORM_TEX(node_2567.rg, _Alpha)).r+_AlphaValue.r),0);
			}
			ENDCG
		}
		Pass {
			Name "ShadowCollector"
			Tags {
				"LightMode"="ShadowCollector"
			}
			
			Fog {Mode Off}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define UNITY_PASS_SHADOWCOLLECTOR
			#define SHADOW_COLLECTOR_PASS
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcollector
			#pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
			#pragma target 3.0
			uniform float4 _TimeEditor;
			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv0 : TEXCOORD0;
			};
			struct VertexOutput {
				V2F_SHADOW_COLLECTOR;
				float4 uv0 : TEXCOORD5;
				float3 normalDir : TEXCOORD6;
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
				float4 node_2570 = _Time + _TimeEditor;
				v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0.rg+node_2570.g*float2(0,-0.7)).g)-0.5))*10.0),1.0));
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				TRANSFER_SHADOW_COLLECTOR(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
				SHADOW_COLLECTOR_FRAGMENT(i)
			}
			ENDCG
		}
		Pass {
			Name "ShadowCaster"
			Tags {
				"LightMode"="ShadowCaster"
			}
			Cull Off
			Offset 1, 1
			
			Fog {Mode Off}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define UNITY_PASS_SHADOWCASTER
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcaster
			#pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
			#pragma target 3.0
			uniform float4 _TimeEditor;
			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv0 : TEXCOORD0;
			};
			struct VertexOutput {
				V2F_SHADOW_CASTER;
				float4 uv0 : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
				float4 node_2572 = _Time + _TimeEditor;
				v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0.rg+node_2572.g*float2(0,-0.7)).g)-0.5))*10.0),1.0));
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "ShaderForgeMaterialInspector"
}
