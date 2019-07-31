using System;
using System.Collections.Generic;
using UnityEngine;

public class turnAround : VGDLEffect
{   
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with turnAround interaction.");
        }

        sprite1.rect = sprite1.lastrect;
        sprite1.lastmove = sprite1.cooldown;
        sprite1.physics.activeMovement(sprite1, VGDLUtils.VGDLDirections.DOWN.getDirection(), sprite1.speed);
        sprite1.lastmove = sprite1.cooldown;
        sprite1.physics.activeMovement(sprite1, VGDLUtils.VGDLDirections.DOWN.getDirection(), sprite1.speed);
        game.reverseDirection(sprite1);
        
        //NOTE: this might be a problem
        Debug.LogWarning("Not updating collision dictionary, might cause multiple reversals, please check");
        //game._updateCollisionDict(sprite1);
    }
}