<h1>Labyrinth Solver</h1>
<h2>Overview</h2>
<p>This project is an automatic labyrinth solver that connects to a TCP server, navigates a maze, and finds the shortest path to the target using pathfinding algorithms.</p>
<p>The challenge was to handle large labyrinths efficiently while keeping execution time under 30 seconds.</p>
    
<h1>Implemented Features</h1>
   
<h3>Pathfinding Algorithms</h3>
<div class="emoji">🔍 Breadth-First Search (BFS) – Locates the target efficiently.</div>
<div class="emoji">🧭 A* Algorithm – Computes the optimal path to the target.</div>
  
<h3>Network Communication</h3>
<div class="emoji">🌐 TCP Client – Handles server interactions.</div>
<div class="emoji">📡 Command Processing – Sends movement and interaction commands.</div>
  
<h3>Labyrinth Navigation</h3>
<div class="emoji">🚶 Movement: up, down, left, right.</div>
<div class="emoji">🚪 Interactions: Enter on 'O' to go up, 'H' to go down.</div>
<div class="emoji">📌 Visibility: Only a 11x11 portion of the maze is visible at any time.</div>
 
<h2>How It Works</h2>
<ol>
    <li>The program connects to the server and retrieves the current labyrinth map.</li>
    <li>BFS is used to locate the closest target (<code>T</code>).</li>
    <li>A* algorithm calculates the shortest path to the target.</li>
    <li>Commands are sent to navigate the maze based on the computed path.</li>
    <li>When on a target, <code>enter</code> is used to interact and progress.</li>
</ol>
    
<h2>Code Structure</h2>
<div class="emoji">📜 <code>Program.cs</code> – Entry point of the application.</div>
<div class="emoji">📜 <code>Maze.cs</code> – Manages game logic and movement.</div>
<div class="emoji">📜 <code>Pathfinding.cs</code> – Implements A* and BFS algorithms.</div>
<div class="emoji">📜 <code>Network.cs</code> – Handles TCP communication.</div>
<div class="emoji">📜 <code>Node.cs</code> – Represents individual nodes in the maze.</div>
<div class="emoji">📜 <code>ServerResponse.cs</code> – Parses responses from the server.</div>
    
<h2>Key Takeaways</h2>
<div class="emoji">✅ Efficient pathfinding and search algorithms.</div>
<div class="emoji">✅ Handling dynamic server responses.</div>
<div class="emoji">✅ Managing visibility constraints in a large-scale maze.</div>
<div class="emoji">✅ Performance optimizations for large labyrinths.</div>
