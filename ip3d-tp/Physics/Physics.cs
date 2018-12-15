using Microsoft.Xna.Framework;
using System;

namespace ip3d_tp.Physics3D
{
    static class Physics
    {
        /// <summary>
        /// gravity constant to perform physics calculations that require it.
        /// this value will be the default value in each body
        /// </summary>
        public static float Gravity = -2.1f;

        /// <summary>
        /// This member will be used to store the MTV for all the SAT intersects
        /// </summary>
        public static Vector3 MinimumTranslationVector = Vector3.Zero;
        
        /// <summary>
        /// intersects two orientaded bouding boxes
        /// </summary>
        /// <see cref="http://www.jkh.me/files/tutorials/Separating%20Axis%20Theorem%20for%20Oriented%20Bounding%20Boxes.pdf"/>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool SATIntersect(Body a, Body b)
        {

            bool result = false;

            // will hold the minimum overlap registered
            float minimumOverlap = float.MaxValue;

            // the minimum overlap axe index
            int minAxeIndex = 0;

            OBB abox = a.CollisionRect;
            OBB bbox = b.CollisionRect;

            // calculate all the possible axes
            Vector3[] axes = new Vector3[15];

            axes[0] = a.Bounds.WorldTransform.Right;  // ax
            axes[1] = a.Bounds.WorldTransform.Up;     // ay
            axes[2] = a.Bounds.WorldTransform.Forward;  // az

            axes[3] = b.Bounds.WorldTransform.Right;  // bx
            axes[4] = b.Bounds.WorldTransform.Up;     // by
            axes[5] = b.Bounds.WorldTransform.Forward;  // bz

            axes[6]  = Vector3.Cross(a.Bounds.WorldTransform.Right, axes[3]);  // ax bx
            axes[7]  = Vector3.Cross(a.Bounds.WorldTransform.Right, axes[4]);  // ax by
            axes[8]  = Vector3.Cross(a.Bounds.WorldTransform.Right, axes[5]);  // ax bz
                     
            axes[9]  = Vector3.Cross(a.Bounds.WorldTransform.Up, axes[3]);  // ay bx
            axes[10] = Vector3.Cross(a.Bounds.WorldTransform.Up, axes[4]);  // ay by
            axes[11] = Vector3.Cross(a.Bounds.WorldTransform.Up, axes[5]);  // ay bz

            axes[12] = Vector3.Cross(a.Bounds.WorldTransform.Forward, axes[3]);  // az bx
            axes[13] = Vector3.Cross(a.Bounds.WorldTransform.Forward, axes[4]);  // az by
            axes[14] = Vector3.Cross(a.Bounds.WorldTransform.Forward, axes[5]);  // az bz

            // test each possible separation axis
            for (int i = 0; i < 15; i++)
            {
                // discard zero length axis
                if(axes[i].Length() <= 0)
                {
                    continue;
                }


                // the projected distance
                float dist = Math.Abs(Vector3.Dot(bbox.Position - abox.Position, axes[i]));

                float boxA = abox.HalfProjection(axes[i]);
                float boxB = bbox.HalfProjection(axes[i]);

                float overlap = boxA + boxB - dist;

                // check or overlap
                // if dist minor, it overlaps
                // if we find only one that doesn't, then we are not overlaping
                // and can early exit
                if (dist > boxA + boxB)
                {
                    // we found a gap, so there is no intersection
                    result = false;

                    break;

                }
                else
                {
                    // we found an overlapping axis
                    // but we need to check all the others
                    result = true;

                    // we will save the intersection MTV
                    // we want only the minimum translations
                    if(minimumOverlap > overlap)
                    {

                        minimumOverlap = overlap;
                        minAxeIndex = i;

                    }
                    
                }

            }

            if(result)
            {

                MinimumTranslationVector = Vector3.Normalize(axes[minAxeIndex]) * minimumOverlap;
                
                // MTV needs to be reversed if the dist offset and 
                // overlap are not facing the same direction
                if(Vector3.Dot(b.Position - a.Position, MinimumTranslationVector) < 0)
                {
                    MinimumTranslationVector = -MinimumTranslationVector;
                }


            }

            return result;

        }

        /// <summary>
        /// Performs an intersection and applys the MTV to the first body
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool SATCollide(Body a, Body b)
        {

            bool intersect = SATIntersect(a, b);

            if(intersect)
            {

                // this works fine for the purpose.
                // just subtract the MTV
                a.Bounds.X -= MinimumTranslationVector.X;
                a.Bounds.Y -= MinimumTranslationVector.Y;
                a.Bounds.Z -= MinimumTranslationVector.Z;

                b.Bounds.X += MinimumTranslationVector.X;
                b.Bounds.Y += MinimumTranslationVector.Y;
                b.Bounds.Z += MinimumTranslationVector.Z;
                
                //a.SetPosition(a.PreviousPosition);
                //a.SetRotation(a.PreviousRotation);

                a.IsColliding = true;
                b.IsColliding = true;

                //a.UpdateCollisionRect();
                //b.UpdateCollisionRect();

                // if both bodys are facing the same direction, cut their speed
                if(Vector3.Dot(a.Bounds.Front, b.Bounds.Front) > 0)
                {
                    a.Speed *= a.Mass / 10f;
                    b.Speed *= b.Mass / 10f;
                }

                //Console.WriteLine(MinimumTranslationVector);

                // we can also just reject the new position
                // and keep the old one, but this has a problem
                // it won't take rotation into account
                //a.Bounds.X = a.PreviousPosition.X;
                //a.Bounds.Y = a.PreviousPosition.Y;
                //a.Bounds.Z = a.PreviousPosition.Z;


                // but I want to try to move the object back
                // by the projection of the MTV on the 
                // delta offset of the moving object
                // note: this works, but it has it's issues.
                // it doesn't account for rotation and doesn't allow sliding

                //Vector3 delta = a.PreviousPosition - a.Position;

                //// calculate the projection of the MTV
                //// into the delta, perpendicular to the MTV
                //float s = (float)Math.Round(Vector3.Dot(MinimumTranslationVector, MinimumTranslationVector) / Vector3.Dot(delta, MinimumTranslationVector), 4);

                //if (!float.IsNaN(s) && !float.IsInfinity(s))
                //{
                //    Vector3 penetrationDelta = delta * s;

                //    Console.WriteLine($"s: {s}, penetrationResolution: {penetrationDelta}, delta {delta}, MTV: {MinimumTranslationVector}");

                //    a.Bounds.X -= penetrationDelta.X;
                //    a.Bounds.Y -= penetrationDelta.Y;
                //    a.Bounds.Z -= penetrationDelta.Z;
                //}

                // also, update the new vectors
                //a.Bounds.UpdateMatrices(Vector3.Up);

            }

            return intersect;

        }


        /// <summary>
        /// calculates the scalar projection of a vector onto an axis
        /// </summary>
        /// <param name="v"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static double VectorScalarProjection(Vector3 v, Vector3 axis)
        {

            float l = v.Length();

            return Vector3.Dot(v, axis) / (l * axis.Length()) * l;

        }

        /// <summary>
        /// returns the angle between two vectors
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double VectorAngleBetween(Vector3 v1, Vector3 v2)
        {

            return Math.Acos(VectorScalarProjection(v1, v2));

        }

    }
}
