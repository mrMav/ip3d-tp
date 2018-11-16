using Microsoft.Xna.Framework;

namespace ip3d_tp
{
    /*
     * Cameras Base Class
     */
    class Camera
    {

        // game reference
        protected Game Game;

        // create variables to hold the current camera position and target
        public Vector3 Position;
        public Vector3 Target;
                
        // these are the matrices to be used when this camera is active
        public Matrix ViewTransform;
        public Matrix ProjectionTransform;

        // the camera field of view
        public float FieldOfView;
        
        // class constructor
        public Camera(Game game, float fieldOfView = 45f)
        {

            Game = game;

            // basic initializations 
            FieldOfView = fieldOfView;

            Position = Vector3.Zero;
            Target = Vector3.Zero;

            ViewTransform = Matrix.Identity;

            // because we change the zoom, we need to refresh the perspective
            // the calculation of the ration must be done with the float cast
            // otherwise we lose precision and the result gets weird
            ProjectionTransform = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView), (float)Game.GraphicsDevice.Viewport.Width / (float)Game.GraphicsDevice.Viewport.Height, 0.1f, 1000f);

        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual string About()
        {
            return "Camera";
        }
        
    }

}