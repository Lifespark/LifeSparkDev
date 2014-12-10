
/*<Material>
  <Params>
  <macro name="LIGHT_MAP" UIName="Enable LightMap" type="bool"/>
  <macro name="RECEIVE_SHADOWS" UIName="Receive Shadows" type="bool"/>
  <macro name="SPEC_ON" UIName="Enable Specular" type="bool"/>
  <macro name="BLEND_MODE_T" UIName="BlendState" UIWidget="combobox" min="0" max="9" default="0" type="int"/>
  
  <var name="g_DiffuseTex" UIName="Diffuse Map" type="string" usage="texture" default="system/texture/light_diffuse.texture" texcoord="0"/>
  <var name="g_SpecularTex" UIName="Specular Map" type="string" usage="texture" default="system/texture/default_specular.texture" texcoord="0"/>
  <var name="g_NormalMapTex" UIName="Normal Map" type="string" usage="texture" default="system/texture/default_normal.texture" texcoord="0"/>
  <var name="g_SelfTex" UIName="Emissive Map" type="string" usage="texture" default="system/texture/default_specular.texture" texcoord="0"/>
  
  <var name="g_LightMapTex" UIName="LightMap" type="string" usage="texture" default="system/texture/light_diffuse.texture" texcoord="0"/>
  <var name="g_MipmapTex" DisplayInUI = "false" UIName="Mipmap Tex" type="string" usage="texture" default="system/texture/mipmap6.texture" texcoord="0"/>

  <var name="g_SpecGlossy" type="float" default="1.0f"/>
  <var name="g_SpecPower" type="float" default="1.0f"/>
  <var name="g_DiffusePow" type="float" default="1.0f"/>
  <var name="g_SelfPower" type="float" default="1.0f"/>
  
  <var name="g_ShadowSoftness" UIName="Shadow Softness" UIWidget="slider" min="0" max="1" default="0.2" type="float"/>
  <var name="g_ShadowIntensity" UIName="Shadow Intensity" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>
  <var name="g_ShadowBias" UIName="Shadow Bias" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>

  <var name="g_DiffuseTexSize" type="float2"/>
  <var name="g_SpecularTexSize" type="float2"/>
  <var name="g_LightMapSize" type="float2"/>
  </Params>
  <Technique>
  <Flow name = "FLOW_SCENE"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain"/>
  </Pass>
  </Technique>
  
  <Technique>
  <Flow name = "FLOW_SHADOWMAP"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psshadow"/>
  </Pass>
  </Technique>

  <Technique>
  <Flow name = "FLOW_TEXTUREONLY"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain_tex"/>
  </Pass>
  </Technique>

  <Technique>
  <Flow name = "FLOW_LIGHTONLY"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain_shading"/>
  </Pass>
  </Technique>

  <Technique>
  <Flow name = "FLOW_SHOWMIPMAP"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain_mipmap"/>
  </Pass>
  </Technique>

  <Technique>
  <Flow name = "FLOW_SHOWMIPMAPFORLIGHTMAP"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain_mipmapforlightmap"/>
  </Pass>
  </Technique>
</Material>*/

#include "header.fx"

uniform sampler2D g_DiffuseTex;
uniform sampler2D g_SpecularTex;
uniform sampler2D g_NormalMapTex;
uniform sampler2D g_SelfTex;
uniform sampler2D g_MipmapTex;


uniform sampler2D g_LightMapTex;

uniform float g_SpecGlossy;
uniform float g_SpecPower;
uniform float g_DiffusePow;
uniform float g_SelfPower;
uniform float g_ShadowSoftness; // 0.0 ~ 1.0
uniform float g_ShadowIntensity; // 0.0 ~ 1.0
uniform float g_ShadowBias; // 0.0 ~ 1.0

uniform vec2  g_DiffuseTexSize;
uniform vec2  g_SpecularTexSize;
uniform vec2  g_LightMapSize;

//uniform float g_SpecPower;
varying vec2 tex;
varying vec2 tex1; // for lightmap
varying vec3 fragPos;
varying vec3 fragNormal;
varying vec3 fragTangent;
varying vec3 fragBinormal;
varying vec2 fragTexCoord0;

//#if ( SHADOW_MAPPING > 0 )  
varying vec4  ShadowUV;
//#endif

#ifdef  VS

void vsmain()
{	
	vec3 Pos = vPos;
	vec3 Normal = vNormal;

#ifdef SKIN_MAXINFL   
	DoSkinVertex(blendindices, blendweights, vec4(vPos,1.0),vNormal,Pos,Normal );

#endif
	
	fragPos= Pos;
	fragNormal= Normal;


	gl_Position = g_WorldViewProj * vec4(Pos,1.0);

//#if ( SHADOW_MAPPING > 0 )

	ShadowUV = g_ProjectMatrix  * vec4(Pos,1.0);

//#endif

	tex  = vTexCoord0;
	tex1 = vTexCoord1;

}

#endif


#ifdef PS

void psmain()
{

	float visb = 1.0;

#ifdef SHADOW_MAPPING
	visb = ShadowPCF8x( g_Shadowmap, ShadowUV, g_ShadowSoftness, g_ShadowBias );
    visb = mix(1.0, visb, g_ShadowIntensity);
#endif

/*
	vec3 vNormalFromMap = textureLod(g_NormalMapTex, tex, 0); 
//	vec3 vNormalFromMap = texture(g_NormalMapTex, tex);    //vTexCoord0ss
	vNormalFromMap = (vNormalFromMap - 0.5) * 2.0;   //expand vec3 from texture to dir
	vNormalFromMap = normalize(vNormalFromMap); 
			
	fragTangent = normalize(fragTangent);
	fragBinormal = normalize(fragBinormal);
	fragNormal = normalize(fragNormal);
	mat3 rotation = mat3(fragTangent, fragBinormal, fragNormal); 
		
	//Inverse the matrix: just the transpose one
	rotation = transpose(rotation);
	vNormalFromMap = vNormalFromMap * rotation; 
					
	vec3 light = CaculateLightSp(fragPos, vNormalFromMap, visb, g_SpecGlossy);*/
	
	vec3 light = vec3(0.0, 0.0, 0.0); //g_AmbientLight.xyz;
	vec4 DiffColor = texture2D(g_DiffuseTex, tex);
	
#if BLEND_MODE_T == 2
	if(DiffColor.a<0.2) discard;
#endif
	vec3 diffuse = vec3(0.0, 0.0, 0.0);
	float specular = 0.0;
	CalculateLightSp(fragPos, fragNormal, 1.0, g_SpecGlossy, diffuse, specular);

	vec3 SpecColor = vec3(0.0, 0.0, 0.0);
#ifdef SPEC_ON
	SpecColor = texture2D(g_SpecularTex, tex);
	SpecColor *= specular;
#endif
	
//#ifdef LIGHT_MAP
//	vec4 packlight = texture2D(g_LightMapTex, tex1);
//	light = packlight.xyz;
//	// rgbe to float: ldexp(1.0,exposure -(int)(128)),use ldexp(x,y) in opengl 4.0 or higher version:
//	float exposure = packlight.w * 256.0 - 128.0;
//	exposure = exp2(exposure);
//	light *= exposure;
//	// gamma
//	light = pow(light, vec3(0.454545, 0.454545, 0.454545));
//	//add specular
//	light += SpecColor;		
//	light *= DiffColor.xyz;
//	
//#ifdef RECEIVE_SHADOWS
//	light *= visb;
//#endif
//
//	
//#else
//	//vec3 SelfColor = texture2D(g_SelfTex, tex).xyz;
//	//SelfColor *= g_SelfPower;
//	
//	light += diffuse;
//	light += SpecColor;
//    //light += SelfColor;
//	light *= DiffColor.xyz * visb;
//#endif


	light += diffuse;
	light += SpecColor;
    //light += SelfColor;
	light *= DiffColor.xyz * visb;
    
	gl_FragColor = vec4(light, 1.0);
}

void psmain_tex()
{
	vec4  color = texture2D(g_DiffuseTex, tex);
#if BLEND_MODE_T==2
	if(color.a<0.01)
		discard;
#endif
    gl_FragColor = color;
}

void psmain_shading()
{
    vec3 light = g_AmbientLight.xyz;
	
#ifdef LIGHT_MAP
	vec4 packlight = texture2D(g_LightMapTex, tex1);
	light = packlight.xyz;
	// rgbe to float: ldexp(1.0,exposure -(int)(128)),use ldexp(x,y) in opengl 4.0 or higher version:
	float exposure = packlight.w * 256.0 - 128.0;
	exposure = exp2(exposure);
	light *= exposure;
	// gamma
    light = pow(light, vec3(0.454545, 0.454545, 0.454545));
#else
    vec3 diffuse = vec3(0.0, 0.0, 0.0);
	float specular = 0.0;
	CalculateLightSp(fragPos, fragNormal, 1.0, g_SpecGlossy, diffuse, specular);
	light += diffuse;
#endif
	gl_FragColor = vec4(light, 1.0);
}


void psmain_mipmap()
{
	vec3 light = vec3(0.0, 0.0, 0.0); 
	vec4 DiffColor = texture2D(g_DiffuseTex, tex);
	
	vec3 diffuse = vec3(0.0, 0.0, 0.0);
	float specular = 0.0;
	CalculateLightSp(fragPos, fragNormal, 1.0, g_SpecGlossy, diffuse, specular);
	vec3 SpecColor = vec3(0.0, 0.0, 0.0);
#ifdef SPEC_ON
	SpecColor = texture2D(g_SpecularTex, tex);
	SpecColor *= specular;
#endif
	
#ifdef LIGHT_MAP
	vec4 packlight = texture2D(g_LightMapTex, tex1);
	light = packlight.xyz;
	// rgbe to float: ldexp(1.0,exposure -(int)(128)),use ldexp(x,y) in opengl 4.0 or higher version:
	float exposure = packlight.w * 256.0 - 128.0;
	exposure = exp2(exposure);
	light *= exposure;
	// gamma
    light = pow(light, vec3(0.454545, 0.454545, 0.454545));	
	//add specular
	light += SpecColor;
#else
	light += diffuse;
	light += SpecColor;
    //light += SelfColor;
	light *= DiffColor.xyz;
#endif
   
	gl_FragColor = MipmapColor(vec4(light, 1.0), g_MipmapTex, tex, g_DiffuseTexSize); 
//	gl_FragColor = MipmapColor(vec4(light, 1.0), g_MipmapTex, tex, g_SpecularTexSize); 
//	gl_FragColor = MipmapColor(vec4(light, 1.0), g_MipmapTex, tex, g_LightMapSize); 
}

void psmain_mipmapforlightmap()
{
	vec3 light = vec3(0.0, 0.0, 0.0); 
	vec4 DiffColor = texture2D(g_DiffuseTex, tex);
	
	vec3 diffuse = vec3(0.0, 0.0, 0.0);
	float specular = 0.0;
	CalculateLightSp(fragPos, fragNormal, 1.0, g_SpecGlossy, diffuse, specular);
	vec3 SpecColor = vec3(0.0, 0.0, 0.0);
#ifdef SPEC_ON
	SpecColor = texture2D(g_SpecularTex, tex);
	SpecColor *= specular;
#endif
	
#ifdef LIGHT_MAP
	vec4 packlight = texture2D(g_LightMapTex, tex1);
	light = packlight.xyz;
	// rgbe to float: ldexp(1.0,exposure -(int)(128)),use ldexp(x,y) in opengl 4.0 or higher version:
	float exposure = packlight.w * 256.0 - 128.0;
	exposure = exp2(exposure);
	light *= exposure;
	// gamma
    light = pow(light, vec3(0.454545, 0.454545, 0.454545));	
	//add specular
	light += SpecColor;
#else
	light += diffuse;
	light += SpecColor;
    //light += SelfColor;
	light *= DiffColor.xyz;
#endif
   
	gl_FragColor = MipmapColor(vec4(light, 1.0), g_MipmapTex, tex, g_LightMapSize); 
}

void psshadow()
{    
	vec4 color = texture2D(g_DiffuseTex, tex);
#if BLEND_MODE_T==2
    if (color.a < 0.01) discard;
#endif
	gl_FragColor = vec4(gl_FragCoord.zzz, 1.0);
}


#endif
