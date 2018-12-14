using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ip3d_tp.Particles
{
    /// <summary>
    /// Class that manages particles and particles systems
    /// </summary>
    public static class ParticleManager
    {

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

            foreach (Particle p in AllParticles)
            {
                p.Spawner.DrawParticle(gameTime, camera, p);
            }            
        }
    }
}
