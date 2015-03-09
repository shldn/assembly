Shader "Custom/CutoffRefraction_OneMinusSrcAlpha-One" 
{
	Properties 
	{
		_RimPower 			("Rim Power", Range(0,2) ) = 1				//Power of the Light around the object
		_InnerOuterRim		("Inner / Outer Rim", Range(0,1)) = 0		//Position of the Rim Light, inner or outer glow.
		_RimWidth			("Rim Width", Range(0,0.99)) = 0.7			//Width of the Rim Light
		_RimColor 			("Rim Color", Color) = (1, 1, 1, 1)			//Color of the Rim Light
		_Color 				("Main Color", Color) = (1,1,1,1)			//Main Color of the Texture
		_MainTex 			("Base (RGB)", 2D) = "white" {}				//Main Texture
		_ReflectionMap 		( "Reflection Cubemap", CUBE ) = "black" {}	//Cube map that is used for reflecting onto the object.
		_BumpScale 			("Bumpmap Scale", Range(1,20) ) = 1			//Intensity scale of the bump map ontop of the object.
		_RefractionPower 	("Refraction Power", Range(0,2) ) = .05 	//Intensity of the refraction (moving texture effect).
		_RefractionHardness	("Refraction Hardness", Range(0,1)) = 0.25	//Intensity on top of the refraction power (give it a boost).
		_RefractionReflect	("Refraction Reflect", Range(0,1)) = 0		//Reflection power onto the object.
		_RotateSpeed		("Rotation Speed", Range(0,5)) = 0.5		//Rotation speed of the inner texture.
		_RefractMap 		("Refraction Bumpmap", 2D ) = "black" {}	//Texture of the bumpmap that is moving on top of the object.
		_SliceAmount 		("Slice Amount", Range(0.0, 1.0)) = 0		//Amount to cut off the texture and shader effect.
		_SliceGuide 		("Slice Guide (RGB)", 2D) = "white" {}		//The texture guide (alpha ramp), so it knows what to cut off and when to cut it off.
	} 	
	
	SubShader 
	{
		Tags { "RenderType"="Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent"}
		LOD 200
		
		 Pass 
		 {
			Blend OneMinusSrcAlpha One
			
		 	Cull back
		 
            CGPROGRAM
            #pragma vertex vert
        	#pragma fragment frag
        	#pragma target 3.0
            #include "UnityCG.cginc"
                
            struct appdata 
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 color : COLOR;

				float4 screenPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				float4 worldPos : TEXCOORD4;
            };
            
            uniform sampler2D 	_MainTex;
            uniform float4 		_MainTex_ST;
            uniform float4 		_Color;
            uniform float4		_RimColor;
            uniform float 		_RimPower;
            float				_InnerOuterRim;
            float				_RimWidth;
			sampler2D 			_SliceGuide;
      		float 				_SliceAmount;
      		float				_RotateSpeed;
      		
      		float 				_RefractionPower; 
  			float				_RefractionHardness;
  			float				_RefractionReflect;
			float 				_BumpScale;
			sampler2D 			_GrabTexture;
			sampler2D 			_RefractMap;
			samplerCUBE 		_ReflectionMap;
            
            v2f vert (appdata_full v) 
            {
            	v2f output = (v2f)0;
            	
                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                float dotProduct = 1 - dot(v.normal, viewDir); 
                float rimWidth = _RimWidth;	
               	
                output.worldNormal = normalize(float3(mul(float4(v.normal, 1.0), _World2Object).xyz));
                output.viewDir = mul(_Object2World, v.vertex).xyz - _WorldSpaceCameraPos.xyz;
                output.worldPos = mul(_Object2World, v.vertex);
                output.screenPos = output.pos;
                output.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                
                output.color = smoothstep(1 - rimWidth, _InnerOuterRim, dotProduct);
                output.color *= _RimColor * _RimPower;
           
                output.uv = v.texcoord;
                output.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                return output;
            }
            
            float4 frag(v2f IN) : COLOR
            {
            	clip(tex2D (_SliceGuide, IN.uv).rgb - _SliceAmount);
            	
            	float4 texcol = tex2D(_MainTex, IN.uv);
            	texcol = tex2D( _MainTex, IN.uv + float2( abs( sin( (half)-_Time * _RotateSpeed) ), 0 ) );
                texcol *= _Color;
                texcol.rgb += IN.color;
            	
            	float2 screenUV;
            	screenUV.x = (IN.screenPos.x / IN.screenPos.w) * 0.5 + 0.5;
            	screenUV.y = (IN.screenPos.y / IN.screenPos.w) * 0.5 + 0.5;
            	
            	float2 tSUV = screenUV;
            	tSUV.y = 1 - tSUV.y;
            	
            	half halfTime = (half)_Time * 2;
            	half2 refraction1 = tex2D( _RefractMap, _BumpScale * IN.uv * 1   + half2(halfTime, 0)).rg * 2.0 - 1.0;
            	half2 refraction2 = tex2D( _RefractMap, _BumpScale * IN.uv * 0.5 - half2(halfTime, 0)).rg * 2.0 - 1.0;
            	half2 refraction3 = tex2D( _RefractMap, _BumpScale * IN.uv * 2   + half2(halfTime, 0)).rg * 2.0 - 1.0;
            	half2 refraction4 = tex2D( _RefractMap, _BumpScale * IN.uv * 2   - half2(halfTime, 0)).rg * 2.0 - 1.0;
            	float2 totalRefraction = (refraction1 + refraction2 + refraction3 + refraction4) * _RefractionHardness;
            	
            	half4 refractTex = tex2D(_RefractMap, IN.uv);
            	screenUV.xy += totalRefraction * _RefractionPower;
            	IN.worldNormal.xy += totalRefraction * _RefractionPower;
            	
            	half3 worldReflect = reflect(IN.viewDir, normalize(IN.worldNormal));
            	half4 reflectTex = texCUBE(_ReflectionMap, worldReflect);
            	half4 grabTex = tex2D(_GrabTexture, tSUV);
            	float fresnel = clamp(dot(normalize(IN.viewDir), normalize(IN.worldNormal)) + 1, _RefractionReflect, 1);

            	return lerp(grabTex, reflectTex, fresnel) * texcol;
            }
                
        	ENDCG
    	}	
	} 
	
	FallBack "Diffuse"
}
