using System;

public class killIfOtherHasMore : VGDLEffect
{
    public string resource;
    public int limit;
    public bool subtract;

    public killIfOtherHasMore()
    {
        is_kill_effect = true;
        subtract = false;
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null){
           throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with KillIfOtherHasMore interaction.");
        }
	
        applyScore = false;
        //If 'sprite2' has more than a limit of the resource type given, sprite dies.
        if(sprite2.getAmountResource(resource) >= limit)
        {
            applyScore = true;
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(sprite1, false);
            if (subtract)
            {
                sprite2.subtractResource(resource, limit);
            }
        }
    }
}