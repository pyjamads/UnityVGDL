using System;
using UnityEngine;


// Anti termination: this prevents counter terminations from triggering if certain conditions are met.
public class StopCounter : VGDLTermination
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
                throw new ArgumentException("stype1/stype2/stype3 and stypes argument undefined for StopCounter");    
            }
        }

        if (VGDLParser.verbose)
        {
            Debug.Log("StopCounter validated with stypes: "+string.Join(", ", stypes));
        }
    }
    
    public override bool isDone(VGDLGame game)
    {
        if (base.isDone(game)) return true;

        var sum = 0;
        foreach (var stype in stypes)
        {
            sum += game.getNumberOfSprites(stype, false);
        }

        if (min) {
            canEnd = sum <= limit;
        }
        else {
            canEnd = sum != limit;
        }
        
        if (VGDLParser.verbose && !canEnd)
        {
            Debug.Log("StopCounter limit("+limit+") condition met: "+sum+" disabling canEnd");
        }

        //this never actually returns true, it just adjusts the canEnd variable.
        return false;
    }
}