// Shader created with Shader Forge v1.04 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.04;sub:START;pass:START;ps:flbk:Custom/Silhoutted Hatching,lico:0,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:1,culm:0,dpts:2,wrdp:True,dith:2,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:1,fgcg:0.9813387,fgcb:0.9411765,fgca:1,fgde:0.002,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:5424,x:32719,y:32712,varname:node_5424,prsc:2|diff-4637-OUT;n:type:ShaderForge.SFN_Tex2d,id:1144,x:31812,y:32758,ptovrint:False,ptlb:node_1144,ptin:_node_1144,varname:node_1144,prsc:2,tex:42ee456235b9e764a803bd4ef8a932ed,ntxv:0,isnm:False|UVIN-7910-UVOUT;n:type:ShaderForge.SFN_Color,id:9586,x:32229,y:32580,ptovrint:False,ptlb:node_9586,ptin:_node_9586,varname:node_9586,prsc:2,glob:False,c1:0.2384299,c2:0.4187598,c3:0.7720588,c4:1;n:type:ShaderForge.SFN_Add,id:4637,x:32443,y:32673,varname:node_4637,prsc:2|A-9586-RGB,B-3821-OUT;n:type:ShaderForge.SFN_Panner,id:7910,x:31636,y:32758,varname:node_7910,prsc:2,spu:0.005,spv:0.01|UVIN-9927-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:9927,x:31453,y:32758,varname:node_9927,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:3821,x:31992,y:32856,varname:node_3821,prsc:2|A-1144-RGB,B-9740-OUT;n:type:ShaderForge.SFN_Slider,id:9740,x:31655,y:32950,ptovrint:False,ptlb:node_9740,ptin:_node_9740,varname:node_9740,prsc:2,min:0,cur:0.4150993,max:1;n:type:ShaderForge.SFN_Tex2d,id:1438,x:31812,y:33070,ptovrint:False,ptlb:node_1144_copy,ptin:_node_1144_copy,varname:_node_1144_copy,prsc:2,tex:42ee456235b9e764a803bd4ef8a932ed,ntxv:0,isnm:False|UVIN-3208-UVOUT;n:type:ShaderForge.SFN_Multiply,id:1845,x:31988,y:33162,varname:node_1845,prsc:2|A-1438-RGB,B-4304-OUT;n:type:ShaderForge.SFN_Slider,id:4304,x:31656,y:33380,ptovrint:False,ptlb:node_9740_copy,ptin:_node_9740_copy,varname:_node_9740_copy,prsc:2,min:0,cur:0.5168914,max:1;n:type:ShaderForge.SFN_TexCoord,id:2938,x:31453,y:33061,varname:node_2938,prsc:2,uv:0;n:type:ShaderForge.SFN_Panner,id:3208,x:31630,y:33061,varname:node_3208,prsc:2,spu:0.01,spv:0.005|UVIN-2938-UVOUT;proporder:9586-1144-9740-1438-4304;pass:END;sub:END;*/

Shader "Shader Forge/WaterShader" {
    Properties {
        _node_9586 ("node_9586", Color) = (0.2384299,0.4187598,0.7720588,1)
        _node_1144 ("node_1144", 2D) = "white" {}
        _node_9740 ("node_9740", Range(0, 1)) = 0.4150993
        _node_1144_copy ("node_1144_copy", 2D) = "white" {}
        _node_9740_copy ("node_9740_copy", Range(0, 1)) = 0.5168914
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
            uniform sampler2D _node_1144; uniform float4 _node_1144_ST;
            uniform float4 _node_9586;
            uniform float _node_9740;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
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
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 indirectDiffuse = float3(0,0,0);
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float4 node_7915 = _Time + _TimeEditor;
                float2 node_7910 = (i.uv0+node_7915.g*float2(0.005,0.01));
                float4 _node_1144_var = tex2D(_node_1144,TRANSFORM_TEX(node_7910, _node_1144));
                float3 node_3821 = (_node_1144_var.rgb*_node_9740);
                float3 diffuse = (directDiffuse + indirectDiffuse) * (_node_9586.rgb+node_3821);
/// Final Color:
                float3 finalColor = diffuse;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Custom/Silhoutted Hatching"
    CustomEditor "ShaderForgeMaterialInspector"
}
