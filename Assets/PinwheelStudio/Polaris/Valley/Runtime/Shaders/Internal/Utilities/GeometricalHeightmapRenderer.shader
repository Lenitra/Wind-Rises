Shader "Hidden/Griffin/GeometricalHeightMapRenderer"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UtilitiesCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float height01 : TEXCOORD2;
            };

            float3 _TerrainSize;
            sampler2D _HeightMap;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(v.uv, 0, 1));
                o.height01 = tex2Dlod(_HeightMap, float4(v.uv, 0, 0));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float height01 = i.height01;
                float2 enc = GriffinEncodeFloatRG(height01);
                return float4(enc.x, enc.y, 0, 1);
            }
            ENDCG
        }
    }
}
