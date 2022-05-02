Shader "Inventio/Balloon Shader" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
	}
	SubShader {
		Tags { 
			"Queue"="Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		LOD 200
		Lighting Off
		ZWrite Off

		Blend SrcAlpha OneMinusSrcAlpha 

		CGPROGRAM
		#pragma surface surf NoLighting keepalpha

		float4 _Color;
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Color.rgb;
			o.Alpha = c.a * _Color.a;
		}

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
			// Declare the variable that will store the final pixel color,
			fixed4 c;
			// Copy the diffuse color component from the SurfaceOutput to the final pixel.
			c.rgb = s.Albedo; 
			// Copy the alpha component from the SurfaceOutput to the final pixel.
			c.a = s.Alpha;
			return c;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
