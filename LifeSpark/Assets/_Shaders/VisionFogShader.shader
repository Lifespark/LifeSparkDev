Shader "Custom/visionFogShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Former ("Base (RGB)", 2D) = "white" {}
		_ControlTex("control", 2D) = "white" {}
		_PointX ("x", Range(0,1)) = 0.5
		_PointY ("y", Range(0,1)) = 0.5
		_Range("range", Range(0,1)) = 0.01
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

		Pass
		{
		
			ZTest Always

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _Former;
			sampler2D _ControlTex;
			float _PointX;
			float _PointY;
			float _Range;

			struct v2f {
				float4 pos : POSITION;
				float4 uv : TEXCOORD0;  
				float4 worlduv : COLOR;
			};

			v2f vert( appdata_base v ) {
				v2f o;
				float4 worldPos = mul(_Object2World,  v.vertex);
				o.worlduv.x = (worldPos.x + 368.6581) / 750;
				o.worlduv.y = (worldPos.z + 370.654) / 750;
				o.worlduv.z = 0;
				o.worlduv.w = 1;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = float4( v.texcoord.xy, 0, 0 );
				return o;
			}

			half4 frag(v2f i) : COLOR {

				half4 result;

				float2 xy;
				xy =  i.uv - float2(_PointX,_PointY);
				float distance = (xy.x * xy.x + xy.y * xy.y);

				float2 uv = i.uv - xy / sqrt(distance) / 128.0f;
				half4 color = tex2D(_Former,uv);
				half4 control = tex2D(_ControlTex,uv);
				half4 currentControl = tex2D(_ControlTex,i.uv.xy);


				if (distance < _Range / 40)
				{
					result.rgb = 1;
				}
				else if (distance < _Range)
				{
					result.rgb = control.r * color * (_Range * _Range - distance*distance) / _Range / _Range;
					//result.rgb = (_Range - distance) / _Range;
				}
				else
					result.rgb = 0;
				result.a = 1;

				return result;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
