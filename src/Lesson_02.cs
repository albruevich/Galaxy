// ❗ For switching between lessons, see Program.cs

// Lesson 2
// Drawing a square game field.
// Adding a new responsibility for rendering - the Renderer class.

using System;
using System.Text;

namespace Lesson_02
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12; // height of the game screen
        int shipX;
        const int shipY = 2; // fixed height of the ship
        bool isGameRunning = true;

        // Renderer - an object responsible only for rendering.
        // Game now holds only logic (ship position, input, loop), and Renderer draws the field and the ship
        // this is a separation of concerns
        Renderer renderer;

        public void Run()
        {
            Init();
            renderer.BuildBoard(); // now Renderer draws instead of Game
            renderer.DrawFirstFrame(shipX, shipY); // now Renderer draws instead of Game

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                renderer.Render(oldX, shipX, shipY); // now Renderer draws instead of Game
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight); // create Renderer (calls the constructor of Renderer class)
        }

        void HandleInput()
        {
            ConsoleKeyInfo info = Console.ReadKey(true);

            if (info.Key == ConsoleKey.Escape)
            {
                isGameRunning = false;
                return;
            }

            if (info.Key == ConsoleKey.LeftArrow)
                shipX = Math.Max(1, shipX - 1);
            else if (info.Key == ConsoleKey.RightArrow)
                shipX = Math.Min(screenWidth - 2, shipX + 1);
        }
    }

    // Renderer class, responsible for everything related to rendering
    class Renderer
    {
        int screenWidth; // copied from Game so Renderer is independent and can build the field and calculate indexes
        int screenHeight; // copied from Game so Renderer is independent and can build the field and calculate indexes

        StringBuilder builder; // now builder belongs to Renderer

        const char dotChar = '.';
        const char shipChar = '#';
        const char wallChar = '|'; // wall symbol
        const char emptyChar = ' '; // empty symbol

        // Renderer class constructor, called when creating an object via new
        public Renderer(int width, int height)
        {
            // assign values passed from Game
            screenWidth = width;
            screenHeight = height;

            // builder is now created here
            builder = new StringBuilder();
        }

        // Create the game field with ceiling, floor, and walls
        public void BuildBoard()
        {
            for (int bY = 0; bY < screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                        builder.Append(dotChar); // ceiling and floor
                    else if (bX == 0 || bX == screenWidth - 1)
                        builder.Append(wallChar); // walls
                    else
                        builder.Append(emptyChar); // empty space
                }
                builder.Append('\n'); // move to a new line
            }
        }

        public void DrawFirstFrame(int shipX, int shipY)
        {
            builder[FindIndex(shipX, shipY)] = shipChar;
            Console.WriteLine(builder);
        }

        public void Render(int oldX, int newX, int y)
        {
            builder[FindIndex(oldX, y)] = emptyChar; // erase old position based on calculated index
            builder[FindIndex(newX, y)] = shipChar;  // draw new position based on calculated index

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        // calculate the index of the element in the builder
        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY; // offset from the top to the game object
            int width = screenWidth + 1; // screen width + 1 (add 1 because each row has an invisible '\n')
            return valX + y * width; // find the index of the ship
        }
    }
}