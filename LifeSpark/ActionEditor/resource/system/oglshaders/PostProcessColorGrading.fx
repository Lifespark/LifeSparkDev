/*<Material>
  <Params>
  <var name="g_SceneTexture" type="string" usage="texture" default="" texcoord="0"/>
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

uniform sampler2D g_SceneTexture;
uniform sampler3D g_ColorLUTTexture;
uniform vec2 g_ColorLUTParam;

varying vec2 tex;

#ifdef VS

void vsmain()
{	
	gl_Position = vec4(vPos, 1.0);
	tex = vPos.xy * 0.5 + 0.5;
}

#endif

#ifdef PS

void psmain()
{
   vec4 sceneCol = texture2D( g_SceneTexture, tex );
   vec3 coord = sceneCol.xyz * vec3(g_ColorLUTParam.y, g_ColorLUTParam.y,g_ColorLUTParam.y) + vec3(g_ColorLUTParam.x, g_ColorLUTParam.x, g_ColorLUTParam.x);
   //vec3 coord =  sceneCol.xyz * g_ColorLUTParam.y + g_ColorLUTParam.x;
   gl_FragColor = texture3D( g_ColorLUTTexture, coord);
}

#endif
