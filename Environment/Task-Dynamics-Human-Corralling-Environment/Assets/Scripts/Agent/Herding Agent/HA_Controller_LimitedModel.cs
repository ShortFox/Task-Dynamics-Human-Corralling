using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HA_Controller_LimitedModel : HA_Controller
{
    #region Private Fields
    protected Vector3 _initPos;                       // Initial position of this agent

    #region System Variables for Integration
    protected float _dt;                      // Timestep for integration. Will equate to Time.fixedDeltaTime if done in FixedUpdate
    protected float _HARt = 0.0f;             // Position Radius from Center
    protected float _HAdRdt = 0.0f;           // Velocity Radius from Center
    protected float _HAdRvdt = 0.0f;          // Acceleration Radius from Center
    protected float _HAThetat = 0.0f;         // Angle from Center
    protected float _HAdThetadt = 0.0f;       // Angle Velocity from Center
    protected float _HAdThetavdt = 0.0f;      // Angle Acceleration from Center

    protected Vector3 _HAPos;                         //Position of HA in task-space coordinates.
    protected Vector3 _targetPos;                     //The position the HA will move towards in task-space coordinates.
    protected float _targetRad;                       //The target radius HA will move towards.
    protected float _targetTheta;                     //The target theta HA will move towards.
    protected Vector3 _polRef = Vector3.zero;         //Reference pole for task-dynamic model.
    #endregion

    #region System Parameters
    protected float _b_rad = 10f;
    protected float _epsilon_rad = 64;
    protected float _offset_rad = 0.35f;

    protected float _b_theta = 10f;
    protected float _epsilon_theta = 64f;
    #endregion
    #endregion

    public HA_Controller_LimitedModel(Agent Self) : base(Self)
    {
    }

    #region Abstract Implementation
    protected override void SetActive()
    {
        // Implement starting behavior here.
        IsActive = true;
    }
    protected override void Reset()
    {

        // System state variables.
        _HARt = 0.0f;
        _HAdRdt = 0.0f;
        _HAdRvdt = 0.0f;
        _HAThetat = 0.0f;
        _HAdThetadt = 0.0f;
        _HAdThetavdt = 0.0f;

        base.Reset();   // Reset position
    }
    protected override void SetInactive()
    {
        // Implement ending behavior here.
        IsActive = false;
    }
    public override void UpdateState()
    {
        ComputeState();
    }
    #endregion

    #region Methods
    protected override void ComputeState()
    {
        if (!IsActive) return;     // Added security

        _dt = Time.deltaTime;

        // Set polar coordinate space to target agent herd's mean position (i.e., center of mass).
        _polRef = SimpleTask.Instance.TargetAgentsCOM;
        _polRef.y = 0;

        // Convert HA position to 2D in reference to task pole.
        _HAPos = Self.transform.position;
        _HAPos.y = 0;
        _HAPos -= _polRef;

        // If values are 0 (due to model reset), then compute state positions.
        if (_HARt == 0 && _HAThetat == 0)
        {
            _HARt = Vector3.Distance(Vector3.zero, _HAPos);
            _HAThetat = Mathf.Deg2Rad * Vector3.SignedAngle(_HAPos, Vector3.forward, Vector3.up);
        }

        // Select target for this agent to pursue in reference to herd's COM. Convert to 2D in reference to task pole.
        _targetPos = SelectTargetPosition(_polRef);
        _targetPos.y = 0;
        _targetPos -= _polRef;

        // Define target in terms of radial and angular position to pole.
        _targetRad = Vector3.Distance(Vector3.zero, _targetPos);
        _targetTheta = Mathf.Deg2Rad * Vector3.SignedAngle(_HAPos, _targetPos, Vector3.up);

        // Convert target angle to same reference frame as herding agent (in case abs(_HAThetat) > pi)
        _targetTheta += _HAThetat;

        // Radial dynamics
        _HAdRvdt = -_b_rad * _HAdRdt - _epsilon_rad * (_HARt - _targetRad);
        // Angular dynamics
        _HAdThetavdt = -_b_theta * _HAdThetadt - _epsilon_theta * (_HAThetat - _targetTheta);

        // Update Variables for next iteration
        _HAdRdt += (_HAdRvdt * _dt);
        _HARt += (_HAdRdt * _dt);
        _HAdThetadt += (_HAdThetavdt * _dt);
        _HAThetat += (_HAdThetadt * _dt);

        // Reset if errors
        if ((float.IsNaN(_HAThetat) || float.IsNaN(_HARt)) == true) Reset();                                                           //If for some reason a NaN value results, Reset variables.

        // Convert task-dynamic values to Cartesian coordinates and update position
        Self.transform.position = new Vector3(
            Mathf.Sin(_HAThetat) * _HARt + _polRef.x, 
            Self.transform.position.y, 
            Mathf.Cos(_HAThetat) * _HARt + _polRef.z
            );
    }
    #endregion

    #region Assistant Methods

    protected Vector3 SelectTargetPosition(Vector3 reference)
    {
        //First try to select object that is furthest from reference and closest to this Actor.
        TA_Controller ta_furthest = null;
        //If none of the objects are closer to this player, select the cloest agent.
        TA_Controller ta_closest = null;

        float maxDist = Mathf.NegativeInfinity;
        float minDist = Mathf.Infinity;

        foreach (TA_Controller ta in _targetAgents)
        {
            //Get Sheep furthest from reference and that is moving away.
            Vector3 velocity = ta.Body.velocity;
            velocity.y = 0;

            Vector3 position = ta.Body.transform.position;
            position.y = 0;

            if (ta is TA_Controller_LimitedModel)
            {
                TA_Controller_LimitedModel ta_limitedModel = (TA_Controller_LimitedModel)ta;

                // Check target agent that are subset closest to this herding agent and select the one furthest from reference that is moving away.
                if (ta_limitedModel.ClosestHA.name == Self.name)
                {
                    float distToRef = Vector3.Distance(reference, ta_limitedModel.Body.position + velocity);

                    if (distToRef > maxDist)
                    {
                        maxDist = distToRef;
                        ta_furthest = ta_limitedModel;
                    }
                }
                // If not a candidate, then check if the target agent is the closest to this herding agent.
                else
                {
                    float distToActor = Vector3.Distance(Self.transform.position, ta_limitedModel.Body.position + velocity);

                    if (distToActor < minDist)
                    {
                        minDist = distToActor;
                        ta_closest = ta_limitedModel;
                    }
                }
            }
        }

        TA_Controller target;

        if (ta_furthest == null) target = ta_closest;
        else target = ta_furthest;

        Vector3 pos_Direction = target.Body.position - reference;
        pos_Direction.y = 0;

        return target.Body.position + pos_Direction.normalized * _offset_rad;
    }
    #endregion
}
