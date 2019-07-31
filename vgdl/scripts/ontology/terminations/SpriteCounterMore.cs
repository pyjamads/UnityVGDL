using System;
using UnityEngine;

public class SpriteCounterMore : VGDLTermination
{
    public string stype;

    public override void Validate(VGDLGame game)
    {
        if (String.IsNullOrEmpty(stype))
        {
            throw new ArgumentException("stype argument undefined for SpriteCounterMore");
        }
        
        if (game.getRegisteredSpriteConstructor(stype, true) == null)
        {
            throw new ArgumentException("Failed to find stype definition for "+stype);
        }
    }
    
    public override bool isDone(VGDLGame game)
    {
        if (base.isDone(game)) return true;

        var sum = game.getNumberOfSprites(stype, false);
        
        if(sum >= limit && canEnd) {
            countScore(game);
            
            if (VGDLParser.verbose)
            {
                Debug.Log("SpriteCounterMore ("+limit+") condition met: "+sum);
            }
            return true;
        }

        return false;
    }
}