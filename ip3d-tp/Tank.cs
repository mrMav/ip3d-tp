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

        // the model direction vectors
        Vector3 Front;
        Vector3 Up;
        Vector3 Right;

        Vector3 Position;
        Vector3 Rotation;
        Vector3 Scale;

        float YawStep = MathHelper.ToRadians(5f);  // in degrees

        // increase in acceleration
        public float AccelerationValue = 2.0f;

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

            UpdateDirectionVectors();

            // update the model position, based on the updated vectors
            if (Controls.IsKeyDown(Controls.TankMoveForward))
            {
                Velocity += Front * AccelerationValue;

            }
            else if (Controls.IsKeyDown(Controls.TankMoveBackward))
            {
                Velocity -= Front * AccelerationValue;
            }

            // cap the velocity so we don't move faster diagonally
            if (Velocity.Length() > MaxVelocity)
            {
                Velocity.Normalize();
                Velocity *= MaxVelocity;
            }

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

            // apply the velocity to the position, based on the delta time between frames
            Position += Velocity * (dt * 0.01f);

            // add some sexy drag
            Velocity *= Drag;

            /* 
             * adjust the Y position:
             */ 

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

            Matrix position = Matrix.CreateTranslation(Position);

            // update the model transform
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                MathHelper.ToRadians(Rotation.Y),
                MathHelper.ToRadians(Rotation.X),
                MathHelper.ToRadians(Rotation.Z)
            );

            Matrix scale = Matrix.CreateScale(Scale);

            Model.Root.Transform = scale * rotation * position;

            Model.CopyAbsoluteBoneTransformsTo(BoneTransforms);

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

        // updates the vectors, using basic trigonometry
        private void UpdateDirectionVectors()
        {

            // thes function was built with the help found on the current article:
            // https://learnopengl.com/Getting-started/Camera

            // first the front vector is calculated and normalized.
            // then, the right vector is calculated, crossing the front and world up vector
            // the camera up vector, is then calculated, crossing the right and the front

            Right.X = (float)Math.Cos(MathHelper.ToRadians(-Rotation.Y)) * (float)Math.Cos(MathHelper.ToRadians(Rotation.X));
            Right.Y = (float)Math.Sin(MathHelper.ToRadians(Rotation.X));
            Right.Z = (float)Math.Sin(MathHelper.ToRadians(-Rotation.Y)) * (float)Math.Cos(MathHelper.ToRadians(Rotation.X));
            Right.Normalize();

            Front = Vector3.Normalize(Vector3.Cross(Right, Vector3.Up));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }


    }
}
