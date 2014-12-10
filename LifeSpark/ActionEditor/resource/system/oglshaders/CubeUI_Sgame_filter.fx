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
uniform vec2 g_TexSize;

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
	//vec4 sample0 = texture2D(texture1, texUV) * vetexColor;
	// vec4 sample0 = texture2D(texture1, texUV + vec2(0.0, 0.0)*g_TexSize);
	// sample0 += texture2D(texture1, texUV + vec2(1.0, 0.0)*g_TexSize);
	// sample0 += texture2D(texture1, texUV + vec2(0.0, 1.0)*g_TexSize);
	// sample0 += texture2D(texture1, texUV + vec2(1.0, 1.0)*g_TexSize);
	// sample0 *= 0.25;
	// sample0 *= vetexColor;
	// sample0.x = 1.0;
	
	//gl_FragColor = sample0;// + vetexColor1;
	//gl_FragColor = texColor1.zyxw;
	
	vec4 texColor0 = texture2D(texture1, texUV + vec2(0.0, 0.0)*g_TexSize);
	vec4 texColor1 = texture2D(texture1, texUV + vec2(1.0, 0.0)*g_TexSize);
	vec4 texColor2 = texture2D(texture1, texUV + vec2(0.0, 1.0)*g_TexSize);
	vec4 texColor3 = texture2D(texture1, texUV + vec2(1.0, 1.0)*g_TexSize);
    vec4 texColor = (texColor0 + texColor1 + texColor2 + texColor3) * 0.25;
	gl_FragColor = texColor;
}

#endif
