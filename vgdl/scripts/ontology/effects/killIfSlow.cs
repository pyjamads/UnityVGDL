using System;
using UnityEngine;

public class killIfSlow: VGDLEffect
{
    public float limspeed;

    public killIfSlow()
    {
        is_kill_effect = true;
        limspeed = 1.0f;
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null){
            throw new ArgumentException("Neither 1st not 2nd sprite can be EOS with KillIfSlow interaction.");
        }
	
        var relspeed = 0.0f;
        if (sprite1.is_static)
        {
            relspeed = sprite2.speed;
        }
        else if (sprite2.is_static)
        {
            relspeed = sprite1.speed;
        }
        else
        {
            var vvx = sprite1.orientation.x - sprite2.orientation.x;
            var vvy = sprite1.orientation.y - sprite2.orientation.y;
            var vv = new Vector2(vvx,vvy);
            relspeed = vv.magnitude;
        }
        if (relspeed < limspeed)
        {
            game.killSprite(sprite1, false);
        }
    }
}