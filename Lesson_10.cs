// ❗ For switching between lessons, see Program.cs

// Lesson 10 — a serious step forward in the game architecture.

// Game objects are now colored.

// Renderer has been refactored:
// Render is simplified, ClearGameObject, FindIndex, DrawFirstFrame, and OldY in game objects are removed.
// StringBuilder is now used only for drawing walls and background.

// Added high score that is saved on the computer.
// In this lesson, we get acquainted with file operations and make the game more complete.

using System;
using System.Text;
using System.Collections.Generic;
using System.IO; // included namespace for file operations

namespace Lesson_10
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

        const int shooterRateAmount = 10;
        const int shooterIncreaseInterval = 30;
        const int shooterMaxChance = 70;
        int shooterTicks = 0;

        int hiScore = 0; // store high score in the game

        Renderer renderer;

        Ship ship;

        Bullet[] bullets = new Bullet[screenHeight - 2];
        Enemy[] enemies = new Enemy[screenWidth - 2];
        Bullet[] enemyBullets = new Bullet[screenWidth - 2];

        Random rnd = new Random();

        FileManager fileManager; // manager for high score file

        List<GameObject> renderedObjects = new List<GameObject>();

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            // Render now accepts score and hiScore
            renderer.Render(renderedObjects, score, hiScore);

            // no more DrawFirstFrame or UpdateScore

            while (isGameRunning)
            {
                HandleInput();
                TryRespawnEnemy();
                MovePlayerBullets();
                MoveEnemyBullets();
                MoveEnemies();
                TryAccelerateEnemies();
                IncreaseShooterChance();

                if (isGameOver)
                    continue;

                // unified frame rendering without oldX and Ship
                renderer.Render(renderedObjects, score, hiScore);
            }
        }

        void Init()
        {
            ship = new Ship(screenWidth / 2, 2);
            renderedObjects.Add(ship); // ship is now immediately added to the general object list
            renderer = new Renderer(screenWidth, screenHeight);
            enemySpeed = startEnemySpeed;

            fileManager = new FileManager(); // create file manager
            hiScore = fileManager.LoadHiScore(); // load high score at game start

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
                ship.X = Math.Max(1, ship.X - 1);
            else if (info.Key == ConsoleKey.RightArrow)
                ship.X = Math.Min(screenWidth - 2, ship.X + 1);
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
                    Bullet bullet = new Bullet(ship.X, ship.Y, direction: 1);
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
                    bullets[i] = null;

                    renderedObjects.Remove(bullet);

                    if (result == BulletMoveResult.Hit && sameColumnEnemy != null)
                    {
                        score += sameColumnEnemy.Cost;

                        renderedObjects.Remove(sameColumnEnemy);
                        enemies[enemyIndex] = null;

                        enemySpawns++;
                    }
                }
            }
        }

        void MoveEnemyBullets()
        {
            for (int i = enemyBullets.Length - 1; i >= 0; i--)
            {
                if (enemyBullets[i] == null)
                    continue;

                BulletMoveResult result = enemyBullets[i].Move(screenHeight, ship);

                if (result == BulletMoveResult.OutOfBounds || result == BulletMoveResult.Hit)
                {
                    renderedObjects.Remove(enemyBullets[i]);
                    enemyBullets[i] = null;

                    if (result == BulletMoveResult.Hit)
                    {
                        GameOver(); // now calling method instead of isGameOver = true; 
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
                    GameOver(); // now calling method instead of isGameOver = true; 

                Bullet bullet = enemy.TryShoot(enemyBullets);
                if (bullet != null)
                {
                    enemyBullets[enemy.X - 1] = bullet;
                    renderedObjects.Add(bullet);
                }

                if (isGameOver)
                    break;
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
                enemySpeed = Math.Min(enemySpeed + enemyDeltaSpeed, maxEnemySpeed);
            }
        }

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

            bool isShooter = rnd.Next(0, 100) < Math.Min(shooterRateAmount + shooterTicks / shooterIncreaseInterval, shooterMaxChance);
            Enemy enemy = new Enemy(posIndex + 1, screenHeight - 1, enemySpeed, isShooter);

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
            shooterTicks = 0;

            ClearObjects(enemies);
            ClearObjects(bullets);
            ClearObjects(enemyBullets);

            CreateEnemies();
        }

        void ClearObjects(GameObject[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    renderedObjects.Remove(objects[i]);

                objects[i] = null;
            }
        }

        // new method for Game Over 
        void GameOver()
        {
            isGameOver = true;

            if (score > hiScore)
            {
                hiScore = score;
                renderer.PrintGameOver(newHiScore: hiScore); // possibility to pass new high score to renderer
                fileManager.SaveHiScore(hiScore); // save high score to file
            }
            else
                renderer.PrintGameOver();
        }
    }

    class Renderer
    {
        int screenWidth;
        int screenHeight;

        // builder now only as background: walls and empty space
        // it no longer draws game objects or text
        StringBuilder backgroundBuilder;

        const char dotChar = '.';
        const char wallChar = '|';
        const char emptyChar = ' ';

        public Renderer(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
            backgroundBuilder = new StringBuilder();
        }

        public void BuildBoard()
        {
            for (int bY = 0; bY <= screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                        backgroundBuilder.Append(dotChar);
                    else if ((bX == 0 || bX == screenWidth - 1) && bY != screenHeight)
                        backgroundBuilder.Append(wallChar);
                    else
                        backgroundBuilder.Append(emptyChar);
                }
                backgroundBuilder.Append('\n');
            }
        }

        public void Render(List<GameObject> gameObjects, int score, int hiScore)
        {
            // draw walls and background, effectively clearing screen of all old objects and text
            Console.SetCursorPosition(0, 0);
            Console.Write(backgroundBuilder);

            // objects are now drawn directly to console
            foreach (var go in gameObjects)
                DrawColoredObject(go);

            // draw score
            DrawText($"score: {score}", 0, 0, ConsoleColor.DarkYellow);       

            // display high score
            if (hiScore > 0)
            {
                string hiStr = $"hi: {hiScore}";
                DrawText(hiStr, screenWidth - hiStr.Length, 0, ConsoleColor.DarkYellow);
            }

            Console.ResetColor();
            Console.SetCursorPosition(0, screenHeight + 2);
        }

        void DrawColoredObject(GameObject go)
        {
            Console.SetCursorPosition(go.X, screenHeight - go.Y);
            Console.ForegroundColor = go.Color;
            Console.Write(go.Symbol);
        }

        public void PrintGameOver(int newHiScore = 0)
        {
            // clear only inner area, do not touch border
            for (int x = 1; x < screenWidth - 1; x++)
                for (int y = 1; y < screenHeight - 1; y++)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(emptyChar);
                }

            int deltaG = 1;
            int deltaS = -1;

            if (newHiScore > 0)
            {
                deltaG = 2;
                deltaS = -2;

                string scoreText = $"NEW HI: {newHiScore}";
                DrawText(scoreText, screenWidth / 2 - scoreText.Length / 2, screenHeight / 2, ConsoleColor.Green);
            }

            const string text = "GAME OVER";
            DrawText(text, screenWidth / 2 - text.Length / 2, screenHeight / 2 + deltaG, ConsoleColor.DarkYellow);

            const string text2 = "SPACE TO PLAY";
            DrawText(text2, screenWidth / 2 - text2.Length / 2, screenHeight / 2 + deltaS);

            Console.SetCursorPosition(0, screenHeight + 2);
        }

        public void DrawText(string text, int x, int y, ConsoleColor color = ConsoleColor.White)
        {
            int consoleY = screenHeight - y;
            Console.SetCursorPosition(x, consoleY);
            Console.ForegroundColor = color;
            Console.Write(text);
        }
    }

    abstract class GameObject
    {
        public int Y { get; set; }
        public int X { get; set; }

        public abstract char Symbol { get; }

        public virtual ConsoleColor Color => ConsoleColor.White;

        public GameObject(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class Bullet : GameObject
    {
        public override char Symbol => direction > 0 ? '^' : '*';

        public override ConsoleColor Color => direction > 0 ? ConsoleColor.Cyan : ConsoleColor.Red;

        int direction;

        public Bullet(int x, int y, int direction) : base(x, y)
        {
            this.direction = direction;
        }

        public BulletMoveResult Move(int screenHeight, GameObject aim)
        {
            Y += direction;

            if (aim != null && aim.X == X && (Y - aim.Y) * direction >= 0)
                return BulletMoveResult.Hit;

            if (Y > screenHeight - 1 || Y < 2)
                return BulletMoveResult.OutOfBounds;

            return BulletMoveResult.None;
        }
    }

    class Enemy : GameObject
    {
        public override char Symbol => isShooter ? '&' : '@';

        public override ConsoleColor Color => isShooter ? ConsoleColor.Red : ConsoleColor.Yellow;

        public int Cost => isShooter ? 2 : 1;

        float enemySpeed;
        float slowY;

        bool isShooter;

        private int shootTick = 0;

        const int rateOfFire = 12;

        public Enemy(int x, int y, float startSpeed, bool isShooter) : base(x, y)
        {
            enemySpeed = startSpeed;
            slowY = y + enemySpeed;
            this.isShooter = isShooter;
        }

        public bool Move(int shipX, int shipY)
        {
            slowY = Math.Max(slowY - enemySpeed, 1);
            Y = (int)slowY;

            if (slowY < 2 || (shipX == X && Y == shipY))
                return true;

            return false;
        }

        public Bullet TryShoot(Bullet[] enemyBullets)
        {
            if (!isShooter)
                return null;

            shootTick++;

            if (shootTick >= rateOfFire && Y > 2 && enemyBullets[X - 1] == null)
            {
                shootTick = 0;
                return new Bullet(X, Y - 1, direction: -1);
            }
            return null;
        }
    }

    class Ship : GameObject
    {
        public override char Symbol => '#';

        public override ConsoleColor Color => ConsoleColor.Cyan;

        public Ship(int x, int y) : base(x, y) { }
    }

    class FileManager
    {
        static readonly string HiScorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Galaxy", "hiscore.txt");

        public int LoadHiScore()
        {
            int hiScore = 0;

            try
            {
                if (File.Exists(HiScorePath))
                {
                    if (int.TryParse(File.ReadAllText(HiScorePath), out int value))
                        hiScore = value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return hiScore;
        }

        public void SaveHiScore(int hiScore)
        {
            try
            {
                string dir = Path.GetDirectoryName(HiScorePath);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(HiScorePath, hiScore.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    enum BulletMoveResult
    {
        None,
        Hit,
        OutOfBounds
    }
}