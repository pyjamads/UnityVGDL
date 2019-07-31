using System;
using UnityEngine;

public class killIfNotUpright : VGDLEffect
{
    public killIfNotUpright()
    {
        is_kill_effect = true;
    }
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        var current_rotation = ((sprite1.rotation+2f*Math.PI)%(2f*Mathf.PI));
        if (!(current_rotation < 5.0f && current_rotation > 4.4f)){
            game.killSprite(sprite1, false);
        }
    }
}