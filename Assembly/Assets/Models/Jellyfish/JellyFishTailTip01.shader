// Shader created with Shader Forge v1.04 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.04;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:2,uamb:True,mssp:True,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:1,culm:0,dpts:2,wrdp:True,dith:2,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:33768,y:32602,varname:node_1,prsc:2|diff-5539-OUT,spec-6271-RGB,emission-1115-OUT;n:type:ShaderForge.SFN_Color,id:2,x:32559,y:32929,ptovrint:False,ptlb:node_MainColor,ptin:_node_MainColor,varname:node_6466,prsc:2,glob:False,c1:0.5588235,c2:0.7626775,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:3,x:33308,y:32131,ptovrint:False,ptlb:node_3,ptin:_node_3,varname:node_3617,prsc:2,tex:086c017932a88a948a5d7eaccebbcadb,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Fresnel,id:9,x:32587,y:32728,varname:node_9,prsc:2|EXP-9388-OUT;n:type:ShaderForge.SFN_Multiply,id:15,x:32847,y:32914,varname:node_15,prsc:2|A-9-OUT,B-2-RGB;n:type:ShaderForge.SFN_Tex2d,id:6271,x:33308,y:32320,ptovrint:False,ptlb:node_6271,ptin:_node_6271,varname:node_6271,prsc:2,tex:f72c90b1784a60a4180eeddb9b5cd61b,ntxv:0,isnm:False|UVIN-2227-OUT;n:type:ShaderForge.SFN_Multiply,id:5539,x:33736,y:32266,varname:node_5539,prsc:2|A-5831-OUT,B-6271-RGB;n:type:ShaderForge.SFN_Exp,id:9388,x:32391,y:32750,varname:node_9388,prsc:2,et:0|IN-5668-OUT;n:type:ShaderForge.SFN_Vector1,id:5668,x:32213,y:32750,varname:node_5668,prsc:2,v1:0;n:type:ShaderForge.SFN_Tex2d,id:9579,x:33382,y:32884,ptovrint:False,ptlb:node_9579,ptin:_node_9579,varname:node_9579,prsc:2,tex:78fb5a3d1ec12fd4d90444f1cb7409be,ntxv:0,isnm:False|UVIN-2227-OUT;n:type:ShaderForge.SFN_Blend,id:5831,x:33576,y:32154,varname:node_5831,prsc:2,blmd:10,clmp:True|SRC-3-RGB,DST-6271-RGB;n:type:ShaderForge.SFN_Add,id:6082,x:33577,y:32901,varname:node_6082,prsc:2|A-9579-B,B-9579-G;n:type:ShaderForge.SFN_TexCoord,id:6038,x:32980,y:32169,varname:node_6038,prsc:2,uv:1;n:type:ShaderForge.SFN_Multiply,id:2227,x:33121,y:32320,varname:node_2227,prsc:2|A-6038-UVOUT,B-4696-OUT;n:type:ShaderForge.SFN_Vector1,id:4696,x:32907,y:32354,varname:node_4696,prsc:2,v1:5;n:type:ShaderForge.SFN_Blend,id:1115,x:32770,y:32687,varname:node_1115,prsc:2,blmd:1,clmp:True|SRC-9-OUT,DST-2-RGB;proporder:2-3-6271-9579;pass:END;sub:END;*/

Shader "Shader Forge/JellyFishTailTip01_2" {
    Properties {
        _node_MainColor ("node_MainColor", Color) = (0.5588235,0.7626775,1,1)
        _node_3 ("node_3", 2D) = "white" {}
        _node_6271 ("node_6271", 2D) = "white" {}
        _node_9579 ("node_9579", 2D) = "white" {}
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
            uniform float4 _node_MainColor;
            uniform sampler2D _node_3; uniform float4 _node_3_ST;
            uniform sampler2D _node_6271; uniform float4 _node_6271_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                LIGHTING_COORDS(4,5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = mul(_Object2World, float4(v.normal,0)).xyz;
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float2 node_2227 = (i.uv1*5.0);
                float4 _node_6271_var = tex2D(_node_6271,TRANSFORM_TEX(node_2227, _node_6271));
                float3 specularColor = _node_6271_var.rgb;
                float3 directSpecular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(reflect(-lightDirection, normalDirection),viewDirection)),specPow);
                float3 specular = directSpecular * specularColor;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 indirectDiffuse = float3(0,0,0);
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float4 _node_3_var = tex2D(_node_3,TRANSFORM_TEX(i.uv0, _node_3));
                float3 diffuse = (directDiffuse + indirectDiffuse) * (saturate(( _node_6271_var.rgb > 0.5 ? (1.0-(1.0-2.0*(_node_6271_var.rgb-0.5))*(1.0-_node_3_var.rgb)) : (2.0*_node_6271_var.rgb*_node_3_var.rgb) ))*_node_6271_var.rgb);
////// Emissive:
                float node_9 = pow(1.0-max(0,dot(normalDirection, viewDirection)),exp(0.0));
                float3 emissive = saturate((node_9*_node_MainColor.rgb));
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
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
            uniform float4 _node_MainColor;
            uniform sampler2D _node_3; uniform float4 _node_3_ST;
            uniform sampler2D _node_6271; uniform float4 _node_6271_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                LIGHTING_COORDS(4,5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = mul(_Object2World, float4(v.normal,0)).xyz;
                o.posWorld = mul(_Object2World, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float2 node_2227 = (i.uv1*5.0);
                float4 _node_6271_var = tex2D(_node_6271,TRANSFORM_TEX(node_2227, _node_6271));
                float3 specularColor = _node_6271_var.rgb;
                float3 directSpecular = attenColor * pow(max(0,dot(reflect(-lightDirection, normalDirection),viewDirection)),specPow);
                float3 specular = directSpecular * specularColor;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float4 _node_3_var = tex2D(_node_3,TRANSFORM_TEX(i.uv0, _node_3));
                float3 diffuse = directDiffuse * (saturate(( _node_6271_var.rgb > 0.5 ? (1.0-(1.0-2.0*(_node_6271_var.rgb-0.5))*(1.0-_node_3_var.rgb)) : (2.0*_node_6271_var.rgb*_node_3_var.rgb) ))*_node_6271_var.rgb);
/// Final Color:
                float3 finalColor = diffuse + specular;
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
