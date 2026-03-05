// ❗ For switching between lessons, see Program.cs

// Lesson 8
// Gradually increasing difficulty: first slow,
// but after a certain number of game cycles we increase the speed of new enemies.
// Adds and displays the score.

using System;
using System.Text;
using System.Collections.Generic;

namespace Lesson_08
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2;
        bool isGameRunning = true;
        bool isGameOver = false;
        const int enemyAmount = 5;
        int enemySpawns = 0;

        int score = 0; // player score       

        const float startEnemySpeed = .12f; // base starting enemy speed        
        const float enemyDeltaSpeed = .02f; // speed increment — mechanism for gradually increasing game difficulty
        const int ticksToAccelerateEnemies = 30; // number of game cycles before enemies accelerate      
        float enemySpeed; // current enemy speed now stored in a variable, not as a constant inside Enemy (as in lesson 7)
        int enemyTicks = 0; // game cycle counter to control acceleration timing       

        Renderer renderer;

        Bullet[] bullets = new Bullet[screenHeight - 2];
        Enemy[] enemies = new Enemy[screenWidth - 2];

        Random rnd = new Random();

        List<GameObject> renderedObjects = new List<GameObject>();

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            renderer.DrawFirstFrame(shipX, shipY, enemies);
            renderer.UpdateScore(score); // initial display of score at game start      

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                TryRespawnEnemy();
                MovePlayerBullets();
                MoveEnemies();
                TryAccelerateEnemies(); // added stage for enemy speed acceleration               

                if (isGameOver)
                    continue;

                renderer.Render(oldX, shipX, shipY, renderedObjects);
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight);
            enemySpeed = startEnemySpeed; // initialize current enemy speed           

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
                        sameColumnEnemy.OldY = sameColumnEnemy.Y;
                        renderer.ClearGameObject(sameColumnEnemy);

                        renderedObjects.Remove(sameColumnEnemy);
                        enemies[enemyIndex] = null;

                        enemySpawns++;

                        renderer.UpdateScore(++score); // increase score on enemy destruction and immediately update display                       
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

                if (enemy.Move(shipX, shipY))
                    isGameOver = true;

                if (isGameOver)
                {
                    renderer.PrintGameOver();
                    break;
                }
            }
        }

        // gradual increase of game difficulty
        void TryAccelerateEnemies()
        {
            if (isGameOver) // don't accelerate after game over
                return;

            enemyTicks++;  // increase game ticks

            if (enemyTicks > ticksToAccelerateEnemies)  // check if enough time has passed
            {
                enemyTicks = 0; // reset tick counter                
                enemySpeed += enemyDeltaSpeed; // increase enemy speed
            }
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

            Enemy enemy = new Enemy(posIndex + 1, screenHeight - 1, enemySpeed); // enemy now created with speed parameter           

            enemies[posIndex] = enemy;
            renderedObjects.Add(enemy);
        }

        void Restart()
        {
            isGameOver = false;
            shipX = screenWidth / 2;
            enemySpeed = startEnemySpeed; // reset enemy speed
            score = 0; // reset score on restart
            enemyTicks = 0; // reset tick counter

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                    renderedObjects.Remove(enemies[i]);

                enemies[i] = null;
            }

            for (int i = 0; i < bullets.Length; i++)
            {
                if (bullets[i] != null)
                    renderedObjects.Remove(bullets[i]);

                bullets[i] = null;
            }

            renderer.ClearBoard(true); // ClearBoard now considers whether to clear the score line            
            renderer.UpdateScore(score); // update score after restart           

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

        public void DrawFirstFrame(int shipX, int shipY, Enemy[] enemies)
        {
            builder[FindIndex(shipX, shipY)] = shipChar;

            foreach (var enemy in enemies)
            {
                if (enemy == null)
                    continue;

                builder[FindIndex(enemy.X, enemy.Y)] = enemy.Symbol;
            }

            Console.WriteLine(builder);
        }

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

        public void PrintGameOver()
        {
            ClearBoard(false); // don't clear score on GameOver          

            const string text = "GAME OVER";
            DrawText(text, screenWidth / 2 - text.Length / 2, screenHeight / 2 + 1);
            const string text2 = "SPACE TO PLAY";
            DrawText(text2, screenWidth / 2 - text2.Length / 2, screenHeight / 2 - 1);
        }

        public void ClearBoard(bool clearScore)
        {
            int deltaWidth = clearScore ? 0 : screenWidth + 1; // partial screen clearing logic to preserve score line

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

        public void UpdateScore(int score) => DrawText($"score: {score}", 0, 0); // dedicated method to update the score line

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

            if (aim != null && aim.X == X && Y >= aim.Y)
                return BulletMoveResult.Hit;

            if (Y > screenHeight - 1)
                return BulletMoveResult.OutOfBounds;

            return BulletMoveResult.None;
        }
    }

    class Enemy : GameObject
    {
        public override char Symbol => '@';

        float enemySpeed; // enemy speed now not constant, passed from outside via constructor         
        float slowY;

        public Enemy(int x, int y, float startSpeed) : base(x, y)
        {
            enemySpeed = startSpeed; // get current speed from Game            
            slowY = y + enemySpeed;
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
    }

    enum BulletMoveResult
    {
        None,
        Hit,
        OutOfBounds
    }
}