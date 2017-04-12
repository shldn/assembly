// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32719,y:32712|diff-4-OUT,diffpow-5-OUT,emission-2-R;n:type:ShaderForge.SFN_Tex2d,id:2,x:33458,y:32672,ptlb:node_2,ptin:_node_2,tex:d4cd29cfbd9b5b94599138621a8dfa8b,ntxv:0,isnm:False|UVIN-32-UVOUT;n:type:ShaderForge.SFN_Color,id:3,x:33458,y:32904,ptlb:node_3,ptin:_node_3,glob:False,c1:1,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:4,x:33272,y:32780|A-2-B,B-3-RGB;n:type:ShaderForge.SFN_Multiply,id:5,x:33082,y:32844|A-4-OUT,B-6-OUT;n:type:ShaderForge.SFN_Vector1,id:6,x:33260,y:32993,v1:1;n:type:ShaderForge.SFN_Panner,id:32,x:33676,y:32672,spu:0,spv:0.5;proporder:2-3;pass:END;sub:END;*/

Shader "Shader Forge/Brain" {
	Properties {
		_node_2 ("node_2", 2D) = "white" {}
		_node_3 ("node_3", Color) = (1,0,0,1)
	}
	SubShader {
		Tags {
			"RenderType"="Opaque"
		}
		Pass {
			Name "ForwardBase"
			Tags {
				"LightMode"="ForwardBase"
			}
			
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define UNITY_PASS_FORWARDBASE
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma multi_compile_fwdbase_fullshadows
			#pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
			#pragma target 3.0
			uniform float4 _LightColor0;
			uniform float4 _TimeEditor;
			uniform sampler2D _node_2; uniform float4 _node_2_ST;
			uniform float4 _node_3;
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
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
/////// Normals:
				float3 normalDirection =  i.normalDir;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float4 node_747 = _Time + _TimeEditor;
				float2 node_32 = (i.uv0.rg+node_747.g*float2(0,0.5));
				float4 node_2 = tex2D(_node_2,TRANSFORM_TEX(node_32, _node_2));
				float3 node_4 = (node_2.b*_node_3.rgb);
				float3 diffuse = pow(max( 0.0, NdotL), (node_4*1.0)) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
				float3 emissive = float3(node_2.r,node_2.r,node_2.r);
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				finalColor += diffuseLight * node_4;
				finalColor += emissive;
/// Final Color:
				return fixed4(finalColor,1);
			}
			ENDCG
		}
		Pass {
			Name "ForwardAdd"
			Tags {
				"LightMode"="ForwardAdd"
			}
			Blend One One
			
			
			Fog { Color (0,0,0,0) }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define UNITY_PASS_FORWARDADD
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma multi_compile_fwdadd_fullshadows
			#pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
			#pragma target 3.0
			uniform float4 _LightColor0;
			uniform float4 _TimeEditor;
			uniform sampler2D _node_2; uniform float4 _node_2_ST;
			uniform float4 _node_3;
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
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
/////// Normals:
				float3 normalDirection =  i.normalDir;
				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float4 node_749 = _Time + _TimeEditor;
				float2 node_32 = (i.uv0.rg+node_749.g*float2(0,0.5));
				float4 node_2 = tex2D(_node_2,TRANSFORM_TEX(node_32, _node_2));
				float3 node_4 = (node_2.b*_node_3.rgb);
				float3 diffuse = pow(max( 0.0, NdotL), (node_4*1.0)) * attenColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				finalColor += diffuseLight * node_4;
/// Final Color:
				return fixed4(finalColor * 1,0);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "ShaderForgeMaterialInspector"
}
