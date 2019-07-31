using System;

public class killIfFromAbove : VGDLEffect
{
    public killIfFromAbove()
    {
        is_kill_effect = true;
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with KillIfFromAbove interaction.");
        }
	
        //Kills the sprite, only if the other one is higher and moving down.
        var otherHigher = sprite1.lastrect.yMin > (sprite2.lastrect.yMin+(sprite2.rect.height/2));
        var goingDown = sprite2.rect.yMin > sprite2.lastrect.yMin;

        applyScore=false;
        if (otherHigher && goingDown)
        {
            applyScore=true;
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(sprite1, false);
        }
    }
}