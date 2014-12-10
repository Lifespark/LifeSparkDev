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
varying vec3 fragPos;
varying vec4 fragColor;

#ifdef VS

void vsmain()
{	
	vec3 Pos = vPos;
	
	fragPos = Pos;
	fragColor = vColor;
	
	gl_Position = g_WorldViewProj * vec4(Pos,1.0);

	tex = vTexCoord0;
}

#endif

#ifdef PS

void psmain()
{
	vec2 offsets[4];
	float scale = 1 / 512.0;
	
	offsets[0].x = -1;
	offsets[0].y = -1;
	offsets[0] *= scale;

	offsets[1].x = 1;
	offsets[1].y = -1;
	offsets[1] *= scale;

	offsets[2].x = 1;
	offsets[2].y = 1;
	offsets[2] *= scale;

	offsets[3].x = -1;
	offsets[3].y = 1;
	offsets[3] *= scale;
	
	float maxValue = 0;
	for (int i=0; i<4; i++)
	{
		vec4 texColor = texture2D(g_DiffuseTex, tex+offsets[i]);
		maxValue = max(maxValue, texColor.a);
	}

	gl_FragColor.rgb = fragColor.rgb / 255.0;
	gl_FragColor.a = (maxValue) * fragColor.a / 255.0;
}

#endif
