// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32560,y:32647|diff-60-OUT,normal-27-RGB,emission-1920-OUT,alpha-96-OUT,voffset-666-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:33522,y:32406,ptlb:ColorMap,ptin:_ColorMap,tex:a187fc0ceba72844f892a4d514fd8d68,ntxv:1,isnm:False;n:type:ShaderForge.SFN_Fresnel,id:8,x:33341,y:32794;n:type:ShaderForge.SFN_Multiply,id:9,x:33146,y:32671|A-34-OUT,B-8-OUT;n:type:ShaderForge.SFN_Multiply,id:20,x:32982,y:32556|A-9-OUT,B-21-OUT;n:type:ShaderForge.SFN_ValueProperty,id:21,x:33157,y:32571,ptlb:ColorVelue,ptin:_ColorVelue,glob:False,v1:3;n:type:ShaderForge.SFN_Tex2d,id:27,x:33078,y:33038,ptlb:node_Normal,ptin:_node_Normal,tex:983442cfb45342b41b45ba9d8ea26b74,ntxv:3,isnm:False;n:type:ShaderForge.SFN_Color,id:33,x:33522,y:32599,ptlb:BasicColor,ptin:_BasicColor,glob:False,c1:1,c2:0.4705882,c3:0.9415821,c4:1;n:type:ShaderForge.SFN_Multiply,id:34,x:33321,y:32520|A-2-RGB,B-33-RGB;n:type:ShaderForge.SFN_Add,id:60,x:32796,y:32463|A-68-OUT,B-20-OUT;n:type:ShaderForge.SFN_Color,id:62,x:33214,y:32335,ptlb:OverlyColor,ptin:_OverlyColor,glob:False,c1:0,c2:0.2965517,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:68,x:32942,y:32319|A-1821-OUT,B-70-OUT;n:type:ShaderForge.SFN_ValueProperty,id:70,x:33114,y:32494,ptlb:OverleyColorVelue,ptin:_OverleyColorVelue,glob:False,v1:2;n:type:ShaderForge.SFN_Add,id:96,x:33135,y:32855|A-8-OUT,B-97-R;n:type:ShaderForge.SFN_Color,id:97,x:33341,y:32963,ptlb:AlphaColor,ptin:_AlphaColor,glob:False,c1:0.1470588,c2:0.1286765,c3:0.1286765,c4:1;n:type:ShaderForge.SFN_Multiply,id:666,x:33078,y:33222|A-673-OUT,B-670-OUT,C-712-OUT;n:type:ShaderForge.SFN_Vector1,id:670,x:33405,y:33555,v1:0.03;n:type:ShaderForge.SFN_NormalVector,id:673,x:33461,y:33361,pt:False;n:type:ShaderForge.SFN_ComponentMask,id:702,x:34271,y:33421,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-1220-UVOUT;n:type:ShaderForge.SFN_Frac,id:704,x:34084,y:33421|IN-702-OUT;n:type:ShaderForge.SFN_Subtract,id:706,x:33875,y:33434|A-704-OUT,B-708-OUT;n:type:ShaderForge.SFN_Vector1,id:708,x:34084,y:33557,v1:0.5;n:type:ShaderForge.SFN_Abs,id:710,x:33699,y:33444|IN-706-OUT;n:type:ShaderForge.SFN_Power,id:712,x:33391,y:33675|VAL-716-OUT,EXP-718-OUT;n:type:ShaderForge.SFN_Vector1,id:714,x:33745,y:33619,v1:1;n:type:ShaderForge.SFN_Multiply,id:716,x:33582,y:33575|A-710-OUT,B-714-OUT;n:type:ShaderForge.SFN_Vector1,id:718,x:33582,y:33773,v1:0.01;n:type:ShaderForge.SFN_Panner,id:1220,x:34507,y:33421,spu:0,spv:1;n:type:ShaderForge.SFN_Tex2d,id:1815,x:33661,y:32062,ptlb:Dots,ptin:_Dots,tex:60883891eb6d13f48850170d1b948842,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:1816,x:33214,y:32141|A-1815-R,B-1937-OUT;n:type:ShaderForge.SFN_Tex2d,id:1819,x:33762,y:32259,ptlb:ColoSwitch,ptin:_ColoSwitch,tex:a6ef3e0449ffc2b499e33c8dce89c92d,ntxv:0,isnm:False|UVIN-1820-UVOUT;n:type:ShaderForge.SFN_Panner,id:1820,x:33924,y:32173,spu:1,spv:1.2;n:type:ShaderForge.SFN_Multiply,id:1821,x:33044,y:32177|A-1816-OUT,B-62-RGB;n:type:ShaderForge.SFN_Multiply,id:1920,x:32825,y:32690|A-1816-OUT,B-1922-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1922,x:32982,y:32804,ptlb:DotShiningValue,ptin:_DotShiningValue,glob:False,v1:0.6;n:type:ShaderForge.SFN_Multiply,id:1937,x:33545,y:32259|A-1819-RGB,B-1939-RGB;n:type:ShaderForge.SFN_Color,id:1939,x:33762,y:32429,ptlb:node_ColorChange,ptin:_node_ColorChange,glob:False,c1:0.9531441,c2:1,c3:0.5147059,c4:1;proporder:2-21-27-33-62-70-97-1815-1819-1922-1939;pass:END;sub:END;*/

Shader "Shader Forge/JellyFishTail02" {
	Properties {
		_ColorMap ("ColorMap", 2D) = "gray" {}
		_ColorVelue ("ColorVelue", Float ) = 3
		_node_Normal ("node_Normal", 2D) = "bump" {}
		_BasicColor ("BasicColor", Color) = (1,0.4705882,0.9415821,1)
		_OverlyColor ("OverlyColor", Color) = (0,0.2965517,1,1)
		_OverleyColorVelue ("OverleyColorVelue", Float ) = 2
		_AlphaColor ("AlphaColor", Color) = (0.1470588,0.1286765,0.1286765,1)
		_Dots ("Dots", 2D) = "white" {}
		_ColoSwitch ("ColoSwitch", 2D) = "white" {}
		_DotShiningValue ("DotShiningValue", Float ) = 0.6
		_node_ColorChange ("node_ColorChange", Color) = (0.9531441,1,0.5147059,1)
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
			uniform sampler2D _ColorMap; uniform float4 _ColorMap_ST;
			uniform float _ColorVelue;
			uniform sampler2D _node_Normal; uniform float4 _node_Normal_ST;
			uniform float4 _BasicColor;
			uniform float4 _OverlyColor;
			uniform float _OverleyColorVelue;
			uniform float4 _AlphaColor;
			uniform sampler2D _Dots; uniform float4 _Dots_ST;
			uniform sampler2D _ColoSwitch; uniform float4 _ColoSwitch_ST;
			uniform float _DotShiningValue;
			uniform float4 _node_ColorChange;
			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 uv0 : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
				float3 tangentDir : TEXCOORD3;
				float3 binormalDir : TEXCOORD4;
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
				o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
				o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				float4 node_2109 = _Time + _TimeEditor;
				float2 node_2108 = o.uv0;
				v.vertex.xyz += (v.normal*0.03*pow((abs((frac((node_2108.rg+node_2109.g*float2(0,1)).g)-0.5))*1.0),0.01));
				o.posWorld = mul(_Object2World, v.vertex);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
				float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
				float2 node_2108 = i.uv0;
				float3 normalLocal = tex2D(_node_Normal,TRANSFORM_TEX(node_2108.rg, _node_Normal)).rgb;
				float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
////// Lighting:
				float attenuation = 1;
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = max( 0.0, NdotL) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
				float4 node_2109 = _Time + _TimeEditor;
				float2 node_1820 = (node_2108.rg+node_2109.g*float2(1,1.2));
				float3 node_1816 = (tex2D(_Dots,TRANSFORM_TEX(node_2108.rg, _Dots)).r*(tex2D(_ColoSwitch,TRANSFORM_TEX(node_1820, _ColoSwitch)).rgb*_node_ColorChange.rgb));
				float3 emissive = (node_1816*_DotShiningValue);
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float node_8 = (1.0-max(0,dot(normalDirection, viewDirection)));
				finalColor += diffuseLight * (((node_1816*_OverlyColor.rgb)*_OverleyColorVelue)+(((tex2D(_ColorMap,TRANSFORM_TEX(node_2108.rg, _ColorMap)).rgb*_BasicColor.rgb)*node_8)*_ColorVelue));
				finalColor += emissive;
/// Final Color:
				return fixed4(finalColor,(node_8+_AlphaColor.r));
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
			uniform sampler2D _ColorMap; uniform float4 _ColorMap_ST;
			uniform float _ColorVelue;
			uniform sampler2D _node_Normal; uniform float4 _node_Normal_ST;
			uniform float4 _BasicColor;
			uniform float4 _OverlyColor;
			uniform float _OverleyColorVelue;
			uniform float4 _AlphaColor;
			uniform sampler2D _Dots; uniform float4 _Dots_ST;
			uniform sampler2D _ColoSwitch; uniform float4 _ColoSwitch_ST;
			uniform float _DotShiningValue;
			uniform float4 _node_ColorChange;
			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 uv0 : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
				float3 tangentDir : TEXCOORD3;
				float3 binormalDir : TEXCOORD4;
				LIGHTING_COORDS(5,6)
			};
			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.uv0 = v.uv0;
				o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
				o.tangentDir = normalize( mul( _Object2World, float4( v.tangent.xyz, 0.0 ) ).xyz );
				o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				float4 node_2111 = _Time + _TimeEditor;
				float2 node_2110 = o.uv0;
				v.vertex.xyz += (v.normal*0.03*pow((abs((frac((node_2110.rg+node_2111.g*float2(0,1)).g)-0.5))*1.0),0.01));
				o.posWorld = mul(_Object2World, v.vertex);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o)
				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR {
				i.normalDir = normalize(i.normalDir);
				float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
				float2 node_2110 = i.uv0;
				float3 normalLocal = tex2D(_node_Normal,TRANSFORM_TEX(node_2110.rg, _node_Normal)).rgb;
				float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
////// Lighting:
				float attenuation = LIGHT_ATTENUATION(i);
				float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
				float NdotL = dot( normalDirection, lightDirection );
				float3 diffuse = max( 0.0, NdotL) * attenColor;
				float3 finalColor = 0;
				float3 diffuseLight = diffuse;
				float4 node_2111 = _Time + _TimeEditor;
				float2 node_1820 = (node_2110.rg+node_2111.g*float2(1,1.2));
				float3 node_1816 = (tex2D(_Dots,TRANSFORM_TEX(node_2110.rg, _Dots)).r*(tex2D(_ColoSwitch,TRANSFORM_TEX(node_1820, _ColoSwitch)).rgb*_node_ColorChange.rgb));
				float node_8 = (1.0-max(0,dot(normalDirection, viewDirection)));
				finalColor += diffuseLight * (((node_1816*_OverlyColor.rgb)*_OverleyColorVelue)+(((tex2D(_ColorMap,TRANSFORM_TEX(node_2110.rg, _ColorMap)).rgb*_BasicColor.rgb)*node_8)*_ColorVelue));
/// Final Color:
				return fixed4(finalColor * (node_8+_AlphaColor.r),0);
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
				o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
				float4 node_2113 = _Time + _TimeEditor;
				v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0.rg+node_2113.g*float2(0,1)).g)-0.5))*1.0),0.01));
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
				o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
				float4 node_2115 = _Time + _TimeEditor;
				v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0.rg+node_2115.g*float2(0,1)).g)-0.5))*1.0),0.01));
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
