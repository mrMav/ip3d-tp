using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ip3d_tp
{
    /// <summary>
    /// Basic controls wrapper for Monogame Input
    /// Self explanatory
    /// </summary>
    public static class Controls
    {

        public static short Tank1ID = 0;
        public static short Tank2ID = 1;

        public static Keys[,] MovementKeys;

        public enum Cursor
        {
            Up,
            Down,
            Left,
            Right
        }

        public static Keys Forward     = Keys.Up;
        public static Keys Backward    = Keys.Down;
        public static Keys StrafeLeft  = Keys.Left;
        public static Keys StrafeRight = Keys.Right;
        
        public static KeyboardState LastKeyboardState;
        public static KeyboardState CurrKeyboardState;

        public static MouseState LastMouseState;
        public static MouseState CurrMouseState;

        public static void Initilalize()
        {

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
