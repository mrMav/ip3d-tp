using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ip3d_tp
{
    /*
     * This camera will be constrained to a certain height from the given surface 
     */
    class FreeCamera : Camera
    {

        public Vector3 Front;
        public Vector3 Up;
        public Vector3 Right;
        public Vector3 WorldUp;

        float Yaw;
        float Pitch = -35;

        float MouseSensitivity = 0.1f;
        float Zoom = 45f;

        float AccelerationValue = 0.2f;
        float MaxVelocity = 4f;
        public Vector3 Acceleration;
        public Vector3 Velocity;
        public Vector3 Rotation;

        public float Drag = 0.95f;
                
        // the surface
        Plane Surface;

        // the offset from the surface
        public float OffsetHeight;

        // used to detect 'just' pressed keys
        private KeyboardState OldKeyboardState;
        private MouseState OldMouseState;

        public FreeCamera(Game game, float fieldOfView, Plane surface, float offset = 0) : base(game, fieldOfView)
        {

            OffsetHeight = offset;

            Surface = surface ?? throw new Exception("ERROR::SurfaceFollowCamera::surface argument cannot be null.");

            Acceleration = new Vector3(AccelerationValue, 0.0f, AccelerationValue);
            Velocity = Vector3.Zero;
            Rotation = Vector3.Zero;

            Front = Vector3.Zero;
            Up = Vector3.Zero;
            Right = Vector3.Zero;
            WorldUp = Vector3.Zero;

            OldKeyboardState = Keyboard.GetState();
            OldMouseState = Mouse.GetState();

        }

        public Matrix GetViewMatrix()
        {
            return Matrix.CreateLookAt(Position, Position + Front, Up);
        }

        private void UpdateCameraVectors()
        {
            Front.X = (float)Math.Cos(MathHelper.ToRadians(Yaw)) * (float)Math.Cos(MathHelper.ToRadians(Pitch));
            Front.Y = (float)Math.Sin(MathHelper.ToRadians(Pitch));
            Front.Z = (float)Math.Sin(MathHelper.ToRadians(Yaw)) * (float)Math.Cos(MathHelper.ToRadians(Pitch));
            Front.Normalize();

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.Up));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        private void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
        {

            xoffset *= MouseSensitivity;
            yoffset *= MouseSensitivity;

            Yaw += xoffset;
            Pitch -= yoffset;  // here we can invert the Y

            if (constrainPitch)
            {
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }

            UpdateCameraVectors();

        }

        private void ProcessMouseScroll(MouseState ms)
        {

            float value = ms.ScrollWheelValue - OldMouseState.ScrollWheelValue;
            value *= 0.01f;
            
            if (Zoom >= 1.0f && Zoom <= 80.0f)
            {
                Zoom -= value;
            }

            if (Zoom <= 1.0f) Zoom = 1.0f;

            if (Zoom >= 80.0f) Zoom = 80.0f;

        }

        public override void Update(GameTime gameTime)
        {

            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // get the keyboard state
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            // get input

            // mouse input
            // implements the algorithm in this answer: https://gamedev.stackexchange.com/questions/7812/mouse-aim-in-an-fps
            // (reset position "shim")            
            float midWidth  = Game.GraphicsDevice.Viewport.Width / 2;
            float midHeight = Game.GraphicsDevice.Viewport.Height / 2;
            Console.WriteLine(Game.GraphicsDevice.Viewport.Width + ", " + Game.GraphicsDevice.Viewport.Width);
            ProcessMouseMovement((ms.Position.X - midWidth),
                                 (ms.Position.Y - midHeight));
            ProcessMouseScroll(ms);
            OldMouseState = ms;

            // position
            if (ks.IsKeyDown(Controls.Forward))
            {
                Velocity += Front * AccelerationValue;

            }
            else if (ks.IsKeyDown(Controls.Backward))
            {
                Velocity -= Front * AccelerationValue;
            }

            if (ks.IsKeyDown(Controls.StrafeLeft))
            {
                Velocity -= Right * AccelerationValue;

            }
            else if (ks.IsKeyDown(Controls.StrafeRight))
            {
                Velocity += Right * AccelerationValue;
            }
            OldKeyboardState = ks;

            if(Velocity.Length() > MaxVelocity)
            {
                Velocity.Normalize();
                Velocity *= MaxVelocity;
            }

            Position += Velocity * (dt * 0.01f);
            Velocity *= Drag;            
                        
            ViewTransform = GetViewMatrix();

            // because we change the zoom, we need to refresh teh perspective
            // the calculation of the ration must be done with the float cast
            // otherwise we lose precision and the result gets weird
            ProjectionTransform = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Zoom), (float)Game.GraphicsDevice.Viewport.Width / (float)Game.GraphicsDevice.Viewport.Height, 0.1f, 1000f);

            base.Update(gameTime);
        }

    }

}
