using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ip3d_tp
{
    /*
     * This camera will be constrained to a certain height from the given surface 
     * it extends the freecamera
     */
    public class SurfaceFollowCamera : FreeCamera
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
            
            // use interpolation to calculate the height at this point in space
            Position.Y = Surface.GetHeightFromSurface(Position) + OffsetHeight;

            // refresh, because we change the height and constrain the position
            ViewTransform = GetViewMatrix();

        }

        public override string About()
        {
            return "Use NumPad 8-6-5-4 to move around.\nLook around with the mouse.\nScroll zooms in and out.\nYou will be snapped and constrained to the terrain.";
        }

    }

}
