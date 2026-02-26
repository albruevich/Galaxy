// ❗ For switching between lessons, see Program.cs

// Lesson 1
// Drawing the bottom row of the game screen and the ship.
// Implementing ship movement to the left and right.
// Creating the basic project structure (starting architecture) for future lessons.

using System;   // include basic .NET classes, including Console, Math, Random, etc.
using System.Text;   // include classes for working with text, e.g., StringBuilder for building the game screen

namespace Lesson_01
{
    // Main class where all game logic is implemented:
    // input handling, player actions, object movement, and screen updates.
    class Game
    {
        const int screenWidth = 21; // width of the game screen
        int shipX; // horizontal position of the ship     
        bool isGameRunning = true; // flag indicating if the game is running

        const char dotChar = '.'; // ceiling and floor symbol
        const char shipChar = '#'; // ship symbol

        StringBuilder builder; // used to quickly change characters on the screen without creating new strings

        public void Run()
        {
            Init();
            BuildBoard();
            DrawFirstFrame();

            // game loop
            while (isGameRunning)
            {
                // remember the old ship position to redraw the previous position
                int oldX = shipX;

                HandleInput();
                Render(oldX);
            }
        }

        void Init()
        {
            // place the ship in the center so the player sees it immediately
            shipX = screenWidth / 2;

            // create the main string for printing; initially it is empty    
            builder = new StringBuilder();
        }

        void BuildBoard()
        {
            // fill the string with dots
            for (int i = 0; i < screenWidth; i++)
                builder.Append(dotChar);
        }

        void DrawFirstFrame()
        {
            // draw the ship at the start           
            builder[shipX] = shipChar;

            // print the very first frame
            Console.WriteLine(builder);
        }

        void HandleInput()
        {
            // wait for a key press and read the key info
            ConsoleKeyInfo info = Console.ReadKey(true);

            // exit the game
            if (info.Key == ConsoleKey.Escape)
            {
                isGameRunning = false;
                return;
            }

            // move left and limit so X does not go below 0
            if (info.Key == ConsoleKey.LeftArrow)
                shipX = Math.Max(0, shipX - 1);

            // move right and limit so X does not exceed the screen width
            else if (info.Key == ConsoleKey.RightArrow)
                shipX = Math.Min(screenWidth - 1, shipX + 1);
        }

        void Render(int oldX)
        {
            builder[oldX] = dotChar;  // erase the ship from the old position
            builder[shipX] = shipChar; // draw the ship in the new position     

            Console.SetCursorPosition(0, 0); // move the cursor to the beginning to redraw the screen            
            Console.WriteLine(builder); // draw the updated screen
        }
    }
}