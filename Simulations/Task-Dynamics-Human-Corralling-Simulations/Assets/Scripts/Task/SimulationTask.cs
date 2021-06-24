using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SimulationTask : SimpleTask
{
    // Buffer to contain data for writing
    public static List<string> Buffer;
    private static string _folderPath;

    public static SimulationTask Instance { get; private set; }
    public SimulationTask(Task Environment) : base(Environment)
    { 
        Instance = this;

        // Set up data writing components
        Buffer = new List<string>();
        _folderPath = Application.dataPath + "/Simulations";
        CreateDirectory();
    }

    // Flag that can be called by other components to force simulation iteration to end.
    public bool EndEarlyFlag;

    public override void Begin()
    {
        base.Begin();
        EndEarlyFlag = false;
    }
    public override void End()
    {
        base.End();
        FlushBuffer();
    }


    #region Simulation Parameters and Properties
    private int _trialRunsPerCombination = 10;
    private float _trialMaxLength = 120f;
    private float _samplesTotal;                        // Defines number of samples for simulation (_trialMaxLength / Time.fixedDeltaTime)
    private int _trialNum;                              // Current trial number for simulation combination

    private const int _decimalPlaces = 1000;            // float values are capped at this decimal place to reduce float point imprecision

    public float TA_MaxSpeed { get; private set; }      //Current TA_MaxSpeed
    private float[] TA_MaxSpeedArray = new float[3] { 0.12f, 0.20f, 0.28f };

    public float HA_Stiffness { get; private set; }     //Current HA_Stiffness
    private float HA_StiffnessStart = 2.5f;
    private float HA_StiffnessEnd = 10f;
    private float HA_StiffnessStepSize = 0.25f;

    public float HA_Dampening { get; private set; }     //Current HA_Dampening - determned from damping ratio

    public float HA_DampingRatio { get; private set; }  //Damping Ratio
    private float HA_DampingRatioStart = 0.5f;
    private float HA_DampingRatioEnd = 2f;
    private float HA_DampingRatioStepSize = 0.05f;

    public float HA_Offset { get; private set; }        //Current radial offset.
    private float HA_OffsetStart = 0.3f;
    private float HA_OffsetEnd = 0.4f;
    private float HA_OffsetStepSize = 0.05f;
    #endregion

    private IEnumerator CurrentCoroutine;             // Reference to coroutine that is active

    public override void Initialize()
    {
        if (IsActive) return;

        // Run Simulations at 100x real-time.
        Time.timeScale = 100;

        // Number of samples per simulation
        _samplesTotal = (int)(_trialMaxLength) / Time.fixedDeltaTime;

        if (CurrentCoroutine != null)
        {
            Environment.StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = null;
        }
        CurrentCoroutine = RunSimulations();
        Environment.StartCoroutine(CurrentCoroutine);
    }
    public override void CheckState()
    {
        // Used to access Update loop if needed.
    }
    private IEnumerator RunSimulations()
    {
        // Delay added to ensure all components are initialized
        yield return new WaitForFixedUpdate(); 

        // Set target agent maximum velocity.
        for (int i = 0; i <TA_MaxSpeedArray.Length;i++)
        {
            TA_MaxSpeed = TA_MaxSpeedArray[i];

            // Set herding agent stiffness
            for (float j =HA_StiffnessStart; (Mathf.Round(j* _decimalPlaces)/ _decimalPlaces <= HA_StiffnessEnd);j+=HA_StiffnessStepSize)
            {
                j = Mathf.Round(j * _decimalPlaces) / _decimalPlaces;
                HA_Stiffness = Mathf.Pow(j, 2);

                // Set herding agent damping
                for (float k = HA_DampingRatioStart; (Mathf.Round(k* _decimalPlaces)/ _decimalPlaces )<= HA_DampingRatioEnd;k+=HA_DampingRatioStepSize)
                {
                    k = Mathf.Round(k * _decimalPlaces) / _decimalPlaces;
                    HA_DampingRatio = k;
                    HA_Dampening = HA_DampingRatio * 2 * Mathf.Sqrt(HA_Stiffness);

                    // Set herding agent radial offset
                    for (float l =HA_OffsetStart;(Mathf.Round(l* _decimalPlaces)/ _decimalPlaces )<= HA_OffsetEnd;l+=HA_OffsetStepSize)
                    {
                        l = Mathf.Round(l * _decimalPlaces) / _decimalPlaces;
                        HA_Offset = l;

                        for (_trialNum = 1; _trialNum <= _trialRunsPerCombination; _trialNum++)
                        {

                            Begin();

                            for (int n=0;n<=_samplesTotal; n++)
                            {
                                // Check if trial ends prematurely
                                if (EndEarlyFlag) break;

                                // Write data here.
                                Buffer.Add(LatestDataString());
                                yield return new WaitForFixedUpdate();
                            }

                            End();
                        }

                    }
                }
            }
        }
        yield return null;
    }

    #region Data Writing Components
    private string _header;
    public string Header
    {
        get
        {
            if (_header == null)
            {
                string output = "";
                output += "TrialNum, TrialMaxTime, TrialMaxSamples, TA_MaxSpeed, HA_Stiffness, HA_DampingRatio, HA_Dampening, HA_Offset, UnityTime, Contained,";
                output += GetHeader(HerdingAgents) + ",";
                output += GetHeader(TargetAgents);
                _header = output;
            }
            return _header;
        }
    }
    public string LatestDataString()
    {
        string output = "";

        output += _trialNum + "," 
            + _trialMaxLength + "," 
            + _samplesTotal + "," 
            + TA_MaxSpeed.ToString("F2") + "," 
            + HA_Stiffness.ToString("F4") + "," 
            + HA_DampingRatio.ToString("F4") + "," 
            + HA_Dampening.ToString("F4") +"," 
            + HA_Offset.ToString("F4") + "," 
            + Time.timeSinceLevelLoad + "," 
            + (Contained?1:0).ToString() + ",";
        output += GetLatestDataString(HerdingAgents) + ",";
        output += GetLatestDataString(TargetAgents);

        return output;
    }
    public string GetHeader(Transform[] objs)
    {
        string output = "";

        for (int i = 0; i < objs.Length; i++)
        {
            output += (objs[i].name + "_X," + objs[i].name + "_Z");
            if (i < objs.Length - 1) output += ",";
        }
        return output;
    }
    public string GetLatestDataString(Transform[] objs)
    {
        string output = "";

        for (int i = 0; i < objs.Length; i++)
        {
            output += (objs[i].position.x + "," + objs[i].position.z);
            if (i < objs.Length - 1) output += ",";
        }

        return output;
    }
    private void CreateDirectory()
    {
        if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);
    }

    private void FlushBuffer()
    {
        if (Buffer.Count == 0) return;
        else
        {
            DateTime time = DateTime.Now;

            string filename = "";
            filename += "TASpd-" + TA_MaxSpeed.ToString("F2").Replace(".", "_") + "-";
            filename += "HAStiff-" + HA_Stiffness.ToString("F3").Replace(".", "_") + "-";
            filename += "HADampRatio-" + HA_DampingRatio.ToString("F3").Replace(".", "_") + "-";
            filename += "HAOffset-" + HA_Offset.ToString("F2").Replace(".", "_") + "-";
            filename += "TrialNum-" + _trialNum.ToString()+"-";
            filename += time.ToString("ddMM'-'HHmmss");
            filename += ".csv";


            using (StreamWriter sw = File.CreateText(Path.Combine(_folderPath, filename)))
            {
                sw.WriteLine(Header);

                for (int i = 0; i < Buffer.Count; i++) sw.WriteLine(Buffer[i]);
            }
            Buffer.Clear();
        }
    }
    #endregion
}