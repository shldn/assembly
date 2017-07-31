// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32563,y:32650|diff-2511-OUT,diffpow-2375-OUT,emission-2375-OUT,amdfl-2-RGB,alpha-3-A,clip-3-A;n:type:ShaderForge.SFN_Color,id:2,x:33305,y:32523,ptlb:node_2,ptin:_node_2,glob:False,c1:1,c2:0.1102941,c3:0.1102941,c4:1;n:type:ShaderForge.SFN_Tex2d,id:3,x:32973,y:32980,ptlb:node_3,ptin:_node_3,tex:fda171a4de424ef43b343b28297bad46,ntxv:1,isnm:False;n:type:ShaderForge.SFN_ComponentMask,id:2365,x:34485,y:32893,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-2383-UVOUT;n:type:ShaderForge.SFN_Frac,id:2367,x:34289,y:32897|IN-2365-OUT;n:type:ShaderForge.SFN_Subtract,id:2369,x:34042,y:32924|A-2367-OUT,B-2371-OUT;n:type:ShaderForge.SFN_Vector1,id:2371,x:34273,y:33061,v1:0.5;n:type:ShaderForge.SFN_Abs,id:2373,x:33827,y:32900|IN-2369-OUT;n:type:ShaderForge.SFN_Power,id:2375,x:33326,y:32985|VAL-2379-OUT,EXP-2381-OUT;n:type:ShaderForge.SFN_Vector1,id:2377,x:33788,y:33047,v1:10;n:type:ShaderForge.SFN_Multiply,id:2379,x:33560,y:32935|A-2373-OUT,B-2377-OUT;n:type:ShaderForge.SFN_Vector1,id:2381,x:33501,y:33088,v1:8;n:type:ShaderForge.SFN_Panner,id:2383,x:34733,y:32893,spu:0,spv:0.4;n:type:ShaderForge.SFN_Blend,id:2511,x:33036,y:32371,blmd:17,clmp:True|SRC-2-RGB,DST-2375-OUT;proporder:3-2;pass:END;sub:END;*/

Shader "Shader Forge/JellyFishSeed" {
	Properties {
		_node_3 ("node_3", 2D) = "gray" {}
		_node_2 ("node_2", Color) = (1,0.1102941,0.1102941,1)
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
			uniform float4 _node_2;
			uniform sampler2D _node_3; uniform float4 _node_3_ST;
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
				float2 node_2640 = i.uv0;
				float4 node_3 = tex2D(_node_3,TRANSFORM_TEX(node_2640.rg, _node_3));
				clip(node_3.a - 0.5);
				i.normalDir = normalize(i.normalDir);
/////// Normals:
				float3 normalDirection =  i.normalDir;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
////// Lighting:
				float attenuation = 1;
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float4 node_2641 = _Time + _TimeEditor;
				float node_2375 = pow((abs((frac((node_2640.rg+node_2641.g*float2(0,0.4)).g)-0.5))*10.0),8.0);
				float3 diffuse = pow(max( 0.0, NdotL), node_2375) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
				float3 emissive = float3(node_2375,node_2375,node_2375);
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float4 node_2 = _node_2;
				diffuseLight += node_2.rgb; // Diffuse Ambient Light
				finalColor += diffuseLight * saturate(abs(node_2.rgb-node_2375));
				finalColor += emissive;
/// Final Color:
				return fixed4(finalColor,node_3.a);
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
			uniform float4 _node_2;
			uniform sampler2D _node_3; uniform float4 _node_3_ST;
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
				float2 node_2642 = i.uv0;
				float4 node_3 = tex2D(_node_3,TRANSFORM_TEX(node_2642.rg, _node_3));
				clip(node_3.a - 0.5);
				i.normalDir = normalize(i.normalDir);
/////// Normals:
				float3 normalDirection =  i.normalDir;
				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float4 node_2643 = _Time + _TimeEditor;
				float node_2375 = pow((abs((frac((node_2642.rg+node_2643.g*float2(0,0.4)).g)-0.5))*10.0),8.0);
				float3 diffuse = pow(max( 0.0, NdotL), node_2375) * attenColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float4 node_2 = _node_2;
				finalColor += diffuseLight * saturate(abs(node_2.rgb-node_2375));
/// Final Color:
				return fixed4(finalColor * node_3.a,0);
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
			uniform sampler2D _node_3; uniform float4 _node_3_ST;
			struct VertexInput {
				float4 vertex : POSITION;
				float4 uv0 : TEXCOORD0;
			};
			struct VertexOutput {
				V2F_SHADOW_COLLECTOR;
				float4 uv0 : TEXCOORD5;
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_SHADOW_COLLECTOR(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				float2 node_2644 = i.uv0;
				float4 node_3 = tex2D(_node_3,TRANSFORM_TEX(node_2644.rg, _node_3));
				clip(node_3.a - 0.5);
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
			uniform sampler2D _node_3; uniform float4 _node_3_ST;
			struct VertexInput {
				float4 vertex : POSITION;
				float4 uv0 : TEXCOORD0;
			};
			struct VertexOutput {
				V2F_SHADOW_CASTER;
				float4 uv0 : TEXCOORD1;
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				float2 node_2645 = i.uv0;
				float4 node_3 = tex2D(_node_3,TRANSFORM_TEX(node_2645.rg, _node_3));
				clip(node_3.a - 0.5);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "ShaderForgeMaterialInspector"
}
