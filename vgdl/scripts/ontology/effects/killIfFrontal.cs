using System;
using UnityEngine;

public class killIfFrontal : VGDLEffect
{
    public killIfFrontal()
    {
        is_kill_effect = true;
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with KillIfFrontal interaction.");
        }
	
        //Kills the sprite, only if they are going in opposite directions or sprite1 is static.
        var firstV = sprite1.lastDirection();
        var otherV = sprite2.lastDirection();

        firstV.Normalize();
        otherV.Normalize();

        //If the sum of the two vectors (normalized) is (0.0), directions are opposite.
        var sumDir = new Vector2(firstV.x + otherV.x, firstV.y + otherV.y);
        var firstDir = new Vector2(firstV.x, firstV.y);

        applyScore=false;
        if( firstDir.Equals(VGDLUtils.VGDLDirections.NONE.getDirection()) || (sumDir.Equals(VGDLUtils.VGDLDirections.NONE.getDirection())))
        {
            applyScore=true;
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(sprite1, false);
        }
    }
}