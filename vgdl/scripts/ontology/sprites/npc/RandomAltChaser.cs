using UnityEngine;

/**
 * Created by Diego on 24/02/14.
 */
public class RandomAltChaser : AlternateChaser{

    public float epsilon;

    public RandomAltChaser()
    {
        epsilon = 0.0f;
    }

    public RandomAltChaser(RandomAltChaser from) : base(from)
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