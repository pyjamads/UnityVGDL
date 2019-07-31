using UnityEngine;

public class RandomBomber : SpawnPoint
{
    public RandomBomber()
    {
        color = VGDLColors.Orange;
        is_static = false;
        is_oriented = true;
        orientation = Vector2.right;
        is_npc = true;
        is_stochastic = true;
        speed = 1.0f;
    }

    public RandomBomber(RandomBomber from) : base(from)
    {
        
    }

    public override void update(VGDLGame game)
    {
        var act = VGDLUtils.BASEDIRS.RandomElement();
        physics.activeMovement(this, act, this.speed);
        
        base.update(game);
    }
}