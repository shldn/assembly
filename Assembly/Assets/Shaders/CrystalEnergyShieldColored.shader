// CrystalEnergyShield shader
//   by Wes Hawkins

Shader "CrystalEnergyShield_colored" {
	Properties {
		_FillColor ("Main Color", COLOR) = (1, 1, 1, 0)
		_RimColor ("Rim Color", Color) = (0.26, 0.19, 0.16, 0.0)
		_Alpha ("Alpha", Range(0.0, 1.0)) = 1.0
	}
	SubShader {
			   
		Tags {"QUEUE"="Transparent" "RenderType" = "Transparent" }
		ZWrite Off
		Blend SrcAlpha One
		Cull off
  
		// Outside
		CGPROGRAM
		#pragma surface surf Lambert
		struct Input{
			float3 viewDir;
		};

		float4 _FillColor;
		float4 _RimColor;
		float _Alpha;
	  
		void surf (Input IN, inout SurfaceOutput o){
			half incidence = 1.0 - saturate(abs(dot(normalize(IN.viewDir), o.Normal)));
			o.Emission = ((_RimColor.rgb * pow(incidence, 4)) + _FillColor.rgb) * _Alpha;
			o.Alpha = 1;
		}
		ENDCG

	} 
	Fallback "Diffuse"
}
  