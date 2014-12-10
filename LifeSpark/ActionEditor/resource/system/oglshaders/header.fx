
uniform vec4  g_DisFogColor;
uniform vec4  g_HeightFogColor;
uniform vec4  g_FogParam;
uniform vec4  g_Eye;
uniform float g_Time;
uniform vec4  g_color_alpha;


uniform vec4 g_AmbientLight;
uniform vec4 g_BoneTM[180];
//uniform DirLight g_Dirlight[2];

uniform vec4 g_Dirlight[6];


uniform vec4 g_Pointlight[48];


uniform mat4 g_WorldViewProj;
uniform mat4 g_WorldView;
uniform mat4 g_World;

uniform sampler2D g_Shadowmap;
uniform mat4 g_ProjectMatrix;
uniform float g_ShadowDensity;

uniform sampler2D g_RandomRotationMap;
uniform vec4 g_RandomRotationMapUVScale;

uniform vec4 g_FadeColor;

#ifdef VS

attribute vec3 vPos;
attribute vec4 blendweights; 
attribute vec4 blendindices;
attribute vec3 vNormal;
attribute vec2 vTexCoord0;
attribute vec2 vTexCoord1;
attribute vec3 vTangent;
attribute vec3 vBinormal;
attribute vec4 vColor;
attribute vec4 vUiPos;
#endif


vec4 MipmapColor(vec4 diffuseColor, sampler2D mipmapTex, vec2 tex, vec2 diffuseTexSize)
{	
	//tex = tex/4.0;   //same as lower the detail level of .texture resources.   
	vec4 resultColor;
	vec4 mipColor = texture2D(mipmapTex, tex * diffuseTexSize / 8.0);
	resultColor.rgb = diffuseColor.rgb * (1.0 - mipColor.a) + mipColor.rgb * mipColor.a; //lerp(diffuseColor.rgb, mipColor.rgb, mipColor.a);
	resultColor.a = diffuseColor.a; 

	return mipColor;
//	return resultColor;
}


vec3 RotateVector(vec4 q,vec3 v)
{
	vec3 result;
	float x1 = q.y * v.z - q.z * v.y;
	float y1 = q.z * v.x - q.x * v.z;
	float z1 = q.x * v.y - q.y * v.x;
	float x2 = q.w * x1 + q.y * z1 - q.z * y1;
	float y2 = q.w * y1 + q.z * x1 - q.x * z1;
	float z2 = q.w * z1 + q.x * y1 - q.y * x1;
	result.x = v.x + 2.0 * x2;
	result.y = v.y + 2.0 * y2;
	result.z = v.z + 2.0 * z2;
	return result;
}

#ifdef  SKIN_MAXINFL

void DoSkinVertex(vec4 blendindices, vec4 blendweights, 
                  in vec4 vPos,in vec3 vNormal,
				  out vec3 pos,out vec3 normal)
{
	int indices[4];
	indices[0] = int(blendindices.x);
	indices[1] = int(blendindices.y);
	indices[2] = int(blendindices.z);
	indices[3] = int(blendindices.w);
	float weights[4];
	weights[0] = blendweights.x/255.0;
	weights[1] = blendweights.y/255.0;
	weights[2] = blendweights.z/255.0;
	weights[3] = blendweights.w/255.0;
	
	pos = vec3(0, 0, 0);
	normal = vec3(0,0,0);

	for(int i=0; i<SKIN_MAXINFL; i++)
	{
		int offset = indices[i]*2;	
		vec3 rotpos = RotateVector(g_BoneTM[offset],vPos.xyz);
		pos +=	(rotpos + g_BoneTM[offset+1].xyz) * weights[i];
		vec3 rotnormal = RotateVector( g_BoneTM[offset], vNormal );
		normal += rotnormal * weights[i];
	}
	
}

void DoSkinVertexTBN(vec4 blendindices, vec4 blendweights, 
				  in vec4 vPos,in vec3 vNormal,in vec3 vTangent,in vec3 vBiNormal,
				  out vec3 pos,out vec3 normal,out vec3 tangent,out vec3 binormal)
{
	int indices[4];
	indices[0] = int(blendindices.x);
	indices[1] = int(blendindices.y);
	indices[2] = int(blendindices.z);
	indices[3] = int(blendindices.w);
	float weights[4];
	weights[0] = blendweights.x/255.0;		//100
	weights[1] = blendweights.y/255.0;
	weights[2] = blendweights.z/255.0;
	weights[3] = blendweights.w/255.0;
	
	pos = vec3(0, 0, 0);
	normal = vec3(0,0,0);
	tangent = vec3(0,0,0);
	binormal = vec3(0,0,0);

	for(int i=0; i<SKIN_MAXINFL; i++)
	{

// used in ios and adroid plafrom ( maxtrix)
/* 
		int offset = indices[i] * 3;	
		vec3 wpos = vec3(dot(g_BoneTM[offset+0],vPos),dot(g_BoneTM[offset+1],vPos), dot(g_BoneTM[offset+2],vPos));
		pos += wpos  * weights[i];
*/
// used in ios plafrom (TODO)
		int offset = indices[i] * 2;
		vec3 rotpos = RotateVector(g_BoneTM[offset],vPos.xyz);
		pos +=	(rotpos + g_BoneTM[offset+1].xyz) * weights[i];
		vec3 rotnormal = RotateVector( g_BoneTM[offset], vNormal );
		normal += rotnormal * weights[i];
		vec3 rottangent = RotateVector( g_BoneTM[offset], vTangent );
		tangent += rottangent * weights[i];
		vec3 rotbinormal = RotateVector( g_BoneTM[offset], vBiNormal );
		binormal += rotbinormal * weights[i];
		
	}
	
}

#endif


float CalculateSpecular( vec3 normal, vec3 lightDir, vec3 viewDir, float glossiness )
{
	vec3 halfway = normalize( lightDir + viewDir );

	float NdotL = max( dot( normal, lightDir ), 0.0 );
	float NdotH = max( dot( normal, halfway ), 0.0 );

	return (glossiness + 2.0) / 8.0 * pow( NdotH, glossiness ) * NdotL;
}


struct DirLight
{
	vec4 lightDir;
	vec4 lightColor;
	vec4 ambient;
};

vec3 DirLightFunc( DirLight dirLight, vec3 pos, vec3 normal, vec4 modelEyePos, float glossiness)
{
	//dirLight.lightDir.xyz = normalize(vec3(2.0, 4.0, 6.0));   //only for test
	vec3 viewVec = modelEyePos.xyz - pos;
	vec3 viewDir = normalize(viewVec);
	
	vec3 diffuse = dirLight.lightColor.xyz * max( dot( normal, dirLight.lightDir.xyz ), 0.0 ) ;
	//diffuse *= texture2D(g_DiffuseTex, tex ).xyz;
	float specular = CalculateSpecular( normal, dirLight.lightDir.xyz, viewDir, glossiness);
	//specular *= texture(g_SpecularTex, tex).xyz * g_SpecPower;
	return  diffuse + specular;
}


struct PointLight
{
	vec4 lightPos;
	vec4 lightColor;
	vec4 PointLightColorAttenuation;
    vec4 ambient;
};

vec3 PointLightFunc(PointLight pointLight, vec3 pos, vec3 normal, vec4 modelEyePos, float glossiness)
{
	vec3 lightVec = pointLight.lightPos.xyz - pos;
	vec3 lightDir = normalize(lightVec);

	vec3 viewVec = modelEyePos.xyz - pos;
	vec3 viewDir = normalize(viewVec);
	
	float lightDist = length(lightVec);   
	float lightRatio = max(1.0 - lightDist / pointLight.lightPos.w, 0.0);
	float falloff = pow(lightRatio, pointLight.PointLightColorAttenuation.w);     //adapt  PointLightColorAttenuation

	vec3 diffuse = pointLight.PointLightColorAttenuation.xyz * max( dot( normal, lightDir ), 0.0 ) /** falloff*/;    //adapt  PointLightColorAttenuation, g_AmbientLight
	//diffuse *= texture2D(g_DiffuseTex, tex ).xyz;
	float specular = CalculateSpecular( normal, lightDir, viewDir, glossiness);
	//specular *= texture(g_SpecularTex, tex).xyz * g_SpecPower;
	
	return (diffuse + specular)*falloff;
}


vec3 CaculateLight(vec3 modelpos,vec3 normal,float Dirvisiblity)
{
	vec3 light = g_AmbientLight.xyz;
	float glossiness = 1.0;

#ifdef  NUM_DIRLIGHT 
	for(int i=0;i<NUM_DIRLIGHT;i++)
	{
		DirLight dL;
		dL.lightDir = g_Dirlight[2*i];
		dL.lightColor = g_Dirlight[2*i+1];
		vec4 g_modelEyePos = g_Eye;
		//float glossiness = 30.0;
		light += Dirvisiblity * DirLightFunc(dL, modelpos, normal, g_modelEyePos, glossiness);
		//light += Dirvisiblity * DirLightFunc(g_Dirlight[2*i],g_Dirlight[2*i+1],normal);
	}
#endif

#ifdef NUM_POINTLIGHT 
	for(int i=0;i<NUM_POINTLIGHT;i++)
	{
		PointLight pL;
		pL.lightPos = g_Pointlight[3*i];
		pL.lightColor = g_Pointlight[3*i+1];
		pL.PointLightColorAttenuation = vec4(pL.lightColor.xyz, 0.8);
		vec4 modelEyePos = g_Eye;
		//float glossiness = 30.0;
		light += Dirvisiblity * PointLightFunc(pL, modelpos, normal, modelEyePos, glossiness);
		//light += PointLightFunc(g_Pointlight[2*i], g_Pointlight[2*i+1],modelpos, normal);
	}
#endif

	return light;
}


void DirLightFuncSp(in DirLight dirLight, vec3 pos, vec3 normal, in vec4 modelEyePos, in float glossiness, out vec3 diffuse, out float specular)
{
	//dirLight.lightDir.xyz = normalize(vec3(2.0, 4.0, 6.0));   //only for test
	vec3 viewVec = modelEyePos.xyz - pos;
	vec3 viewDir = normalize(viewVec);
	
	diffuse = dirLight.lightColor.xyz * max( dot( normal, dirLight.lightDir.xyz ), 0.0 ) + dirLight.ambient.xyz;
	specular = CalculateSpecular( normal, dirLight.lightDir.xyz, viewDir, glossiness );
}

void PointLightFuncSp(in PointLight pointLight, in vec3 pos, in vec3 normal, in vec4 modelEyePos, in float glossiness, out vec3 diffuse, out float specular)
{
	vec3 lightVec = pointLight.lightPos.xyz - pos;
	vec3 lightDir = normalize(lightVec);

	vec3 viewVec = modelEyePos.xyz - pos;
	vec3 viewDir = normalize(viewVec);
	
	float lightDist = length(lightVec);   
	float lightRatio = max(1.0 - lightDist / pointLight.lightPos.w, 0.0);
	float falloff = pow(lightRatio, pointLight.PointLightColorAttenuation.w);     //adapt  PointLightColorAttenuation

	diffuse = pointLight.PointLightColorAttenuation.xyz * max( dot( normal, lightDir ), 0.0 ) + pointLight.ambient.xyz;    //adapt  PointLightColorAttenuation, g_AmbientLight
	diffuse *= falloff;

	specular = CalculateSpecular( normal, lightDir, viewDir, glossiness);
	specular *= falloff;
}

#ifdef PS

vec2 Rotate2D(float sin_t, float cos_t, vec2 v)
{
    mat2 m;
    m[0] = vec2(cos_t, -sin_t);
    m[1] = vec2(sin_t, cos_t);
    return m * v;
}

float GetShadowSample(sampler2D shadowmap, float depth, vec2 shadowUV, vec2 sintcost, float radius, vec2 poisson)
{
    vec2 offset = Rotate2D(sintcost.x, sintcost.y, poisson) * radius;
    return depth > texture2D( shadowmap, shadowUV + offset ).x  ? 0.0 : 1.0;
}

float ShadowPCF8x(sampler2D shadowmap, vec4 shadowUV, float radius, float bias)
{
    float shadow = 0.0;
    vec2  screenUV = gl_FragCoord.xy * g_RandomRotationMapUVScale.xy;
    vec2  sintcost = texture2D( g_RandomRotationMap, screenUV ).rg;
    float depth = min(shadowUV.z/shadowUV.w - bias*0.01, 1.0);
    sintcost = sintcost * 2.0 - 1.0;
    radius *= 0.015;

    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.8067961, -0.1735136));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.1160412, -0.3263741));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.2745395, 0.1202338));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.531391, 0.512445));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.3538843, -0.8416352));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.8273658, -0.1064943));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.359133, 0.8904359));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.4294311, -0.8222));

    shadow *= 0.125;
    return shadow;
}

float ShadowPCF16x(sampler2D shadowmap, vec4 shadowUV, float radius, float bias)
{
    float shadow = 0.0;
    vec2  screenUV = gl_FragCoord.xy * g_RandomRotationMapUVScale.xy;
    vec2  sintcost = texture2D( g_RandomRotationMap, screenUV ).rg;
    float depth = min(shadowUV.z/shadowUV.w - bias*0.01, 1.0);
    sintcost = sintcost * 2.0 - 1.0;
    radius *= 0.015;

    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.3832417, -0.3988019));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.136029, -0.6761076));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.1483051, -0.05585958));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.8628409, -0.03351589));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.6650959, -0.7229457));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.2547194, 0.0685097));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.401102, 0.3331952));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.01623098, 0.5351236));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.6104643, 0.6793484));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.8239985, -0.08826014));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.564146, -0.6682279));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.9115089, 0.3445331));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.1792976, -0.9713122));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(0.1452408, 0.9440845));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.2753218, 0.8258935));
    shadow += GetShadowSample(shadowmap, depth, shadowUV.xy, sintcost, radius, vec2(-0.7016991, 0.6495667));

    shadow *= 0.0625;
    return shadow;
}

#endif

//vec3 CalculateLightSp(vec3 modelpos,vec3 normal,float Dirvisiblity, float g_SpecGlossy, float g_SpecPower, sampler2D g_SpecularTex, sampler2D g_DiffuseTex, float g_DiffusePow, vec2 tex)
void CalculateLightSp(in vec3 modelpos, in vec3 normal, in float Dirvisiblity, in float g_SpecGlossy, out vec3 diffuse, out float specular)
{
	normal = normalize(normal);	
	DirLight dL;
	PointLight pL;
	vec4 modelEyePos = g_Eye;
	float glossiness = g_SpecGlossy;

	/*vec3*/ diffuse = vec3(0.0, 0.0, 0.0);
	/*float*/ specular = 0.0;

#ifdef NUM_DIRLIGHT 
	for(int i=0;i<NUM_DIRLIGHT;i++)
	{		
		dL.lightDir = g_Dirlight[3*i];
		dL.lightColor = g_Dirlight[3*i+1];
		dL.ambient = g_Dirlight[3*i+2];
		
		vec3 diffuseTmp;
		float specularTmp;
		DirLightFuncSp(dL, modelpos, normal, modelEyePos, glossiness, diffuseTmp, specularTmp);
		diffuse += diffuseTmp * Dirvisiblity;
		specular += specularTmp * Dirvisiblity;
	}
#endif
#ifdef NUM_POINTLIGHT	
	for(int i=0;i<NUM_POINTLIGHT;i++)
		{
			pL.lightPos = g_Pointlight[3*i];
			pL.lightColor = g_Pointlight[3*i+1];
            pL.ambient = g_Pointlight[3*i+2];
			pL.PointLightColorAttenuation = pL.lightColor/*vec4(pL.lightColor.xyz, 1.0)*/;

			vec3 diffuseTmp;
			float specularTmp;
			PointLightFuncSp(pL, modelpos, normal, modelEyePos, glossiness, diffuseTmp, specularTmp);
			diffuse += diffuseTmp;
			specular += specularTmp;
		}	
#endif

}
