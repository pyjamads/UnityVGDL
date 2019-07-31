using System;
using System.Collections.Generic;
using UnityEngine;

public class wallReverse : VGDLEffect
{
    private float friction;
    private int lastGameTime;
    private List<VGDLSprite> spritesThisCycle;

    public wallReverse()
    {
        inBatch = true;
        lastGameTime = -1;
        spritesThisCycle = new List<VGDLSprite>();
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with WallReverse interaction.");
        }
	
        doReverse(sprite1, sprite2.rect, game);

        sprite1.rect = sprite1.lastrect;
        sprite2.rect = sprite2.lastrect;
    }
    
    public override int executeBatch(VGDLSprite sprite1, List<VGDLSprite> sprite2list, VGDLGame game) {

        var nColls = base.sortBatch(sprite1, ref sprite2list, game);

        if(nColls == 1)
        {
            doReverse(sprite1, sprite2list[0].rect, game);
        }else{
            doReverse(sprite1, collision, game);
        }

        sprite1.rect = sprite1.lastrect;
        foreach (VGDLSprite sprite2 in sprite2list)
        {
            sprite2.rect = sprite2.lastrect;
        }

        return nColls;
    }

    private void doReverse(VGDLSprite sprite1, Rect s2rect, VGDLGame g)
    {
        var collisions = determineCollision(sprite1, s2rect, g);
        var horizontalBounce = collisions[0];
        var verticalBounce = collisions[1];


        Vector2 v;
        if(verticalBounce)
        {
            v = new Vector2(sprite1.orientation.x, 0);
        }
        else if(horizontalBounce)
        {
            v = new Vector2(-sprite1.orientation.x, 0);
        }
        else
        {
            //By default:
            v = new Vector2(-sprite1.orientation.x, 0);
        }

        var mag = v.magnitude;
        v.Normalize();
        sprite1.orientation = new Vector2(v.x, v.y);
        sprite1.speed = sprite1.speed * mag;
        if (sprite1.speed < sprite1.gravity){
            sprite1.speed = sprite1.gravity;
        }
    }
}