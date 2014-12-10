/*<Material>
  <Params>
  <var name="texture1" type="string" usage="texture" default="" texcoord="0"/>
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

uniform sampler2D texture1;
uniform sampler2D texture2;
uniform float texBlendWeight;
uniform vec4 addColor;
uniform vec4 mulColor;

varying vec2 texUV;

#ifdef VS

void vsmain()
{	
	gl_Position = g_WorldViewProj * vec4(vPos,1.0);
	texUV = vTexCoord0;
}

#endif

#ifdef PS

void psmain()
{
	vec4 texColor1 = texture2D(texture1, texUV);
	vec4 texColor2 = texture2D(texture2, texUV);
	gl_FragColor = mix(texColor1, texColor2, texBlendWeight) * mulColor + addColor;
}

#endif
