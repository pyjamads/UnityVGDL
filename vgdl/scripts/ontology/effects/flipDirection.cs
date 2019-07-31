using System;

public class flipDirection : VGDLEffect
{
    public flipDirection()
    {
        is_stochastic = true;
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with flipDirection interaction");
        }

        sprite1.orientation = VGDLUtils.RandomCardinalDirection();
    }
}