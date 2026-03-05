// ❗ For switching between lessons, see Program.cs

// Lesson 3
// Teaching the ship to shoot (currently only one bullet at a time).
// Creating a new GameObject class.

using System;
using System.Text;

namespace Lesson_03
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2; 
        bool isGameRunning = true;

        Renderer renderer;

        GameObject bullet; // new variable for the player's bullet (currently only one bullet at a time)

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            renderer.DrawFirstFrame(shipX, shipY);

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                MovePlayerBullet(); // move the bullet upward

                renderer.Render(oldX, shipX, shipY, bullet); // draw the ship now together with the bullet
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight);
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
            else if (info.Key == ConsoleKey.UpArrow && bullet == null) // if the up arrow is pressed
                bullet = new GameObject(shipX, shipY); // create a new bullet at the ship's position if there is no bullet currently
        }

        void MovePlayerBullet()
        {
            if (bullet == null) // if there is no bullet, exit the method
                return;

            bullet.OldY = bullet.Y; // remember the old Y for correct clearing of the symbol

            bullet.Y++; // move the bullet upward. +1 works "up" due to the index calculation in FindIndex

            // check if the bullet reached the ceiling
            if (bullet.Y > screenHeight - 1)
            {
                renderer.ClearBullet(bullet); // erase the bullet so no symbol remains on the screen
                bullet = null; // the bullet no longer exists, a new one can be fired
            }
        }
    }

    class Renderer
    {
        int screenWidth;
        int screenHeight;

        StringBuilder builder;

        const char dotChar = '.';
        const char shipChar = '#';
        const char wallChar = '|';
        const char emptyChar = ' ';
        const char bulletChar = '^'; // bullet symbol

        public Renderer(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
            builder = new StringBuilder();
        }

        public void BuildBoard()
        {
            for (int bY = 0; bY < screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                        builder.Append(dotChar);
                    else if (bX == 0 || bX == screenWidth - 1)
                        builder.Append(wallChar);
                    else
                        builder.Append(emptyChar);
                }
                builder.Append('\n');
            }
        }

        public void DrawFirstFrame(int shipX, int shipY)
        {
            builder[FindIndex(shipX, shipY)] = shipChar;
            Console.WriteLine(builder);
        }

        public void Render(int oldShipX, int newShipX, int shipY, GameObject bullet)
        {
            builder[FindIndex(oldShipX, shipY)] = emptyChar;
            builder[FindIndex(newShipX, shipY)] = shipChar;

            if (bullet != null)
            {
                ClearBullet(bullet); // erase the bullet's old position               
                builder[FindIndex(bullet.X, bullet.Y)] = bulletChar; // draw the bullet at the new position
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        // ❗ Why Replace with index instead of builder[...]:
        // If the bullet was in the same position as the ship, builder[...] = emptyChar
        // it would erase the ship symbol (#). Replace with index changes only the bullet symbol (^)
        public void ClearBullet(GameObject bullet) => builder.Replace(bulletChar, emptyChar, FindIndex(bullet.X, bullet.OldY), 1);

        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY;
            int width = screenWidth + 1;
            return valX + y * width;
        }
    }

    // New class GameObject
    class GameObject
    {
        public int Y { get; set; }   // Y coordinate
        public int X { get; set; }   // X coordinate
        public int OldY { get; set; } // previous Y for clearing the symbol      

        // constructor
        public GameObject(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
