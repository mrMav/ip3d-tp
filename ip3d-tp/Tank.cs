using ip3d_tp.Particles;
using ip3d_tp.Physics3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ip3d_tp
{
    public class Tank
    {
        // game reference
        Game Game;

        // the loaded 3d model
        Model Model;

        // defines a physics body
        public Body Body;
        public Box BodyDebug;

        // define a target Body, used for Bot control
        public Body TargetBody;
        public Vector3 Steering;

        // type of autonomous movement
        public Global.BotBehaviour BotBehaviour = Global.BotBehaviour.None;

        
        public Matrix WorldTransform
        {
            get
            {
                return Body.Bounds.WorldTransform;
            }
        }

        // an axis for debug purposes
        Axis3D Axis;

        public Vector3 Scale;

        float YawStep = MathHelper.ToRadians(180f);  // in degrees

        float TurretYaw = 0f;
        float CanonPitch = 0f;
        
        // the shader to render the tank
        Effect Shader;

        // rasterizer
        RasterizerState SolidRasterizerState;

        // blend state to render this model meshes
        BlendState BlendState;

        // array to store the Bones Transformations
        Matrix[] BoneTransforms;

        // create references to the wheels
        ModelBone LFrontWheel;
        ModelBone LBackWheel;
        ModelBone RFrontWheel;
        ModelBone RBackWheel;
        ModelBone RFrontSteer;
        ModelBone LFrontSteer;

        ModelBone Turret;
        ModelBone Canon;

        Matrix LFrontWheelTransform;
        Matrix LBackWheelTransform;
        Matrix RFrontWheelTransform;
        Matrix RBackWheelTransform;
        Matrix RFrontSteerTransform;
        Matrix LFrontSteerTransform;

        Matrix TurretTransform;
        Matrix CanonTransform;

        // current wheels angle
        float WheelsAngle = 0f;
        float SteerAngle = 0f;
        float MaxSteerAngle = MathHelper.ToRadians(10f);

        // an array containing the needed textures
        Texture2D[] Textures;

        // textures for shading enrichment
        Texture2D BurrsMap;
        Texture2D SpecularMap;
        Texture2D NormalMap;

        // this tank ID for controls
        public short TankID;

        // projectiles
        float LastShot = 0f;
        float ProjectilePower = 2.8f;
        float ShootRate = 500f;

        // bullets pool
        public List<Projectile> Bullets;

        // particle emitters for some effects
        QuadParticleEmitter SmokeParticlesLeft;
        QuadParticleEmitter SmokeParticlesRight;
        QuadParticleEmitter DustParticles;

        // constructor
        public Tank(Game game)
        {

            Game = game;

            // tank loaded from fbx
            Model = Game.Content.Load<Model>("Models/Tank/tank2");

            // create the physics body
            Body = new Body(0f, 0f, 0f, 5f, 5f, 7f);
            Body.Acceleration = new Vector3(0.1f);
            Body.MaxVelocity = 0.5f;
            Body.Drag = new Vector3(0.8f);

            // init values
            Body.SetPosition(Vector3.Zero);
            Body.SetRotation(Vector3.Zero);
            Body.Offset = new Vector3(0f, 2f, -0.25f);
            Body.SetSize(4.3f, 3.2f, 6.5f);
            Scale = new Vector3(1.00f);  // the importer is already scaling the model to our needed dimensions

            Body.Bounds.Yaw = MathHelper.ToRadians(90f);

            BodyDebug = new Box(Game, Body.Offset, Body.CollisionRect.Width, Body.CollisionRect.Height, Body.CollisionRect.Depth);
            BodyDebug.ShowSolid = false;
            BodyDebug.ShowWireframe = true;

            // loading the shader
            Shader = Game.Content.Load<Effect>("Effects/Tank");
            BurrsMap = Game.Content.Load<Texture2D>("Textures/metal_diffuse_1k");
            SpecularMap = Game.Content.Load<Texture2D>("Textures/metal_specular_1k");
            NormalMap = Game.Content.Load<Texture2D>("Textures/metal_normal_1k");

            BoneTransforms = new Matrix[Model.Bones.Count];

            LFrontWheel = Model.Bones["l_front_wheel_geo"];
            LBackWheel = Model.Bones["l_back_wheel_geo"];
            RFrontWheel = Model.Bones["r_front_wheel_geo"];
            RBackWheel = Model.Bones["r_back_wheel_geo"];
            RFrontSteer = Model.Bones["r_steer_geo"];
            LFrontSteer = Model.Bones["l_steer_geo"];
            Turret = Model.Bones["turret_geo"];
            Canon = Model.Bones["canon_geo"];

            LFrontWheelTransform = LFrontWheel.Transform;
            LBackWheelTransform = LBackWheel.Transform; 
            RFrontWheelTransform = RFrontWheel.Transform;
            RBackWheelTransform = RBackWheel.Transform;
            RFrontSteerTransform = RFrontSteer.Transform;
            LFrontSteerTransform = LFrontSteer.Transform;
            TurretTransform = Turret.Transform;
            CanonTransform = Canon.Transform;

            Textures = new Texture2D[Model.Meshes.Count];

            // this texture indexing will work. for now.
            // we are indexing the textures to the meshes
            int count = 0;
            foreach(ModelMesh mesh in Model.Meshes)
            {

                Textures[count] = ((BasicEffect)mesh.Effects[0]).Texture;

                count++;

            }

            // setup the rasterizer
            SolidRasterizerState = new RasterizerState();

            SolidRasterizerState.FillMode = FillMode.Solid;

            // the blend state
            BlendState = new BlendState();
            BlendState.AlphaBlendFunction = BlendFunction.Add;
            
            // default the ID to 0
            TankID = 0;

            // create a few bullets
            Bullets = new List<Projectile>();
            for (int i = 0; i < 10; i++)
            {
                Projectile p = new Projectile(Game, ProjectilePower);
                
                // init settings here

                Bullets.Add(p);
            }

            // init particles
            SmokeParticlesLeft = new QuadParticleEmitter(Game, Body.Position, 0.5f, 0.5f, "Textures/smoke_particle", 0.5f, 100, TankID + 1);
            SmokeParticlesLeft.MakeParticles(1f, Color.White);
            SmokeParticlesLeft.ParticleVelocity = new Vector3(0f, 5f, 0f);
            SmokeParticlesLeft.SpawnRate = 250f;
            SmokeParticlesLeft.Burst = true;
            SmokeParticlesLeft.ParticlesPerBurst = 3;
            SmokeParticlesLeft.XVelocityVariationRange = new Vector2(-200f, 200f);
            SmokeParticlesLeft.YVelocityVariationRange = new Vector2(0f, 100f);
            SmokeParticlesLeft.ZVelocityVariationRange = new Vector2(-200f, 200f);
            SmokeParticlesLeft.ParticleLifespanMilliseconds = 1000f;
            SmokeParticlesLeft.ParticleLifespanVariationMilliseconds = 500f;
            SmokeParticlesLeft.Activated = false;
            SmokeParticlesLeft.InitialScale = 0.5f;
            SmokeParticlesLeft.FinalScale = 5f;
            ParticleManager.AddParticleEmitter(SmokeParticlesLeft);

            SmokeParticlesRight = new QuadParticleEmitter(Game, Body.Position, 0.5f, 0.5f, "Textures/smoke_particle", 0.5f, 100, TankID + 2);
            SmokeParticlesRight.MakeParticles(1f, Color.White);
            SmokeParticlesRight.ParticleVelocity = new Vector3(0f, 5f, 0f);
            SmokeParticlesRight.SpawnRate = 250f;
            SmokeParticlesRight.Burst = true;
            SmokeParticlesRight.ParticlesPerBurst = 3;
            SmokeParticlesRight.XVelocityVariationRange = new Vector2(-200f, 200f);
            SmokeParticlesRight.YVelocityVariationRange = new Vector2(0f, 100f);
            SmokeParticlesRight.ZVelocityVariationRange = new Vector2(-200f, 200f);
            SmokeParticlesRight.ParticleLifespanMilliseconds = 1000f;
            SmokeParticlesRight.ParticleLifespanVariationMilliseconds = 500f;
            SmokeParticlesRight.Activated = false;
            SmokeParticlesRight.InitialScale = 0.5f;
            SmokeParticlesRight.FinalScale = 5f;
            ParticleManager.AddParticleEmitter(SmokeParticlesRight);

            // init particles
            DustParticles = new QuadParticleEmitter(Game, Body.Position, 0.5f, 0.5f, "Textures/dust_particle", 5f, 100, TankID + 4);
            DustParticles.MakeParticles(1f, Color.White);
            DustParticles.ParticleVelocity = new Vector3(0f, 1.5f, -2f);
            DustParticles.SpawnRate = 20f;
            DustParticles.Burst = false;
            DustParticles.ParticlesPerBurst = 3;
            DustParticles.XVelocityVariationRange = new Vector2(-300f, 300f);
            DustParticles.YVelocityVariationRange = new Vector2(0f, 200f);
            DustParticles.ZVelocityVariationRange = new Vector2(-300f, 50f);
            DustParticles.ParticleLifespanMilliseconds = 5000f;
            DustParticles.ParticleLifespanVariationMilliseconds = 500f;
            DustParticles.Activated = false;
            DustParticles.InitialScale = 8f;
            DustParticles.FinalScale = 14f;
            ParticleManager.AddParticleEmitter(DustParticles);

            // create the axis for debug
            Axis = new Axis3D(Game, Body.Position, 50f);
            Game.Components.Add(Axis);
                       
        }

        public void Update(GameTime gameTime, Camera camera, Plane surface)
        {
            // delta for time based calcs
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // pre movement function, to store information
            // regarding previous frame
            Body.PreMovementUpdate(gameTime);

            Steering = Vector3.Zero;

            // update tank motion logic
            if(TankID == Global.PlayerID)
            {
                UpdatePlayerControlled(dt);
            } else
            {
                UpdateAutonomousMovement(dt, TargetBody);
            }

            // update the orientation vectors of the tank
            //UpdateDirectionVectors(surface);

            // moves the body
            Body.UpdateMotion(gameTime);


            // keep the tank in the surface
            ConstrainToPlane(surface);

            //UpdateDirectionVectors(surface);
            UpdateMatrices(surface);


            // due to lack of time for optmizing the particles
            // we will only create projectiles and engine smoke for the 
            // player controlled tank
            if(TankID == Global.PlayerID)
            {
                // calculate the particle system position
                // we calculate an offset and a rotation in model space
                // then we transform to world space
                Vector3 offsetLeft  = new Vector3(1.67f, 2.8f, -3f);
                Vector3 offsetRight = new Vector3(-1.67f, 2.8f, -3f);
                float pitch = -35f;

                // now we build the particles system own transform
                Matrix particlesTransformLeft  = Matrix.CreateRotationX(MathHelper.ToRadians(pitch)) * Matrix.CreateTranslation(offsetLeft) * WorldTransform;
                Matrix particlesTransformRight = Matrix.CreateRotationX(MathHelper.ToRadians(pitch)) * Matrix.CreateTranslation(offsetRight) * WorldTransform;

                // finally, set the transform and update
                SmokeParticlesLeft.Activated = true;
                SmokeParticlesLeft.UpdateMatrices(particlesTransformLeft);
                SmokeParticlesLeft.Update(gameTime);

                SmokeParticlesRight.Activated = true;
                SmokeParticlesRight.UpdateMatrices(particlesTransformRight);
                SmokeParticlesRight.Update(gameTime);


            } else
            {
                SmokeParticlesRight.Activated = false;
                SmokeParticlesLeft.Activated = false;
            }

            // we do update all the tanks dust
            DustParticles.UpdateMatrices(WorldTransform);
            DustParticles.Update(gameTime);

        }

        public void MoveForward()
        {
            Body.Speed -= (Body.Acceleration.Z);
            SetFullThrottleEngineParticles();
            DustParticles.Activated = true;
        }

        public void MoveBackward()
        {
            Body.Speed += (Body.Acceleration.Z);
            SetFullThrottleEngineParticles();
            DustParticles.Activated = true;
        }

        public void SetIdle()
        {
            Body.Speed = 0f;
            SetIdleEngineParticles();
            DustParticles.Activated = false;
        }

        public void UpdatePlayerControlled(float dt)
        {
            int dir = Body.Speed > 0f ? -1 : 1;

            // controls rotation
            if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Left]))
            {
                Body.Bounds.Yaw += YawStep * Body.Velocity.Length() * dir * dt;
                SteerAngle += YawStep * dt;

            }
            else if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Right]))
            {
                Body.Bounds.Yaw -= YawStep * Body.Velocity.Length() * dir * dt;
                SteerAngle -= YawStep * dt;
            }
            else
            {
                SteerAngle *= 0.8f;
            }
            SteerAngle = MathHelper.Clamp(SteerAngle, -MaxSteerAngle, MaxSteerAngle);

            // update the model position, based on the updated vectors
            if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Up]))
            {
                MoveForward();

            }
            else if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Down]))
            {
                MoveBackward();

            }
            else
            {
                SetIdle();
            }
        }

        public void UpdateAutonomousMovement(float dt, Body target)
        {
            // behaviour based on steering behaviours by Craig W.Reynolds
            // http://www.red3d.com/cwr/steer/gdc99/

            float repulsionWeight = 120f;

            if(BotBehaviour == Global.BotBehaviour.Seek)
            {
                Vector3 desiredVelocity = Vector3.Normalize(Body.Position - target.Position) * Body.MaxVelocity;
                Vector3 steering = desiredVelocity - Body.Velocity;

                Steering -= steering;

                repulsionWeight = 20f;


            } else if(BotBehaviour == Global.BotBehaviour.Flee)
            {
                Vector3 desiredVelocity = Vector3.Normalize(target.Position - Body.Position) * Body.MaxVelocity;
                Vector3 steering = desiredVelocity - Body.Velocity;

                Steering -= steering;

                repulsionWeight = 20f;


            }
            else if(BotBehaviour == Global.BotBehaviour.Pursuit)
            {

                // estimate prediction interval based on distance
                float dist = Body.Position.Length() - target.Position.Length();
                float c = MathHelper.ToRadians(20f);
                float t = dist * c;

                Vector3 predictedPosition = target.Position + (target.Bounds.Front * target.Speed * (dt * t));

                Vector3 desiredVelocity = Vector3.Normalize(Body.Position - predictedPosition) * Body.MaxVelocity;
                Vector3 steering = desiredVelocity - Body.Velocity;
                
                // limit steering
                float maxforce = 0.1f;  // steering force towards target
                if (steering.Length() > maxforce)
                {
                    steering.Normalize();
                    steering *= maxforce;
                }

                Steering -= steering;

                repulsionWeight = 120f;


            }
            else if (BotBehaviour == Global.BotBehaviour.Evade)
            {

                // estimate prediction interval based on distance
                float dist = Body.Position.Length() - target.Position.Length();
                float c = MathHelper.ToRadians(20f);
                float t = dist * c;

                Vector3 predictedPosition = target.Position + (target.Bounds.Front * target.Speed * (dt * t));

                Vector3 desiredVelocity = Vector3.Normalize(Body.Position - predictedPosition) * Body.MaxVelocity;
                Vector3 steering = desiredVelocity - Body.Velocity;

                // limit steering
                float maxforce = 0.6f;  // steering force towards target
                if (steering.Length() > maxforce)
                {
                    steering.Normalize();
                    steering *= maxforce;
                }

                Steering += steering;

                repulsionWeight = 120f;
            }
            else
            {
                SetIdle();
                return;
            }

            // separation from other tanks
            float weight = 1f / repulsionWeight;
            Vector3 repulsion = Vector3.Zero;

            for (int k = 0; k < Global.Bots.Length; k++)
            {

                if (TankID != Global.Bots[k].TankID)
                {

                    // calculate repulsion to this tank
                    Vector3 offset = Vector3.Normalize(Body.Position - Global.Bots[k].Body.Position);
                    offset *= weight;

                    repulsion += offset;

                }
                

            }

            Steering += repulsion;
            
            Body.Bounds.SetFront(Vector3.Normalize(Body.Bounds.Front - Steering));

            MoveForward();


        }

        public void PostMotionUpdate(GameTime gameTime, Camera camera, Plane surface)
        {

            // keep the tank in the surface
            ConstrainToPlane(surface);

            // adjust height from the terrain surface
            SetHeightFromSurface(surface);
            

            // update the orientation vectors of the tank
            //UpdateDirectionVectors(surface);

            // update the bones matrices
            UpdateMatrices(surface);

            // update the debug axis
            //Axis.worldMatrix = Matrix.CreateScale(new Vector3(50f) / Scale) * Model.Root.Transform;
            Axis.worldMatrix = WorldTransform;
            Axis.UpdateShaderMatrices(camera.ViewTransform, camera.ProjectionTransform);
        }

        public void UpdateProjectiles(GameTime gameTime, Plane surface, Camera camera)
        {
            // user input
            if (TankID == 0 && Controls.CurrMouseState.LeftButton == ButtonState.Pressed)
            {
                
                // check shooting rate
                if (LastShot < gameTime.TotalGameTime.TotalMilliseconds)
                {

                    // update the shooting rate to now
                    LastShot = (float)gameTime.TotalGameTime.TotalMilliseconds + this.ShootRate;

                    // get the first dead bullet from the pool
                    Projectile b = null;
                    for (int i = 0; i < Bullets.Count; i++)
                    {
                        if (!Bullets[i].Alive)
                        {
                            b = Bullets[i];
                            break;
                        }

                    }

                    // if b not null, we have a bullet
                    if (b != null)
                    {

                        b.Revive();

                        // calculate offset from tank origin
                        Vector3 offset = new Vector3(
                            (float)Math.Cos(MathHelper.ToRadians(TurretYaw) - Body.Bounds.Yaw) * 1.5f,
                            (float)Math.Sin(MathHelper.ToRadians(CanonPitch)) * 1f + 3.5f,
                            -(float)Math.Sin(MathHelper.ToRadians(TurretYaw) - Body.Bounds.Yaw) * 1.5f
                        );

                        Vector3 turretCenterOffset = new Vector3(0f, 0f, -0.35f);

                        b.Body.SetPosition(Body.Position + Vector3.Transform(offset + turretCenterOffset, WorldTransform.Rotation));
                        
                        
                        //b.SetVelocity(CanonPitch, MathHelper.ToRadians(TurretYaw) - Body.Bounds.Yaw);

                        //b.Body.SetPosition(Vector3.Transform(Vector3.Zero, Matrix.CreateFromQuaternion(BoneTransforms[9].Rotation) * Matrix.CreateTranslation(BoneTransforms[10].Translation)));
                        //b.Body.SetPosition(Vector3.Transform(Vector3.Zero, Matrix.CreateRotationY(MathHelper.ToRadians(TurretYaw + 90f) - Body.Bounds.Yaw) * Matrix.CreateTranslation(BoneTransforms[10].Translation)));
                        //b.Body.SetPosition(Vector3.Transform(new Vector3(0, 0, 1), WorldTransform));
                        //b.Body.SetPosition(Vector3.Transform(new Vector3(0, 0, 0), Matrix.CreateRotationY(MathHelper.ToRadians(TurretYaw + 90f) - Body.Bounds.Yaw) * Matrix.CreateRotationX(MathHelper.ToRadians(-CanonPitch)) * WorldTransform));
                        //b.Body.SetPosition(Vector3.Transform(CanonTransform.Translation, WorldTransform));
                        //b.Body.SetPosition(Vector3.Transform(new Vector3(0.0f, 3.2f, 1), Matrix.CreateRotationY(MathHelper.ToRadians(TurretYaw + 90f) - Body.Bounds.Yaw) * WorldTransform));
                        //b.Body.SetPosition(Vector3.Transform(new Vector3(0, 10, 0), WorldTransform));
                        

                        b.Body.Velocity = new Vector3(
                            ProjectilePower * (float)Math.Cos(MathHelper.ToRadians(TurretYaw) - Body.Bounds.Yaw),
                            ProjectilePower * (float)Math.Sin(MathHelper.ToRadians(CanonPitch)),
                            ProjectilePower * -(float)Math.Sin(MathHelper.ToRadians(TurretYaw) - Body.Bounds.Yaw)
                        );

                        b.Body.Velocity = Vector3.Transform(b.Body.Velocity, WorldTransform.Rotation);

                        // shaky shaky
                        camera.ActivateShake(gameTime, 300f, 0.8f, 0.02f);

                    }
                }

            }

            foreach (Projectile b in Bullets)
            {
                b.Update(gameTime, surface);
            }

        }

        public void CalculateAnimations(GameTime gameTime, Camera camera, Plane surface)
        {

            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // animate wheels
            RotateWheels(gameTime);

            // update turret and canon direction based on camera direction
            if(TankID == Global.PlayerID)
            {
                if(Global.AimMode == Global.PlayerAimMode.Camera)
                {

                    CanonPitch = ((ThirdPersonCamera)camera).Pitch;
                    CanonPitch += 30f;

                    TurretYaw = ((ThirdPersonCamera)camera).Yaw;

                } else if(Global.AimMode == Global.PlayerAimMode.Keys)
                {

                    float ammount = 0.1f;
                    
                    if(Controls.IsKeyDown(Keys.Up))
                    {
                        CanonPitch += ammount * dt;
                        //CanonPitch += 30f;
                    } else if (Controls.IsKeyDown(Keys.Down))
                    {
                        CanonPitch -= ammount * dt;
                    }

                    if (Controls.IsKeyDown(Keys.Left))
                    {
                        TurretYaw += ammount * dt;
                    }
                    else if (Controls.IsKeyDown(Keys.Right))
                    {
                        TurretYaw -= ammount * dt;
                    }
                    
                }
                
                if (CanonPitch > 60f)
                    CanonPitch = 60f;
                else if (CanonPitch < -30f)
                    CanonPitch = -30f;
            }


            RotateTurret(gameTime, CanonPitch, TurretYaw);

            // recalculate
            PostMotionUpdate(gameTime, camera, surface);

        }

        public void Draw(GameTime gameTime, Camera camera, Vector3 lightDirection, Vector4 lightColor, float lightIntensity)
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

                    Matrix world = BoneTransforms[mesh.ParentBone.Index];
                    Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform * world));
                    
                    Shader.Parameters["DirectionLightDirection"].SetValue(lightDirection);

                    Shader.Parameters["World"].SetValue(world);
                    Shader.Parameters["View"].SetValue(camera.ViewTransform);
                    Shader.Parameters["Projection"].SetValue(camera.ProjectionTransform);

                    Shader.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                    Shader.Parameters["ViewPosition"].SetValue(camera.Position);

                    Shader.Parameters["MaterialDiffuseTexture"].SetValue(Textures[count]);                    
                    Shader.Parameters["Material2DiffuseTexture"].SetValue(BurrsMap);
                    Shader.Parameters["SpecularMapTexture"].SetValue(SpecularMap);                    
                    Shader.Parameters["NormalMapTexture"].SetValue(NormalMap);

                }

                mesh.Draw();
                count++;

            }

            // render projectiles
            foreach (Projectile b in Bullets)
                b.Draw(gameTime, camera, lightDirection, lightColor, lightIntensity);

            //SmokeParticlesLeft.Draw(gameTime, camera);

            if(Global.ShowHelp)
                BodyDebug.Draw(gameTime, camera);

        }

        public void UpdateMatrices(Plane surface)
        {

            // update matrices
            if (TankID == Global.PlayerID)
            {
                Body.Bounds.UpdateMatrices(GetUpVectorFromTerrain(surface));
            }
            else
            {
                Body.Bounds.UpdateMatrices(GetUpVectorFromTerrain(surface), Body.Bounds.Front);
            }

            // Up Vector must be already set
            //Body.Bounds.UpdateMatrices(GetUpVectorFromTerrain(surface));
            Body.UpdateCollisionRect();
            
            Model.Root.Transform = WorldTransform;

            Model.CopyAbsoluteBoneTransformsTo(BoneTransforms);

            BodyDebug.WorldTransform = Body.CollisionRect.WorldTransform;

        }

        public void SetHeightFromSurface(Plane surface)
        {

            // get the nearest vertice from the plane
            // will need to offset 
            int x = (int)Math.Floor((Body.X + surface.Width / 2) / surface.SubWidth);
            int z = (int)Math.Floor((Body.Z + surface.Depth / 2) / surface.SubHeight);

            /* 
             * get the neighbour vertices
             * 
             * 0---1
             * | / |
             * 2---3
             */
            int verticeIndex0 = (surface.XSubs + 1) * z + x;
            int verticeIndex1 = verticeIndex0 + 1;
            int verticeIndex2 = verticeIndex0 + surface.XSubs + 1;
            int verticeIndex3 = verticeIndex2 + 1;

            VertexPositionNormalTexture v0 = surface.VertexList[verticeIndex0];
            VertexPositionNormalTexture v1 = surface.VertexList[verticeIndex1];
            VertexPositionNormalTexture v2 = surface.VertexList[verticeIndex2];
            VertexPositionNormalTexture v3 = surface.VertexList[verticeIndex3];

            // use interpolation to calculate the height at this point in space
            Body.Y = Utils.HeightBilinearInterpolation(Body.Position, v0.Position, v1.Position, v2.Position, v3.Position);

        }
                
        public Vector3 GetUpVectorFromTerrain(Plane surface)
        {

            // get the nearest vertice from the plane
            // will need to offset 
            int x = (int)Math.Floor((Body.Position.X + surface.Width / 2) / surface.SubWidth);
            int z = (int)Math.Floor((Body.Position.Z + surface.Depth / 2) / surface.SubHeight);

            /* 
             * get the neighbour vertices
             * 
             * 0---1
             * | / |
             * 2---3
             */
            int verticeIndex0 = (surface.XSubs + 1) * z + x;
            int verticeIndex1 = verticeIndex0 + 1;
            int verticeIndex2 = verticeIndex0 + surface.XSubs + 1;
            int verticeIndex3 = verticeIndex2 + 1;

            VertexPositionNormalTexture v0 = surface.VertexList[verticeIndex0];
            VertexPositionNormalTexture v1 = surface.VertexList[verticeIndex1];
            VertexPositionNormalTexture v2 = surface.VertexList[verticeIndex2];
            VertexPositionNormalTexture v3 = surface.VertexList[verticeIndex3];

            // interpolate the terrain normals, so we know the tank up vector
            //Up = Utils.NormalBilinearInterpolation(Position, n0.Position, n1.Position, n2.Position, n3.Position);
            //Up = v0.Normal;
            float ratioX0 = 1f - (v1.Position.X - Body.Position.X) / (v1.Position.X - v0.Position.X);
            float ratioX1 = 1f - (v3.Position.X - Body.Position.X) / (v3.Position.X - v2.Position.X);
            float ratioZ = 1f - (v3.Position.Z - Body.Position.Z) / (v2.Position.Z - v0.Position.Z);

            return Utils.NormalBilinearInterpolation(v0.Normal, v1.Normal, v2.Normal, v3.Normal, ratioX0, ratioX1, ratioZ);

        }

        public void ConstrainToPlane(Plane surface)
        {
            // constrain to bounds
            // inset one subdivision

            float halfSurfaceWidth = surface.Width / 2;
            float halfSurfaceDepth = surface.Depth / 2;

            float offset = 2f;

            // because we know that the plane origin is at its center
            // we will have to calculate the bounds with that in mind, and add 
            // te width and depth divided by 2
            if (Body.X + offset < -halfSurfaceWidth + surface.SubWidth)
            {

                Body.X = -halfSurfaceWidth + surface.SubWidth;

            }
            if (Body.X - offset > halfSurfaceWidth - surface.SubWidth)
            {

                Body.X = halfSurfaceWidth - surface.SubWidth;

            }
            if (Body.Z + offset < -halfSurfaceDepth + surface.SubHeight)
            {

                Body.Z = -halfSurfaceDepth + surface.SubHeight;

            }
            if (Body.Z - offset > halfSurfaceDepth - surface.SubHeight)
            {

                Body.Z = halfSurfaceDepth - surface.SubHeight;

            }
        }

        /// <summary>
        /// Set particles state for the full throttle engine
        /// </summary>
        public void SetFullThrottleEngineParticles()
        {
            int n = 4;
            float t = 60f;

            SmokeParticlesLeft.ParticlesPerBurst = n;
            SmokeParticlesLeft.SpawnRate = t;
            SmokeParticlesLeft.ParticleTint = Color.Black;

            SmokeParticlesRight.ParticlesPerBurst = n;
            SmokeParticlesRight.SpawnRate = t;
            SmokeParticlesRight.ParticleTint = Color.Black;


        }

        /// <summary>
        /// Set particles state for the idle engine
        /// </summary>
        public void SetIdleEngineParticles()
        {
            int n = 1;
            float t = 120f;

            SmokeParticlesLeft.ParticlesPerBurst = n;
            SmokeParticlesLeft.SpawnRate = t;
            SmokeParticlesLeft.ParticleTint = Color.White;

            SmokeParticlesRight.ParticlesPerBurst = n;
            SmokeParticlesRight.SpawnRate = t;
            SmokeParticlesRight.ParticleTint = Color.White;

        }

        private void RotateWheels(GameTime gameTime)
        {
            // rotation based on velocity
            // update wheels angle
            // this line calculates the sign of the moving direction based on the delta position
            // this is the true direction that the tank is moving.
            Vector3 delta = Body.Position - Body.PreviousPosition;
            float dot = Vector3.Dot(delta, Body.Bounds.Front);
            float sign = (dot > 0f ? -1 : 1);
            //Console.WriteLine(dot);
            WheelsAngle += delta.Length() * sign;
            // last bit is to get the sign of the speed

            // the resulting matrix
            Matrix rotationMatrix = Matrix.CreateRotationX(this.WheelsAngle);

            // apply
            LFrontWheel.Transform = rotationMatrix * LFrontWheelTransform;
            LBackWheel.Transform = rotationMatrix * LBackWheelTransform;
            RFrontWheel.Transform = rotationMatrix * RFrontWheelTransform;
            RBackWheel.Transform = rotationMatrix * RBackWheelTransform;


            // rotate the steers           
            
            Matrix rotationRightSteer = Matrix.CreateRotationY(SteerAngle );
            Matrix rotationLeftSteer = Matrix.CreateRotationY(SteerAngle);
            

            RFrontSteer.Transform = rotationRightSteer * RFrontSteerTransform;
            LFrontSteer.Transform = rotationLeftSteer * LFrontSteerTransform;


        }

        // handles rotation of the turret and canon
        private void RotateTurret(GameTime gameTime, float pitch, float yaw)
        {

            

            Matrix turretRotationMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(yaw + 90f) - Body.Bounds.Yaw);
            Matrix canonRotationMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(-CanonPitch));

            // apply
            Turret.Transform = turretRotationMatrix * Matrix.CreateTranslation(TurretTransform.Translation);
            Canon.Transform = canonRotationMatrix * Matrix.CreateTranslation(CanonTransform.Translation);
            
        }

        public string GetDebugInfo()
        {

            return $"TankID: {TankID}\n" +
                   $"Wheels: {WheelsAngle}, TurretYaw: {TurretYaw}, CanonPitch: {CanonPitch}";

        }


    }
}
