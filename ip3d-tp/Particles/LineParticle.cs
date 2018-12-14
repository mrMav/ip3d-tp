using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ip3d_tp.Particles
{
    /// <summary>
    /// Represents a single Particle. This particle will render a line on the screen.
    /// </summary>
    class LineParticle
    {
        // reference to the parent spawner
        public LineParticleEmitter Spawner;

        /*
         * Bellow properties are self explanatory
         */ 

        public double SpawnedAtMilliseconds = 0f;
        public double MillisecondsAfterSpawn = 0f;
        public double LifespanMilliseconds = 0f;
        
        public Vector3 Position;
        public Vector3 InitialPosition;
        
        public Vector3 Acceleration;
        public Vector3 Velocity;
        public Vector3 Drag;

        public float Size;
        
        // boolean to specify if this particle is enabled or not
        public bool Alive;
        
        /*
         * Constructor
         */
        public LineParticle(Game game, Color color, Vector3 position, float size)
        {
            
            Acceleration = Vector3.Zero;
            Velocity = Vector3.Zero;

            // set the drag to not affect the velocity
            Drag = new Vector3(1f);

            Position = position;
            InitialPosition = position;

            Size = size;

            // make sure it is dead by default
            Kill();

        }

        public void Update(GameTime gameTime)
        {

            if (Alive)
            {
                // updates timer
                MillisecondsAfterSpawn += gameTime.ElapsedGameTime.TotalMilliseconds;

                // we won't be implementing acceleration yet
                //Velocity += Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

                // apply velocity to the position
                Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                // damp velocity
                Velocity *= Drag;

            }

            // check if particle as come to old age
            if (MillisecondsAfterSpawn >= LifespanMilliseconds)
            {
                Kill();
            }

        }

        /// <summary>
        /// Kills the particle.
        /// </summary>
        public void Kill()
        {

            if(Alive)
                Global.AliveParticles--;

            Alive = false;



        }

        /// <summary>
        /// Revives this particle.
        /// </summary>
        public void Revive()
        {
            if(!Alive)
                Global.AliveParticles++;

            Alive = true;


        }

        /// <summary>
        /// Resets this particle properties.
        /// </summary>
        public void Reset()
        {
            Alive = false;

            SpawnedAtMilliseconds = 0f;
            MillisecondsAfterSpawn = 0f;

            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
            Acceleration = Vector3.Zero;

        }

    }

}