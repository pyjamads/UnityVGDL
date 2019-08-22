using System;
using UnityEngine;

[Serializable]
public class VGDLGameAndLevel
{
    public VGDLExampleTypes type; 
    public string filename;
    public int level;
}

[Serializable]
public class VGDLGameAndLevelForTraining : VGDLGameAndLevel
{
    //[Tooltip("If true, agents that score higher than threshold will progress the curriculum. Otherwise it's threshold < step/maxSteps.")]
    //public bool useRewardMeasure;
    //[Tooltip("Minimum number of lessons, also used for threshold average. Eg. once the average of the last minLessonLength episodes is above the threshold.")]
    //public int minLessonLength;
    //[Tooltip("Value Threshold for measure to continue.")]
    //public float threshold;
    //[Tooltip("Whether the weight of the measure is weighted by the previous score 0.75(new) 0.25(old)")]
    //public bool signalSmoothing;
}

public enum VGDLExampleTypes
{
    Unspecified,
    GridPhysics,
    ContinuousPhysics,
    TwoPlayer,
}