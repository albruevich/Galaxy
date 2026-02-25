// ❗ Для переключения между уроками смотрите Program.cs

// Урок 7
// Теперь несколько врагов одновременно (массив enemies) с распределением по колонкам.
// Непрерывная генерация новых врагов после уничтожения (enemySpawns).

using System;
using System.Text;
using System.Collections.Generic;

namespace Lesson_7
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2;
        bool isGameRunning = true;
        bool isGameOver = false;
        const int enemyAmount = 5; // ограничение стартового количества врагов      
        int enemySpawns = 0; //счетчик отложенного респавна врагов после уничтожения       

        Renderer renderer;

        Bullet[] bullets = new Bullet[screenHeight - 2];
        Enemy[] enemies = new Enemy[screenWidth - 2]; // теперь враги распределяются по колонкам экрана       

        Random rnd = new Random();

        List<GameObject> renderedObjects = new List<GameObject>();

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            renderer.DrawFirstFrame(shipX, shipY, enemies); // метод теперь принимает массив врагов вместо одного          

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                TryRespawnEnemy(); // отдельный этап обработки отложенного спавна               
                MovePlayerBullets();
                MoveEnemies();

                if (isGameOver)
                    continue;

                renderer.Render(oldX, shipX, shipY, renderedObjects);
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight);

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

                Enemy sameColumnEnemy = null; //поиск врага в той же колонке, что и пуля              
                int enemyIndex = -1; // индекс врага для удаления из массива              

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

                BulletMoveResult result = bullet.Move(screenHeight, sameColumnEnemy); //пуля проверяет попадание в конкретного врага

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
                        enemies[enemyIndex] = null; //удаление врага из массива                        

                        enemySpawns++; //увеличиваем счетчик респавна

                    }
                }
            }
        }

        void MoveEnemies()
        {
            if (isGameOver)
                return;

            foreach (var enemy in enemies) //перебор массива врагов
            {
                if (enemy == null)
                    continue;

                if (enemy.Move(shipX, shipY)) //GameOver может вызвать любой враг 
                    isGameOver = true;               

                if (isGameOver)
                {
                    renderer.PrintGameOver();
                    break; //если isGameOver то прерываем цикл
                }
            }
        }

        void CreateEnemies()
        {
            // цикл начального создания нескольких врагов
            for (int i = 0; i < enemyAmount; i++)
                CreateEnemy();
        }

        void TryRespawnEnemy() //механизм отложенного респавна
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
            // 1. Создаем список свободных колонок
            // В массиве enemies каждый индекс соответствует одной колонке на игровом поле.
            // Если enemies[i] == null, значит в этой колонке нет врага, и туда можно поставить нового.
            List<int> freePositions = new List<int>();

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null)
                    freePositions.Add(i);  // добавляем индекс пустой колонки в список свободных
            }

            // 2. Проверка, есть ли свободные позиции
            if (freePositions.Count == 0)
                return; // если нет свободных колонок, не создаем врага, чтобы не перекрывать существующих

            // 3. Выбираем случайную свободную колонку
            int posIndex = freePositions[rnd.Next(freePositions.Count)];

            // 4. Создаем нового врага           
            Enemy enemy = new Enemy(posIndex + 1, screenHeight - 1);

            // 5. Сохраняем врага в массиве и списке рендеринга
            enemies[posIndex] = enemy;
            renderedObjects.Add(enemy);
        }

        void Restart()
        {
            isGameOver = false;
            shipX = screenWidth / 2;

            for (int i = 0; i < enemies.Length; i++)
            {
                // в прошлом уроке удалялся только один враг,
                // теперь проходим по массиву всех врагов и удаляем их из списка рендеринга
                if (enemies[i] != null)
                    renderedObjects.Remove(enemies[i]);

                enemies[i] = null; // обнуляем каждый элемент массива, чтобы старые ссылки на врагов не оставались
            }

            for (int i = 0; i < bullets.Length; i++)
            {
                if (bullets[i] != null)
                    renderedObjects.Remove(bullets[i]);

                bullets[i] = null;
            }

            renderer.ClearBoard();

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
            ClearBoard();

            const string text = "GAME OVER";
            DrawText(text, screenWidth / 2 - text.Length / 2, screenHeight / 2 + 1);
            const string text2 = "SPACE TO PLAY";
            DrawText(text2, screenWidth / 2 - text2.Length / 2, screenHeight / 2 - 1);
        }

        public void ClearBoard()
        {
            for (int i = 0; i < builder.Length; i++)
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

        const float enemySpeed = .12f;

        float slowY;

        public Enemy(int x, int y) : base(x, y)
        {
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
