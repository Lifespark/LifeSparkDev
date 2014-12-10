
/*<Material>
  <Params>
  <var name="g_OutlineSizeSave" type="float"/>
  <var name="g_OutlineColorSave" type="float4"/>
  </Params>
  <Technique>
  <Flow name = "FLOW_SCENE"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain"/>
  <renderstate blendstate = "BLEND_ALPHABLEND"/>

  </Pass>
  </Technique>
</Material>*/

#include "header.fx"

uniform float  g_OutlineSizeSave;
uniform vec4 g_OutlineColorSave;

varying vec2 tex;
varying vec3 fragPos;
varying vec3 fragNormal;
varying vec4 shadowUV;

#ifdef  VS
void vsmain()
{	

	vec3 Pos = vPos;
	vec3 Normal = vNormal;
	vec3 Tangent = vTangent;
	vec3 Binormal = vBinormal;
 
    Pos += normalize(Normal) * 0.003 * g_OutlineSizeSave;
	gl_Position = g_WorldViewProj * vec4(Pos,1.0);

	tex = vTexCoord0;
	fragPos = Pos;
	fragNormal = Normal;
	shadowUV = g_ProjectMatrix  * vec4(Pos,1.0);
}
#endif

#ifdef PS
void psmain()
{
	vec4 color = g_OutlineColorSave;

    if (gl_FrontFacing)
        discard;

	gl_FragColor = color;
}
#endif
