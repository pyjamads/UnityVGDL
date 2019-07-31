using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VGDLHeuristicDescision : VGDLDecision {

	public override float[] Decide(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
	{
		var model = ForwardModel();
		
		//Return Nil
		return new float[]{0};
	}

	public override List<float> MakeMemory(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
	{
		//Pass memory, and ignore inputs
		return memory;
	}
}
