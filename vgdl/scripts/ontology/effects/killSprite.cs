using System;

public class killSprite : VGDLEffect
{
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with KillSprite interaction");
        }

        //boolean variable set to false to indicate the sprite was not transformed
        game.killSprite(sprite1, false);
    }
}