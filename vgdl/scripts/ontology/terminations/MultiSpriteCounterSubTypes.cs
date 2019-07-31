using System;
using System.Linq;
using UnityEngine;

public class MultiSpriteCounterSubTypes : VGDLTermination
{
    public string stype;
    //NOTE: stype vs stype1 inconsistency between framework VGDL 1.0 (pyVGDL) and VGDL 2.0 (GVGAI JavaVGDL)
    public string stype1;
    public int subTypesNum = -1;

    public override void Validate(VGDLGame game)
    {
   
        if(!string.IsNullOrEmpty(stype1) && !string.IsNullOrEmpty(stype)){
            throw new ArgumentException("stype and stype1 argument undefined for MultiSpriteCounterSubTypes, please define one of them!");
        }

        //Use stype by default, or override it with stype1
        if (string.IsNullOrEmpty(stype))
        {
            stype = stype1;
        }

        var type = game.getRegisteredSpriteConstructor(stype, true);

        if (type == null)
        {
            throw new ArgumentException("Failed to find stype/stype1 definition for " + stype);
        }    
    }
    
    public override bool isDone(VGDLGame game)
    {
        if (base.isDone(game)) return true;

        var sum = game.getNumberOfSprites(stype, false);

        if(sum == limit && canEnd) {

            var subtypes = game.getSubTypes(stype);
            int subTypeSum = 0;
            foreach (var subtype in subtypes) {

                var count = (game.getNumberOfSprites(subtype, false));
                if (count <= 0) continue;
                
                if(game.getSprites(subtype).Count != 0) //This avoids non-terminal types
                {
                    subTypeSum += count > 0 ? 1 : 0;
                }
            }

            if(subTypeSum == subTypesNum) {
                countScore(game);
                
                if (VGDLParser.verbose && subTypeSum <= subTypesNum)
                {
                    Debug.Log("MultiSpriteCounterSubTypes subTypesNum("+subTypesNum+") condition met: "+subTypeSum);
                }
                return true;
            }
        }


        return false;
        
    }
}