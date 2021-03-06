﻿Shader "My/Grid" 
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)	
    }

    SubShader
    {
        Lighting Off
        ZTest LEqual Cull Back ZWrite Off Fog { Mode Off } 
		Blend SrcAlpha OneMinusSrcAlpha
		Tags {"Queue" = "Transparent-10"}
		Offset 0, 0

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" 			

            struct v2f 
            {
				float4 pos : POSITION;	
            };
			
            v2f vert(appdata_img v) 
            {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);	
				return o;
            }

            fixed4 _Color;

            fixed4 frag(v2f i) : COLOR
            {
				return _Color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
