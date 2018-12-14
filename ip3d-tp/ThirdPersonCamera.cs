using ip3d_tp.Physics3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace ip3d_tp
{
    public class ThirdPersonCamera : Camera
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
        public float ScrollSensitivity = 0.1f;

        // minimum offset from the floor
        public float OffsetFromFloor = 6f;
        public float OffsetToBack = -0.35f;

        // yaw and pitch angles
        public float Yaw = -64;

        public float Pitch = -30;  // default;
        
        // the length of the offset
        float MaxOffsetDistance;
        float MinOffsetDistance;
        float OffsetDistance;
        float TargetOffsetDistance;

        // holds the last value of the pitch
        float LastPitch;

        // last position
        Vector3 LastPosition;

        // holds the last value of the Yaw
        float LastYaw;

        // the distance to the floor
        float DistanceToTerrrain;

        bool IsCollidingBottom = false;

        // constructor
        public ThirdPersonCamera(Game game, Tank tankToFollow, Plane surface, float offset, float fieldOfView = 45f) : base(game, fieldOfView)
        {

            Surface = surface;

            TankToFollow = tankToFollow;

            LastPitch = Pitch;
            LastYaw = Yaw;

            MaxOffsetDistance = 50f;
            MinOffsetDistance = 2f;
            OffsetDistance = offset;
            TargetOffsetDistance = offset;

        }

        public override void Update(GameTime gameTime)
        {

            //Console.WriteLine("---");

            LastPosition = Position;
            //Target = TankToFollow.Body.Position + new Vector3(0, OffsetFromFloor, 0);
            Target = TankToFollow.Body.Position + Vector3.Transform(new Vector3(0, OffsetFromFloor, OffsetToBack), TankToFollow.WorldTransform.Rotation);

            float midWidth = Game.GraphicsDevice.Viewport.Width / 2;
            float midHeight = Game.GraphicsDevice.Viewport.Height / 2;

            // processing the mouse movements
            // the mouse delta is calculated with the middle of the screen
            // because we will snap the mouse to it                
            ProcessMouseMovement(Controls.CurrMouseState.Position.X - midWidth, Controls.CurrMouseState.Position.Y - midHeight);
            ProcessMouseScroll();

            // update proximity with target
            if (OffsetDistance != TargetOffsetDistance)
            {

                OffsetDistance += (TargetOffsetDistance - OffsetDistance) * (float)gameTime.ElapsedGameTime.TotalSeconds;

            }



            float pushBack = (1f + TankToFollow.Body.Velocity.Length() * 0.4f);

            // the new possible position
            Vector3 position = new Vector3(0f, 0f, OffsetDistance * pushBack);
            position = Vector3.Transform(position, Matrix.CreateRotationX(MathHelper.ToRadians(Pitch)));
            position = Vector3.Transform(position, Matrix.CreateRotationY(MathHelper.ToRadians(Yaw)));            

            Position = position + Target;
            ConstrainToPlane();

            //Console.WriteLine("Calcs pos: " + Position);

            // get the minimum height acceptable in the new position
            float terrainHeight = Surface.GetHeightFromSurface(Position);  
            float height = terrainHeight + 0.5f;  // this is the minimum possible height, at this point
            DistanceToTerrrain = Position.Y - terrainHeight;

            //Position.Y = height;

            // calculate the minimum possible pitch angle at this position
            //Vector3 a = Vector3.Normalize(new Vector3(Position.X - Target.X, height - Target.Y, Position.Z - Target.Z));
            //Vector3 b = Vector3.Normalize(new Vector3(-(Position.X - Target.X), 0f, -(Position.Z - Target.Z)));

            Vector3 a = Vector3.Normalize(new Vector3((Position.X - Target.X), height - Target.Y, (Position.Z - Target.Z)));
            Vector3 b = Vector3.Normalize(new Vector3((Position.X - Target.X), 0f, (Position.Z - Target.Z)));
            Vector3 n = Vector3.Cross(a, Vector3.Forward);
            int sign = n.X < 0 ? -1 : 1;   // gets the sign of the angle, by crossing them. the cross direction tells the polarity
            // help from https://www.mathworks.com/matlabcentral/answers/266282-negative-angle-between-vectors-planes

            float angle = MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(a, b))) * sign;  // this is the maximum angle

            Pitch = MathHelper.Clamp(Pitch, float.MinValue, angle);

            position = new Vector3(0f, 0f, OffsetDistance * pushBack);
            position = Vector3.Transform(position, Matrix.CreateRotationX(MathHelper.ToRadians(Pitch)));
            position = Vector3.Transform(position, Matrix.CreateRotationY(MathHelper.ToRadians(Yaw)));

            Position = position + Target;

            //if(Pitch > angle)
            //{
            //    Pitch = angle;

            //    position = new Vector3(0f, 0f, OffsetDistance * pushBack);
            //    position = Vector3.Transform(position, Matrix.CreateRotationX(MathHelper.ToRadians(Pitch)));
            //    position = Vector3.Transform(position, Matrix.CreateRotationY(MathHelper.ToRadians(Yaw)));

            //    Position = position + Target;

            //}


            //Console.WriteLine("phys func deg:" + MathHelper.ToDegrees((float)Physics.VectorAngleBetween(a, b)));
            //Console.WriteLine("dot product: " + Vector3.Dot(a, b));
            //Console.WriteLine("dot sign: " + sign);
            //Console.WriteLine("angle rad: " + (float)Math.Acos(Vector3.Dot(a, b)));
            //Console.WriteLine("angle deg: " + angle);




            //if (Pitch > angle)
            //{
            //    // if the pitch value is minor than the new angle,
            //    // wee need to raise the camera
            //    Pitch = angle;

            //}

            // the new possible position
            //position = new Vector3(0f, 0f, OffsetDistance * pushBack);
            //position = Vector3.Transform(position, Matrix.CreateRotationX(MathHelper.ToRadians(angle)));
            //position = Vector3.Transform(position, Matrix.CreateRotationY(MathHelper.ToRadians(Yaw)));

            //Position = position + Target;

            //Console.WriteLine("result pos: " + Position);

            //Position.Y = MathHelper.Clamp(Position.Y, height, float.MaxValue);

            //if (Position.Y < height)
            //{
            //    IsCollidingBottom = true;

            //    // try to calculate new pitch
            //    //Vector3 a = Vector3.Normalize(new Vector3(position.X, position.Y, position.Z));
            //    //Vector3 b = Vector3.Normalize(new Vector3(position.X, 0f, position.Z));

            //    //float angle = MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(a, b)));

            //    //Console.WriteLine(angle);

            //    //Pitch = angle;
            //    //Position.Y = height;

            //    //Position.Normalize();
            //    //Position *= (OffsetDistance * pushBack);

            //}
            //else
            //{
            //    IsCollidingBottom = false;
            //}



            //Pitch = angle;
            //Position.Y = height;




            // finally, update view transform            
            //ViewTransform = Matrix.CreateLookAt(Position, Target, Vector3.Up);
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
            Pitch -= yoffset;

            if (constrainPitch)
            {
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }

        }

        // used to update the camera zoom based on mouse scroll
        // the code is self explanatory
        private void ProcessMouseScroll()
        {

            float value = Controls.CurrMouseState.ScrollWheelValue - Controls.LastMouseState.ScrollWheelValue;
            value *= ScrollSensitivity;

            //if (TargetOffsetDistance >= MinOffsetDistance && TargetOffsetDistance <= MinOffsetDistance)
            //{
                TargetOffsetDistance -= value;
            //}

            if (TargetOffsetDistance <= MinOffsetDistance) TargetOffsetDistance = MinOffsetDistance;

            if (TargetOffsetDistance >= MaxOffsetDistance) TargetOffsetDistance = MaxOffsetDistance;

        }


        public override string About()
        {
            return $"Follows the tank.\nRotate the mouse to look around.\nToogle 'C' to lock the angle.\nposition: {Position}\npicth: {Pitch}, yaw: {Yaw}\nDistanceToTerrain: {DistanceToTerrrain}, collision: {IsCollidingBottom}";
        }

    }
}
