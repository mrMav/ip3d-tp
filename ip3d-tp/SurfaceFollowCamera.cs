using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ip3d_tp
{
    /*
     * This camera will be constrained to a certain height from the given surface 
     * it extends the freecamera
     */
    class SurfaceFollowCamera : FreeCamera
    {

        // reference to the surface
        Plane Surface;

        // the offset from the surface
        public float OffsetHeight;
        
        // constructor
        public SurfaceFollowCamera(Game game, float fieldOfView, Plane surface, float offset = 1) : base(game, fieldOfView)
        {

            OffsetHeight = offset;

            // in this phase, I wnat to break if the plane is null
            Surface = surface ?? throw new Exception("ERROR::SurfaceFollowCamera::surface argument cannot be null.");

        }

        public override void Update(GameTime gameTime)
        {
            // we want the free camera functionallity, so update it's logic
            base.Update(gameTime);

            // delta time
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            float halfSurfaceWidth = Surface.Width / 2;
            float halfSurfaceDepth = Surface.Depth / 2;

            // constrain to bounds
            // inset one subdivision

            // because we know that the plane origin is at its center
            // we will have to calculate the bounds with that in mind, and add 
            // te width and depth divided by 2
            if (Position.X < -halfSurfaceWidth + Surface.SubWidth) {

                Position.X = -halfSurfaceWidth + Surface.SubWidth;

            }
            if (Position.X > halfSurfaceWidth - Surface.SubWidth)
            {

                Position.X = halfSurfaceWidth - Surface.SubWidth;

            }
            if (Position.Z < -halfSurfaceDepth + Surface.SubHeight)
            {

                Position.Z = -halfSurfaceDepth + Surface.SubHeight;

            }
            if (Position.Z > halfSurfaceDepth - Surface.SubHeight)
            {

                Position.Z = halfSurfaceDepth - Surface.SubHeight;

            }

            // get the nearest vertice from the plane
            // will need to offset 
            int x = (int)Math.Floor((Position.X + Surface.Width / 2) / Surface.SubWidth);
            int z = (int)Math.Floor((Position.Z + Surface.Depth / 2) / Surface.SubHeight);
            
            /* 
             * get the neighbour vertices
             * 
             * 0---1
             * | / |
             * 2---3
             */
            int verticeIndex0 = (Surface.XSubs + 1) * z + x;
            int verticeIndex1 = verticeIndex0 + 1;
            int verticeIndex2 = verticeIndex0 + Surface.XSubs + 1;
            int verticeIndex3 = verticeIndex2 + 1;
            
            VertexPositionNormalTexture v0 = Surface.VertexList[verticeIndex0];
            VertexPositionNormalTexture v1 = Surface.VertexList[verticeIndex1];
            VertexPositionNormalTexture v2 = Surface.VertexList[verticeIndex2];
            VertexPositionNormalTexture v3 = Surface.VertexList[verticeIndex3];
            
            // use interpolation to calculate the height at this point in space
            Position.Y = Utils.HeightBilinearInterpolation(Position, v0.Position, v1.Position, v2.Position, v3.Position) + OffsetHeight;

            // refresh, because we change the height and constrain the position
            ViewTransform = GetViewMatrix();

        }

        public override string About()
        {
            return "Use WASD to move around.\nLook around with the mouse.\nScroll zooms in and out.\nYou will be snapped and constrained to the terrain.";
        }

    }

}
