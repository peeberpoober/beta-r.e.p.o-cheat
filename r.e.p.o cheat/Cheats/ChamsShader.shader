Shader "Custom/ChamsShader"
{
    Properties
    {
        _Color("Color", Color) = (0,0,1,1)
        _Cull("Cull", Float) = 0
        _ZTest("ZTest", Float) = 8
        _ZWrite("ZWrite", Float) = 1
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Pass
        {
            Cull[_Cull]
            ZTest[_ZTest]
            ZWrite[_ZWrite]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            fixed4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
        FallBack "Diffuse"
}