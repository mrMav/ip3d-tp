using Microsoft.Xna.Framework;
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
        
        public SurfaceFollowCamera(Game game, float fieldOfView, Plane surface, float offset = 0) : base(game, fieldOfView)
        {

            OffsetHeight = offset;

            Surface = surface ?? throw new Exception("ERROR::SurfaceFollowCamera::surface argument cannot be null.");

        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);

            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // constrain to bounds
            if(Position.X < -Surface.Width / 2) { }


            // get the nearest vertice from the plane
            // will need to offset 
            int x = (int)Math.Floor((Position.X + Surface.Width / 2) / Surface.SubWidth);
            int z = (int)Math.Floor((Position.Z + Surface.Depth / 2) / Surface.SubHeight);

            // NUM_COLS * x + y
            int verticeIndex = (Surface.XSubs + 1) * z + x;
            float height = Surface.VertexList[verticeIndex].Position.Y;
            Surface.SetVerticeColor(verticeIndex, Color.Red);

            Console.WriteLine($"pos: {Position}, col: {x}, {z}: index: {verticeIndex}: height: {height.ToString("###0.#########")}");

            Position.Y = height + 4f;

            ViewTransform = GetViewMatrix();

            // because we change the zoom, we need to refresh teh perspective
            // the calculation of the ration must be done with the float cast
            // otherwise we lose precision and the result gets weird
            ProjectionTransform = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Zoom), (float)Game.GraphicsDevice.Viewport.Width / (float)Game.GraphicsDevice.Viewport.Height, 0.1f, 1000f);

        }

    }

}
