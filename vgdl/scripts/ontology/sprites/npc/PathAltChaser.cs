using System.Collections.Generic;
using UnityEngine;

public class PathAltChaser : AlternateChaser
{
    private Vector2 lastKnownTargetPosition;

    public bool randomTarget;

    private VGDLSprite lastTarget;

    public PathAltChaser()
    {
        fleeing = false;
        randomTarget = false;
        targets = new List<VGDLSprite>();
        actions = new List<Vector2>();
        lastKnownTargetPosition = Vector2.negativeInfinity;
    }
    
    public PathAltChaser(PathAltChaser from) : base(from){
        //Insert fields to be copied by copy constructor.

        randomTarget = from.randomTarget;
        lastKnownTargetPosition = from.lastKnownTargetPosition;
        //NOTE: shouldn't this reference be updated from spriteID instead?
        lastTarget = from.lastTarget; 
    }

    public override void init(Vector2 position, Vector2 size)
    {
        base.init(position, size);

        if (randomTarget)
        {
            is_stochastic = true;
        }
    }

    public override void update(VGDLGame game)
    {
        actions.Clear();

        //passive moment.
        base.updatePassive();

        //Get the closest targets
        if ((lastTarget == null) || (lastTarget != null && rect.Overlaps(lastTarget.rect)))
        {
            closestTargets(game, randomTarget);
        }
        else
        {
            targets.Add(lastTarget);
        }

        var act = VGDLUtils.VGDLDirections.NONE.getDirection();
        if (!fleeing && targets.Count > 0)
        {
            //If there's a target, get the path to it and take the first action.
            var target = targets[0];
            lastTarget = target;

            var path = game.getPath(getPosition(), target.getPosition());

            if (path == null && lastKnownTargetPosition != Vector2.negativeInfinity)
            {
                //System.out.println("Recalculating to " + lastKnownTargetPosition);
                path = game.getPath(getPosition(), lastKnownTargetPosition);
            }
            else
            {
                lastKnownTargetPosition = target.getPosition().copy();
            }

            if (path != null && path.Count > 0)
            {
                //lastKnownTargetPosition = target.getPosition().copy();
                var v = path[0].comingFrom;
                act = new Vector2(v.x, v.y);
            }

        }
        else
        {
            foreach (var target in targets)
            {
                //Update the list of actions that moves me towards each target
                // (this includes fleeing)
                movesToward(target);
            }

            //Choose randomly an action among the ones that allows me to chase.
            if (actions.Count == 0)
            {
                //unless, no actions really take me closer to anybody!
                act = VGDLUtils.BASEDIRS.RandomElement();
            }
            else
            {
                act = actions.RandomElement();
            }
        }

        //Apply the action to move.
        physics.activeMovement(this, act, speed);
    }
}