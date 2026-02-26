# Galaxy

Educational C# project that simulates a simple game with a player-controlled ship and incoming enemies.

The project is structured as a sequence of lessons, each extending the previous one and gradually increasing in complexity.vide a **basic understanding of game development**, creating a foundation for later transitioning to Unity game development.

## Demo 

![Gameplay](docs/gameplay.gif)

## Code Example

![Game Loop](docs/code_example.png)

**Purpose:**
  - Help beginners learn C# through practical, incremental development of a simple game
  - Build a solid understanding of programming fundamentals and object-oriented principles
  - Introduce fundamental game development concepts with a strong focus on logic, structure, and game loops
  - Serve as a foundation for further learning in game development and Unity

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