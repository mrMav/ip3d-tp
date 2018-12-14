using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ip3d_tp
{
    public static class Utils
    {

        /// <summary>
        /// Maps a value from a range to another
        /// </summary>
        /// <see>https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/</see>
        /// <param name="value"></param>
        /// <param name="from1"></param>
        /// <param name="to1"></param>
        /// <param name="from2"></param>
        /// <param name="to2"></param>
        /// <returns>The new value mapped</returns>
        public static float Map(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        /// <summary>
        /// Returns a random number between the parameters
        /// </summary>
        /// <param name="rnd"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <see cref="https://stackoverflow.com/a/3365374/5296964"/>
        /// <returns></returns>
        public static double RandomBetween(Random rnd, double min, double max)
        {
            // Perform arithmetic in double type to avoid overflowing
            double range = max - min;
            double sample = rnd.NextDouble();
            double scaled = (sample * range) + min;

            return scaled;
        }

        /// <summary>
        /// Interpolates the height of the given four vertices, and return a new one on the given position.
        /// </summary>
        /// <param name="position">The position inseid the four vertices</param>
        /// <param name="vertex0"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="vertex3"></param>
        /// <returns>The new height</returns>
        public static float HeightBilinearInterpolation(Vector3 position, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
        {

            // interpolate heights
            // based on https://en.wikipedia.org/wiki/Bilinear_interpolation
            // the function of the vertice, is the return of the Y value.
            //
            //  0---1
            //  | / |
            //  2---3
            //

            float x0 = vertex0.X;
            float x1 = vertex3.X;
            float z0 = vertex0.Z;
            float z1 = vertex3.Z;

            // interpolate the x's
            float x0Lerp = (x1 - position.X) / (x1 - x0) * vertex0.Y + (position.X - x0) / (x1 - x0) * vertex1.Y;
            float x1Lerp = (x1 - position.X) / (x1 - x0) * vertex2.Y + (position.X - x0) / (x1 - x0) * vertex3.Y;

            // interpolate in z
            float zLerp = (z1 - position.Z) / (z1 - z0) * x0Lerp + (position.Z - z0) / (z1 - z0) * x1Lerp;

            return zLerp;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal0"></param>
        /// <param name="normal1"></param>
        /// <param name="normal2"></param>
        /// <param name="normal3"></param>
        /// <see cref="https://gamedev.stackexchange.com/questions/18615/how-do-i-linearly-interpolate-between-two-vectors"/>
        /// <returns></returns>
        public static Vector3 NormalBilinearInterpolation(Vector3 normal0, Vector3 normal1, Vector3 normal2, Vector3 normal3, float amountX0, float amountX1, float amountZ)
        {

            // interpolate the x's
            Vector3 x0Lerp = Vector3.Lerp(normal0, normal1, amountX0);
            Vector3 x1Lerp = Vector3.Lerp(normal2, normal3, amountX1);

            // interpolate in z
            return Vector3.Lerp(x0Lerp, x1Lerp, amountZ);

        }
        
    }
}
