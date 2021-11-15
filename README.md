# Task-Dynamics-Human-Corralling
Repository for the paper titled "Task Dynamics Define the Contextual Emergence of Human Corralling Behaviors"

## Reference
Nalepka, P., Silva, P.L., Kallen, R. W., Shockley, K., Chemero, A., Saltzman, E. & Richardson, M. J. (2021). Task dynamics define the contextual emergence of human corralling behaviors. *PLoS ONE*, *16*(11), e0260046. https://doi.org/10.1371/journal.pone.0260046

## Contents
- [Environment](#environment)
- [Simulations](#simulations)
- [Human Data Playback Software](#human-data-playback-software)

## Environment

The task environment can be found in [Environment](https://github.com/ShortFox/Task-Dynamics-Human-Corralling/tree/main/Environment/Task-Dynamics-Human-Corralling-Environment). This project was tested using [Unity 2017.4.40f](https://unity3d.com/get-unity/download/archive).

### Task Director

[Task.cs](https://github.com/ShortFox/Task-Dynamics-Human-Corralling/blob/main/Environment/Task-Dynamics-Human-Corralling-Environment/Assets/Scripts/Task/Task.cs) defines a `TaskDirector` (e.g., `SimpleTask`) which manages the task. Once defined, the state of the `TaskDirector` is assessed every frame.

```csharp
public TaskDirector MyDirector;

private void Awake()
{
  MyDirector = new SimpleTask(this);
}
private void Start()
{
  MyDirector.Initialize();
}
private void Update()
{
  MyDirector.CheckState();
}
```

Within `SimpleTask`, `CheckState()` is overridden to either reset the environment (R key), or toggle whether the task is active (SPACE key).

```csharp
public override void CheckState()
{
    if (Input.GetKeyDown(KeyCode.R))
    {
        Reset();
    }
    else if (Input.GetKeyDown(KeyCode.Space))
    {
        if (IsActive) End();
        else Begin();
    }
}
```

There are also events (defined in `TaskDirector`) that agents can subscribe to

```csharp
// Event to signal Task Start
public delegate void BeginTaskAction();
public static event BeginTaskAction OnBeginTask;

// Event to signal Agent reset
public delegate void AgentResetAction();
public static event AgentResetAction OnResetAgent;

// Event to signal Task End
public delegate void EndTaskAction();
public static event EndTaskAction OnEndTask;
```

### Agent

[Agent.cs](https://github.com/ShortFox/Task-Dynamics-Human-Corralling/blob/main/Environment/Task-Dynamics-Human-Corralling-Environment/Assets/Scripts/Agent/Agent.cs) defines an `AgentController` which defines the behavior of the herding and target agents. This is set in the `Awake()` method.

```csharp
void Awake()
{
switch(this.tag)
    {
      case "Herding Agent":
        MyController = new HA_Controller(this);                   // Your own controller
        //MyController = new HA_Controller_LimitedModel(this);    // Controller introduced in Eqs. 1 and 2 in paper.
        break;
      case "Target Agent":
        MyController = new TA_Controller(this);                   // Basic TA Controller
        //MyController = new TA_Controller_LimitedModel(this);    // Gives information about who is the nearest HA controller
        break;
      default:
        Debug.LogError("Agent attached to unexpected object. Did you define the object's tag?");
        break;
    }
}
```
#### Herding Agents (HAs)

Implement your own HA code in [HA_Controller.cs](https://github.com/ShortFox/Task-Dynamics-Human-Corralling/blob/main/Environment/Task-Dynamics-Human-Corralling-Environment/Assets/Scripts/Agent/Herding%20Agent/HA_Controller.cs). Minimally, define/override `ComputeState()` which is called every fixed timestep (via `UpdateState()`).

#### Target Agents (TAs)

[TA_Controller.cs](https://github.com/ShortFox/Task-Dynamics-Human-Corralling/blob/main/Environment/Task-Dynamics-Human-Corralling-Environment/Assets/Scripts/Agent/Target%20Agent/TA_Controller.cs) defines the TAs' behavior. The forces acting upon the TAs are defined in `ComputeState()`.

## Simulations

The software used for the simulations can be found in [Simulations](https://github.com/ShortFox/Task-Dynamics-Human-Corralling/tree/main/Simulations/Task-Dynamics-Human-Corralling-Simulations). This code is an extension of the [Environment](https://github.com/ShortFox/Task-Dynamics-Human-Corralling/tree/main/Environment/Task-Dynamics-Human-Corralling-Environment) Unity project. This project was tested using [Unity 2017.4.40f](https://unity3d.com/get-unity/download/archive).

The code responsible for the simulations is found in [SimulationTask.cs](https://github.com/ShortFox/Task-Dynamics-Human-Corralling/blob/main/Simulations/Task-Dynamics-Human-Corralling-Simulations/Assets/Scripts/Task/SimulationTask.cs) which derives from `SimpleTask`. The simulations are conducted within the `RunSimulations()` coroutine.

## Human Data Playback Software

The data collected in the human experiment can be publicly accessed at [Open Science Framework](https://osf.io/w4bae/).

Software to play back the experiment data is found in [Human-Data-Playback-Software](https://github.com/ShortFox/Task-Dynamics-Human-Corralling/tree/main/Human-Data-Playback-Software/Windows). Note, the software is currently only available for Windows.

### Instructions

1. Launch *Task-Dynamics-Human-Corralling-Playback.exe*.
2. Select *Open File* and select a .csv file from a particular trial.
3. The software will automatically play back the data. Use the LEFT or RIGHT keyboard arrow keys to toggle between a scene view and the first-person perspective of each participant.
4. Press ESC to return to the title screen. When a trial is over, the title screen will automatically appear.
5. To close, press *Quit* or exit the window.

## Contact

If you have any questions or would like to discuss this research, please contact Dr. Patrick Nalepka ([ShortFox](https://github.com/ShortFox)) at <patrick.nalepka@mq.edu.au>.
