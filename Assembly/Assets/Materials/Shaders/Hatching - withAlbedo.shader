 
// Toon Ramp, Bump Mapped, Orthographic Outline, Detail Texture, Fading and Hatching shader
Shader "Toon/Bumped Orthographic Hatching" {
    Properties {
        _Color ("Main Color", Color) = (0.5,0.5,0.5,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline width", Range (0.0, 0.25)) = .005
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        // Uncomment to add a parallax height value
        // _Parallax ("Height", Range (0.0, 0.08)) = 0.0
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
        _Detail ("Detail", 2D) = "gray" {}
        _BumpMap ("Normalmap ", 2D) = "bump" {}
        // Uncomment to add a parallax height map
        // _ParallaxMap ("Heightmap (A)", 2D) = "black" {}
        _FadeDistance ("Fade Start Distance", float) = 0.5
        _Hatch0 ("Hatch 0 (light)", 2D) = "white" {}
        _Hatch1 ("Hatch 1", 2D) = "white" {}      
        _Hatch2 ("Hatch 2 (dark)", 2D) = "white" {}
        // Uncomment next 3 lines to use 6 hatch textures
        // _Hatch3 ("Hatch 3", 2D) = "white" {}      
        // _Hatch4 ("Hatch 4", 2D) = "white" {}      
        // _Hatch5 ("Hatch 5 (dark)", 2D) = "white" {}    
    }
 
    SubShader {
        Tags {"IgnoreProjector"="False" "RenderType"="TransparentCutout"}
        LOD 400
 
        CGPROGRAM
        #pragma target 3.0
        // ToonRamp lighting and do the final hatching overlay in the FinalColor function
        // The intensity and tint calculations don't work with additive passes so exclude them
        #pragma surface surf ToonRamp dualforward fullforwardshadows vertex:vert finalcolor:FinalColor noforwardadd
 
        sampler2D _Ramp;
 
        sampler2D _MainTex;
        float _Cutoff;
        sampler2D _Detail;
        sampler2D _BumpMap;
        float4 _Color;
        sampler2D _ParallaxMap;
        // Uncomment for parallax and specular
        // float _Shininess;
        // float _Parallax;
        float _FadeDistance;
 
        sampler2D _Hatch0;
        sampler2D _Hatch1;
        sampler2D _Hatch2;
        // Uncomment next 3 lines to use 6 hatch textures
        // sampler2D _Hatch3;
        // sampler2D _Hatch4;
        // sampler2D _Hatch5;
 
 
        #pragma lighting ToonRamp exclude_path:prepass alphatest:_Cutoff dualforward fullforwardshadows
        inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            #ifndef USING_DIRECTIONAL_LIGHT
            lightDir = normalize(lightDir + viewDir);
            #endif
 
            half d = dot (s.Normal, lightDir)*0.5 + 0.5;
            half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;
 
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
            c.a = 0;
            return c;
        }
 
 
        struct Input {
            float2 uv_MainTex : TEXCOORD0;
            float2 uv_Detail;
            float2 uv_BumpMap;
            float3 viewDir;
            float alpha;
            float2 uv_Hatch0;
        };
 
        void vert (inout appdata_full v, out Input o) {
            //this gets rid of a bunch of d3d11 warnings
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float4 viewPos = mul(UNITY_MATRIX_MV, v.vertex);
            o.alpha = (-viewPos.z - _ProjectionParams.y)/_FadeDistance;
            o.alpha = min(o.alpha, 1);
        }
 
        void surf (Input IN, inout SurfaceOutput o) {
            // Uncomment next 4 lines for parallax, may require some further editing
            // half h = tex2D (_ParallaxMap, IN.uv_BumpMap).w;
            // float2 offset = ParallaxOffset (h, _Parallax, IN.viewDir);
            // IN.uv_MainTex += offset;
            // IN.uv_BumpMap += offset;
 
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * IN.alpha;
            o.Albedo = c.rgb * _Color.rgb;
            o.Gloss = c.a;
            o.Alpha = c.a * _Color.a;
            // Clip out the alpha from Cutoff value
            clip (c.a - _Cutoff);
            // Uncomment for specular
            // o.Specular = _Shininess;
            o.Albedo *= (half3)tex2D (_Detail, IN.uv_Detail).rgb * 2;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            // Uncomment for emission
            //o.Emission = c.rgb * tex2D(_MainTex, IN.uv_MainTex).a;
        }
 
 
        // Hatching function for blending the hatch textures according to the intensity
        half3 Hatching(float2 _uv, half _intensity)
        {
            half3 hatch0 = (half3)tex2D(_Hatch0, _uv).rgb;
            half3 hatch1 = (half3)tex2D(_Hatch1, _uv).rgb;
            half3 hatch2 = (half3)tex2D(_Hatch2, _uv).rgb;
            // Uncomment next 3 lines to use 6 hatch textures
            // half3 hatch3 = (half3)tex2D(_Hatch3, _uv).rgb;
            // half3 hatch4 = (half3)tex2D(_Hatch4, _uv).rgb;
            // half3 hatch5 = (half3)tex2D(_Hatch5, _uv).rgb;
           
            const half hatchingScale = 6.0 / 7.0;
            half hatchedIntensity = min(_intensity, hatchingScale);
            half remainingIntensity = _intensity - hatchedIntensity;
            half unitHatchedIntensity = hatchedIntensity / hatchingScale;
 
            // Uncomment next 2 lines if you are going to use 6 textures for the hatching
            // half3 weightsA = saturate((unitHatchedIntensity * 6.0) + half3(-5.0, -4.0, -3.0));
            // half3 weightsB = saturate((unitHatchedIntensity * 6.0) + half3(-2.0, -1.0, 0.0));
            // Comment out next line if you are using 6 hatch textures
            half3 weightsA = saturate((unitHatchedIntensity * 6.0) + half3(-2.0, -1.0, 0.0));
           
            // Uncomment next 2 lines if you are going to use 6 textures for the hatching
            // weightsB.yz = saturate(weightsB.yz - weightsB.xy);
            // weightsB.x = saturate(weightsB.x - weightsA.z);
            weightsA.yz = saturate(weightsA.yz - weightsA.xy);
           
            half3 hatching = remainingIntensity;
            hatching += hatch0 * weightsA.x;
            hatching += hatch1 * weightsA.y;
            hatching += hatch2 * weightsA.z;
            // Uncomment next 3 lines if you are going to use 6 hatch textures
            // hatching += hatch3 * weightsB.x;
            // hatching += hatch4 * weightsB.y;
            // hatching += hatch5 * weightsB.z;
            return hatching;
        }
           
        // Function for final color for including in the hatch textures
        void FinalColor(Input IN, SurfaceOutput o, inout half4 color)
        {
            // Calculate pixel intensity
            half intensity = dot(color.rgb, half3(0.3, 0.59, 0.11));
 
            // Apply hatching
            // Uncomment the next 2 lines if you want to blend in the saturation and tint
            // half3 tint = color.rgb / max(intensity, 1.0 / 255.0);
            // color.rgb = tint * Hatching(IN.uv_Hatch0, intensity);
 
            // Color is not oversaturated when hatching directly, so tint is not necessary
            // Comment this line out if you want to use tint
            color.rgb *= Hatching(IN.uv_Hatch0, intensity);
        }
        ENDCG
 
        // Use orthographic outlining, comment out if not needed (requires toon orthographic outline shader)
        UsePass "Toon/Basic Outline Orthographic/OUTLINE"
 
    }
 
    Fallback "Toon/Lighted"
}