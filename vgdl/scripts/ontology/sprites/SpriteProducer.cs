using System.Collections.Generic;
using UnityEngine;

public class SpriteProducer : VGDLSprite
{
	public string stype;
	public SpriteProducer() { }

	public SpriteProducer(SpriteProducer from) : base(from)
	{
		stype = from.stype;
	}
}