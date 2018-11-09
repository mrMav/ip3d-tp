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

        Axis3D Axis;

        // the model direction vectors
        public Vector3 Front;
        public Vector3 Up;
        public Vector3 Right;

        Vector3 Position;
        Vector3 Rotation;
        Vector3 Scale;

        float YawStep = MathHelper.ToRadians(5f);  // in degrees

        // increase in acceleration
        public float AccelerationValue = 0.1f;

        // velocity will be caped to this maximum
        public float MaxVelocity = 2.5f;

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

        public Tank(Game game)
        {

            Game = game;

            Model = Game.Content.Load<Model>("Tank/tank");

            Shader = Game.Content.Load<Effect>("Effects/Diffuse");

            BoneTransforms = new Matrix[Model.Bones.Count];

            // setup the rasterizers
            SolidRasterizerState = new RasterizerState();

            SolidRasterizerState.FillMode = FillMode.Solid;

            this.BlendState = new BlendState();
            this.BlendState.AlphaBlendFunction = BlendFunction.Add;

            // init values
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = new Vector3(0.01f);

            Axis = new Axis3D(Game, Position, 50f);
            Game.Components.Add(Axis);

        }

        public void Update(GameTime gameTime, Camera camera, Plane surface)
        {

            float dt = (float)gameTime.TotalGameTime.TotalSeconds;

            // controls rotation
            if (Controls.IsKeyDown(Controls.TankRotateLeft))
            {
                Rotation.Y += YawStep * dt;

            }
            else if (Controls.IsKeyDown(Controls.TankRotateRight))
            {
                Rotation.Y -= YawStep * dt;
            }

            // update the orientation vectors of the tank
            UpdateDirectionVectors(surface);

            // update the model position, based on the updated vectors
            if (Controls.IsKeyDown(Controls.TankMoveForward))
            {
                Velocity -= Front * AccelerationValue;

            }
            else if (Controls.IsKeyDown(Controls.TankMoveBackward))
            {
                Velocity += Front * AccelerationValue;
            }

            // cap the velocity so we don't move faster diagonally
            if (Velocity.Length() > MaxVelocity)
            {
                Velocity.Normalize();
                Velocity *= MaxVelocity;
            }
            
            // apply the velocity to the position, based on the delta time between frames
            Position += Velocity * dt;

            // add some sexy drag
            Velocity *= Drag;

            // keep the tank in the surface
            ConstrainToPlane(surface);

            // adjust height from the terrain surface
            SetHeightFromSurface(surface);

            // update the bones matrices
            UpdateMatrices();

            Axis.worldMatrix = Matrix.CreateScale(Vector3.Zero / Scale) * Model.Root.Transform;
            Axis.UpdateShaderMatrices(camera.ViewTransform, camera.ProjectionTransform);


        }

        public void Draw(GameTime gameTime, Camera camera)
        {

            Game.GraphicsDevice.RasterizerState = this.SolidRasterizerState;
            Game.GraphicsDevice.BlendState = this.BlendState;

            foreach (ModelMesh mesh in Model.Meshes)
            {

                foreach(BasicEffect fx in mesh.Effects)
                {

                    fx.World = BoneTransforms[mesh.ParentBone.Index];
                    fx.View  = camera.ViewTransform;
                    fx.Projection = camera.ProjectionTransform;

                    fx.EnableDefaultLighting();

                }

                mesh.Draw();

            }

        }

        private void UpdateMatrices()
        {

            Matrix world = Matrix.CreateWorld(Position, Front, Up);

            Matrix scale = Matrix.CreateScale(Scale);

            Model.Root.Transform = scale * world;

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

            return $"Position: {Position}\n" +
                   $"Rotation: {Rotation}\n" +
                   $"Velocity: {Velocity.Length()}";

        }


    }
}
