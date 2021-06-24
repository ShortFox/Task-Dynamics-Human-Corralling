using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour 
{
	public abstract class AgentController
    {
		public Agent Self;

		public AgentController(Agent self)
        {
			Self = self;
        }

		public abstract bool IsActive { get; protected set; }	// Gets active state of controller
		public abstract Rigidbody Body { get; protected set; } // This rigidbody associated with this agent.
		protected abstract void SetActive();		// Activate the agent
		protected abstract void Reset();           // Resets the agent
		protected abstract void SetInactive();      // Deactivates the agent
		public abstract void UpdateState();     // Update the state of the agent

		public virtual void EventsListen()
		{
			Task.TaskDirector.OnBeginTask += SetActive;
			Task.TaskDirector.OnResetAgent += Reset;
			Task.TaskDirector.OnEndTask += SetInactive;
		}

		public virtual void EventsStopListening()
		{
			Task.TaskDirector.OnBeginTask -= SetActive;
			Task.TaskDirector.OnResetAgent -= Reset;
			Task.TaskDirector.OnEndTask -= SetInactive;
		}
	}

	public AgentController MyController { get; private set; }

	void Awake()
    {
		switch(this.tag)
        {
			case "Herding Agent":
				MyController = new HA_Controller(this);					// Your own controller
				//MyController = new HA_Controller_LimitedModel(this);		// Controller introduced in Eqs. 1 and 2 in paper.
				break;
			case "Target Agent":
				MyController = new TA_Controller(this);					// Basic TA Controller
				//MyController = new TA_Controller_LimitedModel(this);		// Gives information about who is the nearest HA controller
				break;
			default:
				Debug.LogError("Agent attached to unexpected object. Did you define the object's tag?");
				break;
        }
    }
	void FixedUpdate()
    {
		if (MyController.IsActive) MyController.UpdateState();
    }
	void Start()
    {
		MyController.EventsListen();
    }
	void OnDestroy()
    {
		MyController.EventsStopListening();
	}
}
