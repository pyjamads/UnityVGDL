using System;
using System.Collections.Generic;
using UnityEngine;

public class Chaser : RandomNPC
{
    public string stype;
    public bool fleeing;
    public float maxDistance;
    
    protected List<VGDLSprite> targets = new List<VGDLSprite>();
    protected List<Vector2> actions = new List<Vector2>();

    
    public Chaser()
    {
        fleeing = false;
        maxDistance = -1;
    }

    public Chaser(Chaser from) : base(from)
    {
        stype = from.stype;
        fleeing = from.fleeing;
        maxDistance = from.maxDistance;
    }

    public override void update(VGDLGame game)
    {
        actions.Clear();

        //passive moment.
        updatePassive();

        //Get the closest targets
        closestTargets(game);
        foreach(var target in targets)
        {
            //Update the list of actions that moves me towards each target
            movesToward(target);
        }

        //Choose randomly an action among the ones that allows me to chase.
        Vector2 act;
        if(actions.Count == 0)
        {
            //unless, no actions really take me closer to anybody!
            act = getRandomMove(game);
        }else{
            act = actions.RandomElement();
        }

        //Apply the action to move.
        physics.activeMovement(this, act, speed);
    }

    protected void movesToward(VGDLSprite target)
    {
        double distance = physics.distance(rect, target.rect);

        if(maxDistance >= 0 && distance > maxDistance)
        {
            //We have a maximum distance set up, and the target is further than that.
            // -> We don't react to this target.
            return;
        }

        foreach(var act in VGDLUtils.BASEDIRS)
        {
            //Calculate the distance if I'd apply this move.
            var r = new Rect(rect);
            r = r.translate(act.x,act.y);
            var newDist = physics.distance(r, target.rect);

            //depending on getting me closer/farther, if I'm fleeing/chasing, add move:
            if (fleeing && distance < newDist)
            {
                actions.Add(act);
            }

            if (!fleeing && distance > newDist)
            {
                actions.Add(act);
            }
        }
    }

    /**
     * Sets a list with the closest targets (sprites with the type 'stype'), by distance
     * @param game game to access all sprites
     */
    protected virtual void closestTargets(VGDLGame game)
    {
        targets.Clear();
        var bestDist = float.MaxValue;

        var sprites = game.getSprites(stype);
        if (sprites.Count == 0)
        {
            sprites = game.getAllSubSprites(stype);
        }
        
        foreach (var sprite in sprites)
        {
            var distance = physics.distance(rect, sprite.rect);
            if(distance < bestDist)
            {
                bestDist = distance;
                targets.Clear();
                targets.Add(sprite);
            }else if(Mathf.Abs(distance - bestDist) < 0.01f){
                targets.Add(sprite);
            }
        }
    }
}