using Microsoft.Xna.Framework;

namespace ip3d_tp
{
    /*
     * Camera will extend the base GameComponent class
     */
    class Camera : GameComponent
    {
        // create variables to hold the current camera position and target
        public Vector3 Position;
        public Vector3 Target;

        // these are the matrices to be used when this camera is active
        public Matrix ViewTransform;
        public Matrix ProjectionTransform;

        // the camera field of view
        public float FieldOfView;   

        // class constructor
        public Camera(Game game, float fieldOfView = 45f) : base(game)
        {
            // basic initializations 
            FieldOfView = fieldOfView;

            Position = Vector3.Zero;
            Target = Vector3.Zero;

            ViewTransform = Matrix.Identity;

            // the projection matrix is responsible for defining a frustum view.
            // this is the eye emulation
            ProjectionTransform = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView), game.GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 1000f);

        }
        
    }

}