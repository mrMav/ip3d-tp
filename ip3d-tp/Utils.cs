using Microsoft.Xna.Framework;

namespace ip3d_tp
{
    public static class Utils
    {

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

    }
}
