using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HA_Controller : Agent.AgentController 
{
    #region Private Fields
    private Vector3 _initPos;                       // Initial position of this agent
    #endregion

    protected TA_Controller[] _targetAgents;        // The target agents.


    public HA_Controller(Agent Self) : base(Self)
    {
        _initPos = Self.transform.position;
        _targetAgents = GetTargetAgents();
    }

    #region Abstract Implementation
    public override bool IsActive { get; protected set; }

    public override Rigidbody Body
    {
        // Herding Agent does not have a Rigidbody attached.
        get
        {
            throw new System.NotImplementedException();
        }
        protected set { }
    }
    protected override void SetActive()
    {
       // Implement starting behavior here.
    }
    protected override void Reset()
    {
        Self.transform.position = _initPos;
        _targetAgents = GetTargetAgents();
    }
    protected override void SetInactive()
    {
        // Implement ending behavior here.
    }
    public override void UpdateState()
    {
        ComputeState();
    }
    #endregion

    #region Methods
    protected virtual void ComputeState()
    {
        // Implement HA state code here.
    }
    private TA_Controller[] GetTargetAgents()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Target Agent");
        TA_Controller[] output = new TA_Controller[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            Agent.AgentController controller = objs[i].GetComponent<Agent>().MyController;
            output[i] = (TA_Controller)controller;
        }
        return output;
    }
    #endregion
}
