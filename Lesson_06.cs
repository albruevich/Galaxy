// ❗ Для переключения между уроками смотрите Program.cs

// Урок 6
// Враг теперь движется сам со своей скоростью.
// Появляется состояние Game Over и возможность перезапуска игры. 

using System;
using System.Text;

// подключаем коллекции .NET, такие как List, Dictionary, Queue и другие, для хранения и управления объектами игры
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

        bool isGameOver = false; // флаг состояния поражения (игра остановлена, но программа не завершена)

        Renderer renderer;

        Bullet[] bullets = new Bullet[screenHeight - 2];

        Random rnd = new Random();

        Enemy enemy = null;

        // общий список всех игровых объектов для универсального рендеринга
        // List автоматически расширяется и сжимается, поэтому с ним не нужно вручную пересоздавать массив и копировать данные.
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
                MoveEnemies(); // движение врагов вынесено в отдельный метод

                // если игра окончена, то останавливаем рендеринг игровых объектов
                if (isGameOver)
                    continue;

                renderer.Render(oldX, shipX, shipY, renderedObjects);
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight);

            CreateEnemies(); // создание врагов вынесено в отдельный метод
        }

        void HandleInput()
        {
            ConsoleKeyInfo info = Console.ReadKey(true);

            if (info.Key == ConsoleKey.Escape)
            {
                isGameRunning = false;
                return;
            }

            // если игра окончена — разрешаем только рестарт
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

                    renderedObjects.Add(bullet); //добавляем объект в список
                }
            }
        }

        void MovePlayerBullets()
        {
            // при Game Over пули больше не двигаются
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

                    renderedObjects.Remove(bullet); // удаляем объект из списка

                    if (result == BulletMoveResult.Hit)
                    {
                        // корректная очистка врага с учётом OldY
                        enemy.OldY = enemy.Y;
                        renderer.ClearGameObject(enemy);

                        renderedObjects.Remove(enemy); //удаляем объект из списка
                        enemy = null;
                    }
                }
            }
        }

        // движение врагов вынесено в отдельный метод
        void MoveEnemies()
        {
            // при Game Over враги больше не двигаются
            if (isGameOver)
                return;

            if (enemy != null)
            {
                // метод Move врага теперь возвращает true при поражении
                isGameOver = enemy.Move(shipX, shipY);

                //если конец игры, то отображаем это на экране
                if (isGameOver)
                    renderer.PrintGameOver();
            }
        }

        // создание врагов вынесено в отдельный метод (подготовка к массиву врагов)
        void CreateEnemies()
        {
            int enemyX = rnd.Next(1, screenWidth - 1);
            int enemyY = screenHeight - 1;
            enemy = new Enemy(enemyX, enemyY);
            renderedObjects.Add(enemy);
        }

        // полный рестарт игры без перезапуска программы
        void Restart()
        {
            isGameOver = false;
            shipX = screenWidth / 2;

            //удаляем объект из списка рендеринга
            if (enemy != null)
                renderedObjects.Remove(enemy);

            enemy = null; //обнуляем врага

            for (int i = 0; i < bullets.Length; i++)
            {
                //удаляем объект из списка рендеринга
                if (bullets[i] != null)
                    renderedObjects.Remove(bullets[i]);

                // обнуляем каждый элемент массива, чтобы старые ссылки на пули не оставались
                bullets[i] = null;
            }

            renderer.ClearBoard(); // очистка экрана от всех объектов кроме рамки: снен, пола и потолка

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

        // Renderer теперь работает с универсальным списком GameObject
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

        // вывод экрана поражения
        public void PrintGameOver()
        {
            ClearBoard();

            const string text = "GAME OVER";
            DrawText(text, screenWidth / 2 - text.Length / 2, screenHeight / 2 + 1);
            const string text2 = "SPACE TO PLAY";
            DrawText(text2, screenWidth / 2 - text2.Length / 2, screenHeight / 2 - 1);
        }

        // очистка игрового поля без разрушения стен
        public void ClearBoard()
        {
            for (int i = 0; i < builder.Length; i++)
            {
                char c = builder[i];
                if (c != wallChar && c != dotChar && c != '\n')
                    builder[i] = emptyChar;
            }
        }

        // универсальный вывод текста на экран
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

        // скорость врага меньше 1 клетки за кадр
        const float enemySpeed = .12f;

        // slowY — позиция врага с плавающей точкой для плавного движения (мньше 1 клетки за кадр).
        float slowY;

        public Enemy(int x, int y) : base(x, y)
        {
            // Начинаем slowY немного выше целой позиции Y,
            // чтобы первое смещение произошло не сразу на целую клетку,
            // а постепенно — это делает движение врага визуально плавным.
            slowY = y + enemySpeed;
        }

        // метод движения врага теперь возвращает true при поражении игрока
        public bool Move(int shipX, int shipY)
        {
            OldY = Y;
            slowY = Math.Max(slowY - enemySpeed, 1);

            Y = (int)slowY;

            // проверка на поражение, не достиг ли враг пола
            // а также нет ли коллиции с кораблем игрока
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