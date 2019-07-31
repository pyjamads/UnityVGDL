using System;

public class addHealthPointsToMax : VGDLEffect
{
    public int value = 1;
    public bool killSecond;
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null){
            throw new ArgumentException("1st sprite can't be EOS with AddHealthPointsToMax interaction.");
        }
	
        applyScore = true;
        if(sprite1.healthPoints + value < sprite1.limitHealthPoints) {
            sprite1.healthPoints += value;
        } else {
            sprite1.healthPoints = sprite1.limitHealthPoints;
        }

        if (sprite1.healthPoints > sprite1.maxHealthPoints)
            sprite1.maxHealthPoints = sprite1.healthPoints;

        if(killSecond && sprite2 != null)
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(sprite2, false);
    }
}