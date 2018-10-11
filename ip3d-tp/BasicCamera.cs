using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ip3d_tp
{
    /*
     * Basic Camera
     */
    class BasicCamera : Camera
    {
        
        // the camera sphere constrain, for when rotating animation is activated
        public float SphereRadius;

        // trigger to toogle between animated camera or not
        public bool RotateCamera;

        // used to detect 'just' pressed keys
        private KeyboardState OldKeyboardState;

        // class constructor
        public BasicCamera(Game game, float fieldOfView, float sphereRadius = 10f) : base(game, fieldOfView)
        {
            // basic initializations
            SphereRadius = sphereRadius;
            RotateCamera = true;

            Position = new Vector3(SphereRadius, SphereRadius, -SphereRadius);
            Target = Vector3.Zero;

            // view matrix is calculated with a LookAt method. It allows
            // to create a view matrix based on a position and target
            ViewTransform = Matrix.CreateLookAt(Position, Target, Vector3.Up);

            OldKeyboardState = Keyboard.GetState();
        }

        public override void Update(GameTime gameTime)
        {
            // get the keyboard state
            KeyboardState ks = Keyboard.GetState();

            // if the C key was just pressed, toogle camera rotation animation
            if (OldKeyboardState.IsKeyUp(Keys.C) && ks.IsKeyDown(Keys.C))
            {
                RotateCamera = !RotateCamera;
            }

            if (RotateCamera)
            {
                // camera rotation is based on the total milliseconds passed since the game init
                // it allows for continuous animation. We multiply the sin or cos for the sphereRadius
                Position.X = (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds * 0.0001f) * SphereRadius;
                Position.Z = (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds * 0.0001f) * SphereRadius;
                //Position.Y = (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds * 0.00025f) * SphereRadius;
                //Position.Y = SphereRadius;

                // finnaly, update the view matrix
                ViewTransform = Matrix.CreateLookAt(Position, Target, Vector3.Up);

            }

            OldKeyboardState = ks;
            
            base.Update(gameTime);
        }

    }
}