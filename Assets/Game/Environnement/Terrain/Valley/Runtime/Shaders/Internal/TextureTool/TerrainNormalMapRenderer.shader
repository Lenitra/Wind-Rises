Shader "Hidden/Griffin/TerrainNormalMapRenderer"
{
	Properties
	{
		_TangentSpace("Tangent Space", Int) = 0
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" }
		Blend One Zero

		Pass
		{
			Name "Sharp Normal"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
			};

			int _TangentSpace;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(float4(v.uv, 0, 1));
				o.normal = v.normal;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float3 n = _TangentSpace * float3(i.normal.x, i.normal.z, i.normal.y) + (1 - _TangentSpace) * i.normal;
				float3 normal = normalize(n);
				float3 col = float3(
					(normal.x + 1) / 2,
					(normal.y + 1) / 2,
					(normal.z + 1) / 2);
				#ifdef UNITY_COLORSPACE_GAMMA
				return float4(col,1);
				#else
				return float4(GammaToLinearSpace(col.rgb), 1);
				#endif
			}
			ENDCG
		}

		Pass
		{
			Name "Interpolated Normal"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
			};

			sampler2D _SharpNormalMap;
			float4 _SharpNormalMap_TexelSize;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(float4(v.uv, 0, 1));

				float2 t = _SharpNormalMap_TexelSize.xy;
				float3 nC = tex2Dlod(_SharpNormalMap, float4(v.uv, 0, 0)).rgb;

				float3 nL = tex2Dlod(_SharpNormalMap, float4(v.uv + float2(-t.x, 0), 0, 0)).rgb;
				float3 nT = tex2Dlod(_SharpNormalMap, float4(v.uv + float2(0, t.y), 0, 0)).rgb;
				float3 nR = tex2Dlod(_SharpNormalMap, float4(v.uv + float2(t.x, 0), 0, 0)).rgb;
				float3 nB = tex2Dlod(_SharpNormalMap, float4(v.uv + float2(0, -t.y), 0, 0)).rgb;

				float3 nLT = tex2Dlod(_SharpNormalMap, float4(v.uv + float2(-t.x, t.y), 0, 0)).rgb;
				float3 nTR = tex2Dlod(_SharpNormalMap, float4(v.uv + float2(t.x, t.y), 0, 0)).rgb;
				float3 nRB = tex2Dlod(_SharpNormalMap, float4(v.uv + float2(t.x, -t.y), 0, 0)).rgb;
				float3 nBL = tex2Dlod(_SharpNormalMap, float4(v.uv + float2(-t.x, -t.y), 0, 0)).rgb;

				float3 avgNormal = (nC + nL + nT + nR + nB + nLT + nTR + nRB + nBL) / 9.0;

				o.normal = avgNormal;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				return float4(i.normal, 1);
			}
			ENDCG
		}
	}
}
