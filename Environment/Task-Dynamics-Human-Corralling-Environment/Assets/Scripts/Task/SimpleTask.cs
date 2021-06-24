using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTask : Task.TaskDirector 
{
    private Material _targetMaterial;
    private Color _uncontainedColor = new Color(0.83f, 0.83f, 0.83f, 1);
    private Color _containedColor = new Color(0.83f, 0f, 0f, 1);
    public bool Contained { get; protected set; }

    public Transform[] HerdingAgents { get; protected set; }
    public Transform[] TargetAgents { get; protected set; } // That target agents that must be contained.
    protected float _containmentThreshold = 0.72f;          // The distance all target agents must be from COM to be considered contained.
    public Vector3 TargetAgentsCOM { get; protected set; }  // The target agents' mean position (i.e., center of mass)

    private IEnumerator CurrentCoroutine;             // Reference to coroutine that is active

    public static SimpleTask Instance { get; private set; }
    public SimpleTask(Task Environment) : base(Environment)
    {
        Instance = this;

        IsActive = false;
        _targetMaterial = Resources.Load<Material>("Materials/TargetMaterial");
        if (_targetMaterial == null) Debug.LogError("Error: Could not locate TargetMaterial");
        HerdingAgents = GetAgents("Herding Agent");
        TargetAgents = GetAgents("Target Agent");
    }

    #region Abstract Implementation
    public override bool IsActive { get; protected set; }

    public override void Initialize()
    {
        Reset();
    }

    public override void Begin()
    {
        Reset();
        IsActive = true;

        if (CurrentCoroutine != null)
        {
            Environment.StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }
        CurrentCoroutine = CheckContainment();

        Environment.StartCoroutine(CurrentCoroutine);
        RaiseEventBeginTask();
    }
    public override void Reset()
    {
        _targetMaterial.color = _uncontainedColor;
        RaiseEventResetAgent();
    }

    public override void End()
    {
        IsActive = false;
        if (CurrentCoroutine != null)
        {
            Environment.StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }
        Reset();
        RaiseEventEndTask();
    }

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

    #endregion

    #region Methods
    private Transform[] GetAgents(string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        Transform[] output = new Transform[objs.Length];
        for (int i = 0; i < objs.Length; i++) output[i] = objs[i].transform;
        return output;
    }

    private IEnumerator CheckContainment()
    {
        while (true)
        {
            TargetAgentsCOM = UpdateTargetAgentsCOM();

            float maxDist = 0;
            float distance = 0;
            foreach (Transform targetAgent in TargetAgents)
            {
                distance = Vector3.Distance(TargetAgentsCOM, targetAgent.position);
                if (distance > maxDist) maxDist = distance;
            }

            if (maxDist <= _containmentThreshold)
            {
                _targetMaterial.color = _containedColor;
                Contained = true;
            }
            else
            {
                _targetMaterial.color = _uncontainedColor;
                Contained = false;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private Vector3 UpdateTargetAgentsCOM()
    {
        Vector3 meanPosition = Vector3.zero;
        foreach (Transform targetAgent in TargetAgents)
        {
            meanPosition += targetAgent.position;
        }

        meanPosition /= TargetAgents.Length;
        return meanPosition;
    }
    #endregion
}
 