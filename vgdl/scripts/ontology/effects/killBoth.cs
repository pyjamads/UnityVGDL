using System;

public class killBoth : VGDLEffect
{
    public killBoth()
    {
        is_kill_effect = true;
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null && sprite2 == null)
        {
            throw new ArgumentException("1st and 2nd sprite can't be EOS with killBoth interaction.");
        }

        game.killSprite(sprite1, false);
        game.killSprite(sprite2, false);
    }
}
