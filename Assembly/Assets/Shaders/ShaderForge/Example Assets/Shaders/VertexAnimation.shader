// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:True,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:31696,y:32397|diff-9097-RGB,diffpow-9097-RGB,spec-8778-OUT,emission-8778-OUT,alpha-9741-OUT;n:type:ShaderForge.SFN_Panner,id:23,x:32638,y:32415,spu:0,spv:1;n:type:ShaderForge.SFN_Tex2d,id:8763,x:32299,y:32506,ptlb:Specular,ptin:_Specular,tex:47a916abe9a60834c81eaa248716fb54,ntxv:0,isnm:False|UVIN-23-UVOUT;n:type:ShaderForge.SFN_Multiply,id:8778,x:32003,y:32471|A-8763-RGB,B-8878-OUT;n:type:ShaderForge.SFN_Vector1,id:8878,x:32155,y:32593,v1:0.5;n:type:ShaderForge.SFN_Tex2d,id:9097,x:32445,y:32323,ptlb:Color,ptin:_Color,tex:a6ef3e0449ffc2b499e33c8dce89c92d,ntxv:0,isnm:False|UVIN-23-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:9124,x:32269,y:32689,ptlb:Alpha,ptin:_Alpha,tex:0bb69b8b28be8e948b7c3f98d181f674,ntxv:3,isnm:False;n:type:ShaderForge.SFN_Color,id:9739,x:32297,y:32878,ptlb:Alphacolor,ptin:_Alphacolor,glob:False,c1:0.1323529,c2:0.1323529,c3:0.1323529,c4:1;n:type:ShaderForge.SFN_Add,id:9741,x:32087,y:32793|A-9124-R,B-9739-B;proporder:8763-9097-9124-9739;pass:END;sub:END;*/

Shader "Shader Forge/Examples/Vertex Animation" {
    Properties {
        _Specular ("Specular", 2D) = "white" {}
        _Color ("Color", 2D) = "white" {}
        _Alpha ("Alpha", 2D) = "bump" {}
        _Alphacolor ("Alphacolor", Color) = (0.1323529,0.1323529,0.1323529,1)
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
            #pragma exclude_renderers xbox360 ps3 flash 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _TimeEditor;
            uniform sampler2D _Specular; uniform float4 _Specular_ST;
            uniform sampler2D _Color; uniform float4 _Color_ST;
            uniform sampler2D _Alpha; uniform float4 _Alpha_ST;
            uniform float4 _Alphacolor;
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
                float3 shLight : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.uv0;
                o.shLight = ShadeSH9(float4(v.normal * unity_Scale.w,1)) * 0.5;
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
                float4 node_9945 = _Time + _TimeEditor;
                float2 node_9944 = i.uv0;
                float2 node_23 = (node_9944.rg+node_9945.g*float2(0,1));
                float4 node_9097 = tex2D(_Color,TRANSFORM_TEX(node_23, _Color));
                float3 diffuse = pow(max( 0.0, NdotL), node_9097.rgb) * attenColor;
////// Emissive:
                float3 node_8778 = (tex2D(_Specular,TRANSFORM_TEX(node_23, _Specular)).rgb*0.5);
                float3 emissive = node_8778;
///////// Gloss:
                float gloss = exp2(0.5*10.0+1.0);
////// Specular:
                NdotL = max(0.0, NdotL);
                float3 specularColor = node_8778;
                float3 specular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                diffuseLight += i.shLight; // Per-Vertex Light Probes / Spherical harmonics
                finalColor += diffuseLight * node_9097.rgb;
                finalColor += specular;
                finalColor += emissive;
/// Final Color:
                return fixed4(finalColor,(tex2D(_Alpha,TRANSFORM_TEX(node_9944.rg, _Alpha)).r+_Alphacolor.b));
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
            #pragma exclude_renderers xbox360 ps3 flash 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _TimeEditor;
            uniform sampler2D _Specular; uniform float4 _Specular_ST;
            uniform sampler2D _Color; uniform float4 _Color_ST;
            uniform sampler2D _Alpha; uniform float4 _Alpha_ST;
            uniform float4 _Alphacolor;
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
                float4 node_9947 = _Time + _TimeEditor;
                float2 node_9946 = i.uv0;
                float2 node_23 = (node_9946.rg+node_9947.g*float2(0,1));
                float4 node_9097 = tex2D(_Color,TRANSFORM_TEX(node_23, _Color));
                float3 diffuse = pow(max( 0.0, NdotL), node_9097.rgb) * attenColor;
///////// Gloss:
                float gloss = exp2(0.5*10.0+1.0);
////// Specular:
                NdotL = max(0.0, NdotL);
                float3 node_8778 = (tex2D(_Specular,TRANSFORM_TEX(node_23, _Specular)).rgb*0.5);
                float3 specularColor = node_8778;
                float3 specular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                finalColor += diffuseLight * node_9097.rgb;
                finalColor += specular;
/// Final Color:
                return fixed4(finalColor * (tex2D(_Alpha,TRANSFORM_TEX(node_9946.rg, _Alpha)).r+_Alphacolor.b),0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
