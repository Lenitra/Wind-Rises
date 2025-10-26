Shader "Griffin/~Internal/Billboard Creator/Normal"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest"} 
        LOD 100
        Pass
        {
			ZWrite On
			ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
            };

            #define CAMERA_RIGHT UNITY_MATRIX_V[0].xyz
			#define CAMERA_UP UNITY_MATRIX_V[1].xyz
			#define CAMERA_FORWARD UNITY_MATRIX_V[2].xyz

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 normal = normalize(mul(UNITY_MATRIX_I_V, i.normal));
                float r = (normal.x + 1) * 0.5;
                float g = (normal.y + 1) * 0.5;
                float b = (normal.z + 1) * 0.5;
				float a = 1;
                fixed4 col = float4(r,g,b,a);
				
				#if UNITY_COLORSPACE_GAMMA
				return float4(1,0,0,1);
				#else
				return float4(GammaToLinearSpace(col.rgb), 1);
				#endif
            }
            ENDCG
        }
    }
}
