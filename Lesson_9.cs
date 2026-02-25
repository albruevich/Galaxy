// ❗ Для переключения между уроками смотрите Program.cs

// Урок 9
// Учим некоторых врагов стрелять, они будут отличаться по внешнему виду от нестреляющих
// Количество стрелков будет расти со временем (чем дольше игра, тем больше срелков, но не больше критического значения)

using System;
using System.Text;
using System.Collections.Generic;

namespace Lesson_9
{
    class Game
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
        const char shoterChar = '&'; //отдельный символ для врагов-стрелков
        const char enemyBulletChar = '*'; //символ пули врага
        const float startEnemySpeed = .12f; //уменьшена стартовая скорость
        const int enemyAmount = 5;
        const float enemyDeltaSpeed = .02f; //уменьшено ускорение
        const int ticksToAccelerate = 30;
        const int shooterRateAmount = 10; //базовый шанс появления стрелка
        const int shooterIncreaseInterval = 30; //через 30 тиков шанс появляния стрелков уеличится
        const int shooterMaxChance = 70; //максимальный шанс появления стрелка 70%

        public void Run()
        {
            int ticks = 0;
            int allTicks = 0; //общий счётчик времени игры для роста стрелков
            float enemySpeed = startEnemySpeed;
            int boardSymbolsAmount;
            int score = 0;

            int shipX = screenWidth / 2; //переименование x → shipX

            Bullet[] bullets = new Bullet[screenHeight - 2];
            Enemy[] enemies = new Enemy[screenWidth - 2];
            Bullet[] enemyBullets = new Bullet[screenWidth - 2]; //массив пуль врагов

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
                    // разный символ врага
                    builder.Replace(cell, enemy.IsShooter ? shoterChar : enemyChar, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
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
                            GameOver();// вынесена логика GameOver в метод
                            return; //выход из игры(из функции Main)
                        }
                        else if (oldEnemyY != (int)enemy.GetY)
                        {
                            char enemyC = enemy.IsShooter ? shoterChar : enemyChar;

                            builder.Replace(enemyC, cell, CalculateCoords(enemy.GetX, oldEnemyY), 1);
                            builder.Replace(cell, enemyC, CalculateCoords(enemy.GetX, (int)enemy.GetY), 1);
                        }

                        if (enemy.IsShooter) // только стрелки стреляют
                        {
                            Bullet newBullet = enemy.TryCreateBullet(enemyBullets);
                            if (newBullet != null)
                            {
                                enemyBullets[newBullet.GetX - 1] = newBullet;
                            }
                        }
                    }
                }

                //пули врагов
                for (int i = 0; i < enemyBullets.Length; i++)
                {
                    if (enemyBullets[i] != null)
                    {
                        builder.Replace(enemyBulletChar, cell, CalculateCoords(enemyBullets[i].GetX, enemyBullets[i].GetY), 1);

                        enemyBullets[i].SetY = enemyBullets[i].GetY - 1; //движение пули вниз

                        if (enemyBullets[i].GetY <= 1)
                        {
                            enemyBullets[i] = null;
                        }
                        else
                        {
                            if (enemyBullets[i].GetX == shipX &&
                                Math.Abs(enemyBullets[i].GetY - shipY) < 1f)
                            {
                                GameOver();// проигрыш от пули врага
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
                allTicks++;// увеличиваем общий счётчик игры

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

                //вероятность стрелка растёт со временем, но ограничена
                Enemy enemy = new Enemy(posX + 1, screenHeight - 1, rnd.Next(0, 100) < Math.Min(shooterRateAmount + allTicks / shooterIncreaseInterval, shooterMaxChance));
                enemies[posX] = enemy;
            }

            void GameOver() // отдельный метод завершения игры
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
        private bool isShooter; //флаг стрелка
        public bool IsShooter => isShooter;

        private int x;
        private float y;

        public int GetX => x;
        public float GetY => y;

        public float SetY { set { y = value; } }

        private int tick; //таймер выстрела

        const int rateOfFire = 12; //скорострельность

        public Enemy(int x, float y, bool isShooter)  //добавлен параметр
        {
            this.x = x;
            this.y = y;
            this.isShooter = isShooter;
            tick = new Random().Next(2, rateOfFire);
        }

        public Bullet TryCreateBullet(Bullet[] enemyBullets)  //логика стрельбы
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
}