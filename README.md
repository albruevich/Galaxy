# Galaxy
![Language](https://img.shields.io/badge/language-C%23-blue)
![License](https://img.shields.io/badge/license-MIT-green)

Learn C# by building a simple console space shooter.

## Demo
<img src="docs/gameplay.gif" width="300">

The project is organized as **10 step-by-step lessons**, each introducing new mechanics and programming concepts.

## Example Code
![Game Loop](docs/code_example.png)

**Purpose**
- Helps beginners learn C# through practical, step-by-step development of a simple game  
- Builds a solid understanding of programming fundamentals and object-oriented principles  
- Introduces fundamental game development concepts with a focus on logic, structure, and game loops  
- Provides a foundation for further learning in game development, including Unity 

**Controls**
- **Left / Right arrows** — move the ship
- **Up arrow** — shoot
- **Esc** — exit the game

**Game**
- The game updates the frame after each player action
- The game is turn-based rather than real-time
- In later lessons, enemies shoot, and bullets move independently
- Score increases by destroying enemies, with higher rewards for shooting enemies

**Lessons**
- 10 lessons in total. Each lesson gradually increases in difficulty and expands on the previous one
- Each lesson introduces a separate `Game` class with a `Run()` method
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

**Notes**
- Graphics are simple, text-based symbols  
- The focus is on **understanding C# and game development concepts**, not creating a polished game  
- Each lesson introduces only the new features for that lesson, making it easier to follow and understand  

### Requirements

**Windows**
- Visual Studio 2022

**macOS**
- Visual Studio for Mac 2019<br>

### How to Run

1. **Windows:** Open `Galaxy.sln` in **Visual Studio 2022**.
2. **macOS:** Open `Galaxy_Mac/Galaxy.sln` in **Visual Studio for Mac 2019**.
3. In `Program.cs`, set the lesson number you want to run (choose **1–10**).
4. Press **F5** to run.
5. To study the implementation of a specific lesson, open the corresponding `Lesson_N.cs` file

### Visual Studio Code
This is an alternative way to run the project without Visual Studio.

Requires:
- Visual Studio Code
- .NET SDK

Run:
1. Open the project folder in **Visual Studio Code**.
2. From the top menu, select **Terminal → New Terminal**.
3. Run the command below in the terminal: dotnet run

##
**Contact / Support**
- If you have questions, suggestions, or issues with the project, please use the **[GitHub Discussions](https://github.com/albruevich/Galaxy/discussions)** page.  

**Other**

- If you find this project useful, a ⭐ would mean a lot.
- Contributions are welcome — see CONTRIBUTORS.md.