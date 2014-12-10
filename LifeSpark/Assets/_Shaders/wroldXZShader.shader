Shader "Custom/worldXZShader" {
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : POSITION;
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
				return o;
			}

			float4 frag(v2f i) : COLOR {
				return i.worlduv;
			}

			ENDCG
		}
	}
}
