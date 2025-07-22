# Infinite Tic Tac Toe

**Infinite Tic Tac Toe** is a twist on the classic game of tic tac toe, built using **C#** and **WPF**. This project adds an interesting mechanic: once each player has placed their three pieces, they must move their existing pieces to continue playing. The game supports human vs. human, human vs. AI, and AI vs. AI matches.

---

##  Features

-  **Infinite gameplay**: After placing three pieces, players must slide their pieces to create new moves.
-  **Versatile opponents**: Play against another person, challenge an AI, or watch two AIs battle it out.
-  **Extensible architecture**:
  - Easily add new AI algorithms or strategies.
  - Introduce new types of players with minimal changes to the core code.

---

##  Getting Started

No binaries are included in this repo

To run the game:

1. Clone or download this repository.
2. Run dotnet build against the solution file `Infinite-tic-tac-toe.sln`
3. Navigate to the following folder:
Infinite-tic-tac-toe\bin\Release\net8.0-windows
4. Run the executable:
Infinite-tic-tac-toe.exe



>  Requires [.NET 8.0 Runtime and SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) to be installed on your machine.

---

##  Project Structure

- `Game/` and `Model/`: Core rules and game state logic.
- `Solvers/`: Modular AI strategies. Add your own by inheriting a simple abstract class.
- `Services/`: Instansiation for game players and other important objects.
- `UserInterface/`: WPF-based user interface.

---

##  Extending the Game

To add a new AI or player type:

- **New AI Strategy**: Extend the `GameSolverBase` class and plug it into the game.
- **New Player Type**: Inherit from the `IPlayer` abstraction, define a configuration if needed, hook an existing factory or roll your own, and plug it into the playerService to make it available to the UI

The architecture is intentionally modular and designed for experimentation.

---

##  Status

This is a **toy project** built for fun and experimentation with game mechanics, AI strategies, and WPF. Feedback and contributions are welcome!

---
