using System;
using UnityEngine;

public class collectResource : VGDLEffect
{
    public bool killResource = true;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);

        is_kill_effect = killResource;
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    { 
        if(sprite1 == null || sprite2 == null){
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with CollectResource interaction.");
        }

        if (!sprite1.is_resource) return;
        
        var r = (Resource) sprite1;
        
        applyScore = false;
        
        var numResources = sprite2.getAmountResource(r.resource_name);

        if (numResources >= game.getResourceLimit(r.resource_name)) return;
        
        var topup = Mathf.Min(r.value, game.getResourceLimit(r.resource_name) - numResources);
        applyScore=true;
        sprite2.modifyResource(r.resource_name, topup);

        if (killResource)
        {
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(sprite1, true);  
        }
    }
}