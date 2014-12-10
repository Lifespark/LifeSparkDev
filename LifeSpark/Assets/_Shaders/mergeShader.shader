Shader "Custom/mergeShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Second("second", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			ZTest Always 
			Cull Off 
			ZWrite Off 
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _Second;
			
			struct Input
			{
				float4 position : POSITION;
				float4 uv : TEXCOORD0;
			};

			Input vert (appdata_base v)
			{
				Input o;
				o.position = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = float4( v.texcoord.xy, 0, 0 );
				return o;
			}

			float4 frag(Input i) : COLOR {
				float4 result;
				float4 c = tex2D (_MainTex, i.uv.xy);
				float4 w = tex2D (_Second, i.uv.xy);
				if (c.r >= w.r)
					result.rgb = c.rgb;
				else
					result.rgb = w.rgb;
				result.a = c.a;
				return  result;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}

