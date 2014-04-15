Shader "Custom/NoCulling" {
	Properties {
		_Color ("Main Color", Color) = (1,0.25,0.25,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull Off
		Lighting Off

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
