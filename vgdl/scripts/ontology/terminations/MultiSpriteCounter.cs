using System;
using System.Linq;
using UnityEngine;

public class MultiSpriteCounter : VGDLTermination
{
    
    public string stype1, stype2, stype3;
    public string[] stypes;
    public bool min;

    public override void Validate(VGDLGame game)
    {
        if (stypes == null)
        {
            int spriteCnt = 0;
            spriteCnt += !string.IsNullOrEmpty(stype1) ? 1 : 0;
            spriteCnt += !string.IsNullOrEmpty(stype2) ? 1 : 0;
            spriteCnt += !string.IsNullOrEmpty(stype3) ? 1 : 0;
            
            if (spriteCnt > 0)
            {
                stypes = new string[spriteCnt];
                for (int i = 0; i < spriteCnt; i++)
                {
                    if (i < 1 && !string.IsNullOrEmpty(stype1))
                    {
                        if (game.getRegisteredSpriteConstructor(stype1, true) == null)
                        {
                            throw new ArgumentException("Failed to find stype1 definition for "+stype1);
                        }
                        stypes[i] = stype1;
                        continue;
                    }
                     
                    if (i < 2 && !string.IsNullOrEmpty(stype2))
                    {
                        if (game.getRegisteredSpriteConstructor(stype2, true) == null)
                        {
                            throw new ArgumentException("Failed to find stype2 definition for "+stype2);
                        }
                        stypes[i] = stype2;
                        continue;
                    }
                    
                    if (i < 3 && !string.IsNullOrEmpty(stype3))
                    {
                        if (game.getRegisteredSpriteConstructor(stype3, true) == null)
                        {
                            throw new ArgumentException("Failed to find stype3 definition for "+stype3);
                        }
                        stypes[i] = stype3;
                    }
                }
            }
            else
            {
                throw new ArgumentException("stype1/stype2/stype3 and stypes argument undefined for MultiSpriteCounter");    
            }
        }

        if (VGDLParser.verbose)
        {
            Debug.Log("MultiSpriteCounter validated with stypes: "+string.Join(", ", stypes));
        }
    }
    
    public override bool isDone(VGDLGame game)
    {
        if (base.isDone(game)) return true;

        //TODO: do something with min and count_score
        var sum = 0;
        foreach (var stype in stypes)
        {
            sum += game.getNumberOfSprites(stype, false);
        }
        
        //NOTE: What if two sprites are spawned in the same frame? 
        if(sum == limit && canEnd) {
            countScore(game);
            
            if (VGDLParser.verbose)
            {
                Debug.Log("MultiSpriteCounter limit("+limit+") condition met: "+sum);
            }
            return true;
        }
        
        if(min && sum > limit && canEnd) {
            countScore(game);
            return true; //If the limit is a lower bound in what's required.
        }
        
        return false;
    }
}
