# SharpGVGP

A C# framework for General Video Game Playing research, starting with grid-based environments.

---

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Projects](#projects)
- [Usage](#usage)
- [Features](#features)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

SharpGVGP provides core abstractions, sample environments, and built-in agents to accelerate research and prototyping in reinforcement learning and general video-game-playing (GVGP).  

Right now the focus is on discrete, grid-based worlds (e.g., mazes), with plans to support other environment types in future releases.

---

## Installation

For now, clone the repo and build locally:

```bash
git clone https://github.com/guillermoleon16/SharpGVGP.git
cd SharpGVGP
dotnet build
```

NuGet packages will be published once the first learning agent trains successfully.

---

## Getting Started

1. Open **SharpGVGP.sln** in Visual Studio or your editor of choice.  
2. Navigate to the **SharpGVGP.Sandbox** project.  
3. Run **Program.cs** to see a sample maze run with a **RandomAgent**.  
4. Swap in **QLearningAgent** (or your own) by updating the sandbox’s DI wiring.

---

## Projects

Each subfolder under **src/** compiles into its own assembly (and eventual NuGet package):

- **SharpGVGP.Core**  
  Core interfaces: `IAgent`, `IEnvironment`, `IVisualizer`, `IRewardStrategy`.

- **SharpGVGP.Environments**  
  Grid-based worlds (e.g., `GridMazeEnvironment`) and helper types (e.g., `GridCoordinates`).

- **SharpGVGP.Agents**  
  Built-in agents under namespaces like `SharpGVGP.Agents.RL` (e.g., `QLearningAgent`).

- **SharpGVGP.Visualization**  
  Simple renderers (console, GUI stubs) and metrics display tools.

- **SharpGVGP.Sandbox**  
  A demo console app to wire environments, agents, and visualizers together.

---

## Usage

Below is a minimal example wiring a Q-learning agent to a maze:

```csharp
var env   = new GridMazeEnvironment(grid, new GridCoordinates(0, 0));
var agent = new QLearningAgent(env, alpha: 0.1, gamma: 0.99, epsilon: 0.3);
var viz   = new ConsoleVisualizer();

for (int ep = 1; ep <= 100; ep++)
{
  var state = env.Reset();
  double total = 0;
  bool done = false;

  while (!done)
  {
    var action = agent.ChooseAction(state);
    var (next, r, doneNow) = env.Step(action);
    agent.Observe(state, action, r, next, doneNow);
    viz.Render(next, action);
    state = next;
    total += r;
  }

  agent.Reset();
  viz.DisplayMetrics(ep, total);
}
```

---

## Features

| Component        | Description                                  |
|------------------|----------------------------------------------|
| Core Abstractions| `IAgent`, `IEnvironment`, `IVisualizer`      |
| Maze Env         | Discrete grid, walls, goals, reward logic    |
| Q-Learning Agent | ε-greedy, decaying ε, table-based updates    |
| Random Agent     | Baseline stochastic agent                     |
| Console Viz      | Step-by-step render + episode metrics       |

---

## Roadmap

- Add more agents (CBR, neural nets, evolutionary).  
- Support continuous and pixel-based environments.  
- Integrate a simple VGDL-inspired DSL for rapid game prototyping.  
- Publish NuGet packages with CI/CD and automated versioning.

---

## Contributing

1. Fork the repo.  
2. Create a feature branch (`git checkout -b feature/YourAgent`).  
3. Implement, test, and document.  
4. Open a PR against `main`.

Please adhere to the existing folder/project layout and code style.

---

## License

This project is licensed under the MIT License.  
See [LICENSE](LICENSE) for details.
