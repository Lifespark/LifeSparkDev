
/*<Material>
  <Params>
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

varying vec4 color;

#ifdef VS

void vsmain()
{	
	gl_Position = g_WorldViewProj * vec4(vPos,1.0);
	color = vColor.zyxw/255.0;
}

#endif


#ifdef PS

void psmain()
{
	gl_FragColor = color;
}

#endif



