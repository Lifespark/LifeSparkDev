
/*<Material>
  <Params>
	<var name="g_DiffuseTex" UIName="Diffuse"  default ="system/texture/default_diffuse.texture" type = "string" usage="texture" default="" texcoord="0"/>
	<var name="g_MaskTex1" UIName="Mask1" default ="system/texture/default_mask.texture" type = "string" usage="texture" default="" texcoord="0"/>
	<var name="g_MaskTex2" UIName="Mask2" default ="system/texture/default_mask.texture" type = "string" usage="texture" default="" texcoord="0"/>
	<var name="g_DistortTex" UIName="Distort" default ="system/texture/default_mask.texture" type = "string" usage="texture" default="" texcoord="0"/>
	<macro name="DOUBLE_SIDED" UIName="Double Sided" UIWidget="checkbox" min="false" max="true" default="false" type="bool"/>
	<macro name="SELF_UVAIM" UIName="DIFFUSE_UVAIM" UIWidget="checkbox" min="false" max="true" default="false" type="bool"/>
	<macro name="ENABLE_MASK" UIName="ENABLE_MASK" UIWidget="checkbox" min="false" max="true" default="false" type="bool"/>
	<macro name="BLEND_MODE_T" UIName="BlendState" UIWidget="combobox" min="0" max="9" default="0" type="int"/>
	<var name="g_FxTime" UIName="FxTime" UIWidget="slider" min="0.0" default="0" type="float"/>
	<var name="g_TransU" UIName="UVAim_SpeedU" UIWidget="slider" min="-5.0" max="5.0" default="0" type="float"/>
	<var name="g_TransV" UIName="UVAim_SpeedV" UIWidget="slider" min="-5.0" max="5.0" default="0" type="float"/>
	<var name="g_ScaleU" UIName="UV_ScaleU" UIWidget="slider" min="0.1" max="10.0" default="1" type="float"/>
	<var name="g_ScaleV" UIName="UV_ScaleV" UIWidget="slider" min="0.1" max="10.0" default="1" type="float"/>
	<var name="g_DiffuseInitOffsetU" UIName="Diffuse_Init_Offset_U" UIWidget="slider" min="-1.0" max="1.0" default="0" type="float"/>
	<var name="g_DiffuseInitOffsetV" UIName="Diffuse_Init_Offset_V" UIWidget="slider" min="-1.0" max="1.0" default="0" type="float"/>
	<var name="g_DiffuseInitAngle" UIName="Diffuse_Init_Angle" UIWidget="slider" min="0" max="360.0" default="0" type="float"/>
	<var name="g_MaskTransU" UIName="MaskUVAim_SpeedU" UIWidget="slider" min="-200.0" max="200.0" default="0" type="float"/>
	<var name="g_MaskTransV" UIName="MaskUVAim_SpeedV" UIWidget="slider" min="-200.0" max="200.0" default="0" type="float"/>
	<var name="g_MaskScaleU" UIName="MaskUV_ScaleU" UIWidget="slider" min="0.1" max="10.0" default="1" type="float"/>
	<var name="g_MaskScaleV" UIName="MaskUV_ScaleV" UIWidget="slider" min="0.1" max="10.0" default="1" type="float"/>
	<var name="g_MaskInitOffsetU" UIName="Mask_Init_Offset_U" UIWidget="slider" min="-1.0" max="1.0" default="0" type="float"/>
	<var name="g_MaskInitOffsetV" UIName="Mask_Init_Offset_V" UIWidget="slider" min="-1.0" max="1.0" default="0" type="float"/>
	<var name="g_MaskInitAngle" UIName="Mask_Init_Angle" UIWidget="slider" min="0" max="360.0" default="0" type="float"/>
  </Params>
  <Technique>
  <Flow name = "FLOW_SCENE"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain"/>
  </Pass>
  </Technique>
  
  <Technique>
  <Flow name = "FLOW_DISTORT"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psdistortmain"/>
  </Pass>
  </Technique>
</Material>*/

#include "header.fx"

uniform sampler2D g_DiffuseTex;
uniform sampler2D g_MaskTex1;
uniform sampler2D g_MaskTex2;
uniform sampler2D g_DistortTex;

uniform float g_TransU;
uniform float g_TransV;
uniform float g_ScaleU;
uniform float g_ScaleV;
uniform float g_DiffuseInitOffsetU;
uniform float g_DiffuseInitOffsetV;
uniform float g_DiffuseInitAngle;

uniform float g_MaskTransU;
uniform float g_MaskTransV;
uniform float g_MaskScaleU;
uniform float g_MaskScaleV;
uniform float g_MaskInitOffsetU;
uniform float g_MaskInitOffsetV;
uniform float g_MaskInitAngle;

uniform float g_FxTime;

varying vec4 color0;
varying vec2 uv0;
varying vec2 mask_uv;

#ifdef VS

void vsmain()
{
	gl_Position = g_WorldViewProj * vec4(vPos, 1.0);
	vec4 vert_color = vColor.bgra/255.0;
	color0 = vert_color * g_color_alpha;

#ifdef SELF_UVAIM
	uv0 = vTexCoord0 * vec2(g_ScaleU, g_ScaleV) + g_FxTime * vec2(g_TransU, g_TransV);
#else
	uv0 = vTexCoord0 * vec2(g_ScaleU, g_ScaleV) ;
#endif
	float angle = 0.0175 * g_DiffuseInitAngle;
	float sins = sin(angle);
	float coss = cos(angle);
	vec2 uv;
	uv.x = coss * uv0.x + sins * uv0.y + g_DiffuseInitOffsetU;
	uv.y = -sins * uv0.x + coss * uv0.y + g_DiffuseInitOffsetV;
	uv0 = uv;

#ifdef ENABLE_MASK
    mask_uv = vTexCoord0  * vec2(g_MaskScaleU, g_MaskScaleV)+ g_FxTime * vec2(g_MaskTransU, g_MaskTransV);

	angle = 0.0175 * g_MaskInitAngle;
	sins = sin(angle);
	coss = cos(angle);
	uv.x = coss * mask_uv.x + sins * mask_uv.y + g_MaskInitOffsetU;
	uv.y = -sins * mask_uv.x + coss * mask_uv.y + g_MaskInitOffsetV;

	mask_uv = uv;
#endif

}

#endif

#ifdef PS

void psmain()
{
	vec4 diffcolor = texture2D(g_DiffuseTex, uv0);

	vec4 clr = color0 * diffcolor;
#ifdef ENABLE_MASK	
	vec4 mask1 = texture2D(g_MaskTex1, mask_uv);
	vec4 mask2 = texture2D(g_MaskTex2, mask_uv);
	clr.a = clr.a * mask1.r * mask2.r;
#endif
	gl_FragColor = clr;

}

void psdistortmain()
{
	vec4 clr = texture2D(g_DistortTex, uv0);
#if BLEND_MODE_T==2
    // simulate alpah test
    if (clr.a < 0.2) discard;
#endif
	gl_FragColor = clr;
}

#endif

