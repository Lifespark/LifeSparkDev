Shader "LifeSparkShader/ToonLevelSurface" {  
    Properties {  
        _MainTex ("Base (RGB)", 2D) = "white" {}  
        _ControlTex ("Control (RGB)", 2D) = "white" {}  
        _Ramp ("Ramp Texture", 2D) = "white" {}  
        _Tooniness ("Tooniness", Range(0.1,20)) = 4  
		_Lightness ("Lightness", Range(0.1,1)) = 0.2  
		_Color ("Color", Color) = (1,1,1,1)

		//Add the Input Levels Values
		_inBlack ("Input Black", Range(0, 255)) = 0
		_inGamma ("Input Gamma", Range(0, 5)) = 1.61
		_inWhite ("Input White", Range(0, 255)) = 255
		
		//Add the Output Levels
		_outWhite ("Output White", Range(0, 255)) = 255
		_outBlack ("Output Black", Range(0, 255)) = 0

    }  
    SubShader {  

        Tags { "RenderType"="Opaque"
				"MustRender" = "true" 
				}  
        LOD 200  

		CGPROGRAM  
		#pragma target 3.0
		#pragma surface surf Toon
   
		sampler2D _MainTex;  
		sampler2D _Ramp;  
		sampler2D _ControlTex;
		float _Tooniness;  
		float _Lightness;  

		//to the CGPROGRAM
		float _inBlack;
		float _inGamma;
		float _inWhite;
		float _outWhite;
		float _outBlack; 
		float4 _Color;
   
		struct Input {  
			float2 uv_MainTex;  
			float3 worldPos;
		};  
   
		struct Output {
			half3 Albedo;
			half Alpha;
			half3 Normal;     //The normal of the pixel  
			half lightness;    
			half3 Emission;   //The emissive color of the pixel  
			half Specular;     //Specular power of the pixel  
			half Gloss;         //Gloss intensity of the pixel
		};

		float GetPixelLevel(float pixelColor)
		{
			float pixelResult;
			pixelResult = (pixelColor * 255.0);
			pixelResult = max(0, pixelResult - _inBlack);
			pixelResult = saturate(pow(pixelResult / (_inWhite - _inBlack), _inGamma));
			pixelResult = (pixelResult * (_outWhite - _outBlack) + _outBlack)/255.0;	
			return pixelResult;
		}

		void surf (Input IN, inout Output o) {  
			half4 c = tex2D (_MainTex, IN.uv_MainTex);  
			half4 outPixel,lightnessTex;
			float2 uv;
			uv.r = (IN.worldPos.x + 370) /740;
			uv.g = (IN.worldPos.z + 370) /740;
            lightnessTex = tex2D(_ControlTex,uv);
			
			o.lightness = 1;
			if (lightnessTex.r < 0.5)
				o.lightness = _Lightness * (1- lightnessTex.r);

			outPixel.r = GetPixelLevel(c.r) * _Color.r;
			outPixel.g = GetPixelLevel(c.g) * _Color.g;
			outPixel.b = GetPixelLevel(c.b) * _Color.b;
              
			o.Albedo = (floor(outPixel.rgb * _Tooniness)/_Tooniness) * o.lightness;  
			o.Alpha = c.a;  
		}  
   
		half4 LightingToon(Output s, fixed3 lightDir, half3 viewDir, fixed atten)  
		{  
			float difLight = dot (s.Normal, lightDir);  
			float dif_hLambert = difLight;   
              
			float rimLight = dot (s.Normal, viewDir);    
			float rim_hLambert = rimLight;   
              
			float3 ramp = tex2D(_Ramp, float2(dif_hLambert, rim_hLambert)).rgb;     
      
			float4 c;    
			c.rgb = s.Albedo * _LightColor0.rgb * ramp * atten * 2 * s.lightness;  
			c.a = s.Alpha;  
			return c;  
		}  
   
		ENDCG  
    }   
    FallBack "Diffuse"  
}  