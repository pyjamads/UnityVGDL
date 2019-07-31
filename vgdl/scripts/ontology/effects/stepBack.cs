using System;

public class stepBack : VGDLEffect
{
    public bool pixelPerfect;
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with stepBack interaction.");
        }

        if (pixelPerfect && sprite2 != null) //Sprite2 could be Null in an EOS case.
        {
            sprite1.rect = calculatePixelPerfect(sprite1, sprite2);
        }
        else
        {
            sprite1.rect = sprite1.lastrect;
        }
    }
}