using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ip3d_tp
{
    class Tank
    {

        Game Game;

        Model Model;

        public Matrix WorldTransform;

        Axis3D Axis;

        // the model direction vectors
        public Vector3 Front;
        public Vector3 Up;
        public Vector3 Right;

        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        float YawStep = 90f;  // in degrees

        // increase in acceleration
        public float Speed = 0f;
        public float AccelerationValue = 0.3f;

        // velocity will be caped to this maximum
        public float MaxVelocity = 0.75f;

        // the velocity vector
        public Vector3 Velocity = Vector3.Zero;

        // the drag to apply
        public float Drag = 0.8f;

        Effect Shader;

        // rasterizeres for solid and wireframe modes
        RasterizerState SolidRasterizerState;

        // blend state to render this model meshes
        BlendState BlendState;

        Matrix[] BoneTransforms;

        // an array containing the needed textures
        Texture2D[] Textures;

        public short TankID;

        public Tank(Game game)
        {

            Game = game;

            Model = Game.Content.Load<Model>("Models/Tank/tank2"); // tank loaded from fbx

            Shader = Game.Content.Load<Effect>("Effects/Diffuse");

            BoneTransforms = new Matrix[Model.Bones.Count];

            Textures = new Texture2D[Model.Meshes.Count];

            // this texture indexing will work. for now.
            int count = 0;
            foreach(ModelMesh mesh in Model.Meshes)
            {

                Textures[count] = ((BasicEffect)mesh.Effects[0]).Texture;

                count++;

            }

            // setup the rasterizers
            SolidRasterizerState = new RasterizerState();

            SolidRasterizerState.FillMode = FillMode.Solid;

            this.BlendState = new BlendState();
            this.BlendState.AlphaBlendFunction = BlendFunction.Add;

            // init values
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = new Vector3(1.00f);  // the importer is already scaling the model to our needed dimensions

            TankID = 0;

            Axis = new Axis3D(Game, Position, 50f);
            Game.Components.Add(Axis);

        }

        public void Update(GameTime gameTime, Camera camera, Plane surface)
        {

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // controls rotation
            if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Left]))
            {
                Rotation.Y += YawStep * dt;

            }
            else if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Right]))
            {
                Rotation.Y -= YawStep * dt;
            }

            // update the orientation vectors of the tank
            UpdateDirectionVectors(surface);

            // update the model position, based on the updated vectors
            if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Up]))
            {
                Speed -= (AccelerationValue * dt);

            }
            else if (Controls.IsKeyDown(Controls.MovementKeys[TankID, (int)Controls.Cursor.Down]))
            {
                Speed += (AccelerationValue * dt);
                
            } else
            {
                Speed = 0f;
            }

            Velocity += Front * Speed;
            
            // cap the velocity so we don't move faster diagonally
            if (Velocity.Length() > MaxVelocity)
            {
                Velocity.Normalize();
                Velocity *= MaxVelocity;                
            }

            // apply the velocity to the position, based on the delta time between frames
            Position += Velocity;

            // add some sexy drag
            Velocity *= Drag;

            // keep the tank in the surface
            ConstrainToPlane(surface);

            // adjust height from the terrain surface
            SetHeightFromSurface(surface);

            // update the bones matrices
            UpdateMatrices();

            Axis.worldMatrix = Matrix.CreateScale(new Vector3(50f) / Scale) * Model.Root.Transform;
            Axis.UpdateShaderMatrices(camera.ViewTransform, camera.ProjectionTransform);


        }

        public void Draw(GameTime gameTime, Camera camera, Vector4 lightDirection, Vector4 lightColor, float lightIntensity)
        {

            Game.GraphicsDevice.RasterizerState = this.SolidRasterizerState;
            Game.GraphicsDevice.BlendState = this.BlendState;

            int count = 0;
            foreach (ModelMesh mesh in Model.Meshes)
            {
                
                foreach (ModelMeshPart part in mesh.MeshParts)
                {

                    part.Effect = Shader;
               
                    // set the shader properties

                    Matrix world = BoneTransforms[mesh.ParentBone.Index];
                    Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform * world));

                    Shader.Parameters["World"].SetValue(world);
                    Shader.Parameters["View"].SetValue(camera.ViewTransform);
                    Shader.Parameters["Projection"].SetValue(camera.ProjectionTransform);
                    Shader.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                    Shader.Parameters["DiffuseLightDirection"].SetValue(lightDirection);
                    Shader.Parameters["DiffuseColor"].SetValue(lightColor);
                    Shader.Parameters["DiffuseIntensity"].SetValue(lightIntensity);
                    
                    Shader.Parameters["ModelTexture"].SetValue(Textures[count]);
                                        
                }

                mesh.Draw();
                count++;

            }

        }

        private void UpdateMatrices()
        {

            Matrix world = Matrix.CreateWorld(Position, Front, Up);

            Matrix scale = Matrix.CreateScale(Scale);

            WorldTransform = scale * world;

            Model.Root.Transform = WorldTransform;

            Model.CopyAbsoluteBoneTransformsTo(BoneTransforms);

        }

        private void SetHeightFromSurface(Plane surface)
        {

            // get the nearest vertice from the plane
            // will need to offset 
            int x = (int)Math.Floor((Position.X + surface.Width / 2) / surface.SubWidth);
            int z = (int)Math.Floor((Position.Z + surface.Depth / 2) / surface.SubHeight);

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
            Position.Y = Utils.HeightBilinearInterpolation(Position, v0.Position, v1.Position, v2.Position, v3.Position);

        }

        // updates the vectors, using basic trigonometry
        private void UpdateDirectionVectors(Plane surface)
        {

            // get the nearest vertice from the plane
            // will need to offset 
            int x = (int)Math.Floor((Position.X + surface.Width / 2) / surface.SubWidth);
            int z = (int)Math.Floor((Position.Z + surface.Depth / 2) / surface.SubHeight);

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
            float ratioX0 = 1f - (v1.Position.X - Position.X) / (v1.Position.X - v0.Position.X);
            float ratioX1 = 1f - (v3.Position.X - Position.X) / (v3.Position.X - v2.Position.X);
            float ratioZ = 1f - (v3.Position.Z - Position.Z) / (v2.Position.Z - v0.Position.Z);

            Up = Utils.NormalBilinearInterpolation(v0.Normal, v1.Normal, v2.Normal, v3.Normal, ratioX0, ratioX1, ratioZ);

            // create the rotation matrix:
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                MathHelper.ToRadians(Rotation.Y),
                MathHelper.ToRadians(Rotation.X),
                MathHelper.ToRadians(Rotation.Z)
            );

            // Up vector must be already updated
            Right = Vector3.Normalize(Vector3.Cross(Up, Vector3.Transform(Vector3.Forward, rotation)));
            Front = Vector3.Normalize(Vector3.Cross(Up, Vector3.Transform(Vector3.Right, rotation)));
            
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
            if (Position.X < -halfSurfaceWidth + surface.SubWidth)
            {

                Position.X = -halfSurfaceWidth + surface.SubWidth;

            }
            if (Position.X > halfSurfaceWidth - surface.SubWidth)
            {

                Position.X = halfSurfaceWidth - surface.SubWidth;

            }
            if (Position.Z < -halfSurfaceDepth + surface.SubHeight)
            {

                Position.Z = -halfSurfaceDepth + surface.SubHeight;

            }
            if (Position.Z > halfSurfaceDepth - surface.SubHeight)
            {

                Position.Z = halfSurfaceDepth - surface.SubHeight;

            }
        }

        public string GetDebugInfo()
        {

            return $"TankID: {TankID}\n" +
                   $"Position: {Position}\n" +
                   $"Rotation: {Rotation}\n" +
                   $"Velocity: {Math.Round(Velocity.Length(), 4)}\n" +
                   $"Speed: {Math.Round(Speed, 4)}";

        }


    }
}
