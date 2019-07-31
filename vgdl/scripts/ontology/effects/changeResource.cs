using System;

public class changeResource : VGDLEffect
{
    public string resource;
    public int value = 1;
    public bool killResource;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        is_kill_effect = killResource;
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with ChangeResource interaction.");
        }

        var numResources = sprite1.getAmountResource(resource);
        applyScore = false;
        if (numResources + value <= game.getResourceLimit(resource))
        {
            sprite1.modifyResource(resource, value);
            applyScore = true;

            if (killResource)
            {
                //boolean variable set to true, as the sprite was transformed
                game.killSprite(sprite2, true);
            }
        }
    }
}