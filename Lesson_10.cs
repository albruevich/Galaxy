// ❗ Для переключения между уроками смотрите Program.cs


// Урок 10 — серьёзный шаг вперёд в архитектуре игры.

// Игровые объекты теперь цветные.

// Проведён рефакторинг рендерера:
// Render упрощён, удалены ClearGameObject, FindIndex, DrawFirstFrame и OldY у игровых объектов.
// StringBuilder теперь используется только для отрисовки стен и фона.

// Добавлен рекорд (HiScore), который сохраняется на компьютере.
// В этом уроке знакомимся с работой с файлами и делаем игру более завершённой.

using System;
using System.Text;
using System.Collections.Generic;
using System.IO; // подключено пространство имён для работы с файлами 

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

        int hiScore = 0; // хранение рекорда в игре

        Renderer renderer;

        Ship ship; 

        Bullet[] bullets = new Bullet[screenHeight - 2];
        Enemy[] enemies = new Enemy[screenWidth - 2];
        Bullet[] enemyBullets = new Bullet[screenWidth - 2];  

        Random rnd = new Random();

        FileManager fileManager; // менеджер для работы с файлом рекорда

        List<GameObject> renderedObjects = new List<GameObject>();

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            // Render теперь принимает score и hiScore
            renderer.Render(renderedObjects, score, hiScore);
            
            // больше нет DrawFirstFrame и UpdateScore

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

                // единая отрисовка кадра без oldX и Ship
                renderer.Render(renderedObjects, score, hiScore);
            }
        }

        void Init()
        {
            ship = new Ship(screenWidth / 2, 2);
            renderedObjects.Add(ship); // корабль теперь сразу добавляется в общий список объектов
            renderer = new Renderer(screenWidth, screenHeight);
            enemySpeed = startEnemySpeed;

            fileManager = new FileManager(); // создаём менеджер файлов
            hiScore = fileManager.LoadHiScore(); // загрузка рекорда при старте игры

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
                        GameOver(); // теперь вызов метода вместо isGameOver = true; 
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
                    GameOver(); // теперь вызов метода вместо isGameOver = true; 

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

        // новый метод для Game Over 
        void GameOver()
        {
            isGameOver = true;

            if(score > hiScore)
            {
                hiScore = score;
                renderer.PrintGameOver(newHiScore: hiScore); // возможность передать новый рекорд в рендерер
                fileManager.SaveHiScore(hiScore); // сохранение рекорда в файл
            }
            else
                renderer.PrintGameOver();
        }
    }

    class Renderer
    {
        int screenWidth;
        int screenHeight;

        // билдер теперь только как фон: стены и пустоты
        // он больше не рисует игровые объекты и тексты
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
            //рисуются стены и фон, таким образом очищается экран от всех старых игровых объектов и текстов
            Console.SetCursorPosition(0, 0);
            Console.Write(backgroundBuilder);

            // builder больше не содержит объекты, только фон --
            // больше нет FindIndex и ClearGameObject --

            // объекты теперь рисуются напрямую в консоль
            foreach (var go in gameObjects)
                DrawColoredObject(go);            

            // рисуется счет
            DrawText($"score: {score}", 0, 0, ConsoleColor.DarkYellow);

            // отображение рекорда в интерфейсе
            if (hiScore > 0)
            {
                string hiStr = $"hi: {hiScore}";
                DrawText(hiStr, screenWidth - hiStr.Length, 0, ConsoleColor.DarkYellow);               
            }

            Console.ResetColor();
            Console.SetCursorPosition(0, screenHeight + 2);
        }

        // Рисуем объект напрямую в консоль, минуя StringBuilder
        void DrawColoredObject(GameObject go)
        {
            Console.SetCursorPosition(go.X, screenHeight - go.Y); // Позиционируем курсор вручную (инверсия Y)            
            Console.ForegroundColor = go.Color; // Используем цвет, сохранённый в объекте          
            Console.Write(go.Symbol); // Рисуем символ поверх уже выведенного экрана (фона)
        }

        public void PrintGameOver(int newHiScore = 0)
        {
            // очистка только внутренней области, не трогая рамку
            for (int x = 1; x < screenWidth - 1; x++)
                for (int y = 1; y < screenHeight - 1; y++)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(emptyChar);
                }

            // смещение текста динамически меняется при новом рекорде
            int deltaG = 1;
            int deltaS = -1;

            // если есть новый рекорд — выводим дополнительную строку
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
            
            Console.SetCursorPosition(0, screenHeight + 2); // Возврат курсора вниз после отрисовки
        }

        public void DrawText(string text, int x, int y, ConsoleColor color = ConsoleColor.White)
        {
            // текст больше не записывается в StringBuilder
            // теперь текст можно рисовать в любом цвете

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

        // нет больше oldY так как при рендерере сперва затирается весь экран
        // через builder (рисуются тольк стены и пустоты)
        // а затем отрисовывается каждый цветной символ

        public abstract char Symbol { get; }

        // добавлено свойство Color для поддержки цветных объектов
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

        // разные цвета для пуль игрока и врага
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

        // стрелки теперь визуально отличаются цветом
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

        // корабль теперь цветной 
        public override ConsoleColor Color => ConsoleColor.Cyan;

        public Ship(int x, int y) : base(x, y) { }
    }

    // новый класс для работы с файлом рекорда
    class FileManager
    {
        // путь к файлу рекорда
        // использование системной папки ApplicationData
        static readonly string HiScorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Galaxy", "hiscore.txt");

        // загрузка рекорда
        public int LoadHiScore()
        {
            int hiScore = 0;

            try // Попытка прочитать файл с рекордом
            {
                // Проверяем, существует ли файл
                if (File.Exists(HiScorePath))
                {
                    // Если файл есть, пробуем прочитать число
                    if (int.TryParse(File.ReadAllText(HiScorePath), out int value))
                        hiScore = value;
                }
                // Если файла нет — это нормально, просто начинаем с рекордом 0
            }
            catch (Exception ex) // Ловим реальные ошибки чтения, не связанные с отсутствием файла
            {                              
                Console.WriteLine($"Error: {ex.Message}");               
            }

            return hiScore;
        }

        // Метод для сохранения рекорда игрока в файл
        public void SaveHiScore(int hiScore)
        {
            try // Начинаем блок try — здесь мы "пытаемся" выполнить код, который может вызвать ошибку
            {
                // Получаем путь к папке, где будет храниться файл рекорда
                string dir = Path.GetDirectoryName(HiScorePath);

                // Проверяем, существует ли эта папка. Если нет — создаём её                
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Записываем рекорд в файл в виде текста. Если файла нет — он будет создан автоматически                
                File.WriteAllText(HiScorePath, hiScore.ToString());
            }
            catch (Exception ex) // Блок catch "ловит" любые ошибки, возникшие в try
            {
                // Если что-то пошло не так (например, нет прав на запись), выводим сообщение об ошибке                               
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
