using System;

public class reverseDirection : VGDLEffect
{
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with reverseDirection interaction.");
        }

        game.reverseDirection(sprite1);
        //sprite1.orientation = sprite1.orientation * -1;
    }
}