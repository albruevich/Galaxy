// ❗ For switching between lessons, see Program.cs

// Lesson 4
// Teaching the ship to shoot multiple bullets, creating an array of bullets.
// Moving from handling a single object (bullet) to handling a collection of objects.

using System;
using System.Text;

namespace Lesson_04
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2;
        bool isGameRunning = true;

        Renderer renderer;

        // Now, instead of one bullet, we store an array of bullets. The array size limits the maximum number of bullets at the same time.
        // There will be no more bullets than screen height minus floor and ceiling
        GameObject[] bullets = new GameObject[screenHeight - 2];

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            renderer.DrawFirstFrame(shipX, shipY);

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                MovePlayerBullets(); // The method now moves ALL bullets, not just one.               

                renderer.Render(oldX, shipX, shipY, bullets); // Pass the bullets array to Renderer instead of a single bullet.
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
            else if (info.Key == ConsoleKey.UpArrow)
            {
                // Previously we just checked bullet == null.
                // Now we need to find a free slot in the array.
                int emptyIndex = -1;

                // Loop through the array and find null — this is a free slot for a new bullet.
                for (int i = 0; i < bullets.Length; i++)
                    if (bullets[i] == null) // if there is no bullet at current index i...
                    {
                        emptyIndex = i; // ... empty index found
                        break; // exit the loop since we found a free slot
                    }

                if (emptyIndex != -1) // if a free index is found, create a new bullet
                {
                    GameObject bullet = new GameObject(shipX, shipY); // create a bullet object
                    bullets[emptyIndex] = bullet; // place it in the array; now bullets can exist simultaneously.                    
                }
            }
        }

        void MovePlayerBullets()
        {
            // Loop from the end of the array to the beginning. This is a safe approach if we remove elements inside the loop (bullets[i] = null).            
            // When iterating backwards, we don't risk skipping elements. 
            // This is a good habit when working with collections where removals may occur.
            for (int i = bullets.Length - 1; i >= 0; i--)
            {
                GameObject bullet = bullets[i];

                if (bullet == null)
                    continue; // if there is no bullet, skip this iteration

                bullet.OldY = bullet.Y;

                bullet.Y++;

                if (bullet.Y > screenHeight - 1)
                {
                    renderer.ClearBullet(bullet);
                    bullets[i] = null; // Now we remove the specific bullet from the array, freeing the slot for future shots.
                }
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
        const char bulletChar = '^';

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

        public void Render(int oldShipX, int newShipX, int shipY, GameObject[] bullets)
        {
            builder[FindIndex(oldShipX, shipY)] = emptyChar;
            builder[FindIndex(newShipX, shipY)] = shipChar;

            // foreach — a way to iterate through all array elements without using an index           
            // Each element of the bullets array is assigned to the variable bullet in turn. 
            // This makes the code cleaner and easier to read if the index is not needed            
            foreach (var bullet in bullets)
                if (bullet != null)
                {
                    ClearBullet(bullet);
                    builder[FindIndex(bullet.X, bullet.Y)] = bulletChar;
                }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        public void ClearBullet(GameObject bullet) => builder.Replace(bulletChar, emptyChar, FindIndex(bullet.X, bullet.OldY), 1);

        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY;
            int width = screenWidth + 1;
            return valX + y * width;
        }
    }

    class GameObject
    {
        public int Y { get; set; }
        public int X { get; set; }
        public int OldY { get; set; }

        public GameObject(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}