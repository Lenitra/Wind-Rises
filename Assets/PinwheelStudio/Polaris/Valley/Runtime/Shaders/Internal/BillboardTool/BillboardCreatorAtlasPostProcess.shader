Shader "Griffin/~Internal/Billboard Creator/Atlas Post Process"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		CGINCLUDE
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _MainTex_TexelSize;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}
	ENDCG

		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Name "Dilate"
			CGPROGRAM

			float4 frag(v2f i) : SV_Target
			{
				float3 black = float3(0, 0, 0);
				float4 avg = float4(0, 0, 0, 0);

				float2 t = _MainTex_TexelSize.xy;
				float4 cC = tex2D(_MainTex, i.uv);
				avg.rgb += cC.rgb;
				avg.a += cC.rgb != black;

				float4 cL = tex2D(_MainTex, i.uv + float2(-t.x, 0));
				float4 cT = tex2D(_MainTex, i.uv + float2(0, t.y));
				float4 cR = tex2D(_MainTex, i.uv + float2(t.x, 0));
				float4 cB = tex2D(_MainTex, i.uv + float2(0, -t.y));

				avg.rgb += cL.rgb;
				avg.a += cL.rgb != black;
				avg.rgb += cT.rgb;
				avg.a += cT.rgb != black;
				avg.rgb += cR.rgb;
				avg.a += cR.rgb != black;
				avg.rgb += cB.rgb;
				avg.a += cB.rgb != black;

				float4 cL2 = tex2D(_MainTex, i.uv + 2 * float2(-t.x, 0));
				float4 cT2 = tex2D(_MainTex, i.uv + 2 * float2(0, t.y));
				float4 cR2 = tex2D(_MainTex, i.uv + 2 * float2(t.x, 0));
				float4 cB2 = tex2D(_MainTex, i.uv + 2 * float2(0, -t.y));

				avg.rgb += cL2.rgb;
				avg.a += cL2.rgb != black;
				avg.rgb += cT2.rgb;
				avg.a += cT2.rgb != black;
				avg.rgb += cR2.rgb;
				avg.a += cR2.rgb != black;
				avg.rgb += cB2.rgb;
				avg.a += cB2.rgb != black;

				float4 cL4 = tex2D(_MainTex, i.uv + 4 * float2(-t.x, 0));
				float4 cT4 = tex2D(_MainTex, i.uv + 4 * float2(0, t.y));
				float4 cR4 = tex2D(_MainTex, i.uv + 4 * float2(t.x, 0));
				float4 cB4 = tex2D(_MainTex, i.uv + 4 * float2(0, -t.y));

				avg.rgb += cL4.rgb;
				avg.a += cL4.rgb != black;
				avg.rgb += cT4.rgb;
				avg.a += cT4.rgb != black;
				avg.rgb += cR4.rgb;
				avg.a += cR4.rgb != black;
				avg.rgb += cB4.rgb;
				avg.a += cB4.rgb != black;

				avg.rgb = avg.rgb / avg.a;

				float4 color = lerp(avg, cC, cC.a);
				color.a = cC.a;

				return color;
			}
			ENDCG
		}

		Pass
		{
			Name "Sharpening"
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			float4 frag(v2f i) : SV_Target
			{
				float2 t = _MainTex_TexelSize.xy;
				float4 cC = tex2D(_MainTex, i.uv);

				return cC;
			}
			ENDCG
		}

	}
}
