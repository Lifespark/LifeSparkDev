
/*<Material>
  <Params>
  <var name="g_DiffuseTex" type="string" usage="texture" default="system/texture/default_diffuse.texture" texcoord="0"/>
  <macro name="BLEND_MODE_T" UIName="BlendState" UIWidget="combobox" min="0" max="9" default="0" type="int"/>
  <macro name="SELF_UVAIM" UIName="DIFFUSE_UVAIM" UIWidget="checkbox" min="false" max="true" default="false" type="bool"/>
  <var name="g_TransU" UIName="UVAim_SpeedU" UIWidget="slider" min="-2.0" max="2.0" default="0" type="float"/>
  <var name="g_TransV" UIName="UVAim_SpeedV" UIWidget="slider" min="-2.0" max="2.0" default="0" type="float"/>
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
uniform float g_TransU;
uniform float g_TransV;

varying vec2 uv0;

#ifdef VS

void vsmain()
{	
	vec3 Pos = vPos;
	vec3 Normal = vNormal;

#ifdef SKIN_MAXINFL  
	DoSkinVertex(blendindices, blendweights, vec4(vPos,1.0),vNormal,Pos,Normal );
#endif

	gl_Position = g_WorldViewProj * vec4(Pos,1.0);

#ifdef SELF_UVAIM
    float param = g_Time * 0.05; // 0.05 as speed scale
	uv0 = vTexCoord0 + vec2(g_TransU, g_TransV) * vec2(param, param);
#else
	uv0 = vTexCoord0;
#endif

}

#endif


#ifdef PS
void psmain()
{	
    vec4 diffColor = texture2D(g_DiffuseTex, uv0 );
#if BLEND_MODE_T == 2
	if(diffColor.a<0.1) discard;
#endif
	gl_FragColor = diffColor;
}

#endif
