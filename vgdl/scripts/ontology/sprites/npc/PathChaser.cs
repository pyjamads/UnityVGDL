using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathChaser : RandomNPC
{
    public bool fleeing;
    public string stype;
    public float maxDistance = -1;

    List<VGDLSprite> targets = new List<VGDLSprite>();
    List<Vector2> actions = new List<Vector2>();

    public PathChaser()
    {

    }

    public PathChaser(PathChaser from) : base(from)
    {
        fleeing = from.fleeing;
        stype = from.stype;
        maxDistance = from.maxDistance;
    }

    public override void update(VGDLGame game)
    {
        actions.Clear();

        //passive moment.
        base.updatePassive();

        //Get the closest targets
        closestTargets(game);

        var act = VGDLUtils.VGDLDirections.NONE.getDirection();
        if (targets.Count > 0)
        {
            //If there's a target, get the path to it and take the first action.
            var target = targets[0];

            var path = game.getPath(getPosition(), target.getPosition());

            if (path != null && path.Count > 0)
            {
                var v = path[0].comingFrom;
                act = new Vector2(v.x, v.y);
            }

            counter = cons; //Reset the counter of consecutive moves.
        }
        else
        {
            //No target, just move random.
            act = getRandomMove(game);
        }

        //Apply the action to move.
        physics.activeMovement(this, act, speed);
    }

    /**
     * Sets a list with the closest targets (sprites with the type 'stype'), by distance
     * @param game game to access all sprites
     */
    protected void closestTargets(VGDLGame game)
    {
        targets.Clear();
        var bestDist = float.MaxValue;

        var sprites = game.getSprites(stype);
        if (sprites.Count == 0) sprites = game.getAllSubSprites(stype); //Try subtypes


        foreach (var s in sprites)
        {
            var distance = physics.distance(rect, s.rect);
            if (distance < bestDist)
            {
                bestDist = distance;
                targets.Clear();
                targets.Add(s);
            }
            else if (Mathf.Abs(distance - bestDist) < 0.01f)
            {
                targets.Add(s);
            }

        }
    }
}