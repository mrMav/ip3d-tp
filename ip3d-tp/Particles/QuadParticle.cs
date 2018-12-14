using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ip3d_tp.Particles
{
    /// <summary>
    /// Quad particle has particle logic,
    /// but renders a quad with a texture.
    /// The quad always faces the camera.
    /// </summary>
    class QuadParticle : Particle
    {
        Effect Shader;
        
        Texture2D Texture;

        public QuadParticle(Game game, string textureKey) : base(game, Color.White, Vector3.Zero, 1f)
        {

            Shader = Game.Content.Load<Effect>("Effects/QuadParticle");

            Texture = Game.Content.Load<Texture2D>(textureKey);

        }



    }
}
