Shader "Hidden/TerraformerFace"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
		_EyeSelect("Eye Select", Color) = (1,1,1,1)

        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_MouthsA("Mouths A", 2D) = "black" {}
		_MouthsB("Mouths B", 2D) = "black" {}
		_Eyes("Eyes", 2D) = "black" {}
		_EmissionMask("EmissionMask", 2D) = "white" {}
		_EmissionMaskTilling("EmissionMaskTilling", Float) = 1.0
		_EmissionMaskThreshold("EmissionMaskThreshold", Float) = 1.0
		[HDR]_EmissionColor("Emission Color", Color) = (1,1,1,1)


		_OO("OO", Range(0, 1)) = 0.0
		_FV("FV", Range(0, 1)) = 0.0
		_EE("EE", Range(0, 1)) = 0.0
		_BM("BM", Range(0, 1)) = 0.0
		_R("R", Range(0, 1)) = 0.0
		_IH("IH", Range(0, 1)) = 0.0
		_CH("CH", Range(0, 1)) = 0.0
		_AA("AA", Range(0, 1)) = 0.0
		_UU("UU", Range(0, 1)) = 0.0
		_AE("AE", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _MouthsA;
		sampler2D _MouthsB;
		sampler2D _Eyes;
		sampler2D _EmissionMask;

        struct Input
        {
			float2 uv_MainTex;
			float2 uv2_MouthsA;
			float2 uv2_Eyes;
        };

        half _Glossiness;
        half _Metallic;
		half4 _EyeSelect;
		float _EyesOffset;
        fixed4 _Color;
		fixed4 _EmissionColor;
		float _EmissionMaskTilling;
		float _EmissionMaskThreshold;

		//Mouth poses
		float _OO;
		float _FV;
		float _EE;
		float _BM;
		float _R;
		float _IH;
		float _CH;
		float _AA;
		float _UU;
		float _AE;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			//Get the selected face
			float nothing = saturate(1.0 - (_OO + _FV + _EE + _BM + _R + _IH + _CH + _AA + _UU + _AE));
			float4 face = tex2D(_MouthsA, IN.uv2_MouthsA) * float4(_OO, _FV, max(max(_R, _IH), _EE), _BM);
			face += tex2D(_MouthsB, IN.uv2_MouthsA) * float4(_AA, max(_CH, _UU), _AE, nothing);

			face.g = face.r + face.g + face.b + face.a;
			face.r = step(_EmissionMaskThreshold, face.g);


			//Get the selected eyes
			float4 eye = tex2D(_Eyes, IN.uv2_Eyes+ float2(_EyesOffset, 0.0)) * _EyeSelect;
			eye.g = eye.r + eye.g + eye.b + eye.a;
			eye.r = step(_EmissionMaskThreshold, eye.g);

			face.rg += eye.rg;


			float mask = tex2D(_EmissionMask, IN.uv2_MouthsA * _EmissionMaskTilling).r;
			mask *= tex2D(_EmissionMask, IN.uv2_MouthsA * _EmissionMaskTilling + float2(0.0, _Time.y)).g;
			float emit =  mask * face.g * _EmissionColor.a + step(0.3, face.r);

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
			o.Emission = emit * _EmissionColor * mask;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
