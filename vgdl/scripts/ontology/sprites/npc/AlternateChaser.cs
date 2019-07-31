using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlternateChaser : RandomNPC
{
    public bool fleeing;
    public string stype1;
    public string stype2;
    public string[] stypes1;
    public string[] stypes2;

    protected List<VGDLSprite> targets = new List<VGDLSprite>();
    protected List<Vector2> actions = new List<Vector2>();

    public AlternateChaser()
    {
        
    }
    
    public AlternateChaser(AlternateChaser from) : base(from)
    {
        fleeing = from.fleeing;
        stype1 = from.stype1;
        stype2 = from.stype2;
    }

    public override void init(Vector2 position, Vector2 size)
    {
        base.init(position, size);
        
        stypes1 = stype1.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries);
        stypes2 = stype2.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries);
    }

    public override void update(VGDLGame game)
    {
        actions.Clear();

        //passive moment.
        base.updatePassive();

        //Get the closest targets
        closestTargets(game, false);
        foreach (var target in targets)
        {
            //Update the list of actions that moves me towards each target
            movesToward(target);
        }

        //Choose randomly an action among the ones that allows me to chase.
        Vector2 act;
        if (actions.Count == 0)
        {
            //unless, no actions really take me closer to anybody!
            act = VGDLUtils.BASEDIRS.RandomElement();
        }
        else
        {
            act = actions.RandomElement();
        }

        //Apply the action to move.
        physics.activeMovement(this, act, speed);
    }

    protected void movesToward(VGDLSprite target)
    {
        var distance = this.physics.distance(rect, target.rect);
        foreach(var act in VGDLUtils.BASEDIRS)
        {
            //Calculate the distance if I'd apply this move.
            var r = new Rect(this.rect);
            r = r.translate (act.x, act.y);
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
     * @param randomTarget if true, a random target is added to 'targets'. Otherwise, only the closest one is.
     */
    protected void closestTargets(VGDLGame game, bool randomTarget)
    {
        targets.Clear();
        var bestDist = float.MaxValue;

        var targetSpriteId = string.Empty;
        var numChasing = stypes1.Sum(t => game.getNumberOfSprites(t));

        var numFleeing = stypes2.Sum(t => game.getNumberOfSprites(t));

        if(numChasing > numFleeing)
        {
            targetSpriteId = stypes1.RandomElement();
            fleeing = false;
        }else if(numFleeing > numChasing)
        {
            targetSpriteId = stypes2.RandomElement();
            fleeing = true;
        }

        if(!string.IsNullOrEmpty(targetSpriteId))
        {
            var sprites = game.getAllSubSprites(targetSpriteId);
            foreach(var sprite in sprites)
            {
                if(randomTarget)
                {
                    targets.Add(sprite);
                }else{
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

        if(randomTarget)
        {
            var sel = targets.RandomElement();
            targets.Clear();
            targets.Add(sel);
        }
    }
}