using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ip3d_tp.Particles
{
    class QuadParticleEmitter : ParticleEmitter
    {

        float ParticleWidth;

        float ParticleHeight;

        Texture2D Texture;

        Effect Shader;

        BasicEffect ColorShaderEffect;

        VertexBuffer VertexBuffer;

        IndexBuffer IndexBuffer;
        
        /// <summary>
        /// Creates a particle system that emmits textured quads facing the camera.
        /// </summary>
        /// <param name="game">Game Reference</param>
        /// <param name="position">Emitter Position</param>
        /// <param name="pWidth">Particles width</param>
        /// <param name="pHeight">Particles height</param>
        /// <param name="textureKey">The texture to be applied to the particles</param>
        /// <param name="radius">Emitter disc radius</param>
        /// <param name="maxParticles">Max number of Particles to generate</param>
        /// <param name="seed">The seed for randomness</param>
        public QuadParticleEmitter(Game game, Vector3 position, float pWidth, float pHeight, string textureKey, float radius = 5f, int maxParticles = 100, int seed = 0)
            : base(game, position, radius, maxParticles, seed)
        {

            ParticleWidth = pWidth;
            ParticleHeight = pHeight;

            Texture = Game.Content.Load<Texture2D>(textureKey);

            Shader = Game.Content.Load<Effect>("Effects/QuadParticle");

            // this is the shader for the wireframe
            ColorShaderEffect = new BasicEffect(game.GraphicsDevice);
            ColorShaderEffect.VertexColorEnabled = false;
            //ColorShaderEffect.DiffuseColor = new Vector3(1, 1, 1);
            ColorShaderEffect.LightingEnabled = false;  // we won't be using light. we would need normals for that
            ColorShaderEffect.TextureEnabled = true;
            ColorShaderEffect.Texture = Texture;

            CreateGeometry();

        }

        public override void DrawParticle(GameTime gameTime, Camera camera, Particle p)
        {
            if (p.Alive)
            {

                Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);
                Game.GraphicsDevice.Indices = IndexBuffer;
                Game.GraphicsDevice.BlendState = BlendState.Additive;

                // create a matrix to always face the camera
                Matrix world = Matrix.CreateWorld(p.Position, Vector3.Normalize(camera.Position - p.Position), camera.ViewTransform.Up);
                world = Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * world;
                world = Matrix.CreateScale(p.Scale) * world;

                //Shader.Parameters["World"].SetValue(Matrix.CreateTranslation(Particles[i].Position));
                Shader.Parameters["World"].SetValue(world);
                Shader.Parameters["View"].SetValue(camera.ViewTransform);
                Shader.Parameters["Projection"].SetValue(camera.ProjectionTransform);
                Shader.Parameters["Alpha"].SetValue(p.Alpha);
                Shader.Parameters["Texture"].SetValue(Texture);

                // prepare for render
                Game.GraphicsDevice.RasterizerState = RasterizerState;
                Shader.CurrentTechnique.Passes[0].Apply();

                //ColorShaderEffect.World = Matrix.CreateTranslation(Particles[i].Position);
                //ColorShaderEffect.View = camera.ViewTransform;
                //ColorShaderEffect.Projection = camera.ProjectionTransform;
                //ColorShaderEffect.Texture = Texture;

                //Game.GraphicsDevice.RasterizerState = RasterizerState;
                //ColorShaderEffect.CurrentTechnique.Passes[0].Apply();

                // draw with a triangle list
                Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }

        public override void Draw(GameTime gameTime, Camera camera)
        {
            Game.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            Game.GraphicsDevice.Indices = IndexBuffer;
            Game.GraphicsDevice.BlendState = BlendState.Additive;

            /*
            * here we send the data to the shader for processing
            */

            //Matrix world = BoneTransforms[mesh.ParentBone.Index];
            //Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform * world));


            // reorder particles so we can draw transparency
            // wee need to draw the bottom most first
            Array.Sort(Particles, delegate(Particle p1, Particle p2)
            {

                // compare distance to camera
                float p1dist = (p1.Position - camera.Position).Length();
                float p2dist = (p2.Position - camera.Position).Length();

                if (p1dist < p2dist)
                {
                    return 1;
                }
                else if(p1dist > p2dist)
                {
                    return -1;
                } else
                {
                    return 0;
                }
            });


            for(int i = 0; i < MaxParticles; i++)
            {

                if (Particles[i].Alive)
                {
                    

                    // create a matrix to always face the camera
                    Matrix world = Matrix.CreateWorld(Particles[i].Position, Vector3.Normalize(camera.Position - Particles[i].Position), camera.ViewTransform.Up);
                    world = Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * world;
                    world = Matrix.CreateScale(Particles[i].Scale) * world;

                    //Shader.Parameters["World"].SetValue(Matrix.CreateTranslation(Particles[i].Position));
                    Shader.Parameters["World"].SetValue(world);
                    Shader.Parameters["View"].SetValue(camera.ViewTransform);
                    Shader.Parameters["Projection"].SetValue(camera.ProjectionTransform);
                    Shader.Parameters["Alpha"].SetValue(Particles[i].Alpha);
                    Shader.Parameters["Texture"].SetValue(Texture);

                    // prepare for render
                    Game.GraphicsDevice.RasterizerState = RasterizerState;
                    Shader.CurrentTechnique.Passes[0].Apply();

                    //ColorShaderEffect.World = Matrix.CreateTranslation(Particles[i].Position);
                    //ColorShaderEffect.View = camera.ViewTransform;
                    //ColorShaderEffect.Projection = camera.ProjectionTransform;
                    //ColorShaderEffect.Texture = Texture;

                    //Game.GraphicsDevice.RasterizerState = RasterizerState;
                    //ColorShaderEffect.CurrentTechnique.Passes[0].Apply();

                    // draw with a triangle list
                    Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                }
            }

            Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            base.Draw(gameTime, camera);
        }

        /// <summary>
        /// Creates the particles quad
        /// </summary>
        private void CreateGeometry()
        {

            VertexPositionNormalTexture[] vertexList = new VertexPositionNormalTexture[4];
            short[] indices = new short[6];

            VertexPositionNormalTexture topLeft     = new VertexPositionNormalTexture(new Vector3(-0.5f * ParticleWidth, 0f,  0.5f * ParticleHeight), new Vector3(0f, 1f, 0f), new Vector2(0f, 0f));
            VertexPositionNormalTexture topRight    = new VertexPositionNormalTexture(new Vector3( 0.5f * ParticleWidth, 0f,  0.5f * ParticleHeight), new Vector3(0f, 1f, 0f), new Vector2(1f, 0f));
            VertexPositionNormalTexture bottomRight = new VertexPositionNormalTexture(new Vector3( 0.5f * ParticleWidth, 0f, -0.5f * ParticleHeight), new Vector3(0f, 1f, 0f), new Vector2(1f, 1f));
            VertexPositionNormalTexture bottomLeft  = new VertexPositionNormalTexture(new Vector3(-0.5f * ParticleWidth, 0f, -0.5f * ParticleHeight), new Vector3(0f, 1f, 0f), new Vector2(0f, 1f));

            vertexList[0] = topLeft;
            vertexList[1] = topRight;
            vertexList[2] = bottomRight;
            vertexList[3] = bottomLeft;

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;
            indices[3] = 1;
            indices[4] = 2;
            indices[5] = 3;

            VertexBuffer = new VertexBuffer(Game.GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertexList.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData<VertexPositionNormalTexture>(vertexList);

            IndexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData<short>(indices);

        }


    }
}
