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
	float3 Normal : NORMAL0;
	float2 TextureCoord : TEXCOORD0;

};

// the struct that the vertex shader outputs
struct VertexShaderOutput {

	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoord : TEXCOORD0;

	float4 ViewVector : TEXCOORD1;  // TEXCOORD1 is just a  semantic

};

// direction light variables
float3 DirectionLightDirection = float3(-1, -1, -1);
float3 DirectionLightColor    = float3(1, 1, 1);
float3 DirectionLightAmbient  = float3(0.5, 0.5, 0.5);
float3 DirectionLightDiffuse  = float3(1, 1, 1);
float4 DirectionLightSpecular = float4(1, 1, 1, 1);

float4 MaterialColor = float4(0.8, 0.8, 0.8, 1.0);
float MaterialShininess = 32;

// shader vertex
VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {

	VertexShaderOutput output;

	// calculate the vertice position
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);

	// build the output structure
	output.Position = mul(viewPosition, Projection);
	output.Normal = mul(normalize(float4(input.Normal, 0.0)), World);
	output.TextureCoord = input.TextureCoord;
	output.ViewVector = normalize(float4(ViewPosition, 1.0) - worldPosition);

	return output;
}

// pixel shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0{

	// position normal
	float4 normal = input.Normal;
	
	// calculate the diffuse ammount
	float4 diff = saturate(dot(-DirectionLightDirection, normal));

	// calculate reflection vector based on view direction
	float4 reflect = normalize(2 * diff * normal - float4(DirectionLightDirection, 1));

	// specular contribution calculation
	float4 spec = pow(saturate(dot(reflect, input.ViewVector)), MaterialShininess);
	
	// calculate ambient light values
	float4 ambient = MaterialColor * 0.3;

	// calculate diffuse light values
	float4 diffuse = MaterialColor * 0.8 * diff;

	// calc specular contribution
	float4 specular = DirectionLightSpecular * spec;
		
	return ambient + diffuse + specular;

}

// technique name and passes
technique Textured {

	pass Pass1 {

		// program compilation
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();

	}

};