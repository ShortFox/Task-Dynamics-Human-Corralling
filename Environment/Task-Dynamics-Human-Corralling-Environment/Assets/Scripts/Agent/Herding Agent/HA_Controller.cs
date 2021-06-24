using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HA_Controller : Agent.AgentController 
{
    #region Private Fields
    private Vector3 _initPos;                       // Initial position of this agent
    #endregion

    protected TA_Controller[] _targetAgents;        // The target agents.
    protected HA_Controller[] _partnerHerdingAgents; // Other herding agents.


    public HA_Controller(Agent Self) : base(Self)
    {
        _initPos = Self.transform.position;
        _targetAgents = GetTargetAgents();
        _partnerHerdingAgents = GetPartnerHerdingAgents();
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
        _partnerHerdingAgents = GetPartnerHerdingAgents();
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
    private HA_Controller[] GetPartnerHerdingAgents()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Herding Agent");
        HA_Controller[] output = new HA_Controller[objs.Length-1];
        int indx = 0;
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] == Self.gameObject) continue;

            Agent.AgentController controller = objs[i].GetComponent<Agent>().MyController;
            output[indx] = (HA_Controller)controller;
            indx++;
        }
        return output;
    }
    #endregion
}
