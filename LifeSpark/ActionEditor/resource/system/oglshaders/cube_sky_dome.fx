/*<Material>
  <Params>

  <var name="g_AsterTex" UIName="Aster Map" type="string" usage="texture" default="system/texture/default_diffuse.texture" texcoord="0"/>
  <var name="g_ColorTex" UIName="Color Map" type="string" usage="texture" default="system/texture/default_diffuse.texture" texcoord="0"/>
  <var name="g_FogTex" UIName="Fog" type="string" usage="texture" default="system/texture/default_diffuse.texture" texcoord="0"/>
  
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

uniform sampler2D g_AsterTex;
uniform sampler2D g_ColorTex;
uniform sampler2D g_FogTex;
uniform float g_GameTime;

varying vec3 fragPosWorld;
varying vec2 fragTexCoord0;

#ifdef  VS

void vsmain()
{    
	vec3 Pos = vPos;
	vec3 Normal = vNormal;

	fragPosWorld = Pos;
    fragTexCoord0 = vTexCoord0;

	gl_Position	= g_WorldViewProj * vec4(Pos, 1.0);
}

#endif

#ifdef PS

void psmain()
{
    vec3 worldEyePos = vec3(g_World * g_Eye);
	vec2 uv = fragTexCoord0 + vec2(1.5-g_GameTime,0);
	uv.x = mod(uv.x, 1);

	vec4 asterColor = texture2D(g_AsterTex, uv);
	vec4 fogColor = texture2D(g_FogTex, vec2(g_GameTime,0.5-fragPosWorld.z/500.0));	

	vec3 sunDir = normalize(vec3(cos((g_GameTime*10-0.125)*6.2832), 0, sin((g_GameTime*10-0.125)*6.2832)));
	vec3 skyDir = normalize(fragPosWorld - worldEyePos);
	float dotDir = (1.0 - dot(sunDir, skyDir)) * 0.5;
	vec4 domeColor = texture2D(g_ColorTex, vec2(g_GameTime,dotDir));

	gl_FragColor = fogColor+ domeColor + asterColor;
}

#endif
