using System;
using System.Collections.Generic;
using UnityEngine;

public class wallStop : VGDLEffect
{   
    private float friction;
    private int lastGameTime;
    private List<VGDLSprite> spritesThisCycle;

    public wallStop()
    {
        lastGameTime = -1;
        spritesThisCycle = new List<VGDLSprite>();
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with WallStop interaction.");
        }

        // Stop just in front of the wall, removing that velocity component, but possibly sliding along it.

        //Keep in the list, for the current cycle, the sprites that have triggered this event.
        int currentGameTime = game.getGameTick();
        if(currentGameTime > lastGameTime)
        {
            spritesThisCycle.Clear();
            lastGameTime = currentGameTime;
        }

        //the event gets triggered only once per time-step on each sprite.
        if(spritesThisCycle.Contains(sprite1))
            return;

        //sprite1.setRect(sprite1.lastrect);
        sprite1.rect = new Rect(calculatePixelPerfect(sprite1, sprite2));

        double centerXDiff = Mathf.Abs(sprite1.rect.center.x - sprite2.rect.center.x);
        double centerYDiff = Mathf.Abs(sprite1.rect.center.y - sprite2.rect.center.y);

        Vector2 v;
        if(centerXDiff > centerYDiff)
        {
            //sprite1.orientation = new Direction(0, sprite1.orientation.y() * (1.0 - friction));
            v = new Vector2(0, sprite1.orientation.y);
        }
        else
        {
            //sprite1.orientation = new Direction(sprite1.orientation.x() * (1.0 - friction), 0);
            v = new Vector2(sprite1.orientation.x, 0);
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