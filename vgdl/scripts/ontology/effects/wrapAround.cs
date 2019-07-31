using System;

public class wrapAround : VGDLEffect
{
    public float offset;
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null){
            throw new ArgumentException("1st sprite can't be EOS with WrapAround interaction.");
        }
	
        if(sprite1.orientation.x > 0)
        {
            sprite1.rect.x = (int) (offset * sprite1.rect.width);
        }
        else if(sprite1.orientation.x < 0)
        {
            sprite1.rect.x = (int) (game.screenSize.x - sprite1.rect.width * (1+offset));
        }
        else if(sprite1.orientation.y > 0)
        {
            sprite1.rect.y = (int) (offset * sprite1.rect.height);
        }
        else if(sprite1.orientation.y < 0)
        {
            sprite1.rect.y = (int) (game.screenSize.y - sprite1.rect.height * (1+offset));
        }

        sprite1.lastmove = 0;
    }
}