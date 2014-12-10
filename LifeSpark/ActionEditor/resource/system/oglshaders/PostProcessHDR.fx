/*<Material>
  <Params>
  <var name="g_InputTex" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_colorx" default="0 0 0 0" type="float4"/>
  
  <var name="g_SceneTex" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_ColorTex" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_LuminanceTex" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_LuminanceValueTex" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_BloomTex0" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_BloomTex1" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_DownSampleOffsets0" default="0 0 0 0" type="float4"/>
  <var name="g_DownSampleOffsets1" default="0 0 0 0" type="float4"/>
  <var name="g_BloomColor0" default="0 0 0 0" type="float4"/>
  <var name="g_BloomColor1" default="0 0 0 0" type="float4"/>
  <var name="g_AdaptSpeed" default="0" type="float"/>
  <var name="g_DeltaTime" default="0" type="float"/>
  <var name="g_TargetLuminance" default="0" type="float"/>
  <var name="g_BloomThreshold" default="0" type="float"/>
  <var name="g_BloomIntensity" default="0" type="float"/>
  <var name="g_SquaredPureWhite" default="0" type="float"/>
  </Params>
  <Technique>
  <Flow name = "FLOW_SCENE"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain"/>
  </Pass>
  </Technique>
  
  <Technique>
  <Flow name = "FLOW_HDR_LINEARDS"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "lineards_psmain"/>
  </Pass>
  </Technique>
  
   <Technique>
  <Flow name = "FLOW_HDR_LUMIDS"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "lumids_psmain"/>
  </Pass>
  </Technique>
  
  <Technique>
  <Flow name = "FLOW_HDR_DS"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "downsample_psmain"/>
  </Pass>
  </Technique>
  
  <Technique>
  <Flow name = "FLOW_HDR_LUMI"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "lumi_psmain"/>
  </Pass>
  </Technique>
  
  <Technique>
  <Flow name = "FLOW_HDR_LUMIADAPT"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "lumiadapt_psmain"/>
  </Pass>
  </Technique>
  
   <Technique>
  <Flow name = "FLOW_HDR_BRIGHT"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "bright_psmain"/>
  </Pass>
  </Technique>
  
    <Technique>
  <Flow name = "FLOW_HDR_BLUR"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "blur_psmain"/>
  </Pass>
  </Technique>
  
  <Technique>
  <Flow name = "FLOW_HDR_BLURBLEND"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "blurblend_psmain"/>
  </Pass>
  </Technique>
  
  <Technique>
  <Flow name = "FLOW_HDR_TONEMAPPING"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "tone_psmain"/>
  </Pass>
  </Technique>
</Material>*/

#include "header.fx"


uniform sampler2D g_InputTex;
uniform vec4 g_colorx;

uniform sampler2D g_SceneTex;
uniform sampler2D g_ColorTex;
uniform sampler2D g_LuminanceTex;
uniform sampler2D g_LuminanceValueTex;
uniform sampler2D g_BloomTex0;
uniform sampler2D g_BloomTex1;

uniform vec4 g_DownSampleOffsets0;
uniform vec4 g_DownSampleOffsets1;
uniform float g_AdaptSpeed;
uniform float g_DeltaTime;
uniform float g_TargetLuminance;
uniform float g_BloomThreshold;
uniform float g_BloomIntensity;
uniform float g_SquaredPureWhite;
uniform vec4  g_BloomColor0;
uniform vec4  g_BloomColor1;

#define __NumBlurSamples 15

uniform float  g_BlurWeights[__NumBlurSamples];
uniform vec2 g_BlurOffsets[__NumBlurSamples];

varying vec2 texcoord;

#ifdef VS

void vsmain()
{
	gl_Position =  vec4(vPos, 1.0);
	texcoord = vPos.xy*0.5 + 0.5;
	
	//vec2 Pos = sign(gl_Vertex.xy);   
	//gl_Position = vec4(Pos.xy, 0.0, 1.0);
	//texcoord.xy = Pos * 0.5 + 0.5;
}

#endif

#ifdef PS

void psmain()
{
    vec2 texoffset = texcoord.xy* vec2(0.4, 0.4);
    vec4 texColor = texture2D(g_InputTex, texoffset);//g_colorx; //vec4(1.0, 0.0, 0.0, 1.0);
	gl_FragColor = texColor;	
}

void lineards_psmain()
{
   //gamma = 2.2
	vec4 c0 = pow( texture2D( g_ColorTex, texcoord + g_DownSampleOffsets0.xy ), vec4(2.2, 2.2, 2.2, 2.2) );
	vec4 c1 = pow( texture2D( g_ColorTex, texcoord + g_DownSampleOffsets0.zw ), vec4(2.2, 2.2, 2.2, 2.2) );
	vec4 c2 = pow( texture2D( g_ColorTex, texcoord + g_DownSampleOffsets1.xy ), vec4(2.2, 2.2, 2.2, 2.2) );
	vec4 c3 = pow( texture2D( g_ColorTex, texcoord + g_DownSampleOffsets1.zw ), vec4(2.2, 2.2, 2.2, 2.2) );

	gl_FragColor = (c0 + c1 + c2 + c3) * vec4(0.25,0.25,0.25,0.25);
}

void lumids_psmain()
{
	float c0 = texture2D( g_LuminanceTex, texcoord + g_DownSampleOffsets0.xy ).r;
	float c1 = texture2D( g_LuminanceTex, texcoord + g_DownSampleOffsets0.zw ).r;
	float c2 = texture2D( g_LuminanceTex, texcoord + g_DownSampleOffsets1.xy ).r;
	float c3 = texture2D( g_LuminanceTex, texcoord + g_DownSampleOffsets1.zw ).r;

	float c = (c0 + c1 + c2 + c3) * 0.25;
	gl_FragColor = vec4(c, c, c, c);	
}

void downsample_psmain()
{
	vec3 c0 = texture2D( g_ColorTex, texcoord + g_DownSampleOffsets0.xy ).rgb;
	vec3 c1 = texture2D( g_ColorTex, texcoord + g_DownSampleOffsets0.zw ).rgb;
	vec3 c2 = texture2D( g_ColorTex, texcoord + g_DownSampleOffsets1.xy ).rgb;
	vec3 c3 = texture2D( g_ColorTex, texcoord + g_DownSampleOffsets1.zw ).rgb;

	vec4 c = vec4( (c0 + c1 + c2 + c3) * vec3(0.25, 0.25,0.25), 1.0 );
	gl_FragColor = c;	
}

void lumi_psmain()
{
	float epsilon = 0.000001;
	vec3 LuminanceFactor = vec3( 0.299, 0.587, 0.114 );
	
	float c0 = log( dot( texture2D( g_ColorTex, texcoord + g_DownSampleOffsets0.xy ).rgb, LuminanceFactor ) + epsilon );
	float c1 = log( dot( texture2D( g_ColorTex, texcoord + g_DownSampleOffsets0.zw ).rgb, LuminanceFactor ) + epsilon );
	float c2 = log( dot( texture2D( g_ColorTex, texcoord + g_DownSampleOffsets1.xy ).rgb, LuminanceFactor ) + epsilon );
	float c3 = log( dot( texture2D( g_ColorTex, texcoord + g_DownSampleOffsets1.zw ).rgb, LuminanceFactor ) + epsilon );

	float c = (c0 + c1 + c2 + c3) * 0.25;
	gl_FragColor = vec4(c, c, c, c);
}

void lumiadapt_psmain()
{
	float luminance = exp( texture2D( g_LuminanceValueTex, vec2( 0.5, 0.5 ) ).r );
	float alpha = 1.0 - pow( 0.98, g_AdaptSpeed * g_DeltaTime );	
	gl_FragColor = vec4( luminance, luminance, luminance, alpha );
}

void bright_psmain()
{
	float currentLuminance = texture2D( g_LuminanceValueTex, vec2( 0.5, 0.5 ) ).r;

	vec3 color = texture2D( g_ColorTex, texcoord ).rgb;
	//color = color * g_TargetLuminance / (currentLuminance + 0.000001);
	vec3 bcolor = vec3(g_BloomThreshold, g_BloomThreshold, g_BloomThreshold);
	color = max( color - bcolor, vec3(0.0, 0.0, 0.0));
	gl_FragColor = vec4(color, 1.0);
}

void blur_psmain()
{
	vec3 sum = vec3(0.0, 0.0, 0.0);
	for( int i = 0; i < __NumBlurSamples; ++i )
	{
		sum += texture2D( g_ColorTex, texcoord + g_BlurOffsets[i] ).rgb * vec3(g_BlurWeights[i], g_BlurWeights[i], g_BlurWeights[i]);
	}

	gl_FragColor =  vec4( sum, 1.0 );
/*
    //temp
	vec3 c0 = texture2D( g_ColorTex, texcoord + g_DownSampleOffsets0.xy ).rgb;
	vec3 c1 = texture2D( g_ColorTex, texcoord + g_DownSampleOffsets0.zw ).rgb;
	vec3 c2 = texture2D( g_ColorTex, texcoord + g_DownSampleOffsets1.xy ).rgb;
	vec3 c3 = texture2D( g_ColorTex, texcoord + g_DownSampleOffsets1.zw ).rgb;

	vec4 c = vec4( (c0 + c1 + c2 + c3) * vec3(0.25, 0.25, 0.25), 1.0 );

	gl_FragColor = c;
	*/
}

void blurblend_psmain()
{
    vec3 color1 = texture2D( g_BloomTex0, texcoord ).rgb * g_BloomColor0.rgb;
	vec3 color2 = texture2D( g_BloomTex1, texcoord ).rgb * g_BloomColor1.rgb;
	gl_FragColor = vec4( color1 + color2, 1.0 );
}

void tone_psmain()
{
	float currentLuminance = texture2D( g_LuminanceValueTex, vec2( 0.5, 0.5 ) ).r;
	//gamma = 2.2
	vec4 sceneCol = texture2D( g_SceneTex, texcoord );
	vec3 color = pow( sceneCol.rgb, vec3(2.2, 2.2, 2.2) );
	color += texture2D( g_ColorTex, texcoord ).rgb * vec3(g_BloomIntensity,g_BloomIntensity,g_BloomIntensity);

	float lumif =  g_TargetLuminance / (currentLuminance + 0.000001);
	vec3 x = color * vec3(lumif, lumif, lumif);
	float fgamma = 1.0 / 2.2;
	x = pow( x * (vec3(1.0, 1.0, 1.0) + x / vec3(g_SquaredPureWhite, g_SquaredPureWhite, g_SquaredPureWhite)) / (vec3(1.0, 1.0, 1.0) + x), vec3(fgamma, fgamma, fgamma) );

	gl_FragColor =  vec4( x, sceneCol.a );
}

#endif
