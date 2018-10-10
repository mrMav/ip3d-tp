using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ip3d_tp
{
    /*
     * This camera will be constrained to a certain height from the given surface
     */ 
    class SurfaceFollowCamera : Camera
    {
        // the offset from the surface
        public float OffsetHeight;

        // the direction the camera is facing
        private Vector3 LookAt;

        // acceleration of the camera
        public Vector3 Acceleration;

        // current velocity of the camera
        public Vector3 Velocity;

        // rotation vector
        public Vector3 Rotation;

        // ammount of Drag (normalized)
        public float Drag = 0.9f;

        // the speed of rotation
        public float RotationSpeed = MathHelper.ToRadians(1f);
        
        // the surface
        Plane Surface;
        
        // used to detect 'just' pressed keys
        private KeyboardState OldKeyboardState;

        public SurfaceFollowCamera(Game game, float fieldOfView, Plane surface, float offset = 0) : base(game, fieldOfView)
        {

            OffsetHeight = offset;

            Surface = surface ?? throw new Exception("ERROR::SurfaceFollowCamera::surface argument cannot be null.");

            float accel = 0.1f;
            Acceleration = new Vector3(accel, 0.0f, accel);
            Velocity = Vector3.Zero;
            Rotation = Vector3.Zero;
            
        }

        public override void Update(GameTime gameTime)
        {

            // get the keyboard state
            KeyboardState ks = Keyboard.GetState();

            // get input
            // position
            if(ks.IsKeyDown(Controls.Forward))
            {
                Velocity.Z += Acceleration.Z; 

            } else if(ks.IsKeyDown(Controls.Backward))
            {
                Velocity.Z -= Acceleration.Z;
            }

            if (ks.IsKeyDown(Controls.StrafeLeft))
            {
                Velocity.X += Acceleration.X;

            }
            else if (ks.IsKeyDown(Controls.StrafeRight))
            {
                Velocity.X -= Acceleration.X;
            }

            // rotation            
            if (ks.IsKeyDown(Controls.CameraRotateYCW))
            {
                Rotation.Y -= RotationSpeed;

            }
            else if (ks.IsKeyDown(Controls.CameraRotateYCCW))
            {
                Rotation.Y += RotationSpeed;
            }

            if (ks.IsKeyDown(Controls.CameraRotateXCW))
            {
                Rotation.X += RotationSpeed;

            }
            else if (ks.IsKeyDown(Controls.CameraRotateXCCW))
            {
                Rotation.X -= RotationSpeed;
            }

            OldKeyboardState = ks;
            
            // a little help from: 
            // https://stackoverflow.com/questions/15746173/fps-style-camera-target-calculation-in-xna
            // https://www.youtube.com/watch?v=XkpZLzT5OV4

            // values of vector Rotation must be in radians
            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);

            // this is the velocity applied in the direction of the camera facing direction
            Vector3 movement = Vector3.Transform(Velocity, rotationMatrix);
            Position += movement;

            // dampening teh velocity
            Velocity *= Drag;

            // update look at
            Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);
            LookAt = Position + lookAtOffset;

            // finally, update the view matrix
            ViewTransform = Matrix.CreateLookAt(Position, LookAt, Vector3.Up);            

            base.Update(gameTime);
        }



    }
}
