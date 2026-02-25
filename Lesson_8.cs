// ❗ Для переключения между уроками смотрите Program.cs

// Урок 8
// Постепенно растет уровень сложности: сперва медленно,
// но через определенное кол-во игровых циклов увеличиваем скорость врагов.
// Добавляем и показываем счет

using System;
using System.Text;
using System.Collections.Generic;

namespace Lesson_8
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
        const float startEnemySpeed = .15f; //базовая стартовая скорость врагов
        const int enemyAmount = 5;
        const float enemyDeltaSpeed = .03f; //приращение скорости при ускорении
        const int ticksToAccelerate = 30; //количество игровых циклов до увеличения скорости

        public void Run()
        {
            int ticks = 0; //счётчик игровых циклов для контроля ускорения
            float enemySpeed = 0; //startEnemySpeed; текущая изменяемая скорость врагов
            int boardSymbolsAmount; //длина игрового поля в символах(для обновления строки счёта)
            int score = 0; //переменная счёта игрока

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

            boardSymbolsAmount = builder.Length; //запоминаем размер только игрового поля

            builder.Append($"score: {score}"); //добавляем отображение счёта под полем

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
                            builder = new StringBuilder($"GAME OVER\nScore: {score}"); //вывод финального счёта
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

                                score++; //увеличиваем счёт при уничтожении врага

                                CreateEnemy();
                            }

                            continue;
                        }

                        builder.Replace(cell, bullet, CalculateCoords(bullets[i].GetX, bullets[i].GetY), 1);
                    }
                }

                ticks++; //увеличиваем счётчик циклов

                if (ticks > ticksToAccelerate) //проверка необходимости ускорения
                {
                    ticks = 0; //сброс счётчика
                    enemySpeed += enemyDeltaSpeed; // постепенное увеличение скорости
                }

                builder.Remove(boardSymbolsAmount, builder.Length - boardSymbolsAmount); //удаляем старую строку счёта
                builder.Append($"score: {score}");// добавляем обновлённый счёт

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

}
