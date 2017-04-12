// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32503,y:32653|diff-95-OUT,spec-13-OUT,gloss-13-OUT,normal-144-OUT,emission-178-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:33111,y:32634,ptlb:MainTexture,ptin:_MainTexture,tex:cefdeecf10688924fa9bf48caa71782a,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:13,x:32846,y:32701|A-2-G,B-14-OUT;n:type:ShaderForge.SFN_Vector1,id:14,x:33111,y:32803,v1:1.5;n:type:ShaderForge.SFN_Fresnel,id:94,x:33278,y:32839;n:type:ShaderForge.SFN_Add,id:95,x:32846,y:32850|A-2-RGB,B-101-OUT;n:type:ShaderForge.SFN_Multiply,id:101,x:33010,y:32907|A-94-OUT,B-102-RGB;n:type:ShaderForge.SFN_Color,id:102,x:33278,y:32986,ptlb:FresnalColor,ptin:_FresnalColor,glob:False,c1:0.2413797,c2:0,c3:1,c4:1;n:type:ShaderForge.SFN_Color,id:118,x:33121,y:33050,ptlb:node_118,ptin:_node_118,glob:False,c1:0.02890463,c2:0.8382353,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:124,x:32846,y:33035|A-118-RGB,B-126-OUT;n:type:ShaderForge.SFN_ValueProperty,id:126,x:33174,y:33223,ptlb:AmbientValve,ptin:_AmbientValve,glob:False,v1:10;n:type:ShaderForge.SFN_Tex2d,id:137,x:32846,y:33209,ptlb:node_Normal,ptin:_node_Normal,tex:d0a0d1f2363c65a468068bba31b846a2,ntxv:3,isnm:True|UVIN-152-OUT;n:type:ShaderForge.SFN_TexCoord,id:138,x:33256,y:33274,uv:0;n:type:ShaderForge.SFN_Multiply,id:144,x:32639,y:33193|A-137-RGB,B-146-OUT;n:type:ShaderForge.SFN_ValueProperty,id:146,x:32846,y:33407,ptlb:NormalValve,ptin:_NormalValve,glob:False,v1:8;n:type:ShaderForge.SFN_Multiply,id:152,x:33031,y:33274|A-138-UVOUT,B-164-OUT;n:type:ShaderForge.SFN_ValueProperty,id:164,x:33256,y:33435,ptlb:NormalTiling,ptin:_NormalTiling,glob:False,v1:2;n:type:ShaderForge.SFN_Tex2d,id:177,x:32965,y:32460,ptlb:node_177,ptin:_node_177,tex:d0a0d1f2363c65a468068bba31b846a2,ntxv:3,isnm:True|UVIN-189-UVOUT;n:type:ShaderForge.SFN_Multiply,id:178,x:32800,y:32555|A-177-R,B-124-OUT;n:type:ShaderForge.SFN_Panner,id:189,x:33166,y:32423,spu:0.1,spv:0.1|UVIN-201-OUT;n:type:ShaderForge.SFN_TexCoord,id:200,x:33653,y:32331,uv:1;n:type:ShaderForge.SFN_Multiply,id:201,x:33392,y:32423|A-200-UVOUT,B-203-OUT;n:type:ShaderForge.SFN_ValueProperty,id:203,x:33653,y:32528,ptlb:node_203,ptin:_node_203,glob:False,v1:1;proporder:2-102-118-126-137-146-164-177-203;pass:END;sub:END;*/

Shader "Shader Forge/JellyFishWingFersnal" {
	Properties {
		_MainTexture ("MainTexture", 2D) = "white" {}
		_FresnalColor ("FresnalColor", Color) = (0.2413797,0,1,1)
		_node_118 ("node_118", Color) = (0.02890463,0.8382353,0,1)
		_AmbientValve ("AmbientValve", Float ) = 10
		_node_Normal ("node_Normal", 2D) = "bump" {}
		_NormalValve ("NormalValve", Float ) = 8
		_NormalTiling ("NormalTiling", Float ) = 2
		_node_177 ("node_177", 2D) = "bump" {}
		_node_203 ("node_203", Float ) = 1
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
			uniform sampler2D _MainTexture; uniform float4 _MainTexture_ST;
			uniform float4 _FresnalColor;
			uniform float4 _node_118;
			uniform float _AmbientValve;
			uniform sampler2D _node_Normal; uniform float4 _node_Normal_ST;
			uniform float _NormalValve;
			uniform float _NormalTiling;
			uniform sampler2D _node_177; uniform float4 _node_177_ST;
			uniform float _node_203;
			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				float4 posWorld : TEXCOORD2;
				float3 normalDir : TEXCOORD3;
				float3 tangentDir : TEXCOORD4;
				float3 binormalDir : TEXCOORD5;
				LIGHTING_COORDS(6,7)
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.uv1 = v.uv1;
				o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
				o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
				o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
				float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
				float2 node_152 = (i.uv0.rg*_NormalTiling);
				float3 normalLocal = (UnpackNormal(tex2D(_node_Normal,TRANSFORM_TEX(node_152, _node_Normal))).rgb*_NormalValve);
				float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = max( 0.0, NdotL) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
				float4 node_224 = _Time + _TimeEditor;
				float2 node_189 = ((i.uv1.rg*_node_203)+node_224.g*float2(0.1,0.1));
				float3 emissive = (UnpackNormal(tex2D(_node_177,TRANSFORM_TEX(node_189, _node_177))).r*(_node_118.rgb*_AmbientValve));
///////// Gloss:
				float2 node_223 = i.uv0;
				float4 node_2 = tex2D(_MainTexture,TRANSFORM_TEX(node_223.rg, _MainTexture));
				float node_13 = (node_2.g*1.5);
				float gloss = exp2(node_13*10.0+1.0);
////// Specular:
				NdotL = max(0.0, NdotL);
				float3 specularColor = float3(node_13,node_13,node_13);
				float3 specular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				finalColor += diffuseLight * (node_2.rgb+((1.0-max(0,dot(normalDirection, viewDirection)))*_FresnalColor.rgb));
				finalColor += specular;
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
			uniform sampler2D _MainTexture; uniform float4 _MainTexture_ST;
			uniform float4 _FresnalColor;
			uniform float4 _node_118;
			uniform float _AmbientValve;
			uniform sampler2D _node_Normal; uniform float4 _node_Normal_ST;
			uniform float _NormalValve;
			uniform float _NormalTiling;
			uniform sampler2D _node_177; uniform float4 _node_177_ST;
			uniform float _node_203;
			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				float4 posWorld : TEXCOORD2;
				float3 normalDir : TEXCOORD3;
				float3 tangentDir : TEXCOORD4;
				float3 binormalDir : TEXCOORD5;
				LIGHTING_COORDS(6,7)
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.uv1 = v.uv1;
				o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
				o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
				o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
				float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
				float2 node_152 = (i.uv0.rg*_NormalTiling);
				float3 normalLocal = (UnpackNormal(tex2D(_node_Normal,TRANSFORM_TEX(node_152, _node_Normal))).rgb*_NormalValve);
				float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
				float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = max( 0.0, NdotL) * attenColor;
///////// Gloss:
				float2 node_225 = i.uv0;
				float4 node_2 = tex2D(_MainTexture,TRANSFORM_TEX(node_225.rg, _MainTexture));
				float node_13 = (node_2.g*1.5);
				float gloss = exp2(node_13*10.0+1.0);
////// Specular:
				NdotL = max(0.0, NdotL);
				float3 specularColor = float3(node_13,node_13,node_13);
				float3 specular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				finalColor += diffuseLight * (node_2.rgb+((1.0-max(0,dot(normalDirection, viewDirection)))*_FresnalColor.rgb));
				finalColor += specular;
/// Final Color:
				return fixed4(finalColor * 1,0);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "ShaderForgeMaterialInspector"
}
