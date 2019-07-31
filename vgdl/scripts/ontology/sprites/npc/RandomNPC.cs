using UnityEngine;

public class RandomNPC : VGDLSprite
{
    //Number of consecutive moves the sprite performs
    public int cons;

    protected int counter;

    protected Vector2 prevAction;

    public RandomNPC()
    {
        speed = 1;
        cons = 0;
        is_npc = true;
        is_stochastic = true;
        counter = cons;
        prevAction = VGDLUtils.VGDLDirections.NONE.getDirection();
    }

    public RandomNPC(RandomNPC from) : base(from)
    {
        //Insert fields to be copied by copy constructor.
        cons = from.cons;
        counter = from.counter;
        prevAction = from.prevAction;
    }

    protected Vector2 getRandomMove(VGDLGame game)
    {
        if (counter < cons)
        {
            //Apply previous action (repeat cons times).
            counter++;
            return prevAction.copy();
        }

        //Determine a new action
        var act = VGDLUtils.BASEDIRS.RandomElement();
        prevAction = act.copy();
        counter = 0;
        return act;
    }

    public override void update(VGDLGame game)
    {
        updatePassive();
        var act = getRandomMove(game);
        physics.activeMovement(this, act, speed);
    }
}

