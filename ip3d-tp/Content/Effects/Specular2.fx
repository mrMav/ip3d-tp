/*
 * Matrices for vertex shader
 */
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;  // we need to inverse and transpose to 'localize' the light vector
float3 ViewPosition; // camera position

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

	float4 PixelPosition : TEXCOORD1;  // TEXCOORD1 is just a  semantic

};

// direction light variables
float4 DirectionLightDirection;
float3 DirectionLightColor    = float3(1, 1, 1);
float3 DirectionLightAmbient  = float3(0.5, 0.5, 0.5);
float3 DirectionLightDiffuse  = float3(1, 1, 1);
float4 DirectionLightSpecular = float4(1, 1, 1, 1);

// material variables
// this is the diffuse texture
texture MaterialDiffuseTexture;
sampler2D MaterialTextureSampler = sampler_state {

	Texture = (MaterialDiffuseTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;

};
float MaterialShininess = 32;

// shader vertex
VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {

	VertexShaderOutput output;

	// calculate the vertice position
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);

	// build the output structure
	output.Position = mul(viewPosition, Projection);
	output.Normal = mul(normalize(input.Normal), World);
	output.TextureCoord = input.TextureCoord;
	output.PixelPosition = mul(input.Position, World);

	return output;
}

// pixel shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0{

	//float3 norm = normalize(input.Normal);
	//float3 viewDir = normalize(ViewPosition - input.PixelPosition);

	//float3 result = CalcDirLight(norm, viewDir, input);
	
	// ****

	//float3 texDiffuse = tex2D(MaterialTextureSampler, input.TextureCoord);

	//float ambientStrength = 0.1;
	//float3 ambient = ambientStrength * texDiffuse;

	//float3 norm = normalize(input.Normal);
	////float3 lightDir = normalize(DirectionLightPosition - input.PixelPosition);
	//float3 lightDir = normalize(-DirectionLightDirection);
	//float diff = max(dot(norm, lightDir), 0.0);
	//float3 diffuse = DirectionLightColor * diff * texDiffuse;

	//float3 viewDir = normalize(ViewPosition - input.PixelPosition);
	//float3 reflectDir = reflect(-lightDir, norm);
	//float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
	//float3 specular = 5 * spec * DirectionLightColor * texDiffuse;


	//float3 result = (ambient + diffuse + specular);

	// ****

	// ----

	float4 texDiffuse = tex2D(MaterialTextureSampler, input.TextureCoord);

	//Calculate local normalized normal vector
	float4 Normal = normalize(input.Normal);

	//Calculate local normalized view direction
	float3 ViewDir = normalize(ViewPosition - input.PixelPosition);

	//Calculate local normalized light direction
	float3 LightDir = normalize(DirectionLightDirection);

	//Calculate the amount of diffuse light hitting this pixel
	float Diff = saturate(dot(Normal, -LightDir));

	//Calculate reflection vector of the light hitting this pixel
	float4 Reflect = normalize(2 * Diff * Normal - float4(LightDir, 1.0f));

	//Calculate the amount of specular at this pixel
	float4 Specular = pow(saturate(dot(Reflect, ViewDir)), 32);

	//If no light is hitting the pixel cut the specular power
	if (Diff <= 0.0f)
		Specular = 0.0f;

	//Calculate ambient light values
	float4 AmbientColour = texDiffuse * 0.1;

	//Calculate diffuse light values
	float4 DiffuseColour = texDiffuse * Diff;

	//Calculate specular highlight colour
	float4 SpecularColour = (DirectionLightSpecular * 2) * Specular;

	return AmbientColour + DiffuseColour + SpecularColour;

	// ----



	//return float4(result, 1.0);

}

// technique name and passes
technique Textured {

	pass Pass1 {

		// program compilation
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();

	}

};