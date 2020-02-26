Shader "Hidden/Ressemble/BoxBloc" 
{
	Properties
	{
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
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
			uniform float4 _HeaderColor;
			uniform float4 _BackColor;
			uniform float4 _Sizes;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
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
				float2 center = float2(clamp(i.texcoord.x * _Sizes.x, _Sizes.z, _Sizes.x - _Sizes.z), clamp(i.texcoord.y * _Sizes.y, _Sizes.z, _Sizes.y - _Sizes.z));
				float dist = distance(center, i.texcoord * _Sizes.xy) / _Sizes.z;

				float border = 0.7 + smoothstep(0.97, 0.92, dist) * 0.3;
				dist = smoothstep(1.0, 0.95, dist);
				float header = step(_Sizes.y - _Sizes.w, i.texcoord.y * _Sizes.y);
				float4 color = _HeaderColor * header + _BackColor * (1.0 - header);
				return float4(color.rgb * border, dist);
			}
			ENDCG
		}
	}
}