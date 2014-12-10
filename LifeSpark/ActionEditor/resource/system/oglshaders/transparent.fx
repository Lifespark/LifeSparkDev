
/*<Material>
  <Params>
  <macro name="LIGHT_MAP" UIName="Enable LightMap" type="bool"/>
  
  <var name="g_DiffuseTex" UIName="Diffuse Map" type="string" usage="texture" default="system/texture/light_diffuse.texture" texcoord="0"/>
  <var name="g_SpecularTex" UIName="Specular Map" type="string" usage="texture" default="system/texture/default_specular.texture" texcoord="0"/>
  <var name="g_NormalMapTex" UIName="Normal Map" type="string" usage="texture" default="system/texture/default_normal.texture" texcoord="0"/>
  <var name="g_SelfTex" UIName="Emissive Map" type="string" usage="texture" default="system/texture/default_specular.texture" texcoord="0"/>
  
  <var name="g_LightMapTex" UIName="LightMap" type="string" usage="texture" default="system/texture/light_diffuse.texture" texcoord="0"/>
  <var name="g_LmAoTex" UIName="AO Map" type="string" usage="texture" default="system/texture/light_diffuse.texture" texcoord="0"/>

  <var name="g_SpecGlossy" type="float" default="1.0f"/>
  <var name="g_SpecPower" type="float" default="1.0f"/>
  <var name="g_DiffusePow" type="float" default="1.0f"/>
  <var name="g_SelfPower" type="float" default="1.0f"/>
  
  <var name="g_ShadowSoftness" UIName="Shadow Softness" UIWidget="slider" min="0" max="1" default="0.2" type="float"/>
  <var name="g_ShadowIntensity" UIName="Shadow Intensity" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>
  <var name="g_ShadowBias" UIName="Shadow Bias" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>

  <var name="g_LmIntensity" UIName="lm intensity" UIWidget="slider" min="0" max="2" default="1.0" type="float"/>
  <var name="g_LmAoIntensity" UIName="Ao intensity" UIWidget="slider" min="0" max="2" default="0.0" type="float"/>
  </Params>
  <Technique>
  <Flow name = "FLOW_SCENE"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain"/>
  <renderstate blendstate = "BLEND_ALPHATEST"/>
  </Pass>
  </Technique>
  
  <Technique>
  <Flow name = "FLOW_SHADOWMAP"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain"/>
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
</Material>*/

#include "header.fx"

uniform sampler2D g_DiffuseTex;
uniform sampler2D g_SpecularTex;
uniform sampler2D g_NormalMapTex;
uniform sampler2D g_SelfTex;

uniform sampler2D g_LightMapTex;
uniform sampler2D g_LmAoTex;

uniform float g_SpecGlossy;
uniform float g_SpecPower;
uniform float g_DiffusePow;
uniform float g_SelfPower;
uniform float g_ShadowSoftness; // 0.0 ~ 1.0
uniform float g_ShadowIntensity; // 0.0 ~ 1.0
uniform float g_ShadowBias; // 0.0 ~ 1.0

uniform float g_LmIntensity;//0.0 ~ 2.0
uniform float g_LmAoIntensity;//0.0~2.0


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
	
#ifdef LIGHT_MAP
    light = texture2D(g_LightMapTex, tex1).xyz;
	light *= g_LmIntensity;
    vec3 lm_ao = texture2D(g_LmAoTex, tex1).xyz;
	light = g_LmAoIntensity * lm_ao + light;
	light *= DiffColor.xyz;
#else
    vec3 diffuse = vec3(0.0, 0.0, 0.0);
	float specular = 0.0;
	CalculateLightSp(fragPos, fragNormal, 1.0, g_SpecGlossy, diffuse, specular);
	vec4 SpecColor = texture2D(g_SpecularTex, tex);

	//vec3 SelfColor = texture2D(g_SelfTex, tex).xyz;
	//SelfColor *= g_SelfPower;
	
	light += diffuse;
	light += specular * SpecColor.xyz;
    //light += SelfColor;
	light *= DiffColor.xyz * visb;
#endif
    
	gl_FragColor = vec4(light, DiffColor.a);
}

void psmain_tex()
{
    gl_FragColor = texture2D(g_DiffuseTex, tex);
}

void psmain_shading()
{
    vec3 light = g_AmbientLight.xyz;
	
#ifdef LIGHT_MAP
    light = texture2D(g_LightMapTex, tex1).xyz;
	light *= g_LmIntensity;
    vec3 lm_ao = texture2D(g_LmAoTex, tex1).xyz;
	light = g_LmAoIntensity * lm_ao + light;
#else
    vec3 diffuse = vec3(0.0, 0.0, 0.0);
	float specular = 0.0;
	CalculateLightSp(fragPos, fragNormal, 1.0, g_SpecGlossy, diffuse, specular);
	light += diffuse;
#endif
	gl_FragColor = vec4(light, 1.0);
}

#endif
