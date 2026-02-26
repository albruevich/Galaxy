/* 
================================================
                     Galaxy
================================================

Educational project in C# simulating a game with a ship and incoming enemies.
The goal is to learn C# through practice: game and regular loops, input handling, arrays, methods, lists, enums.
Also covers classes, OOP basics, code structure and object organization, and much more.
The project is used as a sequence of lessons to learn programming, not for entertainment.

Controls:
  - Left/Right arrows — move the ship
  - Up arrow — shoot
  - Esc — exit the project

Game:
  - The frame updates after each player action
  - The game is not in real-time (turn-based)

Lessons:
  - 10 lessons. They increase in difficulty, each next expands on the previous one
  - To run a lesson, change: const int lesson = N
  - Each lesson is in a separate file Lesson_N.cs inside the Galaxy folder
  - Each file defines the class Lesson_N.Game, and the Run() method starts the lesson
  - Comments in the code describe only the new elements of the current lesson
*/

namespace Galaxy
{
    class MainClass
    {
        // ❗ Set here the lesson number you want to run: 1 - 10
        const int lesson = 1;

        public static void Main()
        {
            System.Action[] lessons = new System.Action[]
            {
                () => new Lesson_01.Game().Run(),
                () => new Lesson_02.Game().Run(),
                () => new Lesson_03.Game().Run(),
                () => new Lesson_04.Game().Run(),
                () => new Lesson_05.Game().Run(),
                () => new Lesson_06.Game().Run(),
                () => new Lesson_07.Game().Run(),
                () => new Lesson_08.Game().Run(),
                () => new Lesson_09.Game().Run(),
                () => new Lesson_10.Game().Run()
            };

            if (lesson >= 1 && lesson <= lessons.Length)
                lessons[lesson - 1]();
            else
                lessons[lessons.Length - 1]();
        }
    }
}