﻿using Microsoft.Xna.Framework;
using System;

namespace ip3d_tp.Physics3D
{
    /// <summary>
    /// Represents an oriented bounding box.
    /// </summary>
    public class OBB
    {
        protected float _x;
        protected float _y;
        protected float _z;

        protected float _pitch;
        protected float _yaw;
        protected float _roll;

        protected float _width;
        protected float _height;
        protected float _depth;

        protected float _halfwidth;
        protected float _halfheight;
        protected float _halfdepth;

        protected Vector3 _min;
        protected Vector3 _max;

        protected Vector3 _position;
        protected Vector3 _rotation;

        protected Vector3 _up;
        protected Vector3 _right;
        protected Vector3 _front;

        protected Matrix _worldTransform;

        public float X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        public float Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        public float Z
        {
            get
            {
                return _z;
            }
            set
            {
                _z = value;
            }
        }

        public float Pitch
        {
            get
            {
                return _pitch;
            }
            set
            {
                //_pitch = MathHelper.Clamp(value, -89f, 89f);
                _pitch = value;
            }
        }

        public float Yaw
        {
            get
            {
                return _yaw;
            }
            set
            {
                // limit yaw so we don't gimball lock this thing

                _yaw = value;
            }
        }

        public float Roll
        {
            get
            {
                return _roll;
            }
            set
            {
                // limit roll so we don't gimball lock this thing

                _roll = value;
            }
        }

        public float Width
        {
            get
            {
                return _width;
            }
        }

        public float Height
        {
            get
            {
                return _height;
            }
        }

        public float Depth
        {
            get
            {
                return _depth;
            }
        }

        public float HalfWidth
        {
            get
            {
                return _halfwidth;
            }
        }

        public float HalfHeight
        {
            get
            {
                return _halfheight;
            }
        }

        public float HalfDepth
        {
            get
            {
                return _halfdepth;
            }
        }

        public float CenterX
        {
            get
            {
                return _x + _halfwidth;
            }
        }

        public float CenterY
        {
            get
            {
                return _y + _halfheight;
            }
        }

        public float CenterZ
        {
            get
            {
                return _z + _halfdepth;
            }
        }

        public Vector3 Min
        {
            get
            {
                _min.X = _position.X - _halfwidth;
                _min.Y = _position.Y - _halfheight;
                _min.Z = _position.Z - _halfdepth;
                
                _min = Vector3.Transform(_min, _worldTransform);

                return _min;
            }
        }

        public Vector3 Max
        {
            get
            {
                _max.X = _position.X + _halfwidth;
                _max.Y = _position.Y + _halfheight;
                _max.Z = _position.Z + _halfdepth;
                 
                _max = Vector3.Transform(_max, _worldTransform);

                return _max;
            }
        }

        public Vector3 Position
        {
            get
            {
                _position.X = _x;
                _position.Y = _y;
                _position.Z = _z;

                return _position;
            }
        }

        public Vector3 Rotation
        {
            get
            {
                _rotation.X = _pitch;
                _rotation.Y = _yaw;
                _rotation.Z = _roll;

                return _rotation;
            }
        }

        public Vector3 Up
        {
            get
            {
                return _up;
            }
        }

        public Vector3 Right
        {
            get
            {
                return _right;
            }
        }

        public Vector3 Front
        {
            get
            {
                return _front;
            }
        }

        public Matrix WorldTransform
        {
            get
            {
                return _worldTransform;
            }
            set
            {
                _worldTransform = value;
            }
        }

        public OBB(float x, float y, float z, float width, float height, float depth)
        {
            _x = x;
            _y = y;
            _z = z;

            _width = width;
            _height = height;
            _depth = depth;

            _halfwidth = width / 2;
            _halfheight = height / 2;
            _halfdepth = depth / 2;

            // default
            _up = Vector3.Up;
            UpdateMatrices();

        }

        // TODO: don't forget that this value, will have 
        // to be passed by when calling this update method on the tanks
        public void UpdateMatrices()
        {

            // create the rotation matrix:
            Matrix rotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, _roll);
            
            // Up vector must be already updated
            _up    = rotation.Up;
            _right = rotation.Right;
            _front = rotation.Forward;
            //_up = Vector3.Normalize(Vector3.Cross(_right, _front));

            // creates the world matrix
            _worldTransform = Matrix.CreateWorld(Position, rotation.Forward, rotation.Up);

        }

        // TODO: don't forget that this value, will have 
        // to be passed by when calling this update method on the tanks
        public void UpdateMatrices(Vector3 up)
        {

            // create the rotation matrix:
            Matrix rotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, _roll);
            
            // Up vector must be already updated
            _up = up;
            _right = Vector3.Normalize(Vector3.Cross(_up, Vector3.Transform(Vector3.Right, rotation)));
            _front = Vector3.Normalize(Vector3.Cross(_up, Vector3.Transform(Vector3.Forward, rotation)));

            // creates the world matrix
            _worldTransform = Matrix.CreateWorld(Position, _front, _up);

        }

        // TODO: don't forget that this value, will have 
        // to be passed by when calling this update method on the tanks
        public void UpdateMatrices(Vector3 up, Vector3 velocity)
        {

            // create the rotation matrix:
            //Matrix rotation = Matrix.CreateFromYawPitchRoll(_yaw, _pitch, _roll);

            // Up vector must be already updated
            _up = up;
            _front = Vector3.Normalize(velocity);

            _right = Vector3.Normalize(Vector3.Cross(_up, _front));
            _front = Vector3.Normalize(Vector3.Cross(_up, _right));

            // creates the world matrix
            _worldTransform = Matrix.CreateWorld(Position, _front, _up);

        }

        public void Resize(float width, float height, float depth)
        {
            _width = width;
            _height = height;
            _depth = depth;

            _halfwidth = width / 2;
            _halfheight = height / 2;
            _halfdepth = depth / 2;
        }

        /// <summary>
        /// returns the projection value of half a box
        /// onto the given axis
        /// </summary>
        /// <see cref="http://www.jkh.me/files/tutorials/Separating%20Axis%20Theorem%20for%20Oriented%20Bounding%20Boxes.pdf"/>
        /// <param name="axis"></param>
        /// <returns></returns>
        public float HalfProjection(Vector3 axis)
        {

            float projWidth = Math.Abs(Vector3.Dot(_halfwidth * _worldTransform.Right, axis));
            float projHeight = Math.Abs(Vector3.Dot(_halfheight * _worldTransform.Up, axis));
            float projDepth = Math.Abs(Vector3.Dot(_halfdepth * _worldTransform.Forward, axis));

            return projWidth + projHeight + projDepth;

        }

        public void SetWorldTransform(Matrix world)
        {
            _worldTransform = world;
            SetPosition(world.Translation);
        }

        public void SetPosition(Vector3 pos)
        {
            _x = pos.X;
            _y = pos.Y;
            _z = pos.Z;
        }

        public void SetRotation(Vector3 rot)
        {
            _pitch = rot.X;
            _yaw   = rot.Y;
            _roll  = rot.Z;
        }

        public void SetFront(Vector3 front)
        {
            _front = front;
        }

        public void SetRight(Vector3 right)
        {
            _right = right;
        }

        public void SetUp(Vector3 up)
        {
            _up = up;
        }

    }
}
