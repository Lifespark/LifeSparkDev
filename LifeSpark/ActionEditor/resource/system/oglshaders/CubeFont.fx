/*<Material>
  <Params>
  <var name="g_DiffuseTex" type="string" usage="texture" default="" texcoord="0"/>
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

varying vec2 tex;
varying vec4 vertColor;

#ifdef VS

void vsmain()
{	
	vec3 Pos = vPos;
	
	vertColor = vColor;
	
	gl_Position = g_WorldViewProj * vec4(Pos,1.0);

	tex = vTexCoord0;
}

#endif

#ifdef PS

void psmain()
{
	vec4 texColor = texture2D(g_DiffuseTex, tex);
	float alpha = texColor.a;
	alpha = 1.0 - alpha;
	alpha *= alpha;
	alpha = 1.0 - alpha;
	gl_FragColor.rgb = vertColor.rgb / 255.0;
	gl_FragColor.a = alpha * vertColor.a / 255.0;
}

#endif
