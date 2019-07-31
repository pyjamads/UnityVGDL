using System;
using System.Collections.Generic;
using UnityEngine;

public class wallBounce : VGDLEffect
{
    public wallBounce()
    {
        inBatch = true;
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with WallBounce interaction.");
        }
	
        if (sprite1.gravity > 0)
            sprite1.physics.activeMovement(sprite1, new Vector2(0,-1), 0);

        doBounce(sprite1, sprite2.rect, game);

        sprite1.rect.Set(sprite1.lastrect.x, sprite1.lastrect.y,sprite1.lastrect.width, sprite1.lastrect.height);
        sprite2.rect.Set(sprite2.lastrect.x, sprite2.lastrect.y,sprite2.lastrect.width, sprite2.lastrect.height);
    }

    public override int executeBatch(VGDLSprite sprite1, List<VGDLSprite> sprite2list, VGDLGame game)
    {
        int nColls = base.sortBatch(sprite1, ref sprite2list, game);

        if(nColls == 1)
        {
            doBounce(sprite1, sprite2list[0].rect, game);
        }
        else
        {
            doBounce(sprite1, collision, game);
        }

        sprite1.rect.Set(sprite1.lastrect.x, sprite1.lastrect.y,sprite1.lastrect.width, sprite1.lastrect.height);
        foreach(VGDLSprite sprite2 in sprite2list)
        {
            sprite2.rect.Set(sprite2.lastrect.x, sprite2.lastrect.y,sprite2.lastrect.width, sprite2.lastrect.height);
        }

        return nColls;
    }
    
    private void doBounce(VGDLSprite sprite1, Rect s2rect, VGDLGame g)
    {
        var collisions = determineCollision(sprite1, s2rect, g);
        var horizontalBounce = collisions[0];
        var verticalBounce = collisions[1];

        if(verticalBounce)
        {
            sprite1.orientation = new Vector2(sprite1.orientation.x, -sprite1.orientation.y);
        }
        else if(horizontalBounce)
        {
            sprite1.orientation = new Vector2(-sprite1.orientation.x, sprite1.orientation.y);
        }
        else
        {
            sprite1.orientation = new Vector2(-sprite1.orientation.x, -sprite1.orientation.y);
        }
    }
}