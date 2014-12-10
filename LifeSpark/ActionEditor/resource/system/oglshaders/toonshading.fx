
/*<Material>
  <Params>
  <macro name="HAIR_ON" UIName="Enable Hair Specular"/>
  <macro name="SILHOUETTE_ON" UIName="Enable Detail Silhouette"/>
  <macro name="MATERIAL_TRANSITION" UIName="Enable Material Transition"/>
  <macro name="BLEND_MODE_T" UIName="BlendState" UIWidget="combobox" min="0" max="9" default="0" type="int"/>
  <var name="g_DiffuseTex" UIName="Diffuse Map" default="system/texture/toon_default_diffuse.texture" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_MipmapTex" DisplayInUI = "false" UIName="Mipmap Tex" type="string" usage="texture" default="system/texture/mipmap6.texture" texcoord="0"/>
  <var name="g_ToonRamp" UIName="Toon Ramp" default="system/texture/toon_default_ramp.texture" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_SpecularTex" UIName="Specular Masks" default="system/texture/toon_default_specmasks.texture" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_OutlineTex" UIName="Outline Masks" default="system/texture/toon_default_outlinemasks.texture" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_OutlineSizeSave" UIName="Outline Size" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>
  <var name="g_OutlineColorSave" UIName="Outline Color" UIWidget="color" type="float4"/>
  <var name="g_RimSize" UIName="Rim Size" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>
  <var name="g_RimSmoothness" UIName="Rim Smoothness" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>
  <var name="g_RimShadow" UIName="Rim Shadow" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>
  <var name="g_RimIntensity2" UIName="Rim Intensity" UIWidget="slider" min="0" max="1" default="0.0" type="float"/>
  <var name="g_RimColor" UIName="Rim Color" UIWidget="color" default="1 1 1 1" type="float4"/>
  <var name="g_CameraLightIntensity" UIName="Light Compensation" UIWidget="slider" min="0" max="2" default="0.5" type="float"/>
  <var name="g_SpecularShiftAmount" UIName="Specular Shift Amount" UIWidget="slider" min="0" max="1" default="0.8" type="float"/>
  <var name="g_SpecularIntensity" UIName="Specular Intensity" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>
  <var name="g_SpecularLightDirX" UIName="Specular Dir X" UIWidget="slider" min="-1" max="1" default="0" type="float"/>
  <var name="g_SpecularLightDirY" UIName="Specular Dir Y" UIWidget="slider" min="-1" max="1" default="1" type="float"/>
  <var name="g_SpecularLightDirZ" UIName="Specular Dir Z" UIWidget="slider" min="-1" max="1" default="1" type="float"/>
  <var name="g_ShadowSoftness" UIName="Shadow Softness" UIWidget="slider" min="0" max="1" default="0.2" type="float"/>
  <var name="g_ShadowIntensity" UIName="Shadow Intensity" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>
  <var name="g_ShadowBias" UIName="Shadow Bias" UIWidget="slider" min="0" max="1" default="0.5" type="float"/>
  <var name="g_Brightness" UIName="Brightness" UIWidget="slider" min="0" max="2" default="1.0" type="float"/>
  <var name="g_DiffuseTexSize" type="float2"/>
  <var name="g_SpecularTexSize" type="float2"/>
  <var name="g_LightMapSize" type="float2"/>
  <var name="g_MaskTex" UIName="Mask Texture" default="cache/sgame/character/sweet/model/gradientdissolve.texture" type="string" usage="texture" default="" texcoord="0"/>
  <var name="g_DirtyTex" UIName="Dirty Texture" default="cache/sgame/character/sweet/model/309.texture" type="string" usage="texture" default="" texcoord="0"/>
  <var name="TransitionMaterialLerp" default="0" type="float"/>  
  </Params>
  
  <Technique>
  <Flow name = "FLOW_SCENE"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmain"/>
  </Pass>
  </Technique>

  <Technique>
  <Flow name = "FLOW_LIGHTONLY"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "pslight"/>
  </Pass>
  </Technique>

  <Technique>
  <Flow name = "FLOW_TEXTUREONLY"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "pstexture"/>
  </Pass>
  </Technique>

  <Technique>
  <Flow name = "FLOW_SHOWMIPMAP"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmipmap"/>
  </Pass>
  </Technique>

   <Technique>
  <Flow name = "FLOW_SHOWMIPMAPFORLIGHTMAP"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psmipmap"/>
  </Pass>
  </Technique>

  <Technique>
  <Flow name = "FLOW_SHADOWMAP"/>
  <Pass>
  <vertexshader entry = "vsmain"/>
  <fragshader entry = "psshadow"/>
  </Pass>
  </Technique>

</Material>*/

#include "header.fx"

uniform sampler2D g_DiffuseTex;
uniform sampler2D g_MipmapTex;
uniform sampler2D g_MaskTex;
uniform sampler2D g_DirtyTex;


// g_ToonRamp
//   - RGB channel is the diffuse color ramp.
//   - Alpha channel is the specular color ramp.
uniform sampler2D g_ToonRamp; 

// g_SpecularTex:
//   - RGB channel is the specular color map, white for receiving specular light, only effective if HAIR_ON is nonzero.
//   - Alpha channel is the specular distortion map, grey(128) is zero distortion, only effective if HAIR_ON is nonzero.
uniform sampler2D g_SpecularTex;

// g_OutlineTex
//   - Alpha channel is the shadow intensity mask, white for receiving shadows.
uniform sampler2D g_OutlineTex;

uniform float  g_RimSize; // 0.0 ~ 1.0
uniform float  g_RimSmoothness; // 0.0 ~ 1.0
uniform float  g_RimShadow; // 0.0 ~ 1.0
uniform float  g_RimIntensity2; // 0.0 ~ 1.0
uniform vec4   g_RimColor;

uniform float  g_CameraLightIntensity; // 0.0 ~ 1.0

uniform float  g_SpecularShiftAmount; // 0.0 ~ 1.0
uniform float  g_SpecularIntensity; // 0.0 ~ 1.0
uniform float  g_SpecularLightDirX; // in view space
uniform float  g_SpecularLightDirY; // in view space
uniform float  g_SpecularLightDirZ ; // in view space
uniform float  g_ShadowSoftness; // 0.0 ~ 1.0
uniform float  g_ShadowIntensity; // 0.0 ~ 1.0
uniform float  g_ShadowBias; // 0.0 ~ 1.0
uniform float  g_Brightness; // 0.0 ~ 2.0
uniform vec2   g_DiffuseTexSize;
uniform vec2   g_SpecularTexSize;
uniform vec2   g_LightMapSize;

uniform float TransitionMaterialLerp;

varying vec2 tex;
varying vec3 fragPosView;
varying vec3 fragNormal;
varying vec3 fragNormalView;
varying vec3 fragTangentView;
varying vec4 shadowUV;


#ifdef  VS

vec3 calcTangent(vec3 n)
{
    vec3 vUp = vec3(0.0, 1.0, 0.0);
    vec3 vLeft = cross(vUp, n);
    return cross(vLeft, n);
}

void vsmain()
{	

	vec3 Pos = vPos;
	vec3 Normal = vNormal;
	vec3 Tangent = vTangent;
	vec3 Binormal = vBinormal;
    
#ifdef SKIN_MAXINFL  
	DoSkinVertexTBN(blendindices, blendweights, 
					vec4(vPos,1.0),vNormal,vTangent,vBinormal,
					Pos,Normal,Tangent,Binormal );
#endif
	
	gl_Position = g_WorldViewProj * vec4(Pos,1.0);

	tex = vTexCoord0;

	fragPosView = vec3(g_WorldView * vec4(Pos,1.0));
	fragNormal = Normal;
    fragNormalView = mat3(g_WorldView[0].xyz,g_WorldView[1].xyz,g_WorldView[2].xyz) * Normal;
    fragTangentView = calcTangent(fragNormalView);
	shadowUV = g_ProjectMatrix  * vec4(Pos,1.0);
}

#endif

#ifdef PS

vec3 RimLight(vec2 lightAndShadow, vec3 normalView, vec3 viewDir, float shadowMask)
{
    float k = 1.0 - abs(dot(normalView, viewDir));
    float s = (k + g_RimSize - 1.2) / max(0.05, g_RimSmoothness) + 0.5;  
    s = max(0.0, min(s, 1.0));
    float shadowRim = mix(1.0, lightAndShadow.y*lightAndShadow.x, g_RimShadow);
    return s * shadowRim * g_RimColor.xyz * g_RimIntensity2;
}

vec2 CameraLightAndShadow(vec2 lightAndShadow, vec3 normalView, float shadowMask)
{
    // if it doesn't receive shadow, which means it wants to be in light, so add a camera light to make it brighter,
    // this is usually for parts like face.
    float lightCamera = min(1.0, max(normalView.z, 0.1) * g_CameraLightIntensity * 4.0);
    lightAndShadow.x = lightAndShadow.x + lightCamera * (1.0 - shadowMask);
    lightAndShadow.y = mix(1.0, lightAndShadow.y, shadowMask);
    return lightAndShadow;
}

vec3 ToonDiffuse(vec2 lightAndShadow)
{
    // "ramp" the diffuse light intensity and blend it with shadow
    vec2 uvToon = vec2(0.5, 1.0 - lightAndShadow.x*lightAndShadow.y);
	vec3 toon = texture2D(g_ToonRamp, uvToon).xyz;
    return toon;
}

vec3 ToonSpecular(vec2 lightAndShadow, vec3 normal, vec3 tangent, vec3 vdir, float distort, vec3 specColor)
{   
    // shift tangent to mimick the "zigzag" hightlight
    float shift = (distort - 0.5) * g_SpecularShiftAmount;
    tangent += shift * normal;
    tangent = normalize(tangent);

    // modified from reference : http://developer.amd.com/wordpress/media/2012/10/Scheuermann_HairRendering.pdf
    vec3 lightDir = normalize(vec3(g_SpecularLightDirX, g_SpecularLightDirY, g_SpecularLightDirZ));
    vec3 H = normalize(lightDir + vdir);
    float dotTH = dot(tangent, H);
    float sinTH = abs(dotTH);
    float specPow = exp(-10.0*sinTH);
    
    // "ramp" the specular intensity and mask it
    vec2 uvToon = vec2(0.5, 1.0 - specPow);
	vec3 toonSpec = texture2D(g_ToonRamp, uvToon).www;
    toonSpec *= specColor;
    toonSpec *= lightAndShadow.x * lightAndShadow.y;
    toonSpec *= g_SpecularIntensity;

    return toonSpec;
}

vec2 ToonLightAndShadow(vec3 normal, vec4 shadowUV)
{
    vec2 lightAndShadow;
    
#ifdef NUM_DIRLIGHT
    lightAndShadow.x = max(dot(normal, g_Dirlight[0].xyz), 0.0);
#else
    lightAndShadow.x = 1.0;
#endif
#ifdef  SHADOW_MAPPING      
	lightAndShadow.y = ShadowPCF16x(g_Shadowmap, shadowUV, g_ShadowSoftness, g_ShadowBias);
    lightAndShadow.y = mix(1.0, lightAndShadow.y, g_ShadowIntensity*2.0);
#else
    lightAndShadow.y = 1.0;
#endif

    return lightAndShadow;
}

vec4 toonshading_high()
{
    vec3 viewDir = normalize(-fragPosView);
    vec3 oNormal = normalize(fragNormal);
    vec3 vNormal = normalize(fragNormalView);
    vec3 vTangent = normalize(fragTangentView);
	
    // sample the diffuse color and toon masks
	vec4  color = texture2D(g_DiffuseTex, tex);   

#ifdef MATERIAL_TRANSITION
	float temp = texture2D(g_MaskTex, tex).z;
	if( TransitionMaterialLerp>= temp )
	{
		color = texture2D(g_DirtyTex, tex);   
	}
#endif
  
    vec4  specMask = texture2D(g_SpecularTex, tex);
    float shadowMask = texture2D(g_OutlineTex, tex).a;

    color.rgb *= g_Brightness;
    color.a *= g_FadeColor.a;

#if BLEND_MODE_T == 2
    // simulate alpah test
    if (color.a < 0.01) discard;
#endif
    // calculate the light and shadow terms
    vec2 lightAndShadow = ToonLightAndShadow(oNormal, shadowUV);

    // modulate the light and shadow with shadow masks
    lightAndShadow = CameraLightAndShadow(lightAndShadow, vNormal, shadowMask);

    // calculate the rim light term
    vec3 toonRim = RimLight(lightAndShadow, vNormal, viewDir, shadowMask);
		    
    // calculate the toon specular term
#ifdef HAIR_ON 
 	vec3 toonSpecular = ToonSpecular(lightAndShadow, vNormal, vTangent, viewDir, specMask.a, specMask.rgb);
#else
    vec3 toonSpecular = vec3(0.0, 0.0, 0.0);
#endif

    // combine the diffuse and specular terms
#ifndef MATERIAL_TRANSITION
    // calculate the toon diffuse term
    vec3 toonDiffuse = ToonDiffuse(lightAndShadow);
    color.xyz *= toonDiffuse;
#endif
    
    color.xyz += toonSpecular;
    color.xyz += toonRim;
    color.xyz += g_FadeColor.xyz;

	return color;
}

vec4 toonshading_low()
{
	vec4 color = texture2D(g_DiffuseTex, tex);
#ifdef MATERIAL_TRANSITION
	float temp = texture2D(g_MaskTex, tex).z;
	if( TransitionMaterialLerp>= temp )
	{
		color = texture2D(g_DirtyTex, tex);   
	}
#endif

	color.a *= g_FadeColor.a;
	color.xyz += g_FadeColor.xyz;

	return color;
}

void psmain()
{
#ifdef TOONSHADING_LEVEL
    #if TOONSHADING_LEVEL==2
        gl_FragColor = toonshading_high();
    #else
        gl_FragColor = toonshading_low();
    #endif
#else
    gl_FragColor = toonshading_high();
#endif
}

void pslight()
{
    vec3 viewDir = normalize(-fragPosView);
    vec3 oNormal = normalize(fragNormal);
    vec3 vNormal = normalize(fragNormalView);
    vec3 vTangent = normalize(fragTangentView);
	
    // sample the diffuse color and toon masks
	vec4  color = vec4(1.0, 1.0, 1.0, 1.0);   
    vec4  specMask = texture2D(g_SpecularTex, tex);
    float shadowMask = texture2D(g_OutlineTex, tex).a;
#if BLEND_MODE_T==2
    // simulate alpah test
    if (color.a < 0.01) discard;
#endif
    // calculate the light and shadow terms
    vec2 lightAndShadow = ToonLightAndShadow(oNormal, shadowUV);

    // modulate the light and shadow with shadow masks
    lightAndShadow = CameraLightAndShadow(lightAndShadow, vNormal, shadowMask);

    // calculate the rim light term
    vec3 toonRim = RimLight(lightAndShadow, vNormal, viewDir, shadowMask);

    // calculate the toon diffuse term
    vec3 toonDiffuse = ToonDiffuse(lightAndShadow);
		    
    // calculate the toon specular term
#ifdef HAIR_ON 
 	vec3 toonSpecular = ToonSpecular(lightAndShadow, vNormal, vTangent, viewDir, specMask.a, specMask.rgb);
#else
    vec3 toonSpecular = vec3(0.0, 0.0, 0.0);
#endif

    // combine the diffuse and specular terms
	color.xyz *= toonDiffuse;
    color.xyz += toonSpecular;
    color.xyz += toonRim;
    
	gl_FragColor = color;

}

void pstexture()
{
	vec4  color = texture2D(g_DiffuseTex, tex);  
#if BLEND_MODE_T==2
    if (color.a < 0.01) discard;
#endif
	gl_FragColor = color;
}

void psmipmap()
{
	vec4  color = texture2D(g_DiffuseTex, tex);   
#if BLEND_MODE_T==2
    if (color.a < 0.01) discard;
#endif
	gl_FragColor = color;
	//gl_FragColor = MipmapColor(color, g_MipmapTex, tex, g_DiffuseTexSize);
}

void psshadow()
{    
	vec4 color = texture2D(g_DiffuseTex, tex);
#if BLEND_MODE_T==2
    if (color.a < 0.01) discard;
#endif
	gl_FragColor = vec4(gl_FragCoord.zzz, 1.0);

}

#endif

