Shader "Hidden/Ressemble/TextBloc" 
{
	Properties
	{
		_MainTex("Font Texture", 2D) = "white" {}
		_Color("Text Color", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform fixed4 _Color;
			uniform float _Ratio;
			uniform float _Roundness;
			uniform float _BorderOnly;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _Color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			float sdRoundedBox(float2 p, float2 b, float4 r )
			{
				r.xy = (p.x>0.0)?r.xy : r.zw;
				r.x  = (p.y>0.0)?r.x  : r.y;
				float2 q = abs(p)-b+r.x;
				return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r.x;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				i.texcoord.x *= _Ratio;
				float r = _Roundness * 0.6 + 0.4;

				float2 center = float2(clamp(i.texcoord.x, 0.5 * r, _Ratio - 0.5 * r), clamp(i.texcoord.y, 0.5 * r, 1.0 - 0.5 * r));
				float dist = ((distance(center, i.texcoord)) * 2) / r;
				float offset = (1.0 / r) * 0.05;
				dist = smoothstep(0.9, 0.9 - offset, dist) * (1.0 - smoothstep(0.8, 0.8 - offset, dist) * _BorderOnly);
				return float4(_Color.rgb, dist * _Color.a);
			}
			ENDCG
		}
	}
}