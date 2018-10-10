using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ip3d_tp
{
    public static class Controls
    {
        public static Keys Forward     = Keys.W;
        public static Keys Backward    = Keys.S;
        public static Keys StrafeLeft  = Keys.A;
        public static Keys StrafeRight = Keys.D;

        public static Keys CameraRotateYCW  = Keys.Right;
        public static Keys CameraRotateYCCW = Keys.Left;
        public static Keys CameraRotateXCW  = Keys.Up;
        public static Keys CameraRotateXCCW = Keys.Down;

    }
}
