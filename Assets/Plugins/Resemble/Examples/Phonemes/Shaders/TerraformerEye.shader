Shader "Hidden/TerraformerFace"
{
    Properties
    {
		_MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset] _Emission("Emission", 2D) = "black" {}
		[HDR] _EmissionColor("Emission Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_Gloss("Gloss", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _Emission;
		float _Gloss;
		float4 _EmissionColor;

        struct Input
        {
			float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			half4 color = tex2D(_MainTex, IN.uv_MainTex);
			half4 emission = tex2D(_Emission, IN.uv_MainTex);

            fixed4 c = color;
            o.Albedo = c.rgb;
            o.Smoothness = _Gloss;
			o.Emission = emission * color * _EmissionColor;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
