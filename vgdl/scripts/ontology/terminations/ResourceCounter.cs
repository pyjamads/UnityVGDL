using System;
using UnityEngine;

//This class was added by Mads Johansen, allowing a game to terminate when a certain resource reaches a given amount.
public class ResourceCounter : VGDLTermination
{
    public string rtype;
    public string stype;
    public bool min;
    
    public override void Validate(VGDLGame game)
    {
        
        if (string.IsNullOrEmpty(rtype) || string.IsNullOrEmpty(stype))
        {
            throw new ArgumentException("stype and rtype must be defined on ResourceCounter");
        }
        
        if (game.getRegisteredSpriteConstructor(stype, true) == null)
        {
            throw new ArgumentException("Failed to find stype definition for "+stype);
        }
    }

    public override bool isDone(VGDLGame game)
    {
        if(base.isDone(game)) return true;

        var counters = game.getSprites(stype);

        var sum = 0;
        foreach (var counter in counters)
        {
            if (counter.resources.ContainsKey(rtype))
            {
                sum += counter.resources[rtype];    
            }
        }

        if (sum >= limit)
        {
            countScore(game);
            if (VGDLParser.verbose)
            {
                Debug.Log("ResourceCounter limit("+limit+") condition met: "+sum);
            }

            return true;
        }

        if (min && sum <= limit)
        {
            countScore(game);
            if (VGDLParser.verbose)
            {
                Debug.Log("ResourceCounter limit("+limit+") condition met: "+sum);
            }
            return true;
        }

        return false;
    }
}