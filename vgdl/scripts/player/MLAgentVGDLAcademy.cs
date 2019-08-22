using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;
using UnityEngine.Serialization;

public class MLAgentVGDLAcademy : Academy {

	[Header("VGDL games/levels to train on")]
	[Tooltip("Select items randomly from curriculum when training.")]
	public bool RandomOrder;
	
	[Tooltip("Currently training on Index")]
	public int CurrentIndex;
	
	//[Tooltip("Current repetition")]
	//public int CurrentRepetition;
	
	[Tooltip("Curriculum to train on")] 
	public VGDLGameAndLevelForTraining[] Curriculum;
	
	// Signals to all the agents at each environment step so they can reset
	// if their flag has been set to done (assuming the agent has requested a 
	// decision).
	public VGDLGameAndLevel GetNextGameAndLevel()
	{
		//TODO: hook into the curriculum system in MLAgents?
		//TODO: use the features of our curriculum (which is basically the same as the MLAgent features).
		//if (CurrentRepetition > Curriculum[CurrentIndex].minLessonLength)
		//{
		//	CurrentRepetition = 0;
			
		//Select next game+level
		if (RandomOrder)
		{
			CurrentIndex = Random.Range(0, Curriculum.Length);
		}
		else
		{
			CurrentIndex++;
			if (CurrentIndex == Curriculum.Length)
			{
				CurrentIndex = 0;
				//Done();
			}
		}

		//}
		// else
		// {
		// 	//Update repetition count and return the current game+level.
		// 	CurrentRepetition++;
		// }
		
		return Curriculum[CurrentIndex];
	}
	
	public override void InitializeAcademy()
	{
		//Monitor.SetActive(true);
		//NOTE: for debug uncomment the above, and put the below anywhere in code.
		//https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Feature-Monitor.md
		//Monitor.Log(key, value, target)
	}

	public override void AcademyStep()
	{

	}

	public override void AcademyReset()
	{
		
	}
}