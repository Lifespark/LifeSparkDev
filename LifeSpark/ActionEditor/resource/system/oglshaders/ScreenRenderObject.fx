/*<Material>
  <Params>
  <var name="g_DiffuseTex" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_TexelSize" default="0 0" type="float2"/>
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
uniform vec2 g_TexelSize;

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
	vec4 texColor0 = texture2D(g_DiffuseTex, tex + vec2(0.0, 0.0)*g_TexelSize);
	vec4 texColor1 = texture2D(g_DiffuseTex, tex + vec2(1.0, 0.0)*g_TexelSize);
	vec4 texColor2 = texture2D(g_DiffuseTex, tex + vec2(0.0, 1.0)*g_TexelSize);
	vec4 texColor3 = texture2D(g_DiffuseTex, tex + vec2(1.0, 1.0)*g_TexelSize);
    vec4 texColor = (texColor0 + texColor1 + texColor2 + texColor3) * 0.25;
	gl_FragColor = texColor;	
}

#endif
