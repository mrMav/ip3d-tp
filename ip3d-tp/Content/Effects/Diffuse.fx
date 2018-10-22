/*

Based on http://rbwhitaker.wikidot.com/diffuse-lighting-shader

*/

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;

float4 DiffuseLightDirection = float4(100, 100, 0, 0);
float4 DiffuseColor = float4(1.0, 1.0, 1.0, 1.0);
float DiffuseIntensity = 0.001;


struct VertexShaderInput {

	float4 Position : POSITION0;
	float4 Normal : NORMAL0;

};

struct VertexShaderOutput {

	float4 Position : POSITION0;
	float4 Color : COLOR0;

};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {

	VertexShaderOutput output;

	// calculate the vertice position
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	// calculate the normal
	float4 normal = mul(input.Normal, WorldInverseTranspose);
	float lightIntensity = dot(normal, DiffuseLightDirection); // calculate angle between surface normal
	output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);
	
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0{

	return saturate(input.Color + AmbientColor * AmbientIntensity);

}

technique Diffuse {

	pass Pass1 {

		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();

	}

};