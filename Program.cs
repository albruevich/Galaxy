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
//#define LESSON_4
//#define LESSON_5
#define LESSON_6
//#define LESSON_7
//#define LESSON_8
//#define LESSON_9

using System;
using System.Text;
using System.Collections.Generic;

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
               // Создаем новый класс GameObject

    class MainClass { public static void Main() => new Game().Run(); }

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

#elif LESSON_5
    // В этом уроке игра переходит от простого управления объектами к взаимодействию между ними.
    // Мы вводим абстрактный базовый класс, реализуем наследование, добавляем врага и столкновения
    // Это первый настоящий шаг к архитектуре игры.

    class MainClass { public static void Main() => new Game().Run(); }

    class Game
    {
        const int screenWidth = 21;
        const int screenHeight = 12;
        int shipX;
        const int shipY = 2;
        bool isGameRunning = true;

        Renderer renderer;

        // Раньше этот массив был GameObject[]. Теперь массив хранит конкретный тип Bullet.        
        Bullet[] bullets = new Bullet[screenHeight - 2]; //Максимум пуль на экране (по высоте игрового поля без границ)

        Random rnd = new Random(); // генератор случайных чисел

        // В игре появляется еще один тип игрового объекта.
        Enemy enemy = null; //пока единственный объект врага
      
        public void Run()
        {
            Init();
            renderer.BuildBoard();

            //Renderer теперь умеет рисовать не только корабль, но и врага.
            renderer.DrawFirstFrame(shipX, shipY, enemy);            

            while (isGameRunning)
            {
                int oldX = shipX;

                HandleInput();
                MovePlayerBullets();

                renderer.Render(oldX, shipX, shipY, bullets);
            }
        }

        void Init()
        {
            shipX = screenWidth / 2;
            renderer = new Renderer(screenWidth, screenHeight);

            // Создание экземпляра Enemy — теперь используется наследник GameObject.
            int enemyX = rnd.Next(1, screenWidth - 1); //случайный Х в пределах от 1 до ширины экрана (стенки исключаются)
            int enemyY = screenHeight - 1;             // Y под потолком (из-за метода FindIndex начало координат внизу)
            enemy = new Enemy(enemyX, enemyY);          
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
                }
            }
        }

        void MovePlayerBullets()
        {
            for (int i = bullets.Length - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];

                if (bullet == null)
                    continue;

                // Метод возвращает результат движения: попала, вышла за экран или летит дальше.
                // Удобнее, чем просто true/false.
                BulletMoveResult result = bullet.Move(screenHeight, enemy);               

                //если пуля попала по врагу или вышла за экран, то ...
                if (result == BulletMoveResult.OutOfBounds || result == BulletMoveResult.Hit)
                {
                    // ... удаляем пулю
                    renderer.ClearGameObject(bullet);
                    bullets[i] = null;

                    // ... если пуля попала по врагу, то удаляем врага
                    if (result == BulletMoveResult.Hit)
                    {
                        renderer.ClearGameObject(enemy);
                        enemy = null;                       
                    }
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

        // Теперь метод умеет рисовать не только корабль, но и врага.        
        public void DrawFirstFrame(int shipX, int shipY, Enemy enemy)
        {
            builder[FindIndex(shipX, shipY)] = shipChar;
          
            // Если враг существует то рисуем его символ
            if (enemy != null)
                builder[FindIndex(enemy.X, enemy.Y)] = enemy.Symbol;

            Console.WriteLine(builder);
        }

        public void Render(int oldShipX, int newShipX, int shipY, Bullet[] bullets)
        {
            builder[FindIndex(oldShipX, shipY)] = emptyChar;
            builder[FindIndex(newShipX, shipY)] = shipChar;

            foreach (var bullet in bullets)
                if (bullet != null)
                {
                    ClearGameObject(bullet);
                     builder[FindIndex(bullet.X, bullet.Y)] = bullet.Symbol;
                }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder);
        }

        // В прошлом уроке был ClearBullet и использовался bulletChar.
        // Теперь метод универсальный — он принимает любой GameObject и использует его Symbol.
        // Это первый шаг к полиморфизму.
        public void ClearGameObject(GameObject go) =>
            builder.Replace(go.Symbol, emptyChar, FindIndex(go.X, go.OldY), 1);

        int FindIndex(int valX, int valY)
        {
            int y = screenHeight - valY;
            int width = screenWidth + 1;
            return valX + y * width;
        }
    }
    
    // GameObject стал abstract — теперь нельзя создать просто "какой-то объект", но конкретный наследник        
    abstract class GameObject
    {
        public int Y { get; set; }
        public int X { get; set; }
        public int OldY { get; set; }

        //абстрактное свойство которое ОБЯЗАН реализовать каждый наследник.
        public abstract char Symbol { get; }

        public GameObject(int x, int y)
        {
            X = x;
            Y = y;
            OldY = Y;
        }
    }

    // Bullet наследуется от GameObject, таким образом у него есть все свойства и методы родителя
    class Bullet : GameObject
    {
        // override — реализация (переопределение) абстрактного свойства родителя
        public override char Symbol => '^';

        // : base(x, y) — вызов конструктора родительского класса.
        public Bullet(int x, int y) : base(x, y) { }

        // Метод Move отвечает только за движение пули и возвращает результат этого движения.       
        public BulletMoveResult Move(int screenHeight,  GameObject aim)
        {
            OldY = Y; 
            Y++;     

            // если координаты пули совпали с координатами врага — считаем, что произошло попадание
            if (aim != null && X == aim.X && Y >= aim.Y)
                return BulletMoveResult.Hit;

            // если пуля вышла за пределы экрана — сообщаем об этом
            if (Y > screenHeight - 1)
                return BulletMoveResult.OutOfBounds;

            // если ничего особенного не произошло — просто продолжаем движение
            return BulletMoveResult.None;
        }
    }

    // Enemy наследуется от GameObject.
    class Enemy : GameObject
    {
        // override — реализация (переопределение) абстрактного свойства родителя
        public override char Symbol => '@';

        // : base(x, y) — вызов конструктора родительского класса.
        public Enemy(int x, int y) : base(x, y) { }
    }

    //возможные исходы движения пули 
    enum BulletMoveResult
    {
        None,        
        Hit,   
        OutOfBounds  
    }

#elif LESSON_6
    // враг теперь движется сам со своей скоростью,
    // появляется состояние Game Over и возможность перезапуска игры.   

    class MainClass { public static void Main() => new Game().Run(); }

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
        
        List<GameObject> renderedObjects = new List<GameObject>(); // общий список всех игровых объектов для универсального рендеринга

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

            //удаляем объект из списка
            if (enemy != null)
                renderedObjects.Remove(enemy);

            enemy = null;

            for (int i = 0; i < bullets.Length; i++)
            {
                //удаляем объект из списка
                if (bullets[i] != null)
                    renderedObjects.Remove(bullets[i]);

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

#elif LESSON_7

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

#elif LESSON_8

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

#elif LESSON_9

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

#endif

}