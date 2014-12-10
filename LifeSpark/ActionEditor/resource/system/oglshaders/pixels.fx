/*<Material>
  <Params>

  <var name="g_DiffuseTex" UIName="Diffuse Map" type="string" usage="texture" default="system/texture/default_diffuse.texture" texcoord="0"/>
  <var name="g_FogTex" UIName="Fog" type="string" usage="texture" default="system/texture/default_diffuse.texture" texcoord="0"/>
  <var name="g_SkyLightTex" UIName="SkyLight" type="string" usage="texture" default="system/texture/default_diffuse.texture" texcoord="0"/>
  <var name="g_FogNoiseTex" UIName="FogNoise" type="string" usage="texture" default="system/texture/default_diffuse.texture" texcoord="0"/>
  <var name="g_SkyColorTex" UIName="SkyColor" type="string" usage="texture" default="system/texture/default_diffuse.texture" texcoord="0"/>
  
  <var name="g_GameTime" type="float" default="0.0f"/>

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
uniform sampler2D g_FogTex;
uniform sampler2D g_SkyLightTex;
uniform sampler2D g_FogNoiseTex;
uniform sampler2D g_SkyColorTex;

uniform float g_GameTime;

varying vec3 fragNormal;
varying vec2 fragTexCoord0;

#ifdef  VS

void vsmain()
{	
	vec3 Pos = vPos;
	vec3 Normal = vNormal;

#ifdef SKIN_MAXINFL   
	DoSkinVertex(blendindices, blendweights, vec4(vPos,1.0),vNormal,Pos,Normal );

#endif
	
    fragTexCoord0 = vTexCoord0;
	fragNormal = Normal;

	gl_Position = g_WorldViewProj * vec4(Pos,1.0);
}

#endif

#ifdef PS

void psmain()
{
	vec4 outColor = vec4(texture2D(g_DiffuseTex, fragTexCoord0).xyz, 1.0);
	float light = dot(normalize(vec3(1.0, 1.0, 1.0)), fragNormal);
	gl_FragColor = outColor * (light * 0.5 + 0.5);
}

#endif