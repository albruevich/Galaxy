// ❗ Для переключения между уроками смотрите Program.cs

// Урок 3
// Учим корабль стрелять (пока только одной пулей за раз).
// Создаем новый класс GameObject.

using System;
using System.Text;

namespace Lesson_3
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2;
        bool isGameRunning = true;

        Renderer renderer;

        GameObject bullet; // новая переменная для пули игрока (пока только одна пуля одновременно)

        public void Run()
        {
            Init();
            renderer.BuildBoard();
            renderer.DrawFirstFrame(shipX, shipY);

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                MovePlayerBullet(); // движение пули вверх

                renderer.Render(oldX, shipX, shipY, bullet); // отрисовка корабля теперь вместе с пулей
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
            else if (info.Key == ConsoleKey.UpArrow && bullet == null) //если нажата стрелка вверх
                bullet = new GameObject(shipX, shipY); // создаём новую пулю на позиции корабля, если пули сейчас нет
        }

        void MovePlayerBullet()
        {
            if (bullet == null) //еслии пули нет, то выходим из метода
                return;

            bullet.OldY = bullet.Y; // запоминаем старую Y для правильной очистки символа

            bullet.Y++; // двигаем пулю вверх. +1 работает "вверх" благодаря вычислению индекса в FindIndex

            //проверка, не достигла ли пуля потолка
            if (bullet.Y > screenHeight - 1)
            {
                renderer.ClearBullet(bullet); // стираем пулю, чтобы не оставалось символа на экране
                bullet = null; // пуля больше не существует, можно выстрелить новую
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
        const char bulletChar = '^'; // символ пули

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

        public void Render(int oldShipX, int newShipX, int shipY, GameObject bullet)
        {
            builder[FindIndex(oldShipX, shipY)] = emptyChar;
            builder[FindIndex(newShipX, shipY)] = shipChar;

            if (bullet != null)
            {
                ClearBullet(bullet); // стираем старую позицию пули               
                builder[FindIndex(bullet.X, bullet.Y)] = bulletChar; // рисуем пулю на новой позиции
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        // ❗ Почему Replace с индексом, а не builder[...]:
        // Если пуля была на той же позиции, что корабль, builder[...] = emptyChar
        // то затирается символ корабля (#). А Replace с индексом меняет только символ пули (^)
        public void ClearBullet(GameObject bullet) => builder.Replace(bulletChar, emptyChar, FindIndex(bullet.X, bullet.OldY), 1);

        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY;
            int width = screenWidth + 1;
            return valX + y * width;
        }
    }

    //Новый класс GameObject
    class GameObject
    {
        public int Y { get; set; }   //  координата Y 
        public int X { get; set; }   // координата X 
        public int OldY { get; set; } // предыдущая Y для очистки символа      

        // конструктор 
        public GameObject(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
