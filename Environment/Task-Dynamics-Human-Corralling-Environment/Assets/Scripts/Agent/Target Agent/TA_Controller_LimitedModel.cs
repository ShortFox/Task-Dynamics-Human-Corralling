using UnityEngine;
using System.Collections;

public class TA_Controller_LimitedModel : TA_Controller
{
    public Transform ClosestHA { get; private set; }
    public TA_Controller_LimitedModel(Agent Self) : base(Self) { }

    protected override void Reset()
    {
        base.Reset();
        ClosestHA = UpdateClosestHA();
    }
    protected override void ComputeState()
    {
        ClosestHA = UpdateClosestHA();
        base.ComputeState();
    }
    #region Assistant Methods
    /// <summary>
    /// Return transform of the closest herding agent to this object
    /// </summary>
    /// <returns>Transform of closest herding agent to this object</returns>
    private Transform UpdateClosestHA()
    {
        Transform closestHA = null;

        float minDist = Mathf.Infinity;
        foreach (Transform ha in _herdingAgents)
        {
            float distToHA = Vector3.Distance(Self.transform.position, ha.position);
            if (distToHA < minDist)
            {
                minDist = distToHA;
                closestHA = ha;
            }
        }

        return closestHA;
    }
    #endregion
}