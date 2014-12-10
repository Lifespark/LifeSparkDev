
/*<Material>
  <Params>
  <macro name="LIGHT_MAP" UIName="Enable LightMap" type="bool"/>
  <macro name="SPEC_ON" UIName="Enable Specular" type="bool"/>
  <macro name="BLEND_MODE_T" UIName="BlendState" UIWidget="combobox" min="0" max="9" default="0" type="int"/>
  
  <var name="g_DiffuseTex" UIName="Diffuse Map" type="string" usage="texture" default="system/texture/light_diffuse.texture" texcoord="0"/>
  <var name="g_SpecularTex" UIName="Specular Map" type="string" usage="texture" default="system/texture/default_specular.texture" texcoord="0"/>
  
  <var name="g_LightMapTex" UIName="LightMap" type="string" usage="texture" default="system/texture/light_diffuse.texture" texcoord="0"/>
  
  <var name="g_WaveTex" UIName="WaveMap" type="string" usage="texture" default="system/texture/default_wave.texture" texcoord="0"/>

  <var name="g_SpecGlossy" type="float" default="1.0f"/>
  <var name="g_SpecPower" type="float" default="1.0f"/>
  <var name="g_DiffusePow" type="float" default="1.0f"/>
  
  <var name="g_Time" type="float" default="1.0f"/>
  <var name="g_WaveScale" type="float" default="1.0f" UIWidget="slider" min="0" max="20"/>
  <var name="g_WaveScroll" type="float" default="1.0f" UIWidget="slider" min="0" max="1"/>
  <var name="g_NoiseScale" type="float" default="0.05f" UIWidget="slider" min="0" max="1"/>
  
  </Params>
  <Technique>
  <Flow name = "FLOW_SCENE"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain"/>
  </Pass>
  </Technique>
</Material>*/
  

#include "header.fx"

uniform sampler2D g_DiffuseTex;
uniform sampler2D g_SpecularTex;
uniform sampler2D g_NormalMapTex;


uniform sampler2D g_LightMapTex;
uniform sampler2D g_WaveTex;

uniform float g_SpecGlossy;
uniform float g_SpecPower;
uniform float g_DiffusePow;

uniform float g_WaveScroll;
uniform float g_WaveScale;
uniform float g_NoiseScale;


//uniform float g_SpecPower;
varying vec2 tex;
varying vec2 tex1; // for lightmap
varying vec2 noisetex; // for wave
varying vec3 fragPos;
varying vec3 fragNormal;


#ifdef  VS

void vsmain()
{	
	vec3 Pos = vPos;
	vec3 Normal = vNormal;
	
	fragPos= Pos;
	fragNormal= Normal;


	gl_Position = g_WorldViewProj * vec4(Pos,1.0);

	tex  = vTexCoord0;
	tex1 = vTexCoord1;
	
	noisetex = (tex + g_Time * g_WaveScroll) * g_WaveScale;
}

#endif


#ifdef PS

void psmain()
{
	vec3 noiseNormal = (texture2D(g_WaveTex, (noisetex.xy / 5)).rgb - 0.5).rbg * g_NoiseScale;
        vec2 tex_ = tex;
	tex_ += noiseNormal.xz;
	vec2 tex1_ = tex_;

	float visb = 1.0;

	vec3 light = vec3(0.0, 0.0, 0.0); //g_AmbientLight.xyz;
	vec4 DiffColor = texture2D(g_DiffuseTex, tex_);
	
#if BLEND_MODE_T == 2
	if(DiffColor.a<0.2) discard;
#endif
	vec3 diffuse = vec3(0.0, 0.0, 0.0);
	float specular = 0.0;
	CalculateLightSp(fragPos, fragNormal, 1.0, g_SpecGlossy, diffuse, specular);

	vec3 SpecColor = vec3(0.0, 0.0, 0.0);
#ifdef SPEC_ON
	SpecColor = texture2D(g_SpecularTex, tex_).xyz;
	SpecColor *= specular;
#endif
	
#ifdef LIGHT_MAP
	//vec4 packlight = texture2D(g_LightMapTex, tex1_);
	//light = packlight.xyz;
	//// rgbe to float: ldexp(1.0,exposure -(int)(128)),use ldexp(x,y) in opengl 4.0 or higher version:
	//float exposure = packlight.w * 256.0 - 128.0;
	//exposure = exp2(exposure);
	//light *= exposure;
	// gamma
	//light = pow(light, vec3(0.454545, 0.454545, 0.454545));
	//add specular
	//light += SpecColor;
	//light *= DiffColor.xyz;
//#else
	//light += diffuse;
	//light += SpecColor;
	//light *= DiffColor.xyz * visb;
	light = DiffColor.xyz;
	light += SpecColor;
#endif

	gl_FragColor = vec4(light, 1);
}

#endif

