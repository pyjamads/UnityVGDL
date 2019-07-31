using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public abstract class VGDLDecision : Decision
{
	/// <summary>
	/// Use sparingly to fetch forward model.
	/// </summary>
	/// <returns></returns>
	protected VGDLGame ForwardModel()
	{
		//TODO: improve heuristics forward model passing.
		//This will just fetch a random one from the scene.
		var runner = (VGDLRunner)FindObjectOfType(typeof(VGDLRunner));
		return runner.game.fwdModel;
	}
}
