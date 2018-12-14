
/*
 * Matrices for vertex shader
 */
float4x4 World;
float4x4 View;
float4x4 Projection;

// this is the diffuse texture
texture Texture;

/*
Definition of the texture sampler, which states how 
we will 'sample' the texture. In this case we will LinearInterpolate the pixel
colors if the texture gets bigger or smaller
we will also wrap the texture on values bellow 0 and above 1
*/
sampler2D TextureSampler = sampler_state {

	Texture = (Texture);
	Filter = ANISOTROPIC;
	MaxAnisotropy = 8;
	AddressU = Wrap;
	AddressV = Wrap;
};

// this is the struct for our VertexPositionNormalTexture
// this is the structure that is fed to the vertex shader
struct VertexShaderInput {

	float4 Position : POSITION0;
	float2 TextureCoord : TEXCOORD0;

};

// the struct that the vertex shader outputs
struct VertexShaderOutput {

	float4 Position : POSITION0;
	float2 TextureCoord : TEXCOORD0;

};

// shader vertex
VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {

	VertexShaderOutput output;

	// calculate the vertice position
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);

	// build the output structure
	output.Position = mul(viewPosition, Projection);
	output.TextureCoord = input.TextureCoord;
		
	return output;
}

// pixel shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 {
	
	//// calculate the normal for diffuse
	//float4 normal = mul(input.Normal, WorldInverseTranspose);

	//// calculate angle between surface normal
	//float lightIntensity = dot(normal, normalize(DiffuseLightDirection));

	//// diffuse result (saturate clamps values to 0-1 range)
	//float diffuse = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);

	//// sample texture, and get the color
	float4 textureColor = tex2D(TextureSampler, input.TextureCoord);
	//textureColor.a = 1;

	/*if (textureColor.a < 0.99f)
		discard;*/

	//// calculate the shadow side with diffuse and ambient
	//float4 ambient = textureColor + AmbientColor * AmbientIntensity;

	// result
	return saturate(textureColor);

}

// technique name and passes
technique Textured {

	pass Pass1 {

		/*AlphaBlendEnable = TRUE;
		DestBlend = INVSRCALPHA;
		SrcBlend = SRCALPHA;*/

		// program compilation
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();

	}

};