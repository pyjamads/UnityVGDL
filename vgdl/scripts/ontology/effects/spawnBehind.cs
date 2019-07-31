using System;
using UnityEngine;

public class spawnBehind : VGDLEffect
{
    public string stype;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        var exists = game.getRegisteredSpriteConstructor(stype);

        if (exists == null)
        {
            throw new Exception("Undefined sprite " + stype);
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite2 == null){
            throw new ArgumentException("2nd sprite can't be EOS with SpawnBehind interaction.");
        }
	
        if(UnityEngine.Random.value >= prob) return;
        var lastPos = sprite2.getLastPosition();
        if (lastPos != Vector2.negativeInfinity) {
            game.addSprite(stype, lastPos);
        }
    }
}