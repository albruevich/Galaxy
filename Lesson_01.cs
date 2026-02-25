// ❗ Для переключения между уроками смотрите Program.cs

// Урок 1
// Рисуем нижнюю строку игрового экрана и корабль.
// Реализуем управление кораблем влево и вправо.
// Создаём базовую структуру проекта (стартовую архитектуру) для будущих уроков.

using System;   // подключаем базовые классы .NET, включая Console, Math, Random и т.д.
using System.Text;   // подключаем классы для работы с текстом, например StringBuilder для построения игрового экрана

namespace Lesson_01
{
    // Основной класс, в котором реализуется вся логика игры:
    // управление, обработка действий игрока, движение объектов и обновление экрана.
    class Game
    {
        const int screenWidth = 21; //ширина игрового экрана
        int shipX; //позиция корабля по горизонтали     
        bool isGameRunning = true; // флаг работы игры

        const char dotChar = '.'; //символ потолка и пола
        const char shipChar = '#'; //символ корабля

        StringBuilder builder; // используем, чтобы быстро менять символы на экране без создания новых строк

        public void Run()
        {
            Init();
            BuildBoard();
            DrawFirstFrame();

            //игровой цикл
            while (isGameRunning)
            {
                //запоминаем старое положение корабля для отрисовки предыдущей позиции
                int oldX = shipX;

                HandleInput();
                Render(oldX);
            }
        }

        void Init()
        {
            // ставим корабль по центру, чтобы игрок сразу видел его на экране 
            shipX = screenWidth / 2;

            //создание основной строки для распечатки; сперва в нем нет символов, он пустой    
            builder = new StringBuilder();
        }

        void BuildBoard()
        {
            // заполняем строку точками
            for (int i = 0; i < screenWidth; i++)
                builder.Append(dotChar);
        }

        void DrawFirstFrame()
        {
            //рисуем корабль на старте           
            builder[shipX] = shipChar;

            //распечатка самого первого кадра
            Console.WriteLine(builder);
        }

        void HandleInput()
        {
            // ждём нажатия клавиши и считываем инфо о нажатой клавише
            ConsoleKeyInfo info = Console.ReadKey(true);

            // выходим из игры
            if (info.Key == ConsoleKey.Escape)
            {
                isGameRunning = false;
                return;
            }

            // двигаем влево и ограничиваем, чтобы Х не был меньше 0
            if (info.Key == ConsoleKey.LeftArrow)
                shipX = Math.Max(0, shipX - 1);

            // двигаем вправо и ограничиваем, чтобы Х не был больше ширины поля 
            else if (info.Key == ConsoleKey.RightArrow)
                shipX = Math.Min(screenWidth - 1, shipX + 1);
        }

        void Render(int oldX)
        {
            builder[oldX] = dotChar;  // стираем корабль с прошлой позиции
            builder[shipX] = shipChar; // рисуем корабль на новой позиции     

            Console.SetCursorPosition(0, 0); // ставим курсор в начало, чтобы перерисовать экран            
            Console.WriteLine(builder); // рисуем новый экран
        }
    }
}
