using Microsoft.Xna.Framework;
using System;

namespace ip3d_tp
{
    /*
     * Cameras Base Class
     */
    public class Camera
    {

        // game reference
        protected Game Game;

        // create variables to hold the current camera position and target
        public Vector3 OriginalPosition;
        public Vector3 Position;
        public Vector3 Target;
        public Vector3 ShakeOffset;

        public Vector3 View;
                
        // these are the matrices to be used when this camera is active
        public Matrix ViewTransform;
        public Matrix ProjectionTransform;

        // the camera field of view
        public float FieldOfView;

        public float Pitch;
        public float Yaw;

        // shaking properties
        public float StartedShakeMilliseconds;
        public float ShackingInterval;
        public float ShackingAmmount;
        public float ShackingFrequency;

        public bool Shaking;

        // class constructor
        public Camera(Game game, float fieldOfView = 45f)
        {

            Game = game;

            // basic initializations 
            FieldOfView = fieldOfView;

            Position = Vector3.Zero;
            Target = Vector3.Zero;

            ViewTransform = Matrix.Identity;

            // because we change the zoom, we need to refresh the perspective
            // the calculation of the ration must be done with the float cast
            // otherwise we lose precision and the result gets weird
            ProjectionTransform = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView), (float)Game.GraphicsDevice.Viewport.Width / (float)Game.GraphicsDevice.Viewport.Height, 0.1f, 1000f);

        }

        public virtual void Update(GameTime gameTime)
        {

            View = Target - Position;

            if (Shaking)
            {
                if (gameTime.TotalGameTime.TotalMilliseconds > ShackingInterval + StartedShakeMilliseconds)
                {
                    Shaking = false;
                    ShakeOffset = Vector3.Zero;

                    StartedShakeMilliseconds = 0;
                    ShackingInterval = 0;
                    ShackingAmmount = 0;
                    ShackingFrequency = 0;
                }
                else
                {
                    UpdateShake(gameTime);

                    Position += ShakeOffset;

                }

            }

        }

        public void ActivateShake(GameTime gameTime, float interval = 150f, float ammount = 32f, float frequency = 0.01f)
        {

            this.Shaking = true;
            this.StartedShakeMilliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;
            this.ShackingInterval = interval;
            this.ShackingAmmount = ammount;
            this.ShackingFrequency = frequency;

            this.OriginalPosition = Position;


        }

        public void UpdateShake(GameTime gameTime)
        {
            //https://www.desmos.com/calculator/7sgktk2drh

            Random rnd = new Random();

            float freqy = ShackingFrequency > 0 ? ShackingFrequency : rnd.Next(3, 5) * 0.1f;
            float freqx = ShackingFrequency > 0 ? ShackingFrequency : rnd.Next(3, 5) * 0.1f;
            float freqz = ShackingFrequency > 0 ? ShackingFrequency : rnd.Next(3, 5) * 0.1f;

            float x = (float)gameTime.TotalGameTime.TotalMilliseconds - StartedShakeMilliseconds;
            float a = (float)(Math.Sqrt(ShackingAmmount));  // this is the magnitude
            float v = (float)(Math.Pow((a - x * (a / ShackingInterval)), 2));
            float q1 = (float)(freqy * Math.PI * x);
            float q2 = (float)(freqx * Math.PI * x);
            float q3 = (float)(freqz * Math.PI * x);
            float d1 = (float)Math.Sin(q1);
            float d2 = (float)Math.Cos(q2);
            float d3 = (float)Math.Sin(q2);

            ShakeOffset.X = v * d1;
            ShakeOffset.Y = v * d2;
            ShakeOffset.Z = v * d3;

        }

        public virtual string About()
        {
            return "Camera";
        }
        
    }

}