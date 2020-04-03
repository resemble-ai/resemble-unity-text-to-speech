Shader "Hidden/TerraformerFace"
{
    Properties
    {
		[NoScaleOffset] _MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset] _Normal("Normal", 2D) = "bump" {}
		[NoScaleOffset] _Mask ("Mask", 2D) = "grey" {}
		[NoScaleOffset] _Emission("Emission", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _Normal;
		sampler2D _Mask;
		sampler2D _Emission;

        struct Input
        {
			float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			half4 color = tex2D(_MainTex, IN.uv_MainTex);
			half3 normal = UnpackNormal(tex2D(_Normal, IN.uv_MainTex));
			half4 mask = tex2D(_Mask, IN.uv_MainTex);
			half4 emission = tex2D(_Emission, IN.uv_MainTex);

            fixed4 c = color;
            o.Albedo = c.rgb;
			o.Normal = normal;
			o.Occlusion = mask.g;
            o.Metallic = mask.r;
            o.Smoothness = min(mask.a, 0.5);
			o.Emission = emission;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
