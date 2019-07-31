using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Immovable : VGDLSprite {

	public Immovable()
	{
		color = VGDLColors.Gray;
		is_static = true;
	}
	
	public Immovable(Immovable from) : base(from) { }
}