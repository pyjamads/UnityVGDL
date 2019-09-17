

using UnityEngine;

public class Walker : Missile
{
    public bool airsteering;
    
    public Walker()
    {
        speed = 5;
        max_speed = 5;
        is_oriented = true;
        airsteering = false;
        is_stochastic = true;
        on_ground = false;
    }

    public Walker(Walker from) : base(from)
    {
        airsteering = from.airsteering;
    }

    /**
     * Overwritting intersects to check if we are on ground.
     * @return true if it directly intersects with sp (as in the normal case), but additionally checks for on_ground condition.
     */
    public override bool intersects(VGDLSprite s2)
    {
        return groundIntersects(s2);
    }

    public override void update(VGDLGame game)
    {
        base.updatePassive();

        int d;
        if (airsteering || lastDirection().x == 0){
            if (orientation.x > 0){
                d = 1;
            }
            else if (orientation.x < 0){
                d = -1;
            }
            else{
                int[] choices = {-1,1};
                d = choices.RandomElement();
            }
            var dir = new Vector2(d,0);
            orientation = dir.copy();
            physics.activeMovement(this, dir, max_speed);
        }

        speed = max_speed;
        on_ground = false;
    }
}