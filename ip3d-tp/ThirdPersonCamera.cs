using Microsoft.Xna.Framework;
using System;

namespace ip3d_tp
{
    class ThirdPersonCamera : Camera
    {

        Tank TankToFollow;

        Vector3 Offset;

        public ThirdPersonCamera(Game game, Tank tankToFollow, Vector3 offset, float fieldOfView = 45f) : base(game, fieldOfView)
        {

            TankToFollow = tankToFollow;

            Offset = offset;
            
            AxisSystem = new Axis3D(Game, Position, 50f);
            game.Components.Add(AxisSystem);

        }

        public override void Update(GameTime gameTime)
        {

            Position = Vector3.Transform(Offset * (1f + TankToFollow.Velocity.Length()), TankToFollow.WorldTransform);

            ViewTransform = Matrix.CreateLookAt(Position, TankToFollow.Position, Vector3.Up);


        }

        public override string About()
        {
            return "Follows the tank.";
        }

    }
}
