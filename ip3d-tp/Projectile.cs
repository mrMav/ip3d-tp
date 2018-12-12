using ip3d_tp.Physics3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ip3d_tp
{
    class Projectile
    {

        Game Game;

        Model Model;

        Body Body;

        // the shader to render the tank
        Effect Shader;

        // rasterizer
        RasterizerState SolidRasterizerState;

        // blend state to render this model meshes
        BlendState BlendState;

        // textures for shading enrichment
        Texture2D ColorMap;
        Texture2D BurrsMap;
        Texture2D SpecularMap;
        Texture2D NormalMap;

        public Projectile(Game game)
        {

            Game = game;

            // create the physics body
            Body = new Body(0f, 5f, 0f, 5f, 5f, 7f);
            Body.Acceleration = new Vector3(0.1f);
            Body.MaxVelocity = 0.5f;
            Body.Drag = new Vector3(0.8f);

            // init values
            //Body.SetPosition(Vector3.Zero);
            //Body.SetRotation(Vector3.Zero);
            Body.Bounds.Pitch = 3.14f / 2f;
            Body.Offset = new Vector3(0f, 2f, -0.25f);
            Body.SetSize(4.3f, 3.2f, 6.5f);

            Body.Bounds.Yaw = MathHelper.ToRadians(90f);

            Model = Game.Content.Load<Model>("Models/Shell/Shell");
            ColorMap = Game.Content.Load<Texture2D>("Models/Shell/Shell_Easter_Color");
            
            Shader = Game.Content.Load<Effect>("Effects/Tank");
            BurrsMap = Game.Content.Load<Texture2D>("Textures/metal_diffuse_1k");
            SpecularMap = Game.Content.Load<Texture2D>("Textures/metal_specular_1k");
            NormalMap = Game.Content.Load<Texture2D>("Textures/metal_normal_1k");

            // setup the rasterizer
            SolidRasterizerState = new RasterizerState();
            SolidRasterizerState.FillMode = FillMode.Solid;

            // the blend state
            BlendState = new BlendState();
            BlendState.AlphaBlendFunction = BlendFunction.Add;



        }

        public void Draw(GameTime gameTime, Camera camera, Vector3 lightDirection, Vector4 lightColor, float lightIntensity)
        {

            Body.Bounds.Pitch += 10f;
            Body.Bounds.UpdateMatrices();

            Game.GraphicsDevice.RasterizerState = this.SolidRasterizerState;
            Game.GraphicsDevice.BlendState = this.BlendState;

            int count = 0;
            foreach (ModelMesh mesh in Model.Meshes)
            {

                foreach (ModelMeshPart part in mesh.MeshParts)
                {

                    /*
                     * here we send the data to the shader for processing
                     * see the Diffuse.fx for the implementation
                     */

                    part.Effect = Shader;

                    // set the shader properties

                    Matrix world = Body.Bounds.WorldTransform;

                    Shader.Parameters["DirectionLightDirection"].SetValue(lightDirection);

                    Shader.Parameters["World"].SetValue(world);
                    Shader.Parameters["View"].SetValue(camera.ViewTransform);
                    Shader.Parameters["Projection"].SetValue(camera.ProjectionTransform);

                    Shader.Parameters["ViewPosition"].SetValue(camera.Position);

                    Shader.Parameters["MaterialDiffuseTexture"].SetValue(ColorMap);
                    Shader.Parameters["Material2DiffuseTexture"].SetValue(BurrsMap);
                    Shader.Parameters["SpecularMapTexture"].SetValue(SpecularMap);
                    Shader.Parameters["NormalMapTexture"].SetValue(NormalMap);

                }

                mesh.Draw();
                count++;

            }
            
        }

    }
}
