using System;
using UnityEngine;

public class cloneSprite : VGDLEffect
{
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with cloneSprite interaction.");
        }
        
        var type = sprite1.getType();
        var pos = sprite1.getPosition();
        game.addSprite(type, pos);
    }
}