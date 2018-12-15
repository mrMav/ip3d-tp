using ip3d_tp.Particles;
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
    public class Projectile
    {

        Game Game;

        Model Model;

        public Body Body;

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

        public QuadParticleEmitter SmokeParticles;

        // gameplay vars
        public bool Alive;
        public float Power;

        public Projectile(Game game, float power)
        {

            Game = game;

            Alive = false;
            Power = power;

            // create the physics body
            Body = new Body(0f, 0f, 0f, 1f, 1f, 1f);
            Body.Acceleration = Vector3.Zero;   // a projectile doesn't have acceleration, just the initial velocity; acceleration is only in Y by the gravity
            Body.Velocity = Vector3.Zero;
            Body.MaxVelocity = float.MaxValue;
            Body.Drag = new Vector3(1f);
            Body.Gravity = Physics.Gravity;
            Body.Bounds.Yaw = MathHelper.ToRadians(-90f);

            //Body.Bounds.Roll = Math.Cos()

            // init values
            //Body.SetPosition(Vector3.Zero);
            //Body.SetRotation(Vector3.Zero);
            //Body.Bounds.Pitch = 3.14f / 2f;
            //Body.Offset = new Vector3(0f, 2f, -0.25f);
            //Body.SetSize(4.3f, 3.2f, 6.5f);

            //Body.Bounds.Yaw = MathHelper.ToRadians(90f);

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

            // smoke trail
            SmokeParticles = new QuadParticleEmitter(Game, Body.Position, 0.5f, 0.5f, "Textures/white_particle", 0.5f, 300, 3);
            SmokeParticles.MakeParticles(1f, Color.White);
            SmokeParticles.ParticleVelocity = new Vector3(0f, 0f, 0f);
            SmokeParticles.SpawnRate = 0f;
            SmokeParticles.Burst = true;
            SmokeParticles.ParticlesPerBurst = 10;
            SmokeParticles.XVelocityVariationRange = new Vector2(-50f, 50f);
            SmokeParticles.YVelocityVariationRange = new Vector2(-50f, 50f);
            SmokeParticles.ZVelocityVariationRange = new Vector2(-50f, 50f);
            SmokeParticles.ParticleLifespanMilliseconds = 2000f;
            SmokeParticles.ParticleLifespanVariationMilliseconds = 1500f;
            SmokeParticles.Activated = true;
            SmokeParticles.InitialScale = 1f;
            SmokeParticles.FinalScale = 5f;
            ParticleManager.AddParticleEmitter(SmokeParticles);



        }

        /// <summary>
        /// sets the projectile initial velocity based on angle
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        public void SetVelocity(float pitch, float yaw)
        {
            //https://courses.lumenlearning.com/boundless-physics/chapter/projectile-motion/

            //Body.Velocity = new Vector3(
            //    Power * (float)Math.Cos(yaw),
            //    Power * (float)Math.Sin(pitch),
            //    Power * (float)Math.Sin(yaw)
            //);
            //Body.Velocity = new Vector3(
            //    Power * (float)Math.Cos(MathHelper.ToRadians(pitch)) * (float)Math.Cos(MathHelper.ToRadians(yaw)),
            //    Power * (float)Math.Sin(MathHelper.ToRadians(pitch)),
            //    Power * (float)Math.Cos(MathHelper.ToRadians(yaw)) * (float)Math.Sin(MathHelper.ToRadians(yaw))
            //);
            Body.Velocity = new Vector3(
                Power * -(float)Math.Cos((yaw)),
                Power * (float)Math.Sin(MathHelper.ToRadians(pitch)),
                Power * (float)Math.Sin((yaw))
            );
            Console.WriteLine(Body.Velocity);

        }

        public void Kill()
        {
            Alive = false;
            Body.Velocity = Vector3.Zero;
            //Body.SetRotation(Vector3.Zero);
            //Body.SetPosition(Vector3.Zero);

            SmokeParticles.Activated = false;

        }

        public void Revive()
        {
            Alive = true;

            SmokeParticles.Activated = true;
        }

        public void Update(GameTime gameTime, Plane surface)
        {

            if (Alive)
            {

                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                // make it spin
                Body.Bounds.Roll += 6.28f * dt; // 1 rotation per second

                // update motion
                Body.PreMovementUpdate(gameTime);
                Body.UpdateMotionAcceleration(gameTime);

                CheckOutOfBounds(surface);
                
                float groundHeight = surface.GetHeightFromSurface(Body.Position);
                if (Body.Position.Y <= groundHeight)
                {
                    // explode
                    Kill();
                }

                if (!Alive)
                {
                    return;
                }

                //// calculate projectile angle
                //Vector3 a = Vector3.Normalize(new Vector3(Body.DeltaX(), Body.DeltaY(), Body.DeltaZ()));
                //Vector3 b = Vector3.Normalize(new Vector3(Body.DeltaX(), 0f, Body.DeltaZ()));
                //Vector3 n = Vector3.Cross(a, Vector3.Forward);
                //int sign = n.X < 0 ? -1 : 1;   // gets the sign of the angle, by crossing them. the cross direction tells the polarity
                //                               // help from https://www.mathworks.com/matlabcentral/answers/266282-negative-angle-between-vectors-planes

                //float angle = ((float)Math.Acos(Vector3.Dot(a, b))) * sign;  // this is the maximum angle

                //Body.Bounds.Pitch = angle;

                // what if I just set the front to be equal to the delta?
                Vector3 front = Vector3.Normalize(Body.Position - Body.PreviousPosition);
                Vector3 right = Vector3.Normalize(Vector3.Cross(front, Vector3.Up));
                Vector3 up = Vector3.Normalize(Vector3.Cross(front, right));
                right = Vector3.Normalize(Vector3.Cross(front, up));

                //Body.Bounds.UpdateMatrices();
                Body.Bounds.SetWorldTransform(Matrix.CreateWorld(Body.Position, front, up));
                SmokeParticles.Activated = true;
                               
            }

            SmokeParticles.UpdateMatrices(Body.Bounds.WorldTransform);
            SmokeParticles.Update(gameTime);

        }


        public void Draw(GameTime gameTime, Camera camera, Vector3 lightDirection, Vector4 lightColor, float lightIntensity)
        {

            if (Alive)
            {

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

        /// <summary>
        /// Checks if the projectile goes out the world, and kills it if yes
        /// </summary>
        /// <param name="surface"></param>
        public void CheckOutOfBounds(Plane surface)
        {
            // constrain to bounds
            // inset one subdivision

            float halfSurfaceWidth = surface.Width / 2;
            float halfSurfaceDepth = surface.Depth / 2;

            // because we know that the plane origin is at its center
            // we will have to calculate the bounds with that in mind, and add 
            // te width and depth divided by 2
            if (Body.X < -halfSurfaceWidth + surface.SubWidth ||
                Body.X > halfSurfaceWidth - surface.SubWidth ||
                Body.Z < -halfSurfaceDepth + surface.SubHeight ||
                Body.Z > halfSurfaceDepth - surface.SubHeight)
            {

                Kill();   

            }
        }

    }
}
