using Microsoft.Xna.Framework;
using System;

namespace ip3d_tp
{
    class ThirdPersonCamera : Camera
    {

        Tank TankToFollow;

        Vector3 Offset;

        enum CameraType { HardFollow, MouseOrbit };

        private CameraType Type = CameraType.MouseOrbit;

        public float MouseSensitivity = 0.1f;

        public float OffsetFromFloor = 1.76f;

        // yaw and pitch angles
        float Yaw;

        float Pitch = 35;  // default;
        
        float OffsetDistance;

        float LastPitch;

        float LastYaw;

        public ThirdPersonCamera(Game game, Tank tankToFollow, Vector3 offset, float fieldOfView = 45f) : base(game, fieldOfView)
        {

            TankToFollow = tankToFollow;

            Offset = offset;

            LastPitch = Pitch;
            LastYaw = Yaw;

            OffsetDistance = offset.Length();
            
            AxisSystem = new Axis3D(Game, Position, 50f);
            game.Components.Add(AxisSystem);

        }

        public void Update(GameTime gameTime, Plane surface)
        {
            
            if(Type == CameraType.HardFollow)
            {

                Position = Vector3.Transform(Offset * (1f + TankToFollow.Velocity.Length()), TankToFollow.WorldTransform);

                ViewTransform = Matrix.CreateLookAt(Position, TankToFollow.Position, Vector3.Up);

            } else if(Type == CameraType.MouseOrbit)
            {

                // implements the algorithm in this answer: https://gamedev.stackexchange.com/questions/7812/mouse-aim-in-an-fps
                // (reset position "shim")            
                float midWidth = Game.GraphicsDevice.Viewport.Width / 2;
                float midHeight = Game.GraphicsDevice.Viewport.Height / 2;

                // processing the mouse movements
                // the mouse delta is calculated with the middle of the screen
                // because we will snap the mouse to it                
                ProcessMouseMovement(Controls.CurrMouseState.Position.X - midWidth, Controls.CurrMouseState.Position.Y - midHeight);

                // calculate coordinates
                float x = (float)Math.Sin(MathHelper.ToRadians(Yaw)) * OffsetDistance;
                float z = (float)Math.Cos(MathHelper.ToRadians(Yaw)) * OffsetDistance;
                float y = (float)Math.Sin(MathHelper.ToRadians(Pitch)) * OffsetDistance;

                // transform the coordinates, so they are in tank world space
                //Position = Vector3.Transform(new Vector3(x, y, z) / TankToFollow.Scale /* * (1f + TankToFollow.Velocity.Length())*/, TankToFollow.WorldTransform);

                Position = new Vector3(x, y, z) + TankToFollow.Position;
                ConstrainToPlane(surface);

                // check if new height is under desired values
                // camera and ground colision
                float height = surface.GetHeightFromSurface(Position) + OffsetFromFloor;  // this is the minimum possible height, at this point
                if (Position.Y < height)
                {

                    Pitch = LastPitch;

                    Position.Y = height;

                }
                
                ViewTransform = Matrix.CreateLookAt(Position, TankToFollow.Position + new Vector3(0, 1.76f, 0), Vector3.Up);

            }



        }

        public void ConstrainToPlane(Plane surface)
        {
            // constrain to bounds
            // inset one subdivision

            float halfSurfaceWidth = surface.Width / 2;
            float halfSurfaceDepth = surface.Depth / 2;

            // because we know that the plane origin is at its center
            // we will have to calculate the bounds with that in mind, and add 
            // te width and depth divided by 2
            if (Position.X < -halfSurfaceWidth + surface.SubWidth)
            {

                Position.X = -halfSurfaceWidth + surface.SubWidth;
                Yaw = LastYaw;

            }
            if (Position.X > halfSurfaceWidth - surface.SubWidth)
            {

                Position.X = halfSurfaceWidth - surface.SubWidth;
                Yaw = LastYaw;

            }
            if (Position.Z < -halfSurfaceDepth + surface.SubHeight)
            {

                Position.Z = -halfSurfaceDepth + surface.SubHeight;
                Yaw = LastYaw;

            }
            if (Position.Z > halfSurfaceDepth - surface.SubHeight)
            {

                Position.Z = halfSurfaceDepth - surface.SubHeight;
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
            Pitch += yoffset;  // here we can invert the Y

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
            return "Follows the tank.";
        }

    }
}
