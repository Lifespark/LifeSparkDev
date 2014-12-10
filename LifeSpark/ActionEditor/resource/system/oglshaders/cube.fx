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

varying vec4 fragColor;
varying vec3 fragPosWorld;
varying vec3 fragNormal;
varying vec2 fragTexCoord0;

#ifdef  VS

void vsmain()
{
	vec3 Pos = vPos;
	vec3 Normal = vNormal;

	fragPosWorld = Pos;
    fragTexCoord0 = vTexCoord0 / 32767.0;
    fragNormal = vNormal;
    fragColor = vColor / 255.0;

	gl_Position	= g_WorldViewProj * vec4(Pos, 1.0);
}

#endif

#ifdef PS

void psmain()
{
    vec3 worldEyePos = vec3(g_World * g_Eye);

	float x = texture2D(g_FogNoiseTex, vec2(abs(mod(fragPosWorld.x/500.0+g_GameTime, 1)), abs(mod(fragPosWorld.z/500.0, 1)))).x;
	float y = texture2D(g_FogNoiseTex, vec2(abs(mod(fragPosWorld.y/500.0+g_GameTime, 1)), abs(mod(fragPosWorld.z/500.0, 1)))).x;
	float z = texture2D(g_FogNoiseTex, vec2(abs(mod(fragPosWorld.x/500.0+g_GameTime, 1)), abs(mod(fragPosWorld.y/500.0+g_GameTime, 1)))).x;
	float distNoise = (x + y + z) * 0.33333;
	float altitude = min(1.0, abs(fragPosWorld.z) / 300.0);
	float fogDistance = mix(600.0, 900.0, altitude);
	float dist = max(0, length(fragPosWorld - worldEyePos) + distNoise * altitude * fogDistance);
	
	vec4 diffcolor = texture2D(g_DiffuseTex, fragTexCoord0) * (texture2D(g_SkyLightTex, vec2(g_GameTime, 1.0-fragColor.g)) + fragColor.b * vec4(1.0, 0.85, 0.6, 1.0) * (vec4(1,1,1,1) - texture2D(g_SkyLightTex, vec2(g_GameTime, 1.0-fragColor.g)))) * fragColor.r;
	vec4 fogcolor = texture2D(g_FogTex, vec2(g_GameTime,0.5-fragPosWorld.z/500.0));	

	float ave = (diffcolor.r + diffcolor.g + diffcolor.b) * 0.33333;
	diffcolor = mix(diffcolor, vec4(ave, ave, ave, ave) * fogcolor, altitude);

	vec3 sunDir = normalize(vec3(cos((g_GameTime-0.125)*6.2832), 0, sin((g_GameTime-0.125)*6.2832)));
	vec3 skyDir = normalize(fragPosWorld - worldEyePos);
	float dotDir = (1.0 - dot(sunDir, skyDir)) * 0.5;
	vec4 domeColor = texture2D(g_SkyColorTex, vec2(g_GameTime,dotDir));	
	
	gl_FragColor = diffcolor;//mix(diffcolor, fogcolor+domeColor, pow(min(1, dist/1600.0), 3));
	gl_FragColor.a = mix(diffcolor.a, 1, min(1, dist/128.0));
}

#endif