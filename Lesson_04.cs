// ❗ Для переключения между уроками смотрите Program.cs

// Урок 4
// Учим корабль стрелять несколькими пулями, создаем массив пуль.
// Переходим от управления одним объектом (пули) к управлению коллекцией объектов.

using System;
using System.Text;

namespace Lesson_04
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2;
        bool isGameRunning = true;

        Renderer renderer;

        // Теперь вместо одной пули храним массив пуль. Размер массива ограничивает максимальное количество пуль одновременно.
        // Пуль будет не больше чем высота экрана, минус пол и потолок
        GameObject[] bullets = new GameObject[screenHeight - 2];

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            renderer.DrawFirstFrame(shipX, shipY);

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                MovePlayerBullets(); // Метод теперь двигает ВСЕ пули, а не одну.               

                renderer.Render(oldX, shipX, shipY, bullets); // Передаем в Renderer массив пуль вместо одной пули.
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight);
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
                // Раньше мы просто проверяли bullet == null.
                // Теперь нужно найти свободную ячейку в массиве.
                int emptyIndex = -1;

                //Проходим по массиву и ищем null — это свободное место для новой пули.
                for (int i = 0; i < bullets.Length; i++)
                    if (bullets[i] == null) // если по текущему индексу i нет пули значит...
                    {
                        emptyIndex = i; // ... пустой индекс найден
                        break; //выход из массива, так как пустой индекс найден
                    }

                if (emptyIndex != -1) // если пустой индекс найден, то создаем пулю
                {
                    GameObject bullet = new GameObject(shipX, shipY); //создаем объект пули
                    bullets[emptyIndex] = bullet; //устанавливаем ее в массив, nеперь пули могут существовать параллельно.                    
                }
            }
        }

        void MovePlayerBullets()
        {
            // Идем с конца массива к началу. Это безопасный подход, если внутри цикла мы удаляем элементы (bullets[i] = null).            
            // При обратном проходе мы не рискуем пропустить элементы. 
            // Это хорошая привычка при работе с коллекциями, где возможны удаления.
            for (int i = bullets.Length - 1; i >= 0; i--)
            {
                GameObject bullet = bullets[i];

                if (bullet == null)
                    continue; //если пули нет, то пропускаем этот виток цикла

                bullet.OldY = bullet.Y;

                bullet.Y++;

                if (bullet.Y > screenHeight - 1)
                {
                    renderer.ClearBullet(bullet);
                    bullets[i] = null; // Теперь мы удаляем конкретную пулю из массива, освобождая место для будущих выстрелов.
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
        const char bulletChar = '^';

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

        public void DrawFirstFrame(int shipX, int shipY)
        {
            builder[FindIndex(shipX, shipY)] = shipChar;
            Console.WriteLine(builder);
        }

        public void Render(int oldShipX, int newShipX, int shipY, GameObject[] bullets)
        {
            builder[FindIndex(oldShipX, shipY)] = emptyChar;
            builder[FindIndex(newShipX, shipY)] = shipChar;

            // foreach — это способ пройти по всем элементам массива, не используя индекс           
            // Каждый элемент массива bullets по очереди попадает в переменную bullet. Это делает код чище и понятнее, если индекс не нужен            
            foreach (var bullet in bullets)
                if (bullet != null)
                {
                    ClearBullet(bullet);
                    builder[FindIndex(bullet.X, bullet.Y)] = bulletChar;
                }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        public void ClearBullet(GameObject bullet) => builder.Replace(bulletChar, emptyChar, FindIndex(bullet.X, bullet.OldY), 1);

        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY;
            int width = screenWidth + 1;
            return valX + y * width;
        }
    }

    class GameObject
    {
        public int Y { get; set; }
        public int X { get; set; }
        public int OldY { get; set; }

        public GameObject(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
