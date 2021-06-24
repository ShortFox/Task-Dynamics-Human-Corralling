using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task : MonoBehaviour 
{
	public abstract class TaskDirector
    {
		public Task Environment;

		public TaskDirector(Task self)
		{
			Environment = self;
		}

		#region Events
		// Event to signal Task Start
		public delegate void BeginTaskAction();
		public static event BeginTaskAction OnBeginTask;

		// Event to signal Agent reset
		public delegate void AgentResetAction();
		public static event AgentResetAction OnResetAgent;

		// Event to signal Task End
		public delegate void EndTaskAction();
		public static event EndTaskAction OnEndTask;

		public abstract void Initialize();
		public abstract void Begin();
		public abstract void CheckState();
		public abstract void Reset();
		public abstract void End();

		protected void RaiseEventBeginTask()
		{
			OnBeginTask();
		}
		protected void RaiseEventResetAgent()
        {
			OnResetAgent();
        }
		protected void RaiseEventEndTask()
        {
			OnEndTask();
        }
		#endregion
		public abstract bool IsActive { get; protected set; }  // Gets active state of Task
	}

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
}
