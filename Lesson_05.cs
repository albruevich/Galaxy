// ❗ Для переключения между уроками смотрите Program.cs

// Урок 5
// В этом уроке игра переходит от простого управления объектами к взаимодействию между ними.
// Мы вводим абстрактный базовый класс, реализуем наследование, добавляем врага и столкновения
// Это первый настоящий шаг к архитектуре игры.

using System;
using System.Text;

namespace Lesson_05
{
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
        public BulletMoveResult Move(int screenHeight, GameObject aim)
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
}
