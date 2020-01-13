Shader "GUI/Text Shader" 
{
	Properties
	{
		_MainTex("Font Texture", 2D) = "white" {}
		_Color("Text Color", Color) = (1,1,1,1)
	}

		SubShader
		{

			Tags 
		{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
			}

			Lighting Off Cull Off ZTest Always ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass 
		{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
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

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = i.color;
					col.a *= tex2D(_MainTex, i.texcoord).a;
					if (i.texcoord.x < 0.5/ _Ratio)
					{
						i.texcoord.x *= _Ratio;
					}
					else if (1.0 - i.texcoord.x < 0.5 / _Ratio)
					{
						i.texcoord.x = 1.0 - i.texcoord.x;
						i.texcoord.x *= _Ratio;
						i.texcoord.x = 1.0 - i.texcoord.x;
					}
					float dist = distance(clamp(i.texcoord, 0.2, 0.8 ), i.texcoord) * 5;
					dist = smoothstep(1.0, 0.8, dist);
					return float4(0.1, 0.6, 0.9, dist);
				}
				ENDCG
			}
		}
}