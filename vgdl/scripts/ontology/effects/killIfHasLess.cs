﻿using System;

public class killIfHasLess : VGDLEffect
{
    public string resource;
    public int limit;

    public killIfHasLess()
    {
        is_kill_effect = true;
        limit = 0;
    }

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        var exists = game.getRegisteredSpriteConstructor(resource);

        if (exists == null)
        {
            throw new Exception("Undefined resource " + resource);
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with killIfHasLess interaction");
        }

        applyScore = false;
        if(sprite1.getAmountResource(resource) <= limit)
        {
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(sprite1, false);
            applyScore = true;
        }
    }
}