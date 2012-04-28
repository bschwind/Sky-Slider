float4x4 World;
float4x4 View;
float4x4 Projection;

// ***** Light properties *****
float3 LightPosition;
float4 LightColor;
float4 AmbientLightColor;
// ****************************

// ***** material properties *****

// output from phong specular will be scaled by this amount
float Shininess;

// specular exponent from phong lighting model.  controls the "tightness" of
// specular highlights.
float SpecularPower;
// *******************************

texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state
{
    Texture = <NormalMap>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

texture2D Texture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

struct VS_INPUT
{
    float4 position            : POSITION0;
	float3 normal              : NORMAL0;
    float2 texCoord            : TEXCOORD0;
};

// output from the vertex shader, and input to the pixel shader.
// lightDirection and viewDirection are in world space.
// NOTE: even though the tangentToWorld matrix is only marked 
// with TEXCOORD3, it will actually take TEXCOORD3, 4, and 5.
struct VS_OUTPUT
{
    float4 position            : POSITION0;
    float2 texCoord            : TEXCOORD0;
    float3 lightDirection    : TEXCOORD1;
    float3 viewDirection    : TEXCOORD2;
	float3 worldSpacePos      : TEXCOORD3;
	float3 normal             : TEXCOORD4;
};

VS_OUTPUT VertexShaderFunction( VS_INPUT input )
{
    VS_OUTPUT output;
    
    // transform the position into projection space
    float4 worldSpacePos = mul(input.position, World);
	output.worldSpacePos = worldSpacePos;
    output.position = mul(worldSpacePos, View);
    output.position = mul(output.position, Projection);
    
    // calculate the light direction ( from the surface to the light ), which is not
    // normalized and is in world space
    output.lightDirection = LightPosition - worldSpacePos;
        
    // similarly, calculate the view direction, from the eye to the surface.  not
    // normalized, in world space.
    float3 eyePosition = mul(-View._m30_m31_m32, transpose(View));    
    output.viewDirection = worldSpacePos - eyePosition;    
    
	output.normal = input.normal;

    // pass the texture coordinate through without additional processing
    output.texCoord = input.texCoord;
    
    return output;
}

float3x3 compute_tangent_frame(float3 N, float3 P, float2 UV)
{
	float3 dp1 = ddx(P);
	float3 dp2 = ddy(P);
	float2 duv1 = ddx(UV);
	float2 duv2 = ddy(UV);

	float3x3 M = float3x3(dp1, dp2, cross(dp1, dp2));
	float2x3 inverseM = float2x3( cross( M[1], M[2] ), cross( M[2], M[0] ) );
	float3 T = mul(float2(duv1.x, duv2.x), inverseM);
	float3 B = mul(float2(duv1.y, duv2.y), inverseM);

	return float3x3(normalize(T), normalize(B), N);
}

float4 PixelShaderFunction( VS_OUTPUT input ) : COLOR0
{  
	float3x3 tangentFrame = compute_tangent_frame(input.normal, input.worldSpacePos, input.texCoord);
	float3x3 tangentToWorld;
	tangentToWorld[0] = mul(tangentFrame[0], World);
	tangentToWorld[1] = mul(tangentFrame[1], World);
	tangentToWorld[2] = mul(tangentFrame[2], World);
	
	float3 normalFromMap = normalize(mul(2.0f * tex2D(NormalMapSampler, input.texCoord) - 1.0f, tangentToWorld));
	normalFromMap = normalize(normalFromMap);
    
    // clean up our inputs a bit
    input.viewDirection = normalize(input.viewDirection);
    input.lightDirection = normalize(input.lightDirection);    
    
    // use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normalFromMap, input.lightDirection), 0);
    float4 diffuse = LightColor * nDotL;
    
    // use phong to calculate specular highlights: reflect the incoming light
    // vector off the normal, and use a dot product to see how "similar"
    // the reflected vector is to the view vector.    
    float3 reflectedLight = reflect(input.lightDirection, normalFromMap);
    float rDotV = max(dot(reflectedLight, input.viewDirection), 0);
    float4 specular = Shininess * LightColor * pow(rDotV, SpecularPower);
    
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);
    
    // return the combined result.
	
    return (diffuse + AmbientLightColor) * diffuseTexture + specular;
}

Technique NormalMapping
{
    Pass Go
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}