using ip3d_tp.Physics3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ip3d_tp
{
    class Tank
    {
        // game reference
        Game Game;

        // the loaded 3d model
        Model Model;

        // defines a physics body
        public Body Body;
        public Box BodyDebug;
        
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

        float YawStep = MathHelper.ToRadians(90f);  // in degrees
        
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

        // current wheels angle
        float WheelsAngle = 0f;

        // an array containing the needed textures
        Texture2D[] Textures;

        // textures for shading enrichment
        Texture2D BurrsMap;
        Texture2D SpecularMap;
        Texture2D NormalMap;

        // this tank ID for controls
        public short TankID;

        // constructor
        public Tank(Game game)
        {

            Game = game;

            // tank loaded from fbx
            Model = Game.Content.Load<Model>("Models/Tank/tank2");

            // create the physics body
            Body = new Body(0f, 0f, 0f, 5f, 5f, 7f);
            Body.Acceleration = new Vector3(0.01f);
            Body.MaxVelocity = 0.02f;
            Body.Drag = new Vector3(0.8f);

            // init values
            Body.SetPosition(Vector3.Zero);
            Body.SetRotation(Vector3.Zero);
            Body.Offset = new Vector3(0f, 2f, 0f);
            Body.SetSize(4.5f, 2f, 7f);
            Scale = new Vector3(1.00f);  // the importer is already scaling the model to our needed dimensions

            BodyDebug = new Box(Game, Body.Offset, 4.5f, 2f, 7f);
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

            // controls rotation
            if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Left]))
            {
                Body.Bounds.Yaw += YawStep * Velocity.Length() * (Speed >= 0f ? -2f : 2f) * dt;

            }
            else if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Right]))
            {
                Body.Bounds.Yaw -= YawStep * Velocity.Length() * (Speed >= 0f ? -2f : 2f) * dt;
            }
          
            // update the model position, based on the updated vectors
            if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Up]))
            {
                Body.Speed -= (Body.Acceleration.Z * dt);

            }
            else if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Down]))
            {
                Body.Speed += (Body.Acceleration.Z * dt);
                
            } else
            {
                Body.Speed = 0f;
            }

            // update the orientation vectors of the tank
            UpdateDirectionVectors(surface);

            // moves the body
            Body.UpdateMotion(gameTime);
          
            // update wheels angle
            WheelsAngle += Velocity.Length() * MathHelper.ToRadians(25f) * (Speed >= 0f ? -1f : 1f);  // last bit is to get the sign of the speed
          
            // keep the tank in the surface
            ConstrainToPlane(surface);

            // adjust height from the terrain surface
            SetHeightFromSurface(surface);

            // animate wheels
            RotateWheels(gameTime);

            // update the bones matrices
            UpdateMatrices();

            // update the debug axis
            //Axis.worldMatrix = Matrix.CreateScale(new Vector3(50f) / Scale) * Model.Root.Transform;
            Axis.worldMatrix = WorldTransform;
            Axis.UpdateShaderMatrices(camera.ViewTransform, camera.ProjectionTransform);
            
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

                    //Shader.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                    Shader.Parameters["ViewPosition"].SetValue(camera.Position);

                    Shader.Parameters["MaterialDiffuseTexture"].SetValue(Textures[count]);                    
                    Shader.Parameters["Material2DiffuseTexture"].SetValue(BurrsMap);
                    Shader.Parameters["SpecularMapTexture"].SetValue(SpecularMap);                    
                    Shader.Parameters["NormalMapTexture"].SetValue(NormalMap);

                }

                mesh.Draw();
                count++;

            }

            if(Global.ShowHelp)
                BodyDebug.Draw(gameTime, camera);

        }

        public void UpdateMatrices()
        {

            Body.Bounds.UpdateMatrices();

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

        // updates the vectors, using basic trigonometry
        public void UpdateDirectionVectors(Plane surface)
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

            Body.Bounds.SetUp(Utils.NormalBilinearInterpolation(v0.Normal, v1.Normal, v2.Normal, v3.Normal, ratioX0, ratioX1, ratioZ));

            // Up vector must be already updated
            Body.Bounds.UpdateMatrices();
        }

        public void ConstrainToPlane(Plane surface)
        {
            // constrain to bounds
            // inset one subdivision

            float halfSurfaceWidth = surface.Width / 2;
            float halfSurfaceDepth = surface.Depth / 2;

            // because we know that the plane origin is at its center
            // we will have to calculate the bounds with that in mind, and add 
            // te width and depth divided by 2
            if (Body.X < -halfSurfaceWidth + surface.SubWidth)
            {

                Body.X = -halfSurfaceWidth + surface.SubWidth;

            }
            if (Body.X > halfSurfaceWidth - surface.SubWidth)
            {

                Body.X = halfSurfaceWidth - surface.SubWidth;

            }
            if (Body.Z < -halfSurfaceDepth + surface.SubHeight)
            {

                Body.Z = -halfSurfaceDepth + surface.SubHeight;

            }
            if (Body.Z > halfSurfaceDepth - surface.SubHeight)
            {

                Body.Z = halfSurfaceDepth - surface.SubHeight;

            }
        }

        private void RotateWheels(GameTime gameTime)
        {
            // rotation based on velocity

            // the resulting matrix
            Matrix rotationMatrix = Matrix.CreateRotationX(WheelsAngle);

            // apply
            LFrontWheel.Transform = rotationMatrix * Matrix.CreateTranslation(LFrontWheel.Transform.Translation);
            LBackWheel.Transform = rotationMatrix * Matrix.CreateTranslation(LBackWheel.Transform.Translation);
            RFrontWheel.Transform = rotationMatrix * Matrix.CreateTranslation(RFrontWheel.Transform.Translation);
            RBackWheel.Transform = rotationMatrix * Matrix.CreateTranslation(RBackWheel.Transform.Translation);

        }

        public string GetDebugInfo()
        {

            return $"TankID: {TankID}\n" +
                   $"Position: {Body.Position}\n" +
                   $"Rotation: {Body.Rotation}\n" +
                   $"Velocity: {Math.Round(Body.Velocity.Length(), 4)}\n" +
                   $"Speed: {Math.Round(Body.Speed, 4)}";

        }


    }
}
