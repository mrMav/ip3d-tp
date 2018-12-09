/*
 * Matrices for vertex shader
 */
float4x4 World;
float4x4 View;
float4x4 Projection;
//float4x4 WorldInverseTranspose;  // we need to inverse and transpose to 'localize' the light vector
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
float3 DirectionLightDirection;
float3 DirectionLightColor = float3(1, 1, 1);
float3 DirectionLightAmbient = float3(0.5, 0.5, 0.5);
float3 DirectionLightDiffuse = float3(1, 1, 1);
float4 DirectionLightSpecular = float4(1, 1, 1, 1);

// material variables
// this is the diffuse texture
texture MaterialDiffuseTexture;
sampler2D MaterialTextureSampler = sampler_state {

	Texture = (MaterialDiffuseTexture);
	Filter = ANISOTROPIC;
	MaxAnisotropy = 8;
	AddressU = Wrap;
	AddressV = Wrap;
};
// this is the diffuse texture
texture Material2DiffuseTexture;
sampler2D Material2TextureSampler = sampler_state {

	Texture = (Material2DiffuseTexture);
	Filter = ANISOTROPIC;
	MaxAnisotropy = 8;
	AddressU = Wrap;
	AddressV = Wrap;
};
// this is the diffuse texture
texture NormalMapTexture;
sampler2D NormalMapSampler = sampler_state {

	Texture = (NormalMapTexture);
	Filter = ANISOTROPIC;
	MaxAnisotropy = 8;
	AddressU = Wrap;
	AddressV = Wrap;
};
// this is the diffuse texture
texture SpecularMapTexture;
sampler2D SpecularMapSampler = sampler_state {

	Texture = (SpecularMapTexture);
	Filter = ANISOTROPIC;
	MaxAnisotropy = 8;
	AddressU = Wrap;
	AddressV = Wrap;
};
float MaterialShininess = 42;

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
		
	// get normal from map
	float3 normal = tex2D(NormalMapSampler, input.TextureCoord).rgb * 2.0 - 1.0;

	// texture color
	float4 color = tex2D(MaterialTextureSampler, input.TextureCoord);

	float4 burrs = tex2D(Material2TextureSampler, input.TextureCoord);

	float4 texDiffuse = saturate(color + burrs);

	// specular map
	float specularValue = tex2D(SpecularMapSampler, input.TextureCoord).r * 0.5;

	// compute tangent T and bitangent B : https://www.opengl.org/discussion_boards/showthread.php/162857-Computing-the-tangent-space-in-the-fragment-shader
	float3 Q1 = ddx(input.Position.xyz);
	float3 Q2 = ddy(input.Position.xyz);
	float2 st1 = ddx(input.TextureCoord);
	float2 st2 = ddy(input.TextureCoord);

	float3 T = normalize(Q1 * st2.x - Q2 * st1.x);
	float3 B = normalize(-Q1 * st2.y + Q2 * st1.y);

	T = normalize((T - input.Normal.xyz * dot(input.Normal.xyz, T)));

	// the transpose of texture-to-eye space matrix
	float3x3 TBN = float3x3(T, B, input.Normal.xyz);

	// transform the normal to eye space 
	normal = mul(normal, TBN);
	
	//normal = input.Normal.xyz;
	
	// calculate the diffuse ammount
	float4 diff = saturate(dot(-DirectionLightDirection, normal));

	// calculate reflection vector based on view direction
	float4 reflect = normalize(2 * diff * float4(normal, 1) - float4(DirectionLightDirection, 1));

	// specular contribution calculation
	float4 spec = pow(saturate(dot(reflect, input.ViewVector)), MaterialShininess);

	// calculate ambient light values
	float4 ambient = texDiffuse * 0.2;

	// calculate diffuse light values
	float4 diffuse = texDiffuse * 0.6 * diff;

	// calc specular contribution
	float4 specular = DirectionLightSpecular * spec * specularValue;

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