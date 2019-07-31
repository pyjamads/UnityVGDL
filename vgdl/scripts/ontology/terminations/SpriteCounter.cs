using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCounter : VGDLTermination
{
    public string stype;
 
    public override void Validate(VGDLGame game)
    {
        if (String.IsNullOrEmpty(stype))
        {
            throw new ArgumentException("stype argument undefined for SpriteCounter");
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

        if (sum <= limit && canEnd)
        {
            if (VGDLParser.verbose)
            {
                Debug.Log("SpriteCounter limit("+limit+") condition met: "+sum);
            }
            
            countScore(game);
            return true;
        }
        
        return false;
    }
}