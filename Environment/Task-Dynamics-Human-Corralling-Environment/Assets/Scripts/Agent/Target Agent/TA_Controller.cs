using UnityEngine;
using System.Collections;

public class TA_Controller : Agent.AgentController
{
    #region Public Fields and Properties
    public static float MaxVelocity = 0.20f;        // Maximum velocity for this agent. Setting this sets the value for all TA_Controllers
    #endregion

    #region Private Fields
    private Vector3 _initPos;                       // Initial position of this agent

    // Dynamics-related fields
    protected Transform[] _herdingAgents;             // The herding agents that can threaten this agent
    private Vector3 _forceVector;                   // Force vector acting on agent
    private float _repelDistThreshold = 0.6f;       // Distance threshold for when this agent is threatened
    private float _repelFactor = 500f;              // Repulsion multiplication factor when thsi agent is threatened

    private float _maxRepelForce = 36f;             // Clamped repulsion magnitude when threatened
    private float _maxRandomForce = 12f;            // Clamped repulsion magnitude when not threatened
    #endregion

    public TA_Controller(Agent Self) : base(Self) 
    {
        IsActive = false;
        Body = Self.GetComponent<Rigidbody>();
        _initPos = Self.transform.position;
    }

    #region Abstract Implementation
    public override bool IsActive { get; protected set; }
    public override Rigidbody Body { get; protected set; }

    protected override void SetActive()
    {
        IsActive = true;
    }
    protected override void Reset()
    {
        _forceVector = Vector3.zero;
        Body.velocity = Vector3.zero;
        Body.angularVelocity = Vector3.zero;
        Body.transform.position = _initPos;

        _herdingAgents = GetHerdingAgents();
        Physics.IgnoreLayerCollision(Body.gameObject.layer, _herdingAgents[0].gameObject.layer);
    }
    protected override void SetInactive()
    {
        IsActive = false;
    }
    public override void UpdateState()
    {
        ComputeState();
    }
    #endregion

    #region Methods
    private Transform[] GetHerdingAgents()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Herding Agent");
        Transform[] output = new Transform[objs.Length];
        for (int i = 0; i < objs.Length; i++) output[i] = objs[i].transform;
        return output;
    }
    protected virtual void ComputeState()
    {
        Vector3 threatenedForceVector = Vector3.zero;

        Vector3 posToHA = Vector3.zero;
        foreach (Transform ha in _herdingAgents)
        {
            posToHA = (Body.transform.position - ha.position);

            if (posToHA.magnitude < _repelDistThreshold)
            {
                threatenedForceVector += posToHA.normalized * _repelFactor * (_repelDistThreshold / posToHA.magnitude);
            }
        }
        threatenedForceVector.y = 0;        // Only apply forces to (x,z) plane.

        if (threatenedForceVector == Vector3.zero)
        {
            _forceVector += RandomMotion();
            AddForce(_maxRandomForce, false);
        }
        else
        {
            _forceVector = threatenedForceVector;
            AddForce(_maxRepelForce, true);
        }
        Body.velocity = Vector3.ClampMagnitude(Body.velocity, MaxVelocity);
    }
    /// <summary>
    /// Add Force to this agent.
    /// </summary>
    /// <param name="maxForce">Maximum Force Allowed.</param>
    /// <param name="reset">Resets force information.</param>
    private void AddForce(float maxForce, bool reset)
    {
        _forceVector = Vector3.ClampMagnitude(_forceVector, maxForce);
        Body.AddForce(_forceVector * Time.fixedDeltaTime);

        if (reset) _forceVector = Vector3.zero;
    }

    /// <summary>
    /// Apply random force to this agent.
    /// </summary>
    /// <returns></returns>
    private Vector3 RandomMotion()
    {
        return new Vector3(Random.value - .5f, 0.0f, Random.value - .5f);
    }
    #endregion
}