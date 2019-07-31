using UnityEngine;

public class RandomMissile : Missile
{   
    public RandomMissile()
    {
        orientation = VGDLUtils.VGDLDirections.NIL.getDirection();
        speed = Random.Range(0.1f, 0.4f);
    }	
    
    public RandomMissile(RandomMissile from) : base(from) { }

    public override void update(VGDLGame game)
    {
        if(orientation == VGDLUtils.VGDLDirections.NIL.getDirection())
        {
            orientation = VGDLUtils.BASEDIRS.RandomElement();
        }

        updatePassive();
    }
}