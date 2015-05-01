Shader "My/HighlightPost" 
{
    Properties
    {
        _MainTex("", 2D) = "white" {}
        _Highlights("Highlights", 2D) = "white" {}
        _BlendWeight("BlendWeight", Float) = 0.2
    }

    SubShader
    {
        ZTest Always Cull Off ZWrite Off Fog { Mode Off } //Rendering settings

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" 			

            struct v2f 
            {
	float4 pos : POSITION;
	half2 uv : TEXCOORD0;
            };
			
            v2f vert(appdata_img v) {
	v2f o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
	return o;
            }

            sampler2D _MainTex; 
            sampler2D _Highlights; 
            float _BlendWeight;
	

            fixed4 frag(v2f i) : COLOR
            {
	fixed4 org = tex2D(_MainTex, i.uv); 
	fixed4 high = tex2D(_Highlights, half2(i.uv.x, 1 - i.uv.y)); 		
	fixed4 col = fixed4(
	    org.r + high.r * _BlendWeight, 
	    org.g + high.g * _BlendWeight, 
	    org.b + high.b * _BlendWeight, 
	    1);

	return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

