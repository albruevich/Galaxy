// ❗ For switching between lessons, see Program.cs

// Lesson 9
// Teaching some enemies to shoot; they will differ visually from non-shooting enemies.
// The number of shooters will grow over time (the longer the game, the more shooters, but not exceeding a critical value).
// Instead of simple ship coordinates, a Ship class is now used.
// Killing shooting enemies gives a higher reward.

using System;
using System.Text;
using System.Collections.Generic;

namespace Lesson_09
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        bool isGameRunning = true;
        bool isGameOver = false;
        const int enemyAmount = 5;
        int enemySpawns = 0;
        int score = 0;

        const float startEnemySpeed = .12f;
        const float enemyDeltaSpeed = .02f;
        const int ticksToAccelerateEnemies = 30;
        float enemySpeed;
        int enemyTicks = 0;

        const int shooterRateAmount = 10; // base chance for a shooter to appear
        const int shooterIncreaseInterval = 30; // every 30 ticks the chance of shooters appearing increases
        const int shooterMaxChance = 70; // maximum chance for a shooter to appear: 70%
        int shooterTicks = 0; // global game tick counter for shooter growth

        Renderer renderer;

        Ship ship; // instead of simple coordinates, now a Ship object

        Bullet[] bullets = new Bullet[screenHeight - 2];
        Enemy[] enemies = new Enemy[screenWidth - 2];
        Bullet[] enemyBullets = new Bullet[screenWidth - 2]; // enemy bullets

        Random rnd = new Random();

        List<GameObject> renderedObjects = new List<GameObject>();

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            renderer.DrawFirstFrame(ship, enemies); // now Ship is passed
            renderer.UpdateScore(score);

            while (isGameRunning)
            {
                int oldX = ship.X;

                HandleInput();
                TryRespawnEnemy();
                MovePlayerBullets();
                MoveEnemyBullets(); // added enemy bullet movement
                MoveEnemies();
                TryAccelerateEnemies();
                IncreaseShooterChance();

                if (isGameOver)
                    continue;

                renderer.Render(oldX, ship, renderedObjects); // now Ship is passed
            }
        }

        void Init()
        {
            ship = new Ship(screenWidth / 2, 2);  // create Ship instead of coordinates
            renderer = new Renderer(screenWidth, screenHeight);
            enemySpeed = startEnemySpeed;

            CreateEnemies();
        }

        void HandleInput()
        {
            ConsoleKeyInfo info = Console.ReadKey(true);

            if (info.Key == ConsoleKey.Escape)
            {
                isGameRunning = false;
                return;
            }

            if (isGameOver)
            {
                if (info.Key == ConsoleKey.Spacebar)
                    Restart();

                return;
            }

            if (info.Key == ConsoleKey.LeftArrow)
                ship.X = Math.Max(1, ship.X - 1); // now Ship instead of coordinates
            else if (info.Key == ConsoleKey.RightArrow)
                ship.X = Math.Min(screenWidth - 2, ship.X + 1); // now Ship instead of coordinates
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
                    Bullet bullet = new Bullet(ship.X, ship.Y, direction: 1); // now Ship instead of coordinates
                    bullets[emptyIndex] = bullet;

                    renderedObjects.Add(bullet);
                }
            }
        }

        void MovePlayerBullets()
        {
            if (isGameOver)
                return;

            for (int i = bullets.Length - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];

                if (bullet == null)
                    continue;

                Enemy sameColumnEnemy = null;
                int enemyIndex = -1;

                for (int e = 0; e < enemies.Length; e++)
                {
                    var enemy = enemies[e];

                    if (enemy != null && enemy.X == bullet.X)
                    {
                        sameColumnEnemy = enemy;
                        enemyIndex = e;
                        break;
                    }
                }

                BulletMoveResult result = bullet.Move(screenHeight, sameColumnEnemy);

                if (result == BulletMoveResult.OutOfBounds || result == BulletMoveResult.Hit)
                {
                    renderer.ClearGameObject(bullet);
                    bullets[i] = null;

                    renderedObjects.Remove(bullet);

                    if (result == BulletMoveResult.Hit && sameColumnEnemy != null)
                    {
                        // Add points for destroying an enemy
                        // Enemy value is determined by Cost: shooter gives 2 points, normal enemy — 1 point
                        score += sameColumnEnemy.Cost;

                        sameColumnEnemy.OldY = sameColumnEnemy.Y;
                        renderer.ClearGameObject(sameColumnEnemy);

                        renderedObjects.Remove(sameColumnEnemy);
                        enemies[enemyIndex] = null;

                        enemySpawns++;

                        renderer.UpdateScore(score);
                    }
                }
            }
        }

        // new method for enemy bullets
        void MoveEnemyBullets()
        {
            // enemy bullets
            for (int i = enemyBullets.Length - 1; i >= 0; i--)
            {
                if (enemyBullets[i] == null)
                    continue;

                BulletMoveResult result = enemyBullets[i].Move(screenHeight, ship);

                if (result == BulletMoveResult.OutOfBounds || result == BulletMoveResult.Hit)
                {
                    renderer.ClearGameObject(enemyBullets[i]);
                    renderedObjects.Remove(enemyBullets[i]);
                    enemyBullets[i] = null;

                    if (result == BulletMoveResult.Hit)
                        isGameOver = true;

                    if (isGameOver)
                    {
                        renderer.PrintGameOver();
                        break;
                    }
                }
            }
        }

        void MoveEnemies()
        {
            if (isGameOver)
                return;

            foreach (var enemy in enemies)
            {
                if (enemy == null)
                    continue;

                if (enemy.Move(ship.X, ship.Y))
                    isGameOver = true;

                Bullet bullet = enemy.TryShoot(enemyBullets); // enemy shooting logic
                if (bullet != null)
                {
                    enemyBullets[enemy.X - 1] = bullet;
                    renderedObjects.Add(bullet);
                }

                if (isGameOver)
                {
                    renderer.PrintGameOver();
                    break;
                }
            }
        }

        void TryAccelerateEnemies()
        {
            if (isGameOver)
                return;

            enemyTicks++;

            if (enemyTicks > ticksToAccelerateEnemies)
            {
                enemyTicks = 0;

                const float maxEnemySpeed = 0.5f;
                enemySpeed = Math.Min(enemySpeed + enemyDeltaSpeed, maxEnemySpeed); // limit speed
            }
        }

        // new method to increase shooter probability
        void IncreaseShooterChance()
        {
            if (isGameOver)
                return;

            if (shooterTicks < int.MaxValue)
                shooterTicks++;
        }

        void CreateEnemies()
        {
            for (int i = 0; i < enemyAmount; i++)
                CreateEnemy();
        }

        void TryRespawnEnemy()
        {
            if (isGameOver)
                return;

            if (enemySpawns > 0)
            {
                enemySpawns--;
                CreateEnemy();
            }
        }

        void CreateEnemy()
        {
            List<int> freePositions = new List<int>();

            for (int i = 0; i < enemies.Length; i++)
                if (enemies[i] == null)
                    freePositions.Add(i);

            if (freePositions.Count == 0)
                return;

            int posIndex = freePositions[rnd.Next(freePositions.Count)];

            // shooter probability grows over time but is limited            
            bool isShooter = rnd.Next(0, 100) < Math.Min(shooterRateAmount + shooterTicks / shooterIncreaseInterval, shooterMaxChance);
            Enemy enemy = new Enemy(posIndex + 1, screenHeight - 1, enemySpeed, isShooter); // pass shooter flag

            enemies[posIndex] = enemy;
            renderedObjects.Add(enemy);
        }

        void Restart()
        {
            isGameOver = false;
            ship.X = screenWidth / 2;
            score = 0;
            enemySpeed = startEnemySpeed;
            enemyTicks = 0;
            shooterTicks = 0; // reset tick

            // clear objects
            ClearObjects(enemies);
            ClearObjects(bullets);
            ClearObjects(enemyBullets);

            renderer.ClearBoard(true);
            renderer.UpdateScore(score);

            CreateEnemies();
        }

        // new common method for clearing objects
        void ClearObjects(GameObject[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    renderedObjects.Remove(objects[i]);

                objects[i] = null;
            }
        }
    }

    class Renderer
    {
        int screenWidth;
        int screenHeight;

        StringBuilder builder;

        const char dotChar = '.';
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
            for (int bY = 0; bY <= screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                        builder.Append(dotChar);
                    else if ((bX == 0 || bX == screenWidth - 1) && bY != screenHeight)
                        builder.Append(wallChar);
                    else
                        builder.Append(emptyChar);
                }
                builder.Append('\n');
            }
        }

        public void DrawFirstFrame(Ship ship, Enemy[] enemies) // now Ship is passed instead of coordinates
        {
            builder[FindIndex(ship.X, ship.Y)] = ship.Symbol;

            foreach (var enemy in enemies)
            {
                if (enemy == null)
                    continue;

                builder[FindIndex(enemy.X, enemy.Y)] = enemy.Symbol;
            }

            Console.WriteLine(builder);
        }

        public void Render(int oldShipX, Ship ship, List<GameObject> gameObjects)  // now Ship is passed instead of coordinates
        {
            builder[FindIndex(oldShipX, ship.Y)] = emptyChar;
            builder[FindIndex(ship.X, ship.Y)] = ship.Symbol;

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

        public void PrintGameOver()
        {
            ClearBoard(false);

            const string text = "GAME OVER";
            DrawText(text, screenWidth / 2 - text.Length / 2, screenHeight / 2 + 1);
            const string text2 = "SPACE TO PLAY";
            DrawText(text2, screenWidth / 2 - text2.Length / 2, screenHeight / 2 - 1);
        }

        public void ClearBoard(bool clearScore)
        {
            int deltaWidth = clearScore ? 0 : screenWidth + 1;

            for (int i = 0; i < builder.Length - deltaWidth; i++)
            {
                char c = builder[i];
                if (c != wallChar && c != dotChar && c != '\n')
                    builder[i] = emptyChar;
            }
        }

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

        public void UpdateScore(int score) => DrawText($"score: {score}", 0, 0);

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
        public override char Symbol => direction > 0 ? '^' : '*'; // If bullet moves up (player), symbol '^'; if down (enemy), symbol '*'

        int direction; // movement direction: 1 — up, -1 — down

        public Bullet(int x, int y, int direction) : base(x, y)
        {
            this.direction = direction; // constructor receives initial coordinates and bullet direction
        }

        public BulletMoveResult Move(int screenHeight, GameObject aim)
        {
            OldY = Y;
            Y += direction; // move vertically according to direction

            if (aim != null && aim.X == X && (Y - aim.Y) * direction >= 0) // check hit on target
                return BulletMoveResult.Hit;

            if (Y > screenHeight - 1 || Y < 2) // check bullet out of bounds              
                return BulletMoveResult.OutOfBounds;

            return BulletMoveResult.None;
        }
    }

    class Enemy : GameObject
    {
        public override char Symbol => isShooter ? '&' : '@'; // Shooter displayed as '&', normal enemy '@'

        // Enemy cost in points when destroyed
        // If enemy shoots (isShooter == true), it gives 2 points
        // Otherwise, gives 1 point
        public int Cost => isShooter ? 2 : 1;

        float enemySpeed;
        float slowY;

        bool isShooter; // whether enemy can shoot

        private int shootTick = 0; // tick counter for controlling fire rate

        const int rateOfFire = 12; // number of game ticks between enemy shots        

        public Enemy(int x, int y, float startSpeed, bool isShooter) : base(x, y)
        {
            enemySpeed = startSpeed;
            slowY = y + enemySpeed;
            this.isShooter = isShooter; // store shooter info           
        }

        public bool Move(int shipX, int shipY)
        {
            OldY = Y;
            slowY = Math.Max(slowY - enemySpeed, 1);
            Y = (int)slowY;

            if (slowY < 2 || (shipX == X && Y == shipY))
                return true;

            return false;
        }

        // Enemy shooting logic
        public Bullet TryShoot(Bullet[] enemyBullets)
        {
            if (!isShooter)
                return null;

            shootTick++;

            // if fire interval reached and no bullet on this vertical, shoot
            if (shootTick >= rateOfFire && Y > 2 && enemyBullets[X - 1] == null)
            {
                shootTick = 0;
                return new Bullet(X, Y - 1, direction: -1); // enemy bullet moves down              
            }
            return null;
        }
    }

    // New class for player, used as object instead of coordinates
    class Ship : GameObject
    {
        public override char Symbol => '#';

        public Ship(int x, int y) : base(x, y) { }
    }

    enum BulletMoveResult
    {
        None,
        Hit,
        OutOfBounds
    }
}