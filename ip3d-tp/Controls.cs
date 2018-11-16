using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ip3d_tp
{
    /// <summary>
    /// Basic controls wrapper for Monogame Input
    /// </summary>
    public static class Controls
    {

        /*
         * Holds the ids for different tanks
         */ 
        public static short Tank1ID = 0;
        public static short Tank2ID = 1;

        /*
         * this multidimensional array will contain the keys for both tanks
         */ 
        public static Keys[,] MovementKeys;

        // enumerator to simplify basic movement access
        public enum Cursor
        {
            Up,
            Down,
            Left,
            Right
        }

        /*
         * Camera movement keys
         */ 
        public static Keys CameraForward     = Keys.NumPad8;
        public static Keys CameraBackward    = Keys.NumPad5;
        public static Keys CameraStrafeLeft  = Keys.NumPad4;
        public static Keys CameraStrafeRight = Keys.NumPad6;
        public static Keys CameraMoveUp      = Keys.NumPad7;
        public static Keys CameraMoveDown    = Keys.NumPad1;

        public static KeyboardState LastKeyboardState;
        public static KeyboardState CurrKeyboardState;

        public static MouseState LastMouseState;
        public static MouseState CurrMouseState;

        public static void Initilalize()
        {
            // init tanks movement keys array
            MovementKeys = new Keys[2,4];

            MovementKeys[Tank1ID, (int)Cursor.Up]    = Keys.W;
            MovementKeys[Tank1ID, (int)Cursor.Down]  = Keys.S;
            MovementKeys[Tank1ID, (int)Cursor.Left]  = Keys.A;
            MovementKeys[Tank1ID, (int)Cursor.Right] = Keys.D;

            MovementKeys[Tank2ID, (int)Cursor.Up]    = Keys.I;
            MovementKeys[Tank2ID, (int)Cursor.Down]  = Keys.K;
            MovementKeys[Tank2ID, (int)Cursor.Left]  = Keys.J;
            MovementKeys[Tank2ID, (int)Cursor.Right] = Keys.L;

            LastKeyboardState = Keyboard.GetState();
            CurrKeyboardState = Keyboard.GetState();

            LastMouseState = Mouse.GetState();
            CurrMouseState = Mouse.GetState();

        }

        public static void UpdateCurrentStates()
        {
            CurrKeyboardState = Keyboard.GetState();
            CurrMouseState = Mouse.GetState();
        }

        public static void UpdateLastStates()
        {
            LastKeyboardState = CurrKeyboardState;
            LastMouseState = CurrMouseState;
        }
        
        public static bool IsKeyDown(Keys key)
        {

            return CurrKeyboardState.IsKeyDown(key);

        }

        public static bool IsKeyPressed(Keys key)
        {

            return LastKeyboardState.IsKeyUp(key) && CurrKeyboardState.IsKeyDown(key);

        }

    }
}
