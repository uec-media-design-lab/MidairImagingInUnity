Shader "MidairImaging/MaskShader"
{
    Properties 
    {
        _Mask("Mask", Int) = 1
        _Color ("Color", Color) = (1,1,1,0.3)
    }

    SubShader 
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue"="Transparent-1" 
        }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil 
        {
            Ref [_Mask]
            Comp Always
            Pass Replace
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 _Color;
            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
