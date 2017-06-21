﻿#version 330
 
// shader input
in vec2 uv;						// interpolated texture coordinates
in vec4 normal;					// interpolated normal
in vec4 position;


uniform sampler2D pixels;		// texture sampler

uniform vec4 lightPos1;
uniform vec4 lightPos2;
uniform vec4 lightPos3;
uniform vec4 lightPos4;

uniform vec4 lightData[13];

uniform vec4 ambient_Color;

uniform vec4 diffuse_Color_L1;
uniform vec4 speculr_Color_L1;

uniform vec4 diffuse_Color_L2;
uniform vec4 speculr_Color_L2;

uniform vec4 diffuse_Color_L3;
uniform vec4 speculr_Color_L3;

uniform vec4 diffuse_Color_L4;
uniform vec4 speculr_Color_L4;

int alpha = 10;

// shader output
out vec4 outputColor;

// fragment shader
void main()
{
	float correctionFactor = 0.5;

	// diffuse color of the mesh (texture)
    outputColor = texture( pixels, uv );// + 0.5f * vec4( normal.xyz, 1 );


	vec4 realNormal = normalize(normal);
	// times the diffuse illumination (and the diffuse light color)
	vec4 lightD1 = normalize(lightPos1 - position);
	vec4 lightD2 = normalize(lightPos2 - position);
	vec4 lightD3 = normalize(lightPos3 - position);
	vec4 lightD4 = normalize(lightPos4 - position);
	float nDotL1 = max(0, dot (realNormal, lightD1 ));
	float nDotL2 = max(0, dot (realNormal, lightD2 ));
	float nDotL3 = max(0, dot (realNormal, lightD3 ));
	float nDotL4 = max(0, dot (realNormal, lightD4 ));
	outputColor *= (nDotL1 * diffuse_Color_L1 + nDotL2 * diffuse_Color_L2 + nDotL3 * diffuse_Color_L3 + nDotL4 * diffuse_Color_L4) * correctionFactor;

	// plus the specular illumination per light source
	vec4 normalizedP = -normalize(position);
	float nDotP = max(0, dot(normal, normalizedP));
	vec4 reflectedRay = normalize (normalizedP - 2 * nDotP * normal); // ?

	float dotProduct1 = min(1, max(0, -dot(reflectedRay, lightD1)));
	float specularIntensity1 = pow(dotProduct1, alpha);
	vec4 specularColor1 = specularIntensity1 * speculr_Color_L1 * texture( pixels, uv ) * correctionFactor;
	outputColor += specularColor1;

	float dotProduct2 = min(1, max(0, -dot(reflectedRay, lightD2)));
	float specularIntensity2 = pow(dotProduct2, alpha);
	vec4 specularColor2 = specularIntensity2 * speculr_Color_L2 * texture( pixels, uv ) * correctionFactor;
	outputColor += specularColor2;

	float dotProduct3 = min(1, max(0, -dot(reflectedRay, lightD3)));
	float specularIntensity3 = pow(dotProduct3, alpha);
	vec4 specularColor3 = specularIntensity3 * speculr_Color_L3 * texture( pixels, uv ) * correctionFactor;
	outputColor += specularColor3;

	float dotProduct4 = min(1, max(0, -dot(reflectedRay, lightD4)));
	float specularIntensity4 = pow(dotProduct4, alpha);
	vec4 specularColor4 = specularIntensity4 * speculr_Color_L4 * texture( pixels, uv ) * correctionFactor;
	outputColor += specularColor4;


	// plus the ambient light color
	outputColor += ambient_Color * texture( pixels, uv );
	
	/*
	// debug lines
	outputColor.x = dot (realNormal, lightD1 );
	outputColor.y = dot (realNormal, lightD2 );
	outputColor.z = dot (realNormal, lightD3 );
	//outputColor.y = 0.1f;
	//outputColor.y = 1;
	//outputColor.z = specularIntensity4;
	*/
}