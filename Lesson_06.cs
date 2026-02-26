// ❗ For switching between lessons, see Program.cs

// Lesson 6
// The enemy now moves on its own at its own speed.
// Game Over state and the ability to restart the game appear.

using System;
using System.Text;

// include .NET collections such as List, Dictionary, Queue, etc., for storing and managing game objects
using System.Collections.Generic;

namespace Lesson_06
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2;
        bool isGameRunning = true;

        bool isGameOver = false; // flag for game over state (game stopped, but program not terminated)

        Renderer renderer;

        Bullet[] bullets = new Bullet[screenHeight - 2];

        Random rnd = new Random();

        Enemy enemy = null;

        // universal list of all game objects for rendering
        // List automatically grows and shrinks, so we don't need to manually recreate arrays and copy data.
        List<GameObject> renderedObjects = new List<GameObject>();

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            renderer.DrawFirstFrame(shipX, shipY, enemy);

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                MovePlayerBullets();
                MoveEnemies(); // enemy movement moved to a separate method

                // if the game is over, stop rendering game objects
                if (isGameOver)
                    continue;

                renderer.Render(oldX, shipX, shipY, renderedObjects);
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight);

            CreateEnemies(); // enemy creation moved to a separate method
        }

        void HandleInput()
        {
            ConsoleKeyInfo info = Console.ReadKey(true);

            if (info.Key == ConsoleKey.Escape)
            {
                isGameRunning = false;
                return;
            }

            // if game is over — allow only restart
            if (isGameOver)
            {
                if (info.Key == ConsoleKey.Spacebar)
                    Restart();

                return;
            }

            if (info.Key == ConsoleKey.LeftArrow)
                shipX = Math.Max(1, shipX - 1);
            else if (info.Key == ConsoleKey.RightArrow)
                shipX = Math.Min(screenWidth - 2, shipX + 1);
            else if (info.Key == ConsoleKey.UpArrow)
            {
                int emptyIndex = -1;

                for (int i = 0; i < bullets.Length; i++)
                    if (bullets[i] == null)
                    {
                        emptyIndex = i;
                        break;
                    }

                if (emptyIndex != -1)
                {
                    Bullet bullet = new Bullet(shipX, shipY);
                    bullets[emptyIndex] = bullet;

                    renderedObjects.Add(bullet); // add object to the list
                }
            }
        }

        void MovePlayerBullets()
        {
            // bullets stop moving at Game Over
            if (isGameOver)
                return;

            for (int i = bullets.Length - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];

                if (bullet == null)
                    continue;

                BulletMoveResult result = bullet.Move(screenHeight, enemy);

                if (result == BulletMoveResult.OutOfBounds || result == BulletMoveResult.Hit)
                {
                    renderer.ClearGameObject(bullet);
                    bullets[i] = null;

                    renderedObjects.Remove(bullet); // remove object from list

                    if (result == BulletMoveResult.Hit)
                    {
                        // properly clear enemy considering OldY
                        enemy.OldY = enemy.Y;
                        renderer.ClearGameObject(enemy);

                        renderedObjects.Remove(enemy); // remove object from list
                        enemy = null;
                    }
                }
            }
        }

        // enemy movement moved to a separate method
        void MoveEnemies()
        {
            // enemies stop moving at Game Over
            if (isGameOver)
                return;

            if (enemy != null)
            {
                // enemy Move method now returns true if player is defeated
                isGameOver = enemy.Move(shipX, shipY);

                // if game over, display it on screen
                if (isGameOver)
                    renderer.PrintGameOver();
            }
        }

        // enemy creation moved to a separate method (preparing for multiple enemies)
        void CreateEnemies()
        {
            int enemyX = rnd.Next(1, screenWidth - 1);
            int enemyY = screenHeight - 1;
            enemy = new Enemy(enemyX, enemyY);
            renderedObjects.Add(enemy);
        }

        // full game restart without restarting the program
        void Restart()
        {
            isGameOver = false;
            shipX = screenWidth / 2;

            // remove enemy from rendering list
            if (enemy != null)
                renderedObjects.Remove(enemy);

            enemy = null;

            for (int i = 0; i < bullets.Length; i++)
            {
                // remove bullets from rendering list
                if (bullets[i] != null)
                    renderedObjects.Remove(bullets[i]);

                // reset each array element to avoid old bullet references
                bullets[i] = null;
            }

            renderer.ClearBoard(); // clear screen of all objects except borders: ceiling, floor, and walls

            CreateEnemies();
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

        public void DrawFirstFrame(int shipX, int shipY, Enemy enemy)
        {
            builder[FindIndex(shipX, shipY)] = shipChar;

            if (enemy != null)
                builder[FindIndex(enemy.X, enemy.Y)] = enemy.Symbol;

            Console.WriteLine(builder);
        }

        // Renderer now works with a universal list of GameObject
        public void Render(int oldShipX, int newShipX, int shipY, List<GameObject> gameObjects)
        {
            builder[FindIndex(oldShipX, shipY)] = emptyChar;
            builder[FindIndex(newShipX, shipY)] = shipChar;

            foreach (var go in gameObjects)
                if (go != null)
                {
                    ClearGameObject(go);
                    builder[FindIndex(go.X, go.Y)] = go.Symbol;
                }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        public void ClearGameObject(GameObject go) =>
            builder.Replace(go.Symbol, emptyChar, FindIndex(go.X, go.OldY), 1);

        // display Game Over screen
        public void PrintGameOver()
        {
            ClearBoard();

            const string text = "GAME OVER";
            DrawText(text, screenWidth / 2 - text.Length / 2, screenHeight / 2 + 1);
            const string text2 = "SPACE TO PLAY";
            DrawText(text2, screenWidth / 2 - text2.Length / 2, screenHeight / 2 - 1);
        }

        // clear game field without destroying walls
        public void ClearBoard()
        {
            for (int i = 0; i < builder.Length; i++)
            {
                char c = builder[i];
                if (c != wallChar && c != dotChar && c != '\n')
                    builder[i] = emptyChar;
            }
        }

        // universal method for drawing text on screen
        public void DrawText(string text, int x, int y)
        {
            if (x < 0 || x + text.Length > screenWidth || y < 0 || y >= screenHeight)
                return;

            int index = FindIndex(x, y);

            for (int i = 0; i < text.Length; i++)
                builder[index + i] = text[i];

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY;
            int width = screenWidth + 1;
            return valX + y * width;
        }
    }

    abstract class GameObject
    {
        public int Y { get; set; }
        public int X { get; set; }
        public int OldY { get; set; }

        public abstract char Symbol { get; }

        public GameObject(int x, int y)
        {
            X = x;
            Y = y;
            OldY = Y;
        }
    }

    class Bullet : GameObject
    {
        public override char Symbol => '^';

        public Bullet(int x, int y) : base(x, y) { }

        public BulletMoveResult Move(int screenHeight, GameObject aim)
        {
            OldY = Y;
            Y++;

            if (aim != null && X == aim.X && Y >= aim.Y)
                return BulletMoveResult.Hit;

            if (Y > screenHeight - 1)
                return BulletMoveResult.OutOfBounds;

            return BulletMoveResult.None;
        }
    }

    class Enemy : GameObject
    {
        public override char Symbol => '@';

        // enemy speed is less than 1 cell per frame
        const float enemySpeed = .12f;

        // slowY — enemy position as float for smooth movement (<1 cell per frame)
        float slowY;

        public Enemy(int x, int y) : base(x, y)
        {
            // Start slowY slightly above the integer Y,
            // so the first movement doesn't jump a full cell,
            // but progresses gradually — makes enemy movement visually smooth
            slowY = y + enemySpeed;
        }

        // enemy move method now returns true if the player is defeated
        public bool Move(int shipX, int shipY)
        {
            OldY = Y;
            slowY = Math.Max(slowY - enemySpeed, 1);

            Y = (int)slowY;

            // check for defeat: did enemy reach the floor
            // or collide with the player ship
            if (slowY < 2 || (shipX == X && Y == shipY))
                return true;

            return false;
        }
    }

    enum BulletMoveResult
    {
        None,
        Hit,
        OutOfBounds
    }
}