// ❗ Для переключения между уроками смотрите Program.cs

// Урок 2
// Рисуем квадратное игровое поле.
// Добавляем новую ответственность за отрисовку - класс Renderer.

using System;
using System.Text;

namespace Lesson_02
{
    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12; //высота игрового экрана
        int shipX;
        const int shipY = 2; // фиксированная высота корабля
        bool isGameRunning = true;

        //Renderer - объект, который отвечает только за отрисовку.
        //Game теперь хранит только логику (позиция корабля, ввод, цикл), а Renderer — рисует поле и корабль
        //это разделение ответственности
        Renderer renderer;

        public void Run()
        {
            Init();
            renderer.BuildBoard(); //теперь Renderer рисует вместо Game
            renderer.DrawFirstFrame(shipX, shipY); //теперь Renderer рисует вместо Game

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                renderer.Render(oldX, shipX, shipY); //теперь Renderer рисует вместо Game
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight); // создаём Renderer (вызывается конструктор класса Renderer)
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
        }
    }

    // Класс Renderer, отвечает за всё, что связано с отрисовкой
    class Renderer
    {
        int screenWidth; // копируется из Game, чтобы Renderer был независимым и сам мог строить поле и считать индексы
        int screenHeight; // копируется из Game, чтобы Renderer был независимым и сам мог строить поле и считать индексы

        StringBuilder builder; // теперь builder относится к Renderer

        const char dotChar = '.';
        const char shipChar = '#';
        const char wallChar = '|'; //символ стены
        const char emptyChar = ' '; //пустой символ

        //конструктор класса Renderer, вызывается при создании объекта через new
        public Renderer(int width, int height)
        {
            //присваиваются значения, переданные из Game
            screenWidth = width;
            screenHeight = height;

            //теперь builder создается здесь
            builder = new StringBuilder();
        }

        // Создание игрового поля с потолком, полом и стенами
        public void BuildBoard()
        {
            for (int bY = 0; bY < screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                        builder.Append(dotChar); // потолок и пол
                    else if (bX == 0 || bX == screenWidth - 1)
                        builder.Append(wallChar); // стены
                    else
                        builder.Append(emptyChar); // пустое пространство
                }
                builder.Append('\n'); // переход на новую строку
            }
        }

        public void DrawFirstFrame(int shipX, int shipY)
        {
            builder[FindIndex(shipX, shipY)] = shipChar;
            Console.WriteLine(builder);
        }

        public void Render(int oldX, int newX, int y)
        {
            builder[FindIndex(oldX, y)] = emptyChar; // стираем старую позицию, основываясь на высчитанный индекс
            builder[FindIndex(newX, y)] = shipChar;  // рисуем новую позицию, основываясь на высчитанный индекс

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        //высчитываем индекс элемента в билдере 
        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY; //отступ сверху до игрового объекта 
            int width = screenWidth + 1; //ширины экрана + 1 (добавляем 1, так как в каждом ряду есть невидимый '\n')
            return valX + y * width; //находим индекс корабля 
        }
    }
}
