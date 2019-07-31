using System;

public class collectResourceIfHeld : VGDLEffect
{
    public bool killResource = true;
    public string heldResource;
    public int value = 1;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);

        is_kill_effect = killResource;
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null){
            throw new ArgumentException( "Neither the 1st nor 2nd sprite can be EOS with CollectResourceIfHeld interaction.");
        }
	
        if(sprite1.is_resource)
        {
            var r = (Resource) sprite1;
            applyScore = false;

            //Check if we have the secondary resource first
            var numResourcesHeld = sprite2.getAmountResource(heldResource);
            if(numResourcesHeld < value)
                return;

            var numResources = sprite2.getAmountResource(r.resource_name);
            if(numResources + r.value <= game.getResourceLimit(r.resource_name))
            {
                applyScore = true;
                sprite2.modifyResource(r.resource_name, r.value);
            }

            if(killResource)
            {
                //boolean variable set to false to indicate the sprite was not transformed
                game.killSprite(sprite1, false);
            }
        }
    }
}