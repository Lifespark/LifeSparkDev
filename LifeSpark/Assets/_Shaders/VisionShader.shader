Shader "Custom/VisionShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ControlTex("control", 2D) = "white" {}
		_worldUVTex("uv", 2D) = "white" {}
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
			sampler2D _ControlTex;
			sampler2D _worldUVTex;
			
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
				float4 w = tex2D (_worldUVTex, i.uv.xy);
				float4 k1 = tex2D (_ControlTex, w.rg + float2(1.0f/128.0f,1.0f/128.0f));
				float4 k2 = tex2D (_ControlTex, w.rg + float2(1.0f/128.0f,0.0f/128.0f));
				float4 k3 = tex2D (_ControlTex, w.rg + float2(1.0f/128.0f,-1.0f/128.0f));
				float4 k4 = tex2D (_ControlTex, w.rg + float2(0.0f/128.0f,1.0f/128.0f));
				float4 k5 = tex2D (_ControlTex, w.rg + float2(0.0f/128.0f,-1.0f/128.0f));
				float4 k6 = tex2D (_ControlTex, w.rg + float2(-1.0f/128.0f,1.0f/128.0f));
				float4 k7 = tex2D (_ControlTex, w.rg + float2(-1.0f/128.0f,0.0f/128.0f));
				float4 k8 = tex2D (_ControlTex, w.rg + float2(-1.0f/128.0f,-1.0f/128.0f));
				float4 k9 = tex2D (_ControlTex, w.rg + float2(-0.0f/128.0f,-0.0f/128.0f));
				float4 k = tex2D (_ControlTex, w.rg);
				//float4 k = (k1+k2+k3+k4+k5+k6+k7+k8+k9)/9;
				float f = 1;
				if (k.r < 0.2)
					f = 0.2;
				else
					f = k.r;
				result.rgb = c.rgb * f;
				result.a = c.a;
				return  result;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
