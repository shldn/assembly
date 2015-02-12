// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32630,y:32673|diff-68-OUT,emission-89-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:33521,y:32616,ptlb:node_2,ptin:_node_2,tex:086c017932a88a948a5d7eaccebbcadb,ntxv:0,isnm:False|UVIN-3-UVOUT;n:type:ShaderForge.SFN_Panner,id:3,x:33712,y:32616,spu:0,spv:0.5;n:type:ShaderForge.SFN_Color,id:57,x:33521,y:32797,ptlb:node_57,ptin:_node_57,glob:False,c1:0.294388,c2:0.7911705,c3:0.8897059,c4:1;n:type:ShaderForge.SFN_Add,id:68,x:33296,y:32663|A-2-RGB,B-57-RGB;n:type:ShaderForge.SFN_Multiply,id:89,x:33122,y:32873|A-2-RGB,B-90-OUT;n:type:ShaderForge.SFN_ValueProperty,id:90,x:33319,y:32927,ptlb:ColoValue,ptin:_ColoValue,glob:False,v1:0.2;proporder:2-57-90;pass:END;sub:END;*/

Shader "Shader Forge/JellyFishTailTip02" {
	Properties {
		_node_2 ("node_2", 2D) = "white" {}
		_node_57 ("node_57", Color) = (0.294388,0.7911705,0.8897059,1)
		_ColoValue ("ColoValue", Float ) = 0.2
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
			uniform float4 _node_57;
			uniform float _ColoValue;
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
/////// Normals:
				float3 normalDirection =  i.normalDir;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = max( 0.0, NdotL) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
				float4 node_158 = _Time + _TimeEditor;
				float2 node_3 = (i.uv0.rg+node_158.g*float2(0,0.5));
				float4 node_2 = tex2D(_node_2,TRANSFORM_TEX(node_3, _node_2));
				float3 emissive = (node_2.rgb*_ColoValue);
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				finalColor += diffuseLight * (node_2.rgb+_node_57.rgb);
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
			uniform float4 _node_57;
			uniform float _ColoValue;
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
				float4 node_160 = _Time + _TimeEditor;
				float2 node_3 = (i.uv0.rg+node_160.g*float2(0,0.5));
				float4 node_2 = tex2D(_node_2,TRANSFORM_TEX(node_3, _node_2));
				finalColor += diffuseLight * (node_2.rgb+_node_57.rgb);
/// Final Color:
				return fixed4(finalColor * 1,0);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "ShaderForgeMaterialInspector"
}
