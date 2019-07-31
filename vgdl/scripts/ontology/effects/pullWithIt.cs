using System;
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

public class pullWithIt : VGDLEffect
{
    private int lastGameTime;
    
    private List<VGDLSprite> spritesThisCycle;

    public bool pixelPerfect;

    public pullWithIt()
    {
        pixelPerfect = true;
        lastGameTime = -1;
        spritesThisCycle = new List<VGDLSprite>();
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with PullWithIt interaction.");
        }
        
        //Keep in the list, for the current cycle, the sprites that have triggered this event.
        var currentGameTime = game.getGameTick();
        if(currentGameTime > lastGameTime)
        {
            spritesThisCycle.Clear();
            lastGameTime = currentGameTime;
        }

        //the event gets triggered only once per time-step on each sprite.
        if(spritesThisCycle.Contains(sprite1))
            return;

        spritesThisCycle.Add(sprite1);

        //And go on.
        var r = sprite1.lastrect;
        var v = sprite2.lastDirection();
        v.Normalize();

        var gridsize = 1f;
        if(sprite1.physicstype.CompareAndIgnoreCase(VGDLPhysics.GRID))
        {
            GridPhysics gp = (GridPhysics)(sprite1.physics);
            gridsize = gp.gridSize.x;
        }else
        {
            ContinuousPhysics gp = (ContinuousPhysics)(sprite1.physics);
            gridsize = gp.gridSize.x;
        }

        sprite1.updatePos(new Vector2(v.x, v.y), (int) (sprite2.speed*gridsize));

        if(sprite1.physicstype.CompareAndIgnoreCase(VGDLPhysics.GRID))
        {
            sprite1.rect.y = sprite2.rect.y-sprite2.rect.height;
            sprite1.orientation = new Vector2(sprite1.orientation.x,0.0f);
        }

        sprite1.lastrect = new Rect(r);

        if(pixelPerfect)
        {
            sprite1.rect = new Rect(sprite2.rect);
        }
    }
}