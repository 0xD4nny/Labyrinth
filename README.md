# Labyrinth Solver
## Overview
- This project is an automatic labyrinth solver that connects to a TCP server, navigates a maze, and finds the shortest path to the target using pathfinding algorithms.
- The challenge was to handle large labyrinths efficiently while keeping execution time under 30 seconds.
    
# Implemented Features
   
### 🔍 Pathfinding Algorithms
- Breadth-First Search (BFS) – Locates the target efficiently.
- A* Algorithm – Computes the optimal path to the target.
  
### 🌐 Network Communication
- TCP Client – Handles server interactions.
- - 📡 Command Processing – Sends movement and interaction commands.
  
### 🚶 Labyrinth Navigation
- Movement: up, down, left, right.
- Interactions: Enter on 'O' to go up, 'H' to go down.
- Visibility: Only a 11x11 portion of the maze is visible at any time.
 
### 📡 How It Works
- The program connects to the server and retrieves the current labyrinth map.
- BFS is used to locate the closest target (T) or the next edge point to explore the map to find targed (T).
- A* algorithm calculates the shortest path to the nextTarget.
- Commands are sent to navigate the maze based on the computed path.
- When on a target, enter is used to interact and progress.

### 📜 Code Structure
- Program.cs – Entry point of the application.
- Maze.cs – Manages game logic and movement.
- SearchNextTarget - Searched the Next TargetPoint for the A* Algorithm. 
- Pathfinding.cs – Implements A* and BFS algorithms.
- Network.cs – Handles TCP communication.
- Node.cs – Represents individual nodes in the maze.
- ServerResponse.cs – Parses responses from the server.
    
## ✅ Key Takeaways
- Efficient pathfinding and search algorithms.
- Handling dynamic server responses.
- Managing visibility constraints in a large-scale maze.
- Performance optimizations for large labyrinths.
