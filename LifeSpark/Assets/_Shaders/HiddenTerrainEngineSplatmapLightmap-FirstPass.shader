Shader "Hidden/TerrainEngine/Splatmap/Lightmap-FirstPass" {  
Properties {  
    _Control ("Control (RGBA)", 2D) = "red" {}  
    _Splat3 ("Layer 3 (A)", 2D) = "white" {}  
    _Splat2 ("Layer 2 (B)", 2D) = "white" {}  
    _Splat1 ("Layer 1 (G)", 2D) = "white" {}  
    _Splat0 ("Layer 0 (R)", 2D) = "white" {}  
    // used in fallback on old cards  
    _MainTex ("BaseMap (RGB)", 2D) = "white" {}  
    _Color ("Main Color", Color) = (1,1,1,1)  
}  
      
SubShader {  
    Tags {  
        "SplatCount" = "4"  
        "Queue" = "Geometry-100"  
        "RenderType" = "Opaque"  
		"MustRender" = "true"
    }  

CGPROGRAM  
#pragma surface surf Lambert  
struct Input {  
    float2 uv_Control : TEXCOORD0;  
    float2 uv_Splat0 : TEXCOORD1;  
    float2 uv_Splat1 : TEXCOORD2;  
    float2 uv_Splat2 : TEXCOORD3;  
    float2 uv_Splat3 : TEXCOORD4;  
	float3 worldPos;
};  
  
sampler2D _Control;  
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;  
  
      
void surf (Input IN, inout SurfaceOutput o) {  
    fixed4 splat_control = tex2D (_Control, IN.uv_Control);  
	fixed4 white = (1,1,1,1);
	fixed4 black = (0,0,0,1);
    fixed3 col = (0,0,0);  
	float _Part = -0;  
	float _Amount = 2;  
    col  = splat_control.r * tex2D (_Splat0, IN.uv_Splat0).rgb * (tex2D(_Splat0,IN.uv_Splat0 * _Part).rgb * _Amount);  
    col += splat_control.g * tex2D (_Splat1, IN.uv_Splat1).rgb * (tex2D(_Splat1,IN.uv_Splat1 * _Part).rgb * _Amount);  
    col += splat_control.b * tex2D (_Splat2, IN.uv_Splat2).rgb * 2; 

	//float2 xz;
	//xz.x = (IN.worldPos.x + 368.6581) / 750;
	//xz.y = (IN.worldPos.z + 370.654) / 750;

	//fixed4 cont = tex2D (_Splat3, xz);
	//if (cont.r < 0.5)
	//	col *= 0.2;
    //col += splat_control.a * tex2D (_Splat3, IN.uv_Splat3).rgb * (tex2D(_Splat2,IN.uv_Splat3 * _Part).rgb * _Amount); 
    
	o.Albedo = col;  
    o.Alpha = 0.0;  
}  
ENDCG    
}  
  
// Fallback to Diffuse  
Fallback "Diffuse"  
}  