using Microsoft.Xna.Framework;
using System;

namespace ip3d_tp.Physics3D
{
    /// <summary>
    /// Represents a Physics body
    /// </summary>
    public class Body
    {
        public string Tag;

        public Vector3 Acceleration;
        public Vector3 Velocity;
        public Vector3 Drag;
        public Vector3 Origin;
        public float MaxVelocity;
        public float Speed;

        public Vector3 PreviousPosition;
        public Vector3 PreviousRotation;

        public Vector3 Delta;

        public Vector3 Intersection;

        public OBB Bounds;
        
        // the collision rect shape to be used on collisions
        public OBB CollisionRect;
        public Vector3 Offset;

        public float X
        {
            get
            {
                return Bounds.X;
            }
            set
            {
                Bounds.X = value;
            }
        }

        public float Y
        {
            get
            {
                return Bounds.Y;
            }
            set
            {
                Bounds.Y = value;
            }
        }

        public float Z
        {
            get
            {
                return Bounds.Z;
            }
            set
            {
                Bounds.Z = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return Bounds.Position;
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return Bounds.Rotation;
            }
        }

        public bool Enabled { get; set; }

        public bool IsColliding { get; set; }

        public bool MovingUp { get; set; }
        public bool MovingRight { get; set; }
        public bool MovingDown { get; set; }
        public bool MovingLeft { get; set; }
        public bool MovingForward { get; set; }
        public bool MovingBackward { get; set; }

        public bool IsOnFloor { get; set; }

        /// <summary>
        /// Creates a body at the position
        /// </summary>
        /// <param name="position">The body position</param>
        public Body(float x, float y, float z, float width, float height, float depth)
        {
            Acceleration = Vector3.Zero;
            Velocity = Vector3.Zero;
            Origin = new Vector3(0.5f);

            Bounds = new OBB(x, y, z, width, height, depth);
            CollisionRect = new OBB(x, y, z, width, height, depth);
            SetSize(width, height, depth);
            Offset = Vector3.Zero;

            Drag = new Vector3(1f);
            MaxVelocity = 10f;

            Enabled = false;
        }

        /// <summary>
        /// Call before moving the body.
        /// </summary>
        /// <param name="gameTime"></param>
        public void PreMovementUpdate(GameTime gameTime)
        {
            PreviousPosition = Position;
            PreviousRotation = Rotation;
        }

        /// <summary>
        /// Prepares this body to be tested for collisions.
        /// Call this after moving the body.
        /// </summary>
        /// <param name="gameTime"></param>
        public void PreCollisionUpdate(GameTime gameTime)
        {

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // update collision shape
            //UpdateCollisionRect();

            IsColliding = false;

            Velocity += Acceleration;

            if(Velocity.Length() > MaxVelocity)
            {
                Velocity.Normalize();
                Velocity *= MaxVelocity;
            }

            Bounds.X += Velocity.X * dt;
            Bounds.Y += Velocity.Y * dt;
            Bounds.Z += Velocity.Z * dt;

            Velocity *= Drag;
            Acceleration = Vector3.Zero;  // reset accelereration

        }

        /// <summary>
        /// Call this method after all the variables are regarding movement
        /// are updated
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateMotion(GameTime gameTime)
        {
            // apply speed to velocity
            Velocity += Bounds.Front * Speed;

            // cap the velocity so we don't move faster than we should
            if (Velocity.Length() > MaxVelocity)
            {
                Velocity.Normalize();
                Velocity *= MaxVelocity;
            }

            // apply the velocity to the position
            SetPosition(Position + Velocity);

            // add some sexy drag
            Velocity *= Drag;

            Update(gameTime);
            UpdateCollisionRect();
        }

        public void UpdateCollisionRect()
        {
            // update collision shape

            CollisionRect.SetPosition(Position + Offset);
            CollisionRect.SetRotation(Rotation);

            CollisionRect.SetUp(Bounds.Up);
            CollisionRect.UpdateMatrices();

        }

        /// <summary>
        /// Updates this body logic.
        /// call after movement is done
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {

            ResetMovingDirections();

            // determing which side the body is moving
            if (DeltaY() < 0)
            {
                MovingDown = true;
                MovingUp = false;
            }
            else if (DeltaY() > 0)
            {
                MovingUp = true;
                MovingDown = false;
            }
            else
            {
                MovingUp = false;
                MovingDown = false;
            }

            if (DeltaX() > 0)
            {
                MovingRight = true;
                MovingLeft = false;
            }
            else if (DeltaX() < 0)
            {
                MovingLeft = true;
                MovingRight = false;
            }
            else
            {
                MovingRight = false;
                MovingLeft = false;
            }

            if (DeltaZ() > 0)
            {
                MovingForward = true;
                MovingBackward = false;
            }
            else if (DeltaZ() < 0)
            {
                MovingBackward = true;
                MovingForward = false;
            }
            else
            {
                MovingForward = false;
                MovingBackward = false;
            }


        }

        public void ResetMovingDirections()
        {
            MovingUp = false;
            MovingRight = false;
            MovingDown = false;
            MovingLeft = false;
            MovingForward = false;
            MovingBackward = false;
        }

        public float DeltaX()
        {
            return PreviousPosition.X - Position.X;
        }

        public float DeltaY()
        {
            return PreviousPosition.Y - Position.Y;
        }

        public float DeltaZ()
        {
            return PreviousPosition.Z - Position.Z;
        }

        public void SetSize(float width, float height, float depth)
        {

            UpdateCollisionRect();

            CollisionRect.Resize(width, height, depth);
            
        }

        public void SetPosition(Vector3 pos)
        {

            Bounds.SetPosition(pos);
        }

        public void SetRotation(Vector3 rot)
        {
            Bounds.SetRotation(rot);
        }

        public string GetDebugString()
        {
            string debug = $"Moving:\n Up: {MovingUp}, Down: {MovingDown}, Right: {MovingRight}, Left: {MovingLeft}, Forward: {MovingForward}, Backward: {MovingBackward}\n";
            debug += $"Colliding:\n: {IsColliding}";
            debug += $"Position: {Position}\n";
            debug += $"Prev Pos: {PreviousPosition}\n";
            debug += $"DeltaX: {DeltaX()}\n";
            debug += $"DeltaY: {DeltaY()}\n";
            debug += $"DeltaZ: {DeltaY()}\n";
            debug += $"Velocity: {Velocity}\n";

            return debug;

        }

    }
}
