using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ip3d_tp
{
    /*
     * This camera will be constrained to a certain height from the given surface 
     */
    class SurfaceFollowCamera : FreeCamera
    {

        // the surface
        Plane Surface;

        // the offset from the surface
        public float OffsetHeight;
        
        public SurfaceFollowCamera(Game game, float fieldOfView, Plane surface, float offset = 1) : base(game, fieldOfView)
        {

            OffsetHeight = offset;

            Surface = surface ?? throw new Exception("ERROR::SurfaceFollowCamera::surface argument cannot be null.");

        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);

            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // constrain to bounds
            // inset one subdivision
            if(Position.X < -Surface.Width / 2 + Surface.SubWidth) {

                Position.X = -Surface.Width / 2 + Surface.SubWidth;

            }
            if (Position.X > Surface.Width / 2 - Surface.SubWidth)
            {

                Position.X = Surface.Width / 2 - Surface.SubWidth;

            }
            if (Position.Z < -Surface.Depth / 2 + Surface.SubHeight)
            {

                Position.Z = -Surface.Depth / 2 + Surface.SubHeight;

            }
            if (Position.Z > Surface.Depth / 2 - Surface.SubHeight)
            {

                Position.Z = Surface.Depth / 2 - Surface.SubHeight;

            }

            // get the nearest vertice from the plane
            // will need to offset 
            int x = (int)Math.Floor((Position.X + Surface.Width / 2) / Surface.SubWidth);
            int z = (int)Math.Floor((Position.Z + Surface.Depth / 2) / Surface.SubHeight);

            // NUM_COLS * x + y
            int verticeIndex0 = (Surface.XSubs + 1) * z + x;
            int verticeIndex1 = verticeIndex0 + 1;
            int verticeIndex2 = verticeIndex0 + Surface.XSubs + 1;
            int verticeIndex3 = verticeIndex2 + 1;

            /*
             * 0---1
             * | / |
             * 2---3
             */


            // interpolate heights
            // based on https://en.wikipedia.org/wiki/Bilinear_interpolation
            // the function of the vertice, is the return of the Y value.

            VertexPositionTexture v0 = Surface.VertexList[verticeIndex0];
            VertexPositionTexture v1 = Surface.VertexList[verticeIndex1];
            VertexPositionTexture v2 = Surface.VertexList[verticeIndex2];
            VertexPositionTexture v3 = Surface.VertexList[verticeIndex3];
                        
            Position.Y = Utils.HeightBilinearInterpolation(Position, v0.Position, v1.Position, v2.Position, v3.Position) + OffsetHeight;

            //float camPosX = Position.X;
            //float camPosZ = Position.Z;

            //float x0 = Surface.VertexList[verticeIndex0].Position.X;
            //float x1 = Surface.VertexList[verticeIndex1].Position.X;
            //float z0 = Surface.VertexList[verticeIndex0].Position.Z;
            //float z1 = Surface.VertexList[verticeIndex2].Position.Z;

            //// interpolate the x's
            //float x0Lerp = (x1 - camPosX) / (x1 - x0) * v0.Position.Y + (camPosX - x0) / (x1 - x0) * v1.Position.Y;
            //float x1Lerp = (x1 - camPosX) / (x1 - x0) * v2.Position.Y + (camPosX - x0) / (x1 - x0) * v3.Position.Y;

            //float zLerp = (z1 - camPosZ) / (z1 - z0) * x0Lerp + (camPosZ - z0) / (z1 - z0) * x1Lerp;

            ////Surface.SetVerticeColor(verticeIndex0, Color.Red);
            ////Surface.SetVerticeColor(verticeIndex1, Color.Green);
            ////Surface.SetVerticeColor(verticeIndex2, Color.Blue);
            ////Surface.SetVerticeColor(verticeIndex3, Color.HotPink);
            
            //float height = Surface.VertexList[verticeIndex0].Position.Y;
            //Position.Y = zLerp + 4f;

            //Console.WriteLine($"pos: {Position}, col: {x}, {z}: index: {verticeIndex0}: height: {height.ToString("###0.#########")}, zlerp: {zLerp.ToString("###0.#########")}, x0lerp: {x0Lerp.ToString("###0.#########")}, x1lerp: {x1Lerp.ToString("###0.#########")}");

            ViewTransform = GetViewMatrix();

            // because we change the zoom, we need to refresh teh perspective
            // the calculation of the ration must be done with the float cast
            // otherwise we lose precision and the result gets weird
            ProjectionTransform = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Zoom), (float)Game.GraphicsDevice.Viewport.Width / (float)Game.GraphicsDevice.Viewport.Height, 0.1f, 1000f);

        }
        
    }

}
