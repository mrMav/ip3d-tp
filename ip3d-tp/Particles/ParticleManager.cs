using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ip3d_tp.Particles
{
    /// <summary>
    /// Class that manages particles and particles systems
    /// </summary>
    /// <remarks>
    /// This whole particle systems and managers are not very performative.
    /// Due to the lack of time for optimization, Instancing rendering was
    /// not studied or applied.
    /// </remarks>
    public static class ParticleManager
    {

        public static Game Game;

        public static List<ParticleEmitter> ParticleEmitters = new List<ParticleEmitter>();

        public static Particle[] AllParticles = new Particle[0];
        
        public static void AddParticleEmitter(ParticleEmitter p)
        {

            ParticleEmitters.Add(p);

            // add the particles managed by this emitter
            // to the global particle array

            int lastIndex = AllParticles.Length;

            Array.Resize(ref AllParticles, AllParticles.Length + p.MaxParticles);

            // copy all the elements

            for(int i = lastIndex; i < lastIndex + p.MaxParticles; i++)
            {

                AllParticles[i] = p.Particles[i - lastIndex];

            }

        }

        public static void DrawEmitters(GameTime gameTime, Camera camera)
        {
            // set a quad ready in the buffer
            Game.GraphicsDevice.SetVertexBuffer(((QuadParticleEmitter)ParticleEmitters[0]).VertexBuffer);
            Game.GraphicsDevice.Indices = ((QuadParticleEmitter)ParticleEmitters[0]).IndexBuffer;
            Game.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            // reorder particles so we can draw transparency
            // wee need to draw the bottom most first
            Array.Sort(AllParticles, delegate (Particle p1, Particle p2)
            {

                // compare distance to camera
                float p1dist = (p1.Position - camera.Position).Length();
                float p2dist = (p2.Position - camera.Position).Length();

                if (p1dist < p2dist)
                {
                    return 1;
                }
                else if (p1dist > p2dist)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            });

            // for performance, we should already have a quad in the buffer
            foreach (Particle p in AllParticles)
            {
                if(p.Alive)
                    p.Spawner.DrawParticle(gameTime, camera, p);
            }            
        }
    }
}
