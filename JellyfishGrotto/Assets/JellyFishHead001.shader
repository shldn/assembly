// Shader created with Shader Forge Beta 0.26 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.26;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.3386678,fgcg:0.4978404,fgcb:0.5294118,fgca:1,fgde:0.001,fgrn:1.3,fgrf:347.54,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32591,y:32679|diff-8-OUT,diffpow-1856-OUT,spec-193-OUT,normal-160-RGB,emission-71-OUT,voffset-666-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:33554,y:32605,ptlb:BasicMap,ptin:_BasicMap,tex:cefdeecf10688924fa9bf48caa71782a,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:8,x:33204,y:32748|A-2-RGB,B-10-OUT;n:type:ShaderForge.SFN_Tex2d,id:9,x:33743,y:32492,ptlb:ColorChange,ptin:_ColorChange,tex:086c017932a88a948a5d7eaccebbcadb,ntxv:0,isnm:False|UVIN-17-UVOUT;n:type:ShaderForge.SFN_Multiply,id:10,x:33422,y:32838|A-9-RGB,B-1538-OUT;n:type:ShaderForge.SFN_Panner,id:17,x:33816,y:32852,spu:0,spv:-0.3;n:type:ShaderForge.SFN_Fresnel,id:60,x:33234,y:33057|EXP-139-OUT;n:type:ShaderForge.SFN_Multiply,id:71,x:33003,y:32944|A-112-RGB,B-60-OUT;n:type:ShaderForge.SFN_Color,id:112,x:33234,y:32911,ptlb:FresnelColor,ptin:_FresnelColor,glob:False,c1:0,c2:0.3232759,c3:0.5514706,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:139,x:33418,y:33135,ptlb:Fresnel_Range,ptin:_Fresnel_Range,glob:False,v1:1.5;n:type:ShaderForge.SFN_Tex2d,id:160,x:32991,y:33117,ptlb:Normal,ptin:_Normal,tex:d0a0d1f2363c65a468068bba31b846a2,ntxv:3,isnm:True;n:type:ShaderForge.SFN_ValueProperty,id:172,x:33184,y:32663,ptlb:SpecularValue,ptin:_SpecularValue,glob:False,v1:5;n:type:ShaderForge.SFN_Multiply,id:193,x:32978,y:32593|A-2-G,B-172-OUT;n:type:ShaderForge.SFN_Multiply,id:666,x:33171,y:33403|A-673-OUT,B-670-OUT,C-712-OUT;n:type:ShaderForge.SFN_Vector1,id:670,x:33405,y:33555,v1:0.03;n:type:ShaderForge.SFN_NormalVector,id:673,x:33461,y:33361,pt:False;n:type:ShaderForge.SFN_ComponentMask,id:702,x:34271,y:33421,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-1220-UVOUT;n:type:ShaderForge.SFN_Frac,id:704,x:34084,y:33421|IN-702-OUT;n:type:ShaderForge.SFN_Subtract,id:706,x:33875,y:33434|A-704-OUT,B-708-OUT;n:type:ShaderForge.SFN_Vector1,id:708,x:34084,y:33557,v1:0.5;n:type:ShaderForge.SFN_Abs,id:710,x:33699,y:33444|IN-706-OUT;n:type:ShaderForge.SFN_Power,id:712,x:33391,y:33675|VAL-716-OUT,EXP-718-OUT;n:type:ShaderForge.SFN_Multiply,id:716,x:33582,y:33575|A-710-OUT,B-1749-OUT;n:type:ShaderForge.SFN_Vector1,id:718,x:33582,y:33773,v1:1;n:type:ShaderForge.SFN_Panner,id:1220,x:34506,y:33427,spu:0,spv:-0.5;n:type:ShaderForge.SFN_ValueProperty,id:1495,x:33849,y:33109,ptlb:ColorValue,ptin:_ColorValue,glob:False,v1:8;n:type:ShaderForge.SFN_Multiply,id:1538,x:33570,y:33047|A-1495-OUT,B-1539-RGB;n:type:ShaderForge.SFN_Color,id:1539,x:33825,y:33191,ptlb:ColorAdd,ptin:_ColorAdd,glob:False,c1:1,c2:0.5586207,c3:0,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:1749,x:33859,y:33714,ptlb:BabolValue,ptin:_BabolValue,glob:False,v1:3;n:type:ShaderForge.SFN_Multiply,id:1856,x:33416,y:32397|A-1857-OUT,B-9-RGB;n:type:ShaderForge.SFN_ValueProperty,id:1857,x:33563,y:32343,ptlb:node_1857,ptin:_node_1857,glob:False,v1:4;proporder:2-9-112-139-160-172-1495-1539-1749-1857;pass:END;sub:END;*/

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
                float4 node_1882 = _Time + _TimeEditor;
                float2 node_1881 = o.uv0;
                v.vertex.xyz += (v.normal*0.03*pow((abs((frac((node_1881.rg+node_1882.g*float2(0,-0.5)).g)-0.5))*_BabolValue),1.0));
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
                float2 node_1881 = i.uv0;
                float3 normalLocal = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(node_1881.rg, _Normal))).rgb;
                float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float4 node_1882 = _Time + _TimeEditor;
                float2 node_17 = (node_1881.rg+node_1882.g*float2(0,-0.3));
                float4 node_9 = tex2D(_ColorChange,TRANSFORM_TEX(node_17, _ColorChange));
                float3 diffuse = pow(max( 0.0, NdotL), (_node_1857*node_9.rgb)) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
                float3 emissive = (_FresnelColor.rgb*pow(1.0-max(0,dot(normalDirection, viewDirection)),_Fresnel_Range));
///////// Gloss:
                float gloss = exp2(0.5*10.0+1.0);
////// Specular:
                NdotL = max(0.0, NdotL);
                float4 node_2 = tex2D(_BasicMap,TRANSFORM_TEX(node_1881.rg, _BasicMap));
                float node_193 = (node_2.g*_SpecularValue);
                float3 specularColor = float3(node_193,node_193,node_193);
                float3 specular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                finalColor += diffuseLight * (node_2.rgb*(node_9.rgb*(_ColorValue*_ColorAdd.rgb)));
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
                float4 node_1884 = _Time + _TimeEditor;
                float2 node_1883 = o.uv0;
                v.vertex.xyz += (v.normal*0.03*pow((abs((frac((node_1883.rg+node_1884.g*float2(0,-0.5)).g)-0.5))*_BabolValue),1.0));
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
                float2 node_1883 = i.uv0;
                float3 normalLocal = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(node_1883.rg, _Normal))).rgb;
                float3 normalDirection =  normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float4 node_1884 = _Time + _TimeEditor;
                float2 node_17 = (node_1883.rg+node_1884.g*float2(0,-0.3));
                float4 node_9 = tex2D(_ColorChange,TRANSFORM_TEX(node_17, _ColorChange));
                float3 diffuse = pow(max( 0.0, NdotL), (_node_1857*node_9.rgb)) * attenColor;
///////// Gloss:
                float gloss = exp2(0.5*10.0+1.0);
////// Specular:
                NdotL = max(0.0, NdotL);
                float4 node_2 = tex2D(_BasicMap,TRANSFORM_TEX(node_1883.rg, _BasicMap));
                float node_193 = (node_2.g*_SpecularValue);
                float3 specularColor = float3(node_193,node_193,node_193);
                float3 specular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),gloss) * specularColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                finalColor += diffuseLight * (node_2.rgb*(node_9.rgb*(_ColorValue*_ColorAdd.rgb)));
                finalColor += specular;
/// Final Color:
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
                float4 node_1886 = _Time + _TimeEditor;
                v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0.rg+node_1886.g*float2(0,-0.5)).g)-0.5))*_BabolValue),1.0));
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
            uniform float _BabolValue;
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
                float4 node_1888 = _Time + _TimeEditor;
                v.vertex.xyz += (v.normal*0.03*pow((abs((frac((o.uv0.rg+node_1888.g*float2(0,-0.5)).g)-0.5))*_BabolValue),1.0));
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
