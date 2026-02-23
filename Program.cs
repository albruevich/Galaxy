// Игра, в которой корабль стреляет по надвигающимся врагам
// Управление: стрелки влево/вправо - перемещение, стрелка вверх - выстрел

// Занятия разбиты на уроки
// Для прохождения урока нужно раскомментировать нужный #define LESSON_1
// Все остальные уроки должны быть закомментированы

// Каждый следующий урок — это продолжение предыдущего

// Все поясняющие комментарии относятся только к новому уроку
// В новом уроке комментарии из прошлых уроков отсутствуют
// Таким образом, все новое в текущем уроке имеет поясняющие комментарии


//#define LESSON_1
//#define LESSON_2
//#define LESSON_3
#define LESSON_4
//#define LESSON_5
//#define LESSON_6
//#define LESSON_7
//#define LESSON_8
//#define LESSON_9
//#define LESSON_10
//#define LESSON_11

using System;
using System.Text;

namespace Galaxy
{

#if LESSON_1 // рисуем самую первую (нижнюю строку) игрового экрана, рисуем корабль, управляем кораблем влево и вправо

    class MainClass
    {      
        public static void Main()
        {
            // Создаем игру и запускаем её
            new Game().Run();  
        }       
    }

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


#elif LESSON_2 //рисуем квадратное игровое поле, добавляем новую ответственность за отрисовку - класс Renderer

    class MainClass
    {
        public static void Main() => new Game().Run();
    }

    // Класс игры, отвечает за логику: позицию корабля, ввод и игровой цикл
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

#elif LESSON_3 // учим корабль стрелять (пока только одной пулей за раз)

    class MainClass { public static void Main() => new Game().Run(); }

    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2;
        bool isGameRunning = true;

        Renderer renderer;

        Bullet bullet; // новая переменная для пули игрока (пока только одна пуля одновременно)

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
                bullet = new Bullet(shipX, shipY); // создаём новую пулю на позиции корабля, если пули сейчас нет
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

        public void Render(int oldShipX, int newShipX, int shipY, Bullet bullet)
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
        public void ClearBullet(Bullet bullet) => builder.Replace(bulletChar, emptyChar, FindIndex(bullet.X, bullet.OldY), 1);

        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY;
            int width = screenWidth + 1;
            return valX + y * width;
        }
    }

    //Новый класс пули
    class Bullet
    {
        public int Y { get; set; } //  координата Y пули
        public int X { get; }       // координата X пули (фиксирована на момент выстрела)
        public int OldY { get; set; } // предыдущая Y пули для очистки символа      

        // конструктор пули
        public Bullet(int x, int y)
        {
            X = x;
            Y = y;
        }
    }


#elif LESSON_4 //Учим корабль стрелять несколькими пулями, создаем массив пуль
               //Переходим от управления одним объектом (пули) к управлению коллекцией объектов.

    class MainClass { public static void Main() => new Game().Run(); }

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
        Bullet[] bullets = new Bullet[screenHeight - 2];        

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
                    Bullet bullet = new Bullet(shipX, shipY); //создаем объект пули
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
                Bullet bullet = bullets[i];

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

        public void Render(int oldShipX, int newShipX, int shipY, Bullet[] bullets)
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

        public void ClearBullet(Bullet bullet) => builder.Replace(bulletChar, emptyChar, FindIndex(bullet.X, bullet.OldY), 1);

        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY;
            int width = screenWidth + 1;
            return valX + y * width;
        }
    }

    class Bullet
    {
        public int Y { get; set; }
        public int X { get; }
        public int OldY { get; set; }

        public Bullet(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

#elif LESSON_5

    //УРОК 5
    //рождаем одного врага, попадание пули во врага, уничтожение пули после попадания, уничтожение врага   
    class MainClass
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        const int shipY = 2; //корабль всегда на 2-й кооординате по вертикали
        const char cell = ' ';
        const char ship = '#';
        const char dot = '.';
        const char line = '|';
        const char bullet = '^'; //пуля
        const char enemy = '@'; //враг 

        public static void Main()
        {
            //начальные установки
            //x - это позиция корабля по горизонтали
            int shipX = screenWidth / 2;

            //создаем массивы пуль                 
            int[] bulletX = new int[screenHeight - 2];
            int[] bulletY = new int[screenHeight - 2];

            //заполняем массивы стартовыми пустыми значениями   
            for (int i = 0; i < bulletX.Length; i++)
            {
                bulletX[i] = 0;
                bulletY[i] = 0;
            }

            Random rnd = new Random();                       
          
            //враг, задаем ему стартовые координаты
            int enemyX = rnd.Next(1, screenWidth); //случайный Х 
            int enemyY = screenHeight - 1; // Y под потолком 

            //создание основной строки для распечатки
            StringBuilder builder = new StringBuilder();

            //заполняем поле необходимыми символами: '.', ' ', '|'
            for (int bY = 0; bY < screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                    {
                        builder.Append(dot);
                    }
                    else
                    {
                        if (bX == 0 || bX == screenWidth - 1)
                            builder.Append(line);
                        else
                            builder.Append(cell);
                    }
                }

                builder.Append('\n');
            }

            //рисуем корабль на старте           
            builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1); 

            //рисуем врага на старте          
            builder.Replace(cell, enemy, CalculateCoords(enemyX, enemyY), 1);            

            //распечатка первого кадра
            Console.WriteLine(builder);

            //игровой цикл
            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();

                int oldX = shipX;

                if (info.Key == ConsoleKey.Escape)                
                    break;    

                if (info.Key == ConsoleKey.LeftArrow)
                {
                    shipX--;
                    shipX = Math.Max(1, shipX);
                }
                else if (info.Key == ConsoleKey.RightArrow)
                {
                    shipX++;
                    shipX = Math.Min(screenWidth - 2, shipX);
                }
                else if (info.Key == ConsoleKey.UpArrow)
                {
                    int emptyIndex = -1;

                    for (int i = 0; i < bulletX.Length; i++)
                    {
                        if (bulletX[i] == 0)
                        {
                            emptyIndex = i;
                            break;
                        }
                    }

                    if (emptyIndex != -1)
                    {
                        bulletX[emptyIndex] = shipX;
                        bulletY[emptyIndex] = shipY;
                    }
                }

                builder.Replace(ship, cell, CalculateCoords(oldX, shipY), 1); 
                builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);    

                for (int i = 0; i < bulletX.Length; i++)
                {
                    if (bulletX[i] != 0)
                    {
                        builder.Replace(bullet, cell, CalculateCoords(bulletX[i], bulletY[i]), 1); 

                        bulletY[i]++;

                        //проверка, попала ли пуля во врага (координаты совпадают)
                        bool bulletHitEnemy = bulletX[i] == enemyX && bulletY[i] == enemyY; 
                       
                        if (bulletY[i] > screenHeight - 1 || bulletHitEnemy) //удаление пули в 2-х случаях: когда она достигла потолка или попала по врагу
                        {
                            bulletX[i] = 0;
                            bulletY[i] = 0;

                            //если пуля попала то удаляем врага
                            if (bulletHitEnemy) 
                            {
                                //очищаем экран вот врага
                                builder.Replace(enemy, cell, CalculateCoords(enemyX, enemyY), 1); 

                                //обнуляем значения врага
                                enemyX = 0; 
                                enemyY = 0; 
                            }

                            continue;
                        }

                        //отрисовка пули
                        builder.Replace(cell, bullet, CalculateCoords(bulletX[i], bulletY[i]), 1);
                    }
                }

                //обновляем экран (очищаем его)
                Console.Clear();
                //и рисуем новый экран
                Console.WriteLine(builder);
            }
                   
            int CalculateCoords(int valX, int valY) 
            {
                int y = screenHeight - valY; //отступ сверху до игрового объекта
                int width = screenWidth + 1; //ширины экрана + 1 ( +1 - это прибавка , так как в конце каждой строки ..... есть еще символ \n, который тоже занимает индекс)
                return valX + y * width;    //находим индекс корабля: позиция корабля + высота * на ширину экрана
            }          
        }
    }

#elif LESSON_6
    
    //двигаем врага вниз с определенной скоростью, которая меньше чем 1 за кадр
    //проигрыш, если враг дошел до пола
    class MainClass
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        const int shipY = 2; //корабль всегда на 2-й кооординате по вертикали
        const char cell = ' ';
        const char ship = '#';
        const char dot = '.';
        const char line = '|';
        const char bullet = '^'; //пуля
        const char enemy = '@'; //враг 
        const float enemySpeed = .4f; 

        public static void Main()
        {
            //начальные установки
            //x - это позиция корабля по горизонтали
            int shipX = screenWidth / 2;

            //создаем массивы пуль                 
            int[] bulletX = new int[screenHeight - 2];
            int[] bulletY = new int[screenHeight - 2];

            //заполняем массивы стартовыми пустыми значениями   
            for (int i = 0; i < bulletX.Length; i++)
            {
                bulletX[i] = 0;
                bulletY[i] = 0;
            }

            Random rnd = new Random();

            //враг, задаем ему стартовые координаты
            int enemyX = rnd.Next(1, screenWidth); //случайный Х
            float enemyY = screenHeight - 1; // Y под потолком, теперь это float 

            //создание основной строки для распечатки
            StringBuilder builder = new StringBuilder();

            //заполняем поле необходимыми символами: '.', ' ', '|'
            for (int bY = 0; bY < screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                    {
                        builder.Append(dot);
                    }
                    else
                    {
                        if (bX == 0 || bX == screenWidth - 1)
                            builder.Append(line);
                        else
                            builder.Append(cell);
                    }
                }

                builder.Append('\n');
            }

            //рисуем корабль на старте        
            builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

            //рисуем врага на старте
            builder.Replace(cell, enemy, CalculateCoords(enemyX, (int)enemyY), 1);

            //распечатка первого кадра
            Console.WriteLine(builder);

            //игровой цикл
            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();

                int oldX = shipX;

                if (info.Key == ConsoleKey.Escape)                
                    break;    

                if (info.Key == ConsoleKey.LeftArrow)
                {
                    shipX--;
                    shipX = Math.Max(1, shipX);
                }
                else if (info.Key == ConsoleKey.RightArrow)
                {
                    shipX++;
                    shipX = Math.Min(screenWidth - 2, shipX);
                }
                else if (info.Key == ConsoleKey.UpArrow)
                {
                    int emptyIndex = -1;

                    for (int i = 0; i < bulletX.Length; i++)
                    {
                        if (bulletX[i] == 0)
                        {
                            emptyIndex = i;
                            break;
                        }
                    }

                    if (emptyIndex != -1)
                    {
                        bulletX[emptyIndex] = shipX;
                        bulletY[emptyIndex] = shipY;
                    }
                }

                
                //движение врага
                if (enemyX != 0) 
                {
                    int oldEnemyY = (int)enemyY; 
                    enemyY -= enemySpeed;       

                    //проверка на поражение, не достиг ли враг пола
                    if (enemyY < 2f) 
                    {
                        builder = new StringBuilder("GAME OVER"); 
                        Console.Clear();                          
                        Console.WriteLine(builder);   
                        break;                                   
                    }
                    
                    //если позиция врага изменилась, то перерисовываем его
                    else if (oldEnemyY != (int)enemyY)
                    {
                        builder.Replace(enemy, cell, CalculateCoords(enemyX, oldEnemyY), 1); 
                        builder.Replace(cell, enemy, CalculateCoords(enemyX, (int)enemyY), 1); 
                    }
                }

                builder.Replace(ship, cell, CalculateCoords(oldX, shipY), 1);
                builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

                for (int i = 0; i < bulletX.Length; i++)
                {
                    if (bulletX[i] != 0)
                    {
                        builder.Replace(bullet, cell, CalculateCoords(bulletX[i], bulletY[i]), 1);

                        bulletY[i]++;

                        //проверка, попала ли пуля во врага (координаты совпадают)
                        
                        //Abs - это модуль числа, то есть всегда положительное (знак минус отбрасывается)
                        //это нужно так как у нас enemyY это float и точно не сравнить, измеряем дистанцию между пулей и врагом по вертикали
                        bool bulletHitEnemy = bulletX[i] == enemyX && Math.Abs(bulletY[i] - enemyY) < 1f; 

                        if (bulletY[i] > screenHeight - 1 || bulletHitEnemy)
                        {
                            bulletX[i] = 0;
                            bulletY[i] = 0;

                            //если пуля попала то удаляем врага
                            if (bulletHitEnemy) 
                            {
                                //очищаем экран вот врага
                                builder.Replace(enemy, cell, CalculateCoords(enemyX, (int)enemyY), 1); 

                                //обнуляем значения врага
                                enemyX = 0; 
                                enemyY = 0;
                            }

                            continue;
                        }

                        builder.Replace(cell, bullet, CalculateCoords(bulletX[i], bulletY[i]), 1);
                    }
                }

                Console.Clear();
                Console.WriteLine(builder);
            }

            int CalculateCoords(int valX, int valY)
            {
                int y = screenHeight - valY; 
                int width = screenWidth + 1; 
                return valX + y * width;   
            } 
        }
    }

#elif LESSON_7

    // для пуль и врагов создаем классы вместо простых значений (начало "Объектно ориентируемого программирования")
    // игровой процес не изменился внешне, изменения только в коде, но это очень важно для обучения
    class MainClass
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        const int shipY = 2; 
        const char cell = ' ';
        const char ship = '#';
        const char dot = '.';
        const char line = '|';
        const char bullet = '^'; 
        const char enemyChar = '@'; 
        const float enemySpeed = .4f; 

        public static void Main()
        {                   
            int shipX = screenWidth / 2;

            
            //создаем массив пуль как массив объектов Bullet
            Bullet[] bullets = new Bullet[screenHeight - 2];           
           
            Random rnd = new Random();

            
            //создаем объект врага вместо отдельных переменных enemyX / enemyY
            Enemy enemy = new Enemy(rnd.Next(1, screenWidth), screenHeight - 1);            
            
            StringBuilder builder = new StringBuilder();
          
            for (int bY = 0; bY < screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                    {
                        builder.Append(dot);
                    }
                    else
                    {
                        if (bX == 0 || bX == screenWidth - 1)
                            builder.Append(line);
                        else
                            builder.Append(cell);
                    }
                }

                builder.Append('\n');
            }
                  
            builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

            
            //используем геттеры объекта enemy
            builder.Replace(cell, enemyChar, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
            
            Console.WriteLine(builder);

            //игровой цикл
            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();

                int oldX = shipX;

                if (info.Key == ConsoleKey.Escape)                
                    break;    

                if (info.Key == ConsoleKey.LeftArrow)
                {
                    shipX--;
                    shipX = Math.Max(1, shipX);
                }
                else if (info.Key == ConsoleKey.RightArrow)
                {
                    shipX++;
                    shipX = Math.Min(screenWidth - 2, shipX);
                }
                else if (info.Key == ConsoleKey.UpArrow)
                {
                    int emptyIndex = -1;

                    
                    //ищем пустую ячейку (null вместо 0)
                    for (int i = 0; i < bullets.Length; i++)
                    {
                        if (bullets[i] == null)
                        {
                            emptyIndex = i;
                            break;
                        }
                    }

                    if (emptyIndex != -1)
                    {
                        
                        //создаем объект пули
                        Bullet bulletObj = new Bullet(shipX, shipY);
                        bullets[emptyIndex] = bulletObj;                      
                    }
                }
               
                //движение врага                 
                if (enemy != null)  //проверяем врага, что он существует (не равен null)   NEW
                {
                    int oldEnemyY = (int)enemy.GetY;

                    
                    //двигаем врага через сеттер                    
                    enemy.SetY = enemy.GetY - enemySpeed;       
                   
                    if (enemy.GetY < 2f) 
                    {
                        builder = new StringBuilder("GAME OVER"); 
                        Console.Clear();                          
                        Console.WriteLine(builder);   
                        break;                                   
                    }                                    
                    else if (oldEnemyY != (int)enemy.GetY)
                    {
                        builder.Replace(enemyChar, cell, CalculateCoords(enemy.GetX, oldEnemyY), 1); 
                        builder.Replace(cell, enemyChar, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1); 
                    }
                }

                builder.Replace(ship, cell, CalculateCoords(oldX, shipY), 1);
                builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

                for (int i = 0; i < bullets.Length; i++)
                {
                    if (bullets[i] != null)
                    {
                        
                        //используем геттеры объекта пули
                        builder.Replace(bullet, cell, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);

                        
                        //двигаем пулю через сеттер
                        bullets[i].SetY = bullets[i].GetY + 1;

                        
                        //проверка попадания через объекты Bullet и Enemy
                        bool bulletHitEnemy = enemy != null ?
                            bullets[i].GetX == enemy.GetX && Math.Abs(bullets[i].GetY - enemy.GetY) < 1f :
                            false;

                        if (bullets[i].GetY > screenHeight - 1 || bulletHitEnemy)
                        {
                            
                            //удаляем пулю (null вместо обнуления координат)
                            bullets[i] = null;                          
                           
                            if (bulletHitEnemy) 
                            {                                
                                builder.Replace(enemyChar, cell, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);

                                
                                //удаляем врага как объект
                                enemy = null;
                            }

                            continue;
                        }

                        builder.Replace(cell, bullet, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);
                    }
                }

                Console.Clear();
                Console.WriteLine(builder);
            }
                   
             int CalculateCoords(int valX, int valY)
            {
                int y = screenHeight - valY; 
                int width = screenWidth + 1; 
                return valX + y * width;   
            } 
        }       
    }

    
    //класс пули
    public class Bullet
    {
        private int x;
        private int y;

        //геттеры           
        public int GetX => x;
        public int GetY => y;

        //сеттер           
        public int SetY { set { y = value; } }

        //конструктор           
        public Bullet(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    
    //класс врага
    public class Enemy
    {
        private int x;
        private float y;

        //геттеры           
        public int GetX => x;
        public float GetY => y;

        //сеттер            
        public float SetY { set { y = value; } }

        //конструктор           
        public Enemy(int x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }


#elif LESSON_8

    //создаем массив врагов вместо одного врага, непрерывная генерация врагов
    class MainClass
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        const int shipY = 2;
        const char cell = ' ';
        const char ship = '#';
        const char dot = '.';
        const char line = '|';
        const char bullet = '^';
        const char enemyChar = '@';
        const float enemySpeed = .18f;  изменена скорость врага (адаптация под несколько врагов)
        const int enemyAmount = 5;      количество одновременно создаваемых врагов

        public static void Main()
        {
            int shipX = screenWidth / 2;

            Bullet[] bullets = new Bullet[screenHeight - 2];
            Enemy[] enemies = new Enemy[screenWidth - 2];  массив врагов вместо одного объекта Enemy

            Random rnd = new Random();

            for (int i = 0; i < enemyAmount; i++)  цикл начального создания нескольких врагов
            {
                CreateEnemy();  метод генерации врага
            }

            StringBuilder builder = new StringBuilder();

            for (int bY = 0; bY < screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                    {
                        builder.Append(dot);
                    }
                    else
                    {
                        if (bX == 0 || bX == screenWidth - 1)
                            builder.Append(line);
                        else
                            builder.Append(cell);
                    }
                }

                builder.Append('\n');
            }

            builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

            foreach (var enemy in enemies)  отрисовка всех врагов
            {
                if (enemy != null)
                    builder.Replace(cell, enemyChar, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
            }

            Console.WriteLine(builder);

            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();

                int oldX = shipX;

                if (info.Key == ConsoleKey.Escape)                
                    break;    

                if (info.Key == ConsoleKey.LeftArrow)
                {
                    shipX--;
                    shipX = Math.Max(1, shipX);
                }
                else if (info.Key == ConsoleKey.RightArrow)
                {
                    shipX++;
                    shipX = Math.Min(screenWidth - 2, shipX);
                }
                else if (info.Key == ConsoleKey.UpArrow)
                {
                    int emptyIndex = -1;

                    for (int i = 0; i < bullets.Length; i++)
                    {
                        if (bullets[i] == null)
                        {
                            emptyIndex = i;
                            break;
                        }
                    }

                    if (emptyIndex != -1)
                    {
                        Bullet bulletObj = new Bullet(shipX, shipY);
                        bullets[emptyIndex] = bulletObj;
                    }
                }

                bool isGameOver = false;  флаг завершения игры (из-за обработки массива врагов)

                //движение всех врагов
                foreach (var enemy in enemies)  обработка движения каждого врага
                {
                    if (enemy != null)
                    {
                        int oldEnemyY = (int)enemy.GetY;

                        enemy.SetY = enemy.GetY - enemySpeed;

                        if (enemy.GetY < 2f)
                        {
                            builder = new StringBuilder("GAME OVER");
                            Console.Clear();
                            Console.WriteLine(builder);
                            isGameOver = true;  установка флага завершения
                            break;
                        }
                        else if (oldEnemyY != (int)enemy.GetY)
                        {
                            builder.Replace(enemyChar, cell, CalculateCoords(enemy.GetX, oldEnemyY), 1);
                            builder.Replace(cell, enemyChar, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
                        }
                    }
                }

                if (isGameOver)  выход из игрового цикла по флагу
                    break;

                builder.Replace(ship, cell, CalculateCoords(oldX, shipY), 1);
                builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

                for (int i = 0; i < bullets.Length; i++)
                {
                    if (bullets[i] != null)
                    {
                        builder.Replace(bullet, cell, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);

                        bullets[i].SetY = bullets[i].GetY + 1;

                        Enemy enemyHit = null;   переменная для хранения врага, в которого попали
                        int enemyHitIndex = -1;  индекс этого врага в массиве

                        for (int e = 0; e < enemies.Length; e++)  поиск попадания среди всех врагов
                        {
                            var enemy = enemies[e];

                            if (enemy != null && bullets[i].GetX == enemy.GetX && Math.Abs(bullets[i].GetY - enemy.GetY) < 1f)
                            {
                                enemyHit = enemy;
                                enemyHitIndex = e;
                                break;
                            }
                        }

                        if (bullets[i].GetY > screenHeight - 1 || enemyHit != null)  условие учитывает попадание в любого врага
                        {
                            bullets[i] = null;

                            if (enemyHit != null)
                            {
                                builder.Replace(enemyChar, cell, CalculateCoords(enemyHit.GetX, (int)enemyHit.GetY), 1);
                                enemies[enemyHitIndex] = null;  удаление конкретного врага из массива

                                CreateEnemy();  создание нового врага после уничтожения
                            }

                            continue;
                        }

                        builder.Replace(cell, bullet, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);
                    }
                }

                Console.Clear();
                Console.WriteLine(builder);
            }

            int CalculateCoords(int valX, int valY)
            {
                int y = screenHeight - valY; 
                int width = screenWidth + 1; 
                return valX + y * width;   
            } 

            void CreateEnemy()  метод генерации врага в случайной свободной колонке
            {
                int countEmptyXs = 0; // считаем количество свободных позиций в массиве врагов

                foreach (var item in enemies)
                {
                    if (item == null)
                    {
                        countEmptyXs++; // увеличиваем счётчик, если позиция свободна
                    }
                }

                int[] candidates = new int[countEmptyXs]; // создаём массив индексов свободных позиций

                int count = 0;
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i] == null)
                    {
                        candidates[count] = i; // сохраняем индекс свободной позиции
                        count++;
                    }
                }

                // выбираем случайный индекс из списка свободных колонок
                int posX = candidates[rnd.Next(0, candidates.Length)];

                // создаём врага в выбранной колонке ( +1 потому что 0 — это стена )
                Enemy enemy = new Enemy(posX + 1, screenHeight - 1);

                // помещаем нового врага в массив
                enemies[posX] = enemy;               
            }
        }      
    }

    public class Bullet
    {
        private int x;
        private int y;

        //геттеры           
        public int GetX => x;
        public int GetY => y;

        //сеттер           
        public int SetY { set { y = value; } }

        //конструктор           
        public Bullet(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Enemy
    {
        private int x;
        private float y;

        //геттеры           
        public int GetX => x;
        public float GetY => y;

        //сеттер            
        public float SetY { set { y = value; } }

        //конструктор           
        public Enemy(int x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

#elif LESSON_9

    //постепенно растет уровень сложности: сперва медленно, но через определенное кол-во игровых циклов увеличиваем скорость врагов
    //добавляем и показываем счет
    class MainClass
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        const int shipY = 2;
        const char cell = ' ';
        const char ship = '#';
        const char dot = '.';
        const char line = '|';
        const char bullet = '^';
        const char enemyChar = '@';
        const float startEnemySpeed = .15f;  базовая стартовая скорость врагов
        const int enemyAmount = 5;
        const float enemyDeltaSpeed = .03f;  приращение скорости при ускорении
        const int ticksToAccelerate = 30;     количество игровых циклов до увеличения скорости

        public static void Main()
        {
            int ticks = 0;  счётчик игровых циклов для контроля ускорения
            float enemySpeed = startEnemySpeed;  текущая изменяемая скорость врагов
            int boardSymbolsAmount;  длина игрового поля в символах (для обновления строки счёта)
            int score = 0;  переменная счёта игрока

            int shipX = screenWidth / 2;

            Bullet[] bullets = new Bullet[screenHeight - 2];
            Enemy[] enemies = new Enemy[screenWidth - 2];

            Random rnd = new Random();

            for (int i = 0; i < enemyAmount; i++)
            {
                CreateEnemy();
            }

            StringBuilder builder = new StringBuilder();

            for (int bY = 0; bY < screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                    {
                        builder.Append(dot);
                    }
                    else
                    {
                        if (bX == 0 || bX == screenWidth - 1)
                            builder.Append(line);
                        else
                            builder.Append(cell);
                    }
                }

                builder.Append('\n');
            }

            boardSymbolsAmount = builder.Length;  запоминаем размер только игрового поля

            builder.Append($"score: {score}");  добавляем отображение счёта под полем

            builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

            foreach (var enemy in enemies)
            {
                if (enemy != null)
                    builder.Replace(cell, enemyChar, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
            }

            Console.WriteLine(builder);

            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();

                int oldX = shipX;

                if (info.Key == ConsoleKey.Escape)                
                    break;    

                if (info.Key == ConsoleKey.LeftArrow)
                {
                    shipX--;
                    shipX = Math.Max(1, shipX);
                }
                else if (info.Key == ConsoleKey.RightArrow)
                {
                    shipX++;
                    shipX = Math.Min(screenWidth - 2, shipX);
                }
                else if (info.Key == ConsoleKey.UpArrow)
                {
                    int emptyIndex = -1;

                    for (int i = 0; i < bullets.Length; i++)
                    {
                        if (bullets[i] == null)
                        {
                            emptyIndex = i;
                            break;
                        }
                    }

                    if (emptyIndex != -1)
                    {
                        Bullet bulletObj = new Bullet(shipX, shipY);
                        bullets[emptyIndex] = bulletObj;
                    }
                }

                bool isGameOver = false;

                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                    {
                        int oldEnemyY = (int)enemy.GetY;

                        enemy.SetY = enemy.GetY - enemySpeed; 

                        if (enemy.GetY < 2f)
                        {
                            builder = new StringBuilder($"GAME OVER\nScore: {score}");  вывод финального счёта
                            Console.Clear();
                            Console.WriteLine(builder);
                            isGameOver = true;
                            break;
                        }
                        else if (oldEnemyY != (int)enemy.GetY)
                        {
                            builder.Replace(enemyChar, cell, CalculateCoords(enemy.GetX, oldEnemyY), 1);
                            builder.Replace(cell, enemyChar, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
                        }
                    }
                }

                if (isGameOver)
                    break;

                builder.Replace(ship, cell, CalculateCoords(oldX, shipY), 1);
                builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

                for (int i = 0; i < bullets.Length; i++)
                {
                    if (bullets[i] != null)
                    {
                        builder.Replace(bullet, cell, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);

                        bullets[i].SetY = bullets[i].GetY + 1;

                        Enemy enemyHit = null;
                        int enemyHitIndex = -1;

                        for (int e = 0; e < enemies.Length; e++)
                        {
                            var enemy = enemies[e];

                            if (enemy != null && bullets[i].GetX == enemy.GetX && Math.Abs(bullets[i].GetY - enemy.GetY) < 1f)
                            {
                                enemyHit = enemy;
                                enemyHitIndex = e;
                                break;
                            }
                        }

                        if (bullets[i].GetY > screenHeight - 1 || enemyHit != null)
                        {
                            bullets[i] = null;

                            if (enemyHit != null)
                            {
                                builder.Replace(enemyChar, cell, CalculateCoords(enemyHit.GetX, (int)enemyHit.GetY), 1);
                                enemies[enemyHitIndex] = null;

                                score++;  увеличиваем счёт при уничтожении врага

                                CreateEnemy();
                            }

                            continue;
                        }

                        builder.Replace(cell, bullet, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);
                    }
                }

                ticks++;  увеличиваем счётчик циклов

                if (ticks > ticksToAccelerate)  проверка необходимости ускорения
                {
                    ticks = 0;  сброс счётчика
                    enemySpeed += enemyDeltaSpeed;  постепенное увеличение скорости
                }

                builder.Remove(boardSymbolsAmount, builder.Length - boardSymbolsAmount);  удаляем старую строку счёта
                builder.Append($"score: {score}");  добавляем обновлённый счёт

                Console.Clear();
                Console.WriteLine(builder);
            }

            int CalculateCoords(int valX, int valY)
            {
                int y = screenHeight - valY; 
                int width = screenWidth + 1; 
                return valX + y * width;   
            } 

            void CreateEnemy()
            {
                int countEmptyXs = 0;

                foreach (var item in enemies)
                {
                    if (item == null)
                    {
                        countEmptyXs++;
                    }
                }

                int[] candidates = new int[countEmptyXs];

                int count = 0;
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i] == null)
                    {
                        candidates[count] = i;
                        count++;
                    }
                }

                int posX = candidates[rnd.Next(0, candidates.Length)];

                Enemy enemy = new Enemy(posX + 1, screenHeight - 1);

                enemies[posX] = enemy;
            }
        }       
    }

    public class Bullet
    {
        private int x;
        private int y;

        public int GetX => x;
        public int GetY => y;

        public int SetY { set { y = value; } }

        public Bullet(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Enemy
    {
        private int x;
        private float y;

        public int GetX => x;
        public float GetY => y;

        public float SetY { set { y = value; } }

        public Enemy(int x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

#elif LESSON_10

    //учим некоторых врагов стрелять, они будут отличаться по внешнему виду от нестреляющих
    //количество стрелков будет расти со временем (чем дольше игра, тем больше срелков, но не больше критического значения)
    //немного замедлили врагов
    class MainClass
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        const int shipY = 2;
        const char cell = ' ';
        const char ship = '#';
        const char dot = '.';
        const char line = '|';
        const char bullet = '^';
        const char enemyChar = '@';
        const char shoterChar = '&';  отдельный символ для врагов-стрелков
        const char enemyBulletChar = '*';  символ пули врага
        const float startEnemySpeed = .12f;  уменьшена стартовая скорость 
        const int enemyAmount = 5;
        const float enemyDeltaSpeed = .02f;  уменьшено ускорение 
        const int ticksToAccelerate = 30;
        const int shooterRateAmount = 10;  базовый шанс появления стрелка
        const int shooterIncreaseInterval = 30;  через 30 тиков шанс появляния стрелков уеличится
        const int shooterMaxChance = 70;  максимальный шанс появления стрелка 70%

        public static void Main()
        {
            int ticks = 0;
            int allTicks = 0;  общий счётчик времени игры для роста стрелков
            float enemySpeed = startEnemySpeed;
            int boardSymbolsAmount;
            int score = 0;

            int shipX = screenWidth / 2;  переименование x → shipX

            Bullet[] bullets = new Bullet[screenHeight - 2];
            Enemy[] enemies = new Enemy[screenWidth - 2];
            Bullet[] enemyBullets = new Bullet[screenWidth - 2];  массив пуль врагов

            Random rnd = new Random();

            for (int i = 0; i < enemyAmount; i++)
            {
                CreateEnemy();
            }

            StringBuilder builder = new StringBuilder();

            for (int bY = 0; bY < screenHeight; bY++)
            {
                for (int bX = 0; bX < screenWidth; bX++)
                {
                    if (bY == 0 || bY == screenHeight - 1)
                    {
                        builder.Append(dot);
                    }
                    else
                    {
                        if (bX == 0 || bX == screenWidth - 1)
                            builder.Append(line);
                        else
                            builder.Append(cell);
                    }
                }

                builder.Append('\n');
            }

            boardSymbolsAmount = builder.Length;

            builder.Append($"score: {score}");

            builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

            foreach (var enemy in enemies)
            {
                if (enemy != null)
                     разный символ врага
                    builder.Replace(cell,  enemy.IsShooter ? shoterChar : enemyChar, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
            }

            Console.WriteLine(builder);

            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();

                int oldX = shipX;

                if (info.Key == ConsoleKey.Escape)                
                    break;    

                if (info.Key == ConsoleKey.LeftArrow)
                {
                    shipX--;
                    shipX = Math.Max(1, shipX);
                }
                else if (info.Key == ConsoleKey.RightArrow)
                {
                    shipX++;
                    shipX = Math.Min(screenWidth - 2, shipX);
                }
                else if (info.Key == ConsoleKey.UpArrow)
                {
                    int emptyIndex = -1;

                    for (int i = 0; i < bullets.Length; i++)
                    {
                        if (bullets[i] == null)
                        {
                            emptyIndex = i;
                            break;
                        }
                    }

                    if (emptyIndex != -1)
                    {
                        Bullet bulletObj = new Bullet(shipX, shipY);
                        bullets[emptyIndex] = bulletObj;
                    }
                }

                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                    {
                        int oldEnemyY = (int)enemy.GetY;

                        enemy.SetY = enemy.GetY - enemySpeed;

                        if (enemy.GetY < 2f)
                        {
                            GameOver();  вынесена логика GameOver в метод
                            return;  выход из игры ( из функции Main)
                        }
                        else if (oldEnemyY != (int)enemy.GetY)
                        {
                            char enemyC = enemy.IsShooter ? shoterChar : enemyChar; 

                            builder.Replace(enemyC, cell, CalculateCoords(enemy.GetX, oldEnemyY), 1);
                            builder.Replace(cell, enemyC, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
                        }

                        if (enemy.IsShooter)  только стрелки стреляют
                        {
                            Bullet newBullet = enemy.TryCreateBullet(enemyBullets);
                            if (newBullet != null)
                            {
                                enemyBullets[newBullet.GetX - 1] = newBullet;
                            }
                        }
                    }
                }

                 пули врагов
                for (int i = 0; i < enemyBullets.Length; i++)
                {
                    if (enemyBullets[i] != null)
                    {
                        builder.Replace(enemyBulletChar, cell, CalculateCoords(enemyBullets[i].GetX, enemyBullets[i].GetY), 1);

                        enemyBullets[i].SetY = enemyBullets[i].GetY - 1;  движение пули вниз

                        if (enemyBullets[i].GetY <= 1)
                        {
                            enemyBullets[i] = null;
                        }
                        else
                        {
                            if (enemyBullets[i].GetX == shipX &&
                                Math.Abs(enemyBullets[i].GetY - shipY) < 1f)
                            {
                                GameOver();  проигрыш от пули врага
                                return;
                            }

                            builder.Replace(cell, enemyBulletChar, CalculateCoords(enemyBullets[i].GetX, enemyBullets[i].GetY), 1);
                        }
                    }
                }

                builder.Replace(ship, cell, CalculateCoords(oldX, shipY), 1);
                builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);

                for (int i = 0; i < bullets.Length; i++)
                {
                    if (bullets[i] != null)
                    {
                        builder.Replace(bullet, cell, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);

                        bullets[i].SetY = bullets[i].GetY + 1;

                        Enemy enemyHit = null;
                        int enemyHitIndex = -1;

                        for (int e = 0; e < enemies.Length; e++)
                        {
                            var enemy = enemies[e];

                            if (enemy != null &&
                                bullets[i].GetX == enemy.GetX &&
                                Math.Abs(bullets[i].GetY - enemy.GetY) < 1f)
                            {
                                enemyHit = enemy;
                                enemyHitIndex = e;
                                break;
                            }
                        }

                        if (bullets[i].GetY > screenHeight - 1 || enemyHit != null)
                        {
                            bullets[i] = null;

                            if (enemyHit != null)
                            {
                                builder.Replace(enemies[enemyHitIndex].IsShooter ? shoterChar : enemyChar, cell, CalculateCoords(enemyHit.GetX, (int)enemyHit.GetY), 1);

                                enemies[enemyHitIndex] = null;

                                score++;

                                CreateEnemy();
                            }

                            continue;
                        }

                        builder.Replace(cell, bullet, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);
                    }
                }

                ticks++;
                allTicks++;  увеличиваем общий счётчик игры

                if (ticks > ticksToAccelerate)
                {
                    ticks = 0;
                    enemySpeed += enemyDeltaSpeed;
                }

                builder.Remove(boardSymbolsAmount, builder.Length - boardSymbolsAmount);
                builder.Append($"score: {score}");

                Console.Clear();
                Console.WriteLine(builder);
            }

            int CalculateCoords(int valX, int valY)
            {
                int y = screenHeight - valY; 
                int width = screenWidth + 1; 
                return valX + y * width;   
            } 

            void CreateEnemy()
            {
                int countEmptyXs = 0;

                foreach (var item in enemies)
                {
                    if (item == null)
                        countEmptyXs++;
                }

                int[] candidates = new int[countEmptyXs];

                int count = 0;
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i] == null)
                    {
                        candidates[count] = i;
                        count++;
                    }
                }

                int posX = candidates[rnd.Next(0, candidates.Length)];

                 вероятность стрелка растёт со временем, но ограничена
                Enemy enemy = new Enemy(posX + 1, screenHeight - 1, rnd.Next(0, 100) <  Math.Min(shooterRateAmount + allTicks / shooterIncreaseInterval, shooterMaxChance)); 
                enemies[posX] = enemy;
            }

            void GameOver()  отдельный метод завершения игры
            {
                builder = new StringBuilder($"GAME OVER\nScore: {score}");
                Console.Clear();
                Console.WriteLine(builder);
            }
        }       
    }

    public class Bullet
    {
        private int x;
        private int y;

        public int GetX => x;
        public int GetY => y;

        public int SetY { set { y = value; } }

        public Bullet(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Enemy
    {
        private bool isShooter;  флаг стрелка
        public bool IsShooter => isShooter; 

        private int x;
        private float y;

        public int GetX => x;
        public float GetY => y;

        public float SetY { set { y = value; } }

        private int tick;  таймер выстрела

        const int rateOfFire = 12;  скорострельность

        public Enemy(int x, float y, bool isShooter)  добавлен параметр
        {
            this.x = x;
            this.y = y;
            this.isShooter = isShooter;
            tick = new Random().Next(2, rateOfFire);
        }

        public Bullet TryCreateBullet(Bullet[] enemyBullets)  логика стрельбы
        {
            tick++;
            if (tick > rateOfFire && y > 2 && enemyBullets[x - 1] == null) //упрощенная логика, на одной вертикали только одна пуля врага
            {
                tick = 0;
                return new Bullet(x, (int)y - 1);
            }
            return null;
        }
    }

#elif LESSON_11

    //наводим красоту в коде, все разложено по отдельным методам для удобства и читабельности
    //всё заккоментированно
    class MainClass
    {
        const int screenWidth = 21;              // ширина игрового поля в символах
        const int screenHeight = 12;             // высота игрового поля в символах
        const int shipY = 2;                    // Y-координата корабля игрока (фиксированная)
        const char cell = ' ';                  // пустая ячейка поля
        const char ship = '#';                  // символ корабля игрока
        const char dot = '.';                   // потолок и пол
        const char line = '|';                  // боковые стены
        const char bullet = '^';                // пуля игрока
        const char enemyChar = '@';             // обычный враг
        const char shoterChar = '&';             // враг-стрелок
        const char enemyBulletChar = '*';       // пуля врага
        const float startEnemySpeed = .12f;     // начальная скорость движения врагов
        const int enemyAmount = 5;              // сколько врагов одновременно на поле
        const float enemyDeltaSpeed = .02f;     // на сколько увеличивается скорость врагов
        const int ticksToAccelerate = 30;       // через сколько тиков ускорять врагов
        const int shooterRateAmount = 10;       // базовый шанс сделать врага стрелком
        const int shooterIncreaseInterval = 30; // интервал увеличения шанса стрелков
        const int shooterMaxChance = 70;         // максимальный шанс врага-стрелка

        public static void Main()
        {
            int ticks = 0;                      // счетчик тиков для ускорения врагов
            int allTicks = 0;                   // общее количество тиков с начала игры
            float enemySpeed = startEnemySpeed; // текущая скорость врагов
            int boardSymbolsAmount;             // длина строки с игровым полем (без счёта)
            int score = 0;                      // счёт игрока

            int shipX = screenWidth / 2;         // стартовая позиция корабля по X

            Bullet[] bullets = new Bullet[screenHeight - 2]; // массив пуль игрока
            Enemy[] enemies = new Enemy[screenWidth - 2];    // массив врагов (по X)
            Bullet[] enemyBullets = new Bullet[screenWidth - 2]; // массив пуль врагов

            Random rnd = new Random();           // генератор случайных чисел
            StringBuilder builder = new StringBuilder(); // единый буфер для экрана

            InitEnemies();                       // создаём стартовых врагов
            BuildBoard();                        // строим игровое поле
            DrawEnemies();                       // рисуем врагов
            DrawShip();                          // рисуем корабль игрока

            boardSymbolsAmount = builder.Length; // запоминаем длину поля
            builder.Append($"score: {score}");   // добавляем счёт в конец строки

            Console.WriteLine(builder); //рисуем поле  на старте

            //игровой цикл
            while (true)
            {
                int oldX = shipX;                // сохраняем старую позицию корабля

                HandleInput();                   // обработка ввода игрока
                MoveEnemies();                   // движение врагов
                MoveEnemyBullets();              // движение пуль врагов
                MovePlayerBullets();             // движение пуль игрока
                UpdateShipPosition(oldX);        // перерисовка корабля
                UpdateDifficulty();              // усложнение игры со временем
                Redraw();                        // обновление экрана

                ticks++;                         // прирост тика для ускорения врагов
                allTicks++;                     // прирост общиего тика игры
            }

            void InitEnemies()
            {
                // создаём начальное количество врагов
                for (int i = 0; i < enemyAmount; i++)
                    CreateEnemy();
            }

            void BuildBoard()
            {
                // формируем статичное поле (стены, пол, потолок)
                for (int bY = 0; bY < screenHeight; bY++)
                {
                    for (int bX = 0; bX < screenWidth; bX++)
                    {
                        if (bY == 0 || bY == screenHeight - 1)
                            builder.Append(dot);     // потолок и пол
                        else if (bX == 0 || bX == screenWidth - 1)
                            builder.Append(line);    // боковые стены
                        else
                            builder.Append(cell);    // пустая ячейка
                    }

                    builder.Append('\n');             // перенос строки
                }
            }

            void DrawEnemies()
            {
                // отрисовываем всех существующих врагов
                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                        builder.Replace(cell, enemy.IsShooter ? shoterChar : enemyChar, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
                }
            }

            void DrawShip()
            {
                // рисуем корабль игрока
                builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);
            }

            void HandleInput()
            {
                // читаем ввод с клавиатуры (блокирующий)
                ConsoleKeyInfo info = Console.ReadKey();

                if (info.Key == ConsoleKey.Escape)                
                    break;    

                if (info.Key == ConsoleKey.LeftArrow)
                {
                    shipX--;                         // движение влево
                    shipX = Math.Max(1, shipX);      // ограничение стеной
                }
                else if (info.Key == ConsoleKey.RightArrow)
                {
                    shipX++;                         // движение вправо
                    shipX = Math.Min(screenWidth - 2, shipX); // ограничение стеной
                }
                else if (info.Key == ConsoleKey.UpArrow)
                {
                    // создаём пулю в первом свободном слоте
                    for (int i = 0; i < bullets.Length; i++)
                    {
                        if (bullets[i] == null)
                        {
                            bullets[i] = new Bullet(shipX, shipY);
                            break;
                        }
                    }
                }
            }

            void MoveEnemies()
            {
                // двигаем каждого врага
                foreach (var enemy in enemies)
                {
                    if (enemy == null)
                        continue;                   // пропускаем пустые слоты

                    int oldEnemyY = (int)enemy.GetY; // старая позиция по Y
                    enemy.SetY = enemy.GetY - enemySpeed; // движение вниз

                    if (enemy.GetY < 2f)
                    {
                        GameOver();                 // враг достиг игрока
                        Environment.Exit(0);       //выход из игры
                    }

                    if (oldEnemyY != (int)enemy.GetY) //проверка, переместился ли враг кратно 1 вниз (так как из-за малеького float смещения, его не нужно визуально смещать)
                    {
                        char enemyC = enemy.IsShooter ? shoterChar : enemyChar; //выбираем символ для врага

                        // стираем старую позицию
                        builder.Replace(enemyC, cell, CalculateCoords(enemy.GetX, oldEnemyY), 1);
                        // рисуем новую
                        builder.Replace(cell, enemyC, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
                    }

                    if (enemy.IsShooter)
                    {
                        // враг-стрелок пытается создать пулю
                        Bullet newBullet = enemy.TryCreateBullet(enemyBullets);
                        if (newBullet != null)
                            enemyBullets[newBullet.GetX - 1] = newBullet;
                    }
                }
            }

            void MoveEnemyBullets()
            {
                // движение всех пуль врагов
                for (int i = 0; i < enemyBullets.Length; i++)
                {
                    if (enemyBullets[i] == null)
                        continue;

                    // стираем старую позицию пули
                    builder.Replace(enemyBulletChar, cell, CalculateCoords(enemyBullets[i].GetX, enemyBullets[i].GetY), 1);

                    enemyBullets[i].SetY = enemyBullets[i].GetY - 1; // пуля летит вниз

                    if (enemyBullets[i].GetY <= 1) // пуля вышла за поле
                    {
                        enemyBullets[i] = null;    //удаляем пулю 
                        continue;
                    }

                    // попадание по кораблю
                    if (enemyBullets[i].GetX == shipX && Math.Abs(enemyBullets[i].GetY - shipY) < 1f)
                    {
                        GameOver();
                        Environment.Exit(0);  //выход из игры
                    }

                    // рисуем пулю в новой позиции
                    builder.Replace(cell, enemyBulletChar, CalculateCoords(enemyBullets[i].GetX, enemyBullets[i].GetY), 1);
                }
            }

            void MovePlayerBullets()
            {
                // движение пуль игрока
                for (int i = 0; i < bullets.Length; i++)
                {
                    if (bullets[i] == null)
                        continue;

                    // стираем старую позицию
                    builder.Replace(bullet, cell, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);

                    bullets[i].SetY = bullets[i].GetY + 1; // пуля летит вверх

                    Enemy enemyHit = null;           // ссылка на поражённого врага
                    int enemyHitIndex = -1;          // индекс врага в массиве

                    // проверка попадания по всем врагам
                    for (int e = 0; e < enemies.Length; e++)
                    {
                        if (enemies[e] != null && bullets[i].GetX == enemies[e].GetX && Math.Abs(bullets[i].GetY - enemies[e].GetY) < 1f)
                        {
                            enemyHit = enemies[e];
                            enemyHitIndex = e;
                            break;
                        }
                    }

                    if (bullets[i].GetY > screenHeight - 1 || enemyHit != null)
                    {
                        bullets[i] = null;           // удаляем пулю

                        if (enemyHit != null)
                        {
                            // стираем врага
                            builder.Replace(enemies[enemyHitIndex].IsShooter ? shoterChar : enemyChar, cell, CalculateCoords(enemyHit.GetX, (int)enemyHit.GetY), 1);

                            enemies[enemyHitIndex] = null; // удаляем врага
                            score++;                        // увеличиваем счёт
                            CreateEnemy();                  // создаём нового врага
                        }

                        continue;
                    }

                    // рисуем пулю в новой позиции
                    builder.Replace(cell, bullet, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);
                }
            }

            void UpdateShipPosition(int oldX)
            {
                // стираем старую позицию корабля
                builder.Replace(ship, cell, CalculateCoords(oldX, shipY), 1);
                // рисуем новую
                builder.Replace(cell, ship, CalculateCoords(shipX, shipY), 1);
            }

            void UpdateDifficulty()
            {
                // постепенное усложнение игры
                if (ticks > ticksToAccelerate)
                {
                    ticks = 0;
                    enemySpeed += enemyDeltaSpeed;
                }
            }

            void Redraw()
            {
                // удаляем старый счёт
                builder.Remove(boardSymbolsAmount, builder.Length - boardSymbolsAmount);
                // добавляем обновлённый
                builder.Append($"score: {score}");

                Console.Clear();
                Console.WriteLine(builder);
            }

            // перевод 2D-координат в индекс строки StringBuilder
            int CalculateCoords(int valX, int valY)
            {
                int y = screenHeight - valY; 
                int width = screenWidth + 1; 
                return valX + y * width;   
            } 

            void CreateEnemy()
            {
                int countEmpty = 0;

                // считаем свободные позиции по X
                foreach (var e in enemies)
                    if (e == null)
                        countEmpty++;

                int[] candidates = new int[countEmpty];
                int index = 0;

                // собираем индексы свободных позиций
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i] == null)
                    {
                        candidates[index] = i;
                        index++;
                    }
                }

                // выбираем случайную свободную позицию
                int posX = candidates[rnd.Next(0, candidates.Length)];

                // создаём врага
                Enemy enemy = new Enemy(posX + 1, screenHeight - 1,
                    // возможно стрелка
                    rnd.Next(0, 100) < Math.Min(shooterRateAmount + allTicks / shooterIncreaseInterval, shooterMaxChance)); 

                enemies[posX] = enemy;
            }

            void GameOver()
            {
                // экран окончания игры
                builder = new StringBuilder($"GAME OVER\nScore: {score}");

                Console.Clear();
                Console.WriteLine(builder);
            }
        }       
    }

    public class Bullet
    {
        private int x, y;              // координаты пули

        public int GetX => x;          // геттер X
        public int GetY => y;          // геттер Y

        public int SetY { set { y = value; } } // сеттер Y

        public Bullet(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Enemy
    {
        private bool isShooter;        // является ли враг стрелком
        public bool IsShooter => isShooter;

        private int x;                // координата X
        private float y;              // координата Y (float для плавного движения)

        public int GetX => x;
        public float GetY => y;

        public float SetY { set { y = value; } }

        private int tick;              // счётчик тиков стрельбы

        const int rateOfFire = 12;     // скорострельность врага

        public Enemy(int x, float y, bool isShooter)
        {
            this.x = x;
            this.y = y;
            this.isShooter = isShooter;
            tick = new Random().Next(2, rateOfFire); // случайный старт стрельбы
        }

        public Bullet TryCreateBullet(Bullet[] enemyBullets)
        {
            // попытка выстрелить
            tick++;
            if (tick > rateOfFire && y > 2 && enemyBullets[x - 1] == null)
            {
                tick = 0;
                return new Bullet(x, (int)y - 1);
            }
            return null;
        }
    }

#endif

}
