using System;

public class halfSpeed : VGDLEffect
{
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null){
            throw new ArgumentException("1st sprite can't be EOS with HalfSpeed interaction.");
        }
	
        sprite1.speed *= 0.5f;
    }
}

