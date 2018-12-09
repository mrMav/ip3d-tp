
/*
 * Matrices for vertex shader
 */
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;  // we need to inverse and transpose to 'localize' the light vector

// ambient color to apply in the pixel shading
float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.3;  // ambient intensity factor

// right now, only one directional diffuse light is supported
// these are fed to the shader
float4 DiffuseLightDirection;
float4 DiffuseColor;
float DiffuseIntensity;

// this is the diffuse texture
texture ModelTexture;

// bump map texture
texture BumpTexture;

/*
Definition of the texture sampler, which states how
we will 'sample' the texture. In this case we will LinearInterpolate the pixel
colors if the texture gets bigger or smaller
we will also wrap the texture on values bellow 0 and above 1
*/
sampler2D textureSampler = sampler_state {

	Texture = (ModelTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;

};

sampler2D textureSamplerBump = sampler_state {

	Texture = (BumpTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;

};

// this is the struct for our VertexPositionNormalTexture
// this is the structure that is fed to the vertex shader
struct VertexShaderInput {

	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoord : TEXCOORD0;

};

// the struct that the vertex shader outputs
struct VertexShaderOutput {

	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoord : TEXCOORD0;

};

// reference: http://api.unrealengine.com/attachments/Engine/Rendering/LightingAndShadows/BumpMappingWithoutTangentSpace/mm_sfgrad_bump.pdf
float3 PerturbNormal(float3 surfacePosition, float3 surfaceNormal, float height) {

	float3 vSigmaS = ddx(surfacePosition);
	float3 vSigmaT = ddy(surfacePosition);
	float3 vN = surfaceNormal;  // must be normalized

	float3 vR1 = cross(vSigmaT, vN);
	float3 vR2 = cross(vN, vSigmaS);

	float fDet = dot(vSigmaS, vR1);

	float dBs = ddx_fine(height);
	float dBt = ddy_fine(height);

	float3 vSurfGrad = sign(fDet) * (dBs * vR1 + dBt * vR2);

	return normalize(abs(fDet) * vN - vSurfGrad);

}

// shader vertex
VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {

	VertexShaderOutput output;

	// calculate the vertice position
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);

	// build the output structure
	output.Position = mul(viewPosition, Projection);
	output.Normal = input.Normal;
	output.TextureCoord = input.TextureCoord;

	return output;
}

// pixel shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 {

	// calculate the normal for diffuse
	// using bumpmapping
	float4 height = tex2D(textureSamplerBump, input.TextureCoord);
	float3 perturbedNormal = PerturbNormal(input.Position, input.Normal, height.r * 100);

	// the normal is transformed to obj world space
	float4 normal = mul(perturbedNormal, WorldInverseTranspose);

	// calculate angle between surface normal
	float lightIntensity = dot(normal, normalize(DiffuseLightDirection));

	// diffuse result (saturate clamps values to 0-1 range)
	float diffuse = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);

	// sample texture, and get the color
	float4 textureColor = tex2D(textureSampler, input.TextureCoord);
	textureColor.a = 1;

	// calculate the shadow side with diffuse and ambient
	float4 ambient = textureColor + AmbientColor * AmbientIntensity;

	// result
	return saturate(textureColor * diffuse + ambient * AmbientIntensity);

}

// technique name and passes
technique Textured {

	pass Pass1 {

		// program compilation
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();

	}

};