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
        public float Mass;
        public float Gravity = Physics.Gravity;

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
            Mass = 7.5f;

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

            IsColliding = false;

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

            //IsColliding = false;

        }

        /// <summary>
        /// Call this method after all the variables are regarding movement
        /// are updated
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateMotion(GameTime gameTime)
        {

            // delta for time based calcs
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // apply speed to velocity
            Velocity += Bounds.Front * Speed * dt;

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
        }

        /// <summary>
        /// Call this method after all the variables are regarding movement
        /// are updated
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateBotMotion(GameTime gameTime, Vector3 steering)
        {

            // delta for time based calcs
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;


            // cap the velocity so we don't move faster than we should
            float steeringforce = 0.4f;
            if (steering.Length() > steeringforce)
            {
                steering.Normalize();
                steering *= steeringforce;
            }

            //Acceleration = steering / Mass;

            // apply speed to velocity
            Velocity += steering * dt;

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
        }


        /// <summary>
        /// Same as UpdateMotion but instead of being based in the front vector,
        /// it uses the acceleration variable to calculate motion.
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateMotionAcceleration(GameTime gameTime)
        {
            // delta for time based calcs
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // calculate acceleraion
            Vector3 accel = Acceleration;
            accel.Y += Gravity;

            // apply speed to velocity
            Velocity += accel * dt;

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
        }

        public void UpdateCollisionRect()
        {
            // update collision shape

            CollisionRect.SetPosition(Position + Offset);
            CollisionRect.SetRotation(Rotation);

            CollisionRect.SetUp(Bounds.Up);
            CollisionRect.UpdateMatrices();  // transform needs to be updated based on the origin vector
            CollisionRect.SetWorldTransform(Matrix.CreateTranslation(Offset) * Bounds.WorldTransform);

        }

        /// <summary>
        /// Updates this body logic.
        /// call after movement is done
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {

            ResetMovingDirections();

            UpdateCollisionRect();

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
            string debug = $"";
            debug += $"Colliding:: {IsColliding}\n";
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
