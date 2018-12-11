using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ip3d_tp
{
    class ThirdPersonCamera : Camera
    {
        // the terrain surface for collisions
        Plane Surface;

        // the tank to follow
        Tank TankToFollow;

        // the offset to keep the camera away from teh tank
        Vector3 Offset;

        // an enumeration to define
        // the type of camera to use
        enum CameraType {
            HardFollow,
            MouseOrbit
        };

        // the type
        private CameraType Type = CameraType.MouseOrbit;

        // sensitivity
        public float MouseSensitivity = 0.1f;

        // minimum offset from the floor
        public float OffsetFromFloor = 5f;

        // yaw and pitch angles
        public float Yaw = -64;

        public float Pitch = -30;  // default;
        
        // the length of the offset
        float OffsetDistance;

        // holds the last value of the pitch
        float LastPitch;

        // holds the last value of the Yaw
        float LastYaw;

        // constructor
        public ThirdPersonCamera(Game game, Tank tankToFollow, Plane surface, Vector3 offset, float fieldOfView = 45f) : base(game, fieldOfView)
        {

            Surface = surface;

            TankToFollow = tankToFollow;

            Offset = offset;

            LastPitch = Pitch;
            LastYaw = Yaw;

            OffsetDistance = offset.Length();
            
        }

        public override void Update(GameTime gameTime)
        {

            Target = TankToFollow.Body.Position + new Vector3(0, OffsetFromFloor, 0);

            float midWidth = Game.GraphicsDevice.Viewport.Width / 2;
            float midHeight = Game.GraphicsDevice.Viewport.Height / 2;

            // processing the mouse movements
            // the mouse delta is calculated with the middle of the screen
            // because we will snap the mouse to it                
            ProcessMouseMovement(Controls.CurrMouseState.Position.X - midWidth, Controls.CurrMouseState.Position.Y - midHeight);

            // get user input to listen for type change
            if (Controls.IsKeyPressed(Keys.C))
            {
                if (Type == CameraType.HardFollow) Type = CameraType.MouseOrbit;
                else Type = CameraType.HardFollow;
            }

            // update the camera logic based on type
            if(Type == CameraType.HardFollow)
            {

                // calculate a new offset based on original offset and current pitch value
                Vector3 offset = new Vector3(Offset.X, MathHelper.ToRadians(Pitch) * OffsetDistance, Offset.Z);

                // the result will be an offset in the tank world space
                // the offset will also push back when accelerating
                Position = Vector3.Transform(offset * (1f + TankToFollow.Body.Velocity.Length() * 0.4f), TankToFollow.WorldTransform);

            } else if(Type == CameraType.MouseOrbit)
            {
                               
                float pushBack = (1f + TankToFollow.Body.Velocity.Length() * 0.4f);
                Vector3 position = new Vector3(0f, 0f, OffsetDistance * pushBack);

                position = Vector3.Transform(position, Matrix.CreateRotationX(MathHelper.ToRadians(Pitch)));
                position = Vector3.Transform(position, Matrix.CreateRotationY(MathHelper.ToRadians(Yaw)));

                Position = position + Target;

            }

            ConstrainToPlane();

            // check if new height is under desired values
            // camera and ground colision
            float height = Surface.GetHeightFromSurface(Position) + OffsetFromFloor;  // this is the minimum possible height, at this point
            if (Position.Y < height)
            {

                Pitch = LastPitch;

                Position.Y = height;

            } else
            {
                //LastPitch = Pitch;                
            }

            // finally, update view transform            
            ViewTransform = Matrix.CreateLookAt(Position, Target, Vector3.Up);

            base.Update(gameTime);

        }

        public void ConstrainToPlane()
        {
            // constrain to bounds
            // inset one subdivision

            float halfSurfaceWidth = Surface.Width / 2;
            float halfSurfaceDepth = Surface.Depth / 2;

            // because we know that the plane origin is at its center
            // we will have to calculate the bounds with that in mind, and add 
            // te width and depth divided by 2
            if (Position.X < -halfSurfaceWidth + Surface.SubWidth)
            {

                Position.X = -halfSurfaceWidth + Surface.SubWidth;
                Yaw = LastYaw;

            }
            if (Position.X > halfSurfaceWidth - Surface.SubWidth)
            {

                Position.X = halfSurfaceWidth - Surface.SubWidth;
                Yaw = LastYaw;

            }
            if (Position.Z < -halfSurfaceDepth + Surface.SubHeight)
            {

                Position.Z = -halfSurfaceDepth + Surface.SubHeight;
                Yaw = LastYaw;

            }
            if (Position.Z > halfSurfaceDepth - Surface.SubHeight)
            {

                Position.Z = halfSurfaceDepth - Surface.SubHeight;
                Yaw = LastYaw;

            }
        }

        // handles the mouse movement, updating the yaw, pitch and vectors
        // constrain the picth to avoid angles lock
        private void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
        {
            // the given offset, is the diference from the previous mouse position and the current one
            xoffset *= MouseSensitivity;
            yoffset *= MouseSensitivity;

            LastPitch = Pitch;
            LastYaw = Yaw;

            Yaw -= xoffset;
            Pitch -= yoffset;  // here we can invert the Y

            if (constrainPitch)
            {
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }

        }


        public override string About()
        {
            return $"Follows the tank.\nRotate the mouse to look around.\nToogle 'C' to lock the angle.\npicth: {Pitch}, yaw: {Yaw}";
        }

    }
}
