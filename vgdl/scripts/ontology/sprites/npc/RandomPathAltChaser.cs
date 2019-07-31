using UnityEngine;

public class RandomPathAltChaser : PathAltChaser
{
    public float epsilon;

    public RandomPathAltChaser()
    {
        epsilon = 0.0f;
    }

    public RandomPathAltChaser(RandomPathAltChaser from) : base(from)
    {
        epsilon = from.epsilon;
    }

    public override void update(VGDLGame game)
    {
        var roll = Random.value;
        if(roll < epsilon)
        {
            //do a sampleRandom move.
            updatePassive();
            var act = VGDLUtils.BASEDIRS.RandomElement();
            physics.activeMovement(this, act, speed);
        }else
        {
            base.update(game);
        }
    }
}