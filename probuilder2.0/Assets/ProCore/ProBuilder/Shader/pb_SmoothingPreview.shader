Shader "Hidden/ProBuilder/SmoothingPreview"
{
	Properties {
        _Opacity ("Opacity", Float) = .5

	}

	SubShader
	{
		Tags { "IgnoreProjector"="True" "RenderType"="Geometry" }
		Lighting Off
		ZTest LEqual
		ZWrite On
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			AlphaTest Greater .25

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _Opacity;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;

				/// https://www.opengl.org/discussion_boards/showthread.php/166719-Clean-Wireframe-Over-Solid-Mesh
				#if UNITY_VERSION > 550
				o.pos = float4(UnityObjectToViewPos(v.vertex.xyz), 1);
				#else
				o.pos = mul(UNITY_MATRIX_MV, v.vertex);
				#endif
				o.pos.xyz *= .998;
				o.pos = mul(UNITY_MATRIX_P, o.pos);
                o.color = v.color;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
//                i.pos.xy = floor(i.pos.xy * 1) * .5;
//                float checker = -frac(i.pos.x + i.pos.y);
//                clip(checker);

                half4 c = half4(i.color.rgb, i.color.a * _Opacity);
				return c;
			}

			ENDCG
		}
	}
}
