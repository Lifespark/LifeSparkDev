/*<Material>
  <Params>
	<var name="gInputSampler" type="string" usage="texture" default="" texcoord="0"/>
	<var name="gDistortSampler" type="string" usage="texture" default="" texcoord="0"/>
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

uniform sampler2D 	gInputSampler;
uniform sampler2D 	gDistortSampler;

varying vec2 		tex0;


#ifdef VS

void vsmain()
{	
	vec2 Pos = sign(gl_Vertex.xy);   
	gl_Position = vec4(Pos.xy, 0.0, 1.0);
	tex0.xy = Pos * 0.5 + 0.5;   
}

#endif

#ifdef PS

void psmain()
{
	// scene pixel
	vec4 cSource = texture2D (gInputSampler, tex0);
	
	// distort pixel
	vec4 vNormal = texture2D (gDistortSampler, tex0);
	
	// calculate uv offset
	vNormal.rgb = (vNormal.rgb * 2.0) - 1.0;
	vec2 offset = vNormal.xy * 0.5;// * 0.18;
	
	// square
	cSource.a *= cSource.a;   
	
	// compute distorted texture coords
	offset.xy = ((offset.xy * cSource.a) * 0.004f) + tex0;
	
	offset.x = clamp( offset.x, 0.0, 1.0 );
	offset.y = clamp( offset.y, 0.0, 1.0 );
	
	// fetch the distorted color
	gl_FragColor.rgb = texture2D (gInputSampler, offset);
	gl_FragColor.a = 1.0f;
	
}

#endif
