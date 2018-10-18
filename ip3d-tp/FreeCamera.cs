using Microsoft.Xna.Framework;
using System;

namespace ip3d_tp
{
    /*
     * A free camera to explore the world.
     */
    class FreeCamera : Camera
    {
        // the camera vectors
        public Vector3 Front;
        public Vector3 Up;
        public Vector3 Right;
        public Vector3 WorldUp;

        // yaw and pitch angles
        float Yaw;
        float Pitch = -35;  // default;

        // sensicity scaler
        float MouseSensitivity = 0.1f;

        // zoom 
        protected float Zoom = 45f;

        // movement variables
        float AccelerationValue = 0.2f;
        public float MaxVelocity = 4f;
        public Vector3 Acceleration;
        public Vector3 Velocity;
        public Vector3 Rotation;

        public float Drag = 0.95f;
        
        // constructor 
        public FreeCamera(Game game, float fieldOfView) : base(game, fieldOfView)
        {

            Acceleration = new Vector3(AccelerationValue, 0.0f, AccelerationValue);
            Velocity = Vector3.Zero;
            Rotation = Vector3.Zero;

            Front = Vector3.Zero;
            Up = Vector3.Zero;
            Right = Vector3.Zero;
            WorldUp = Vector3.Zero;

        }

        // gets a new and refreshed view matrix, based on the current vectors
        public Matrix GetViewMatrix()
        {
            return Matrix.CreateLookAt(Position, Position + Front, Up);
        }

        // updates the camera vectors, using basic trigonometry
        private void UpdateCameraVectors()
        {

            // thes function was built with the help found on the current article:
            // https://learnopengl.com/Getting-started/Camera

            // first the front vector is calculated and normalized.
            // then, the right vector is calculated, crossing the front and world up vector
            // the camera up vector, is then calculated, crossing the right and the front

            Front.X = (float)Math.Cos(MathHelper.ToRadians(Yaw)) * (float)Math.Cos(MathHelper.ToRadians(Pitch));
            Front.Y = (float)Math.Sin(MathHelper.ToRadians(Pitch));
            Front.Z = (float)Math.Sin(MathHelper.ToRadians(Yaw)) * (float)Math.Cos(MathHelper.ToRadians(Pitch));
            Front.Normalize();

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.Up));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        // handles the mouse movement, updating the yaw, pitch and vectors
        // constrain the picth to avoid angles lock
        private void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
        {
            // the given offset, is the diference from the previous mouse position and the current one
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

        // used to update the camera zoom based on mouse scroll
        // the code is self explanatory
        private void ProcessMouseScroll()
        {

            float value = Controls.CurrMouseState.ScrollWheelValue - Controls.LastMouseState.ScrollWheelValue;
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
            // delta time from the last frame
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            
            // implements the algorithm in this answer: https://gamedev.stackexchange.com/questions/7812/mouse-aim-in-an-fps
            // (reset position "shim")            
            float midWidth  = Game.GraphicsDevice.Viewport.Width / 2;
            float midHeight = Game.GraphicsDevice.Viewport.Height / 2;

            // processing the mouse movements
            // the mouse delta is calculated with the middle of the screen
            // because we will snap the mouse to it
            ProcessMouseMovement(Controls.CurrMouseState.Position.X - midWidth,
                                 Controls.CurrMouseState.Position.Y - midHeight);
            ProcessMouseScroll();            

            // update the camera position, based on the updated vectors
            if (Controls.IsKeyDown(Controls.Forward))
            {
                Velocity += Front * AccelerationValue;

            }
            else if (Controls.IsKeyDown(Controls.Backward))
            {
                Velocity -= Front * AccelerationValue;
            }

            if (Controls.IsKeyDown(Controls.StrafeLeft))
            {
                Velocity -= Right * AccelerationValue;

            }
            else if (Controls.IsKeyDown(Controls.StrafeRight))
            {
                Velocity += Right * AccelerationValue;
            }

            // cap the velocity so we don't move faster diagonally
            if(Velocity.Length() > MaxVelocity)
            {
                Velocity.Normalize();
                Velocity *= MaxVelocity;
            }

            // apply the velocity to the position, based on the delta time between frames
            Position += Velocity * (dt * 0.01f);

            // add some sexy drag
            Velocity *= Drag;
               
            // finally, update the view matrix
            ViewTransform = GetViewMatrix();

            // because we change the zoom, we need to refresh teh perspective
            // the calculation of the ration must be done with the float cast
            // otherwise we lose precision and the result gets weird
            ProjectionTransform = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(Zoom), (float)Game.GraphicsDevice.Viewport.Width / (float)Game.GraphicsDevice.Viewport.Height, 0.1f, 1000f);

            base.Update(gameTime);
        }

        public override string About()
        {
            return "Use WASD to move around.\nLook around with the mouse.\nScroll zooms in and out.\nYou are free in the world, enjoy it.";
        }

    }

}
