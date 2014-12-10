
/*<Material>
  <Params>
  <var name="g_DiffuseTex" UIName="Diffuse Map" default="system/texture/toon_default_diffuse.texture" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_OutlineTex" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_Brightness" UIName="Brightness" UIWidget="slider" min="0" max="2" default="1.0" type="float"/>
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

uniform sampler2D g_DiffuseTex;

// g_OutlineTex:
//   - Red channel is the outline mask, white for drawing outlines.
//   - Green channel is the detail silhouette masks, white for drawing detail silhouette.
uniform sampler2D g_OutlineTex;

uniform float  g_Brightness; // 0.0 ~ 2.0
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

#ifdef SKIN_MAXINFL 
	DoSkinVertexTBN(blendindices, blendweights, 
					vec4(vPos,1.0),vNormal,vTangent,vBinormal,
					Pos,Normal,Tangent,Binormal);
#endif
	
 
#ifdef SILHOUETTE_ON 
    // silhouette line rendering, will use alpha blend, so make it closer to the camera
    vec4 PosView = g_WorldViewProj * vec4(Pos,1.0);
    PosView.z -= 3e-6;
	gl_Position = PosView;
#else
    Pos += normalize(Normal) * 0.003 * g_OutlineSizeSave;
	gl_Position = g_WorldViewProj * vec4(Pos,1.0);
#endif

	tex = vTexCoord0;
	fragPos = Pos;
	fragNormal = Normal;
	shadowUV = g_ProjectMatrix  * vec4(Pos,1.0);
}
#endif

#ifdef PS
void psmain()
{
    if (texture2D(g_DiffuseTex, tex).a < 0.01) discard;

	vec4 color = g_OutlineColorSave;
    color.rgb *= g_Brightness;
	color.w *= g_FadeColor.w;

#ifdef SILHOUETTE_ON
    // silhouette line rendering
    color.w *= texture2D(g_OutlineTex, tex).g;

#else
    color.w *= texture2D(g_OutlineTex, tex).r;
#endif
        
    if (color.w < 1e-2)
        discard;
        
    color.rgb += g_FadeColor.rgb;

	gl_FragColor = color;
}
#endif
