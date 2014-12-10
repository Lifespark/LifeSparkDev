/*<Material>
  <Params>
  <var name="g_ColorTex" type="string" usage="texture" default="" texcoord="0"/>
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

#define __NumSamples 17

uniform sampler2D g_ColorTex;
uniform sampler2D g_SceneTex;

uniform vec2 g_SampleOffsets[__NumSamples];
uniform float g_SampleWeights[__NumSamples];

uniform float g_FocusDistance;
uniform float g_FocusRange;
uniform float g_FadeRange;


varying vec2 tex;

#ifdef VS

void vsmain()
{	
	gl_Position = vec4(vPos, 1.0);
	tex = vPos.xy * 0.5 + 0.5;
}

#endif

#ifdef PS

void psmain()
{
   vec4 sceneCol = texture2D( g_SceneTex, tex );
   float depth = sceneCol.a;//* 50.0;
   float sampleScale = clamp((abs( depth - g_FocusDistance) - g_FocusRange) / g_FadeRange, 0.0, 1.0);
   sampleScale = pow( sampleScale, 1.5);
   vec2 vecScale = vec2( sampleScale, sampleScale);
   
   vec3 sum = vec3(0.0, 0.0, 0.0);
   for(int i=0; i< __NumSamples; ++i)
   {
        sum +=  texture2D( g_SceneTex, tex + g_SampleOffsets[i].xy * vecScale).rgb * vec3(g_SampleWeights[i], g_SampleWeights[i], g_SampleWeights[i]);
   }
   gl_FragColor = vec4(sum, sceneCol.a);
  // gl_FragColor = vec4( sceneCol.a, sceneCol.a, sceneCol.a, sceneCol.a);
}

#endif
