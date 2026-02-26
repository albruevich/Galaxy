// ❗ Для переключения между уроками смотрите Program.cs

// Урок 10
// Добавлен Рекорд, который сохраняется на компьютере
// Таким образом учимся работать с файлами

// Игровые объекты теперь в цвете

// Рефакторинг рендерера:
// Переделан метод Render. Удалено: ClearGameObject, FindIndex, DrawFirstFrame, OldY у объектов
// StringBuilder теперь используется только для отрисовки стен

using System;
using System.Text;
using System.Collections.Generic;

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

        Renderer renderer;

        Ship ship; 

        Bullet[] bullets = new Bullet[screenHeight - 2];
        Enemy[] enemies = new Enemy[screenWidth - 2];
        Bullet[] enemyBullets = new Bullet[screenWidth - 2];  

        Random rnd = new Random();

        List<GameObject> renderedObjects = new List<GameObject>();

        public void Run()
        {
            Init();
            renderer.BuildBoard();         
            renderer.Render(renderedObjects, score);                     

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

                renderer.Render(renderedObjects, score);
            }
        }

        void Init()
        {
            ship = new Ship(screenWidth / 2, 2);
            renderedObjects.Add(ship);
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

                        renderer.UpdateScore(score);
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

                Bullet bullet = enemy.TryShoot(enemyBullets); 
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

        public void Render(List<GameObject> gameObjects, int score)
        {
            //рисуются стены и фон, таким образом очищается экран от всех игровых объектов
            Console.SetCursorPosition(0, 0);            
            Console.Write(builder);

            // рисуются игровые объекты
            foreach (var go in gameObjects)               
                DrawColoredObject(go);

            // рисуется счет
            DrawText($"score: {score}", 0, 0, ConsoleColor.DarkYellow);
           
            Console.ResetColor();  //сброс цвета
            Console.SetCursorPosition(0, screenHeight + 2); // установка курсора вниз
        }

        void DrawColoredObject(GameObject go)
        {
            Console.SetCursorPosition(go.X, screenHeight - go.Y);
            Console.ForegroundColor = go.Color;
            Console.Write(go.Symbol);            
        }

        public void PrintGameOver()
        {
            for (int x = 1; x < screenWidth - 1; x++)            
                for (int y = 1; y < screenHeight - 1; y++)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(emptyChar);
                }            

            const string text = "GAME OVER";
            DrawText(text, screenWidth / 2 - text.Length / 2, screenHeight / 2 + 1, ConsoleColor.DarkYellow);
            const string text2 = "SPACE TO PLAY";
            DrawText(text2, screenWidth / 2 - text2.Length / 2, screenHeight / 2 - 1);

            Console.SetCursorPosition(0, screenHeight + 2);
        }       

        public void DrawText(string text, int x, int y, ConsoleColor color = ConsoleColor.White)
        {
            if (x < 0 || x + text.Length > screenWidth || y < 0 || y >= screenHeight)
                return;          

            int consoleY = screenHeight - y;
            Console.SetCursorPosition(x, consoleY);
            Console.ForegroundColor = color;
            Console.Write(text);           
        }    

        public void UpdateScore(int score) => DrawText($"score: {score}", 0, 0);            
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

    enum BulletMoveResult
    {
        None,
        Hit,
        OutOfBounds
    }
}
