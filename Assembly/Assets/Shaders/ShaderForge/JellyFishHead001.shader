// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.04 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.04;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,dith:2,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:34410,y:32679,varname:node_1,prsc:2|diff-8-OUT,diffpow-1856-OUT,spec-193-OUT,normal-160-RGB,emission-71-OUT,voffset-666-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:33516,y:32605,ptovrint:False,ptlb:BasicMap,ptin:_BasicMap,varname:node_649,prsc:2,tex:cefdeecf10688924fa9bf48caa71782a,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:8,x:33866,y:32748,varname:node_8,prsc:2|A-2-RGB,B-10-OUT;n:type:ShaderForge.SFN_Tex2d,id:9,x:33327,y:32492,ptovrint:False,ptlb:ColorChange,ptin:_ColorChange,varname:node_2157,prsc:2,tex:086c017932a88a948a5d7eaccebbcadb,ntxv:0,isnm:False|UVIN-17-UVOUT;n:type:ShaderForge.SFN_Multiply,id:10,x:33648,y:32838,varname:node_10,prsc:2|A-9-RGB,B-1538-OUT;n:type:ShaderForge.SFN_Panner,id:17,x:33254,y:32852,varname:node_17,prsc:2,spu:0,spv:-0.3;n:type:ShaderForge.SFN_Fresnel,id:60,x:33836,y:33057,varname:node_60,prsc:2|EXP-139-OUT;n:type:ShaderForge.SFN_Multiply,id:71,x:34067,y:32944,varname:node_71,prsc:2|A-112-RGB,B-60-OUT;n:type:ShaderForge.SFN_Color,id:112,x:33836,y:32911,ptovrint:False,ptlb:FresnelColor,ptin:_FresnelColor,varname:node_7380,prsc:2,glob:False,c1:0,c2:0.3232759,c3:0.5514706,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:139,x:33652,y:33135,ptovrint:False,ptlb:Fresnel_Range,ptin:_Fresnel_Range,varname:node_9018,prsc:2,glob:False,v1:1.5;n:type:ShaderForge.SFN_Tex2d,id:160,x:34079,y:33117,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:node_4878,prsc:2,tex:d0a0d1f2363c65a468068bba31b846a2,ntxv:3,isnm:True;n:type:ShaderForge.SFN_ValueProperty,id:172,x:33886,y:32663,ptovrint:False,ptlb:SpecularValue,ptin:_SpecularValue,varname:node_8510,prsc:2,glob:False,v1:5;n:type:ShaderForge.SFN_Multiply,id:193,x:34092,y:32593,varname:node_193,prsc:2|A-2-G,B-172-OUT;n:type:ShaderForge.SFN_Multiply,id:666,x:33899,y:33403,varname:node_666,prsc:2|A-673-OUT,B-670-OUT,C-712-OUT;n:type:ShaderForge.SFN_Vector1,id:670,x:33665,y:33555,varname:node_670,prsc:2,v1:0.03;n:type:ShaderForge.SFN_NormalVector,id:673,x:33609,y:33361,prsc:2,pt:True;n:type:ShaderForge.SFN_ComponentMask,id:702,x:32799,y:33421,varname:node_702,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-1220-UVOUT;n:type:ShaderForge.SFN_Frac,id:704,x:32986,y:33421,varname:node_704,prsc:2|IN-702-OUT;n:type:ShaderForge.SFN_Subtract,id:706,x:33195,y:33434,varname:node_706,prsc:2|A-704-OUT,B-708-OUT;n:type:ShaderForge.SFN_Vector1,id:708,x:32986,y:33557,varname:node_708,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Abs,id:710,x:33371,y:33444,varname:node_710,prsc:2|IN-706-OUT;n:type:ShaderForge.SFN_Power,id:712,x:33679,y:33675,varname:node_712,prsc:2|VAL-716-OUT,EXP-718-OUT;n:type:ShaderForge.SFN_Multiply,id:716,x:33488,y:33575,varname:node_716,prsc:2|A-710-OUT,B-1749-OUT;n:type:ShaderForge.SFN_Vector1,id:718,x:33488,y:33773,varname:node_718,prsc:2,v1:1;n:type:ShaderForge.SFN_Panner,id:1220,x:32564,y:33427,varname:node_1220,prsc:2,spu:0,spv:-0.5;n:type:ShaderForge.SFN_ValueProperty,id:1495,x:33221,y:33109,ptovrint:False,ptlb:ColorValue,ptin:_ColorValue,varname:node_5659,prsc:2,glob:False,v1:8;n:type:ShaderForge.SFN_Multiply,id:1538,x:33500,y:33047,varname:node_1538,prsc:2|A-1495-OUT,B-1539-RGB;n:type:ShaderForge.SFN_Color,id:1539,x:33245,y:33191,ptovrint:False,ptlb:ColorAdd,ptin:_ColorAdd,varname:node_9596,prsc:2,glob:False,c1:1,c2:0.5586207,c3:0,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:1749,x:33211,y:33714,ptovrint:False,ptlb:BabolValue,ptin:_BabolValue,varname:node_5709,prsc:2,glob:False,v1:3;n:type:ShaderForge.SFN_Multiply,id:1856,x:33654,y:32397,varname:node_1856,prsc:2|A-1857-OUT,B-9-RGB;n:type:ShaderForge.SFN_ValueProperty,id:1857,x:33507,y:32343,ptovrint:False,ptlb:node_1857,ptin:_node_1857,varname:node_6484,prsc:2,glob:False,v1:4;proporder:2-9-112-139-160-172-1495-1539-1749-1857;pass:END;sub:END;*/

Shader "Shader Forge/JellyFishHead001" {
    Properties {
        _BasicMap ("BasicMap", 2D) = "white" {}
        _ColorChange ("ColorChange", 2D) = "white" {}
        _FresnelColor ("FresnelColor", Color) = (0,0.3232759,0.5514706,1)
        _Fresnel_Range ("Fresnel_Range", Float ) = 1.5
        _Normal ("Normal", 2D) = "bump" {}
        _SpecularValue ("SpecularValue", Float ) = 5
        _ColorValue ("ColorValue", Float ) = 8
        _ColorAdd ("ColorAdd", Color) = (1,0.5586207,0,1)
        _BabolValue ("BabolValue", Float ) = 3
        _node_1857 ("node_1857", Float ) = 4
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
            uniform sampler2D _BasicMap; uniform float4 _BasicMap_ST;
            uniform sampler2D _ColorChange; uniform float4 _ColorChange_ST;
            uniform float4 _FresnelColor;
            uniform float _Fresnel_Range;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform float _SpecularValue;
            uniform float _ColorValue;
            uniform float4 _ColorAdd;
            uniform float _BabolValue;
            uniform float _node_1857;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 binormalDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 node_316 = _Time + _TimeEditor;
                v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0+node_316.g*float2(0,-0.5)).g)-0.5))*_BabolValue),1.0));
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)));
                float3 normalLocal = _Normal_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _BasicMap_var = tex2D(_BasicMap,TRANSFORM_TEX(i.uv0, _BasicMap));
                float node_193 = (_BasicMap_var.g*_SpecularValue);
                float3 specularColor = float3(node_193,node_193,node_193);
                float3 directSpecular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),specPow);
                float3 specular = directSpecular * specularColor;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float4 node_316 = _Time + _TimeEditor;
                float2 node_17 = (i.uv0+node_316.g*float2(0,-0.3));
                float4 _ColorChange_var = tex2D(_ColorChange,TRANSFORM_TEX(node_17, _ColorChange));
                float3 indirectDiffuse = float3(0,0,0);
                float3 directDiffuse = pow(max( 0.0, NdotL), (_node_1857*_ColorChange_var.rgb)) * attenColor;
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 diffuse = (directDiffuse + indirectDiffuse) * (_BasicMap_var.rgb*(_ColorChange_var.rgb*(_ColorValue*_ColorAdd.rgb)));
////// Emissive:
                float3 emissive = (_FresnelColor.rgb*pow(1.0-max(0,dot(normalDirection, viewDirection)),_Fresnel_Range));
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
            uniform float4 _TimeEditor;
            uniform sampler2D _BasicMap; uniform float4 _BasicMap_ST;
            uniform sampler2D _ColorChange; uniform float4 _ColorChange_ST;
            uniform float4 _FresnelColor;
            uniform float _Fresnel_Range;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform float _SpecularValue;
            uniform float _ColorValue;
            uniform float4 _ColorAdd;
            uniform float _BabolValue;
            uniform float _node_1857;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 binormalDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 node_5256 = _Time + _TimeEditor;
                v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0+node_5256.g*float2(0,-0.5)).g)-0.5))*_BabolValue),1.0));
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)));
                float3 normalLocal = _Normal_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = 0.5;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float4 _BasicMap_var = tex2D(_BasicMap,TRANSFORM_TEX(i.uv0, _BasicMap));
                float node_193 = (_BasicMap_var.g*_SpecularValue);
                float3 specularColor = float3(node_193,node_193,node_193);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow);
                float3 specular = directSpecular * specularColor;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float4 node_5256 = _Time + _TimeEditor;
                float2 node_17 = (i.uv0+node_5256.g*float2(0,-0.3));
                float4 _ColorChange_var = tex2D(_ColorChange,TRANSFORM_TEX(node_17, _ColorChange));
                float3 directDiffuse = pow(max( 0.0, NdotL), (_node_1857*_ColorChange_var.rgb)) * attenColor;
                float3 diffuse = directDiffuse * (_BasicMap_var.rgb*(_ColorChange_var.rgb*(_ColorValue*_ColorAdd.rgb)));
/// Final Color:
                float3 finalColor = diffuse + specular;
                return fixed4(finalColor * 1,0);
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
            uniform float _BabolValue;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_COLLECTOR;
                float2 uv0 : TEXCOORD5;
                float3 normalDir : TEXCOORD6;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                float4 node_8162 = _Time + _TimeEditor;
                v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0+node_8162.g*float2(0,-0.5)).g)-0.5))*_BabolValue),1.0));
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_SHADOW_COLLECTOR(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
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
            uniform float _BabolValue;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                float4 node_5420 = _Time + _TimeEditor;
                v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0+node_5420.g*float2(0,-0.5)).g)-0.5))*_BabolValue),1.0));
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
