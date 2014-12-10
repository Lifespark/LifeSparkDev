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
varying vec2 texUV;
varying vec4 vetexColor;
varying vec4 vetexColor1;

#ifdef VS

void vsmain()
{	
	gl_Position = g_WorldViewProj * vec4(vPos,1.0);
	texUV = vTexCoord0;
	vetexColor = vColor.zyxw / 255.0;
	vetexColor1 = vUiPos;
}

#endif

#ifdef PS

void psmain()
{
	vec4 sample0 = texture2D(texture1, texUV) * vetexColor;
	gl_FragColor = sample0 + vetexColor1;
	//gl_FragColor = texColor1.zyxw;
}

#endif
