using UnityEngine;

public class WalkerJumper : Walker
{
    //Known as WalkJumper in pyVGDL
    
    public float probability;
    
    public WalkerJumper()
    {
        //NOTE: added this, because of probability
        is_stochastic = true;
        probability = 0.1f;
        jump_strength = 1;
        max_speed = 5.0f;
    }

    public WalkerJumper(WalkerJumper from) : base(from)
    {
        probability = from.probability;
    }
    
    /**
     * Overwritting intersects to check if we are on ground.
     * @return true if it directly intersects with sp (as in the normal case), but additionally checks for on_ground condition.
     */
    public override bool intersects(VGDLSprite s2)
    {
        //TODO: test if we can remove this, because it's already defined in our base class Walker
        return groundIntersects(s2);
    }

    public override void update(VGDLGame game)
    {
        updatePassive();

        if (on_ground && probability > Random.value)
        {
            var dd = new Vector2(0, -jump_strength);
            orientation = new Vector2(orientation.x, 0.0f);
            physics.activeMovement(this, dd, speed);

            var temp = new Vector2(0, -1);
            lastmove = cooldown; //need this to force this movement.
            updatePos(temp, 5);
        }
        else
        {
            physics.activeMovement(this, orientation, speed);
        }

        on_ground = false;
    }
}