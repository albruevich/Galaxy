# Galaxy

Educational project in C# simulating a game with a ship and incoming enemies.  
The goal is to learn C# through practice: game loops, input handling, arrays, methods, lists, enums.  
It also covers classes, object-oriented programming basics, code structure, object organization, and more.  

These lessons are designed to help you **understand the principles of the C# language and object-oriented programming in practice**.  
They also provide a **basic understanding of game development**, creating a foundation for later transitioning to Unity game development.

## Demo 

![Gameplay](docs/gameplay.gif)

## Code Example

![Game Loop](docs/code_example.png)

**Purpose:**
  - Designed as an educational sequence, not for entertainment
  - Helps beginners **gain practical experience in C#** while gradually learning more advanced programming concepts
  - Bridges the gap between learning programming fundamentals and creating actual games
  - Encourages experimenting with object-oriented design, game loops, and rendering logic

**Controls:**
  - Left/Right arrows — move the ship
  - Up arrow — shoot
  - Esc — exit the project

**Game:**
  - The frame updates after each player action
  - The game is turn-based, not real-time
  - In later lessons, enemies shoot, and bullets move independently
  - Score increases by destroying enemies, with higher rewards for shooting enemies

**Lessons:**
  - 10 lessons in total. Each lesson gradually increases in difficulty and expands on the previous one
  - Each lesson defines a separate `Game` class with a `Run()` method
  - Comments in the code describe only the new elements introduced in the current lesson
  - Lessons progressively cover:
    - Basic console output and input
    - Loops and arrays
    - Methods and enums
    - Lists and dynamic collections
    - Object-oriented programming
    - Game architecture and refactoring
    - Rendering techniques
    - Handling multiple objects (player, enemies, bullets)
    - Enemy movement and shooting logic
    - High score management using file storage

**Notes:**
  - The project is primarily a learning tool; graphics are simple text-based symbols
  - The focus is on **understanding C# and game development concepts**, not creating a polished game
  - Each lesson introduces only the new features for that lesson, making it easier to follow and understand

**Technical Configuration:**
  - Select which lesson to run in Program.cs:
    - //❗ Set here the lesson number you want to run: 1 - 10
    -    const int lesson = 1;
  - Change the number from 1 to 10 to launch the corresponding lesson.
  - To study the implementation of a specific lesson, open the corresponding Lesson_N.cs file located in the root project folder (Galaxy).