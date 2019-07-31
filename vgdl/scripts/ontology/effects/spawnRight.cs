using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class spawnRight : VGDLEffect
{
    public string stype;
    public bool stepBack;
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite2 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with SpawnBehind interaction.");
        }
	
        if(Random.value >= prob) return;
        var currentPos = Vector2.negativeInfinity;
        if (stepBack)
            currentPos = sprite2.getLastPosition();
        else
            currentPos = sprite2.getPosition();
        var dir = new Vector2(1,0)  * game.block_size;
        if (currentPos != Vector2.negativeInfinity) {
            Vector2 nextPos = currentPos + dir;
            game.addSprite(stype, nextPos);
        }
    }
}