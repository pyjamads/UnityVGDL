using System;

public class killIfAlive : VGDLEffect
{
    public killIfAlive()
    {
        is_kill_effect = true;
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither 1st not 2nd sprite can be EOS with KillIfAlive interaction.");   
        }

        if (!game.killList.Contains(sprite2))
        {
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(sprite1, false);
        }
    }
}