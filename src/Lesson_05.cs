// ❗ For switching between lessons, see Program.cs

// Lesson 5
// In this lesson, the game moves from simple object control to interaction between objects.
// We introduce an abstract base class, implement inheritance, add an enemy, and collisions.
// This is the first real step toward a game architecture.

using System;
using System.Text;

namespace Lesson_05
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2;
        bool isGameRunning = true;

        Renderer renderer;

        // Previously this array was GameObject[]. Now it stores the specific type Bullet.        
        Bullet[] bullets = new Bullet[screenHeight - 2]; // Maximum bullets on screen (by game field height without borders)

        Random rnd = new Random(); // random number generator

        // Another type of game object appears in the game.
        Enemy enemy = null; // currently the only enemy object

        public void Run()
        {
            Init();
            renderer.BuildBoard();

            // Renderer can now draw not only the ship but also the enemy.
            renderer.DrawFirstFrame(shipX, shipY, enemy);

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                MovePlayerBullets();

                renderer.Render(oldX, shipX, shipY, bullets);
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight);

            // Creating an instance of Enemy — now using a GameObject descendant.
            int enemyX = rnd.Next(1, screenWidth - 1); // random X within 1 to screenWidth (excluding walls)
            int enemyY = screenHeight - 1;             // Y below the ceiling (because FindIndex treats origin at bottom)
            enemy = new Enemy(enemyX, enemyY);
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
                }
            }
        }

        void MovePlayerBullets()
        {
            for (int i = bullets.Length - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];

                if (bullet == null)
                    continue;

                // The method returns the movement result: hit, out of bounds, or continues flying.
                // More convenient than just true/false.
                BulletMoveResult result = bullet.Move(screenHeight, enemy);

                // if the bullet hits the enemy or goes out of bounds ...
                if (result == BulletMoveResult.OutOfBounds || result == BulletMoveResult.Hit)
                {
                    // ... remove the bullet
                    renderer.ClearGameObject(bullet);
                    bullets[i] = null;

                    // ... if the bullet hit the enemy, remove the enemy
                    if (result == BulletMoveResult.Hit)
                    {
                        renderer.ClearGameObject(enemy);
                        enemy = null;
                    }
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

        // Now the method can draw not only the ship but also the enemy.        
        public void DrawFirstFrame(int shipX, int shipY, Enemy enemy)
        {
            builder[FindIndex(shipX, shipY)] = shipChar;

            // If the enemy exists, draw its symbol
            if (enemy != null)
                builder[FindIndex(enemy.X, enemy.Y)] = enemy.Symbol;

            Console.WriteLine(builder);
        }

        public void Render(int oldShipX, int newShipX, int shipY, Bullet[] bullets)
        {
            builder[FindIndex(oldShipX, shipY)] = emptyChar;
            builder[FindIndex(newShipX, shipY)] = shipChar;

            foreach (var bullet in bullets)
                if (bullet != null)
                {
                    ClearGameObject(bullet);
                    builder[FindIndex(bullet.X, bullet.Y)] = bullet.Symbol;
                }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        // In the previous lesson there was ClearBullet using bulletChar.
        // Now the method is universal — it accepts any GameObject and uses its Symbol.
        // This is the first step toward polymorphism.
        public void ClearGameObject(GameObject go) =>
            builder.Replace(go.Symbol, emptyChar, FindIndex(go.X, go.OldY), 1);

        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY;
            int width = screenWidth + 1;
            return valX + y * width;
        }
    }

    // GameObject is now abstract — you cannot create a generic "object" but only a specific descendant        
    abstract class GameObject
    {
        public int Y { get; set; }
        public int X { get; set; }
        public int OldY { get; set; }

        // abstract property that EACH descendant MUST implement.
        public abstract char Symbol { get; }

        public GameObject(int x, int y)
        {
            X = x;
            Y = y;
            OldY = Y;
        }
    }

    // Bullet inherits from GameObject, so it has all the parent's properties and methods
    class Bullet : GameObject
    {
        // override — implementation (overriding) of the abstract property from parent
        public override char Symbol => '^';

        // : base(x, y) — call the constructor of the parent class.
        public Bullet(int x, int y) : base(x, y) { }

        // The Move method is responsible only for bullet movement and returns the result.       
        public BulletMoveResult Move(int screenHeight, GameObject aim)
        {
            OldY = Y;
            Y++;

            // if bullet coordinates match enemy coordinates, it's considered a hit
            if (aim != null && X == aim.X && Y >= aim.Y)
                return BulletMoveResult.Hit;

            // if the bullet goes out of screen bounds, report it
            if (Y > screenHeight - 1)
                return BulletMoveResult.OutOfBounds;

            // if nothing special happened — continue movement
            return BulletMoveResult.None;
        }
    }

    // Enemy inherits from GameObject.
    class Enemy : GameObject
    {
        // override — implementation (overriding) of the abstract property from parent
        public override char Symbol => '@';

        // : base(x, y) — call the constructor of the parent class.
        public Enemy(int x, int y) : base(x, y) { }
    }

    // possible outcomes of bullet movement 
    enum BulletMoveResult
    {
        None,
        Hit,
        OutOfBounds
    }
}