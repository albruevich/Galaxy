// ❗ Для переключения между уроками смотрите Program.cs

// Урок 9
// Учим некоторых врагов стрелять, они будут отличаться по внешнему виду от нестреляющих.
// Количество стрелков будет расти со временем (чем дольше игра, тем больше срелков, но не больше критического значения).
// Вместо простых координат корабля теперь используется класс Ship.
// За убийство стреляющих врагов начисляется больше награда.

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

        const int shooterRateAmount = 10; //базовый шанс появления стрелка 
        const int shooterIncreaseInterval = 30; //через 30 тиков шанс появляния стрелков увеличится 
        const int shooterMaxChance = 70; //максимальный шанс появления стрелка 70% 
        int shooterTicks = 0; //общий счётчик времени игры для роста стрелков 

        Renderer renderer;

        Ship ship; // вместо простых координат теперь объект Ship 

        Bullet[] bullets = new Bullet[screenHeight - 2];
        Enemy[] enemies = new Enemy[screenWidth - 2];
        Bullet[] enemyBullets = new Bullet[screenWidth - 2]; // пули врагов 

        Random rnd = new Random();

        List<GameObject> renderedObjects = new List<GameObject>();

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            renderer.DrawFirstFrame(ship, enemies); // теперь Ship передаётся
            renderer.UpdateScore(score);

            while (isGameRunning)
            {
                int oldX = ship.X;

                HandleInput();
                TryRespawnEnemy();
                MovePlayerBullets();
                MoveEnemyBullets(); // добавлен метод движения пуль врагов 
                MoveEnemies();
                TryAccelerateEnemies();
                IncreaseShooterChance();

                if (isGameOver)
                    continue;

                renderer.Render(oldX, ship, renderedObjects); // теперь Ship передаётся
            }
        }

        void Init()
        {
            ship = new Ship(screenWidth / 2, 2);  // создание корабля вместо координат
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
                ship.X = Math.Max(1, ship.X - 1); // теперь Ship вместо координат
            else if (info.Key == ConsoleKey.RightArrow)
                ship.X = Math.Min(screenWidth - 2, ship.X + 1);// теперь Ship вместо координат
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
                    Bullet bullet = new Bullet(ship.X, ship.Y, direction: 1); // теперь Ship вместо координат
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
                        // Добавляем очки за уничтоженного врага
                        // Стоимость врага определяется свойством Cost: стрелок даёт 2 очка, обычный враг — 1 очко
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

        // новый метод для пуль врагов 
        void MoveEnemyBullets() 
        {
            //пули врагов
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

                Bullet bullet = enemy.TryShoot(enemyBullets); // логика стрельбы врагов
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
                enemySpeed = Math.Min(enemySpeed + enemyDeltaSpeed, maxEnemySpeed); // ограничиваем скорость
            }
        }

        // новый метод для роста вероятности стрелков 
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

            //вероятность стрелка растёт со временем, но ограничена            
            bool isShooter = rnd.Next(0, 100) < Math.Min(shooterRateAmount + shooterTicks / shooterIncreaseInterval, shooterMaxChance); 
            Enemy enemy = new Enemy(posIndex + 1, screenHeight - 1, enemySpeed, isShooter); // передается флаг стрелка

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
            shooterTicks = 0; // сброс тика

            // очистка объектов
            ClearObjects(enemies);
            ClearObjects(bullets);
            ClearObjects(enemyBullets);        

            renderer.ClearBoard(true);
            renderer.UpdateScore(score);

            CreateEnemies();
        }

        // новый общий метод для очистки объектов
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

        public void DrawFirstFrame(Ship ship, Enemy[] enemies) // теперь Ship передается вместо координат
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

        public void Render(int oldShipX, Ship ship, List<GameObject> gameObjects)  // теперь Ship передается вместо координат
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
        public override char Symbol => direction > 0 ? '^' : '*'; // Если пуля движется вверх (игрок), символ '^', если вниз (враг), символ '*'      

        int direction; //  Направление движения: 1 — вверх, -1 — вниз
       
        public Bullet(int x, int y, int direction) : base(x, y)
        {          
            this.direction = direction; //Конструктор получает начальные координаты и направление пули
        }

        public BulletMoveResult Move(int screenHeight, GameObject aim)
        {
            OldY = Y;
            Y += direction; // Изменяем позицию по вертикали в зависимости от направления
          
            if (aim != null && aim.X == X && (Y - aim.Y) * direction >= 0) // Проверка попадания по цели. Сравниваем направление и координаты              
                return BulletMoveResult.Hit;

            if (Y > screenHeight - 1 || Y < 2) // Проверка выхода пули за границы экрана               
                return BulletMoveResult.OutOfBounds;

            return BulletMoveResult.None;
        }
    }
  
    class Enemy : GameObject
    {
        public override char Symbol => isShooter ? '&' : '@'; // Стрелок отображается символом '&', обычный враг '@'

        // Стоимость врага в очках при уничтожении.
        // Если враг стреляет (isShooter == true), он ценнее и даёт 2 очка.
        // Если враг обычный, даёт 1 очко.
        public int Cost => isShooter ? 2 : 1;

        float enemySpeed;
        float slowY;

        bool isShooter; // Определяет, умеет ли враг стрелять
     
        private int shootTick = 0; // Счётчик тиков для контроля частоты выстрела
      
        const int rateOfFire = 12; // Количество игровых тиков между выстрелами врага        

        public Enemy(int x, int y, float startSpeed, bool isShooter) : base(x, y)
        {           
            enemySpeed = startSpeed;
            slowY = y + enemySpeed;
            this.isShooter = isShooter; // Сохраняем информацию, стрелок или нет           
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

        // Логика стрельбы врага
        public Bullet TryShoot(Bullet[] enemyBullets)
        {          
            if (!isShooter)
                return null;

            shootTick++;

            //Если достигнут интервал выстрела и нет пули на этой вертикали, стреляем
            if (shootTick >= rateOfFire && Y > 2 && enemyBullets[X - 1] == null)
            {               
                shootTick = 0;
                return new Bullet(X, Y - 1, direction: -1); // Пуля врага движется вниз              
            }
            return null;
        }
    }

    // Новый класс для игрока, используется как объект вместо координат
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