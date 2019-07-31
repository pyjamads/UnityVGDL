using System;
using UnityEngine;

public class align : VGDLEffect
{
    public bool orient = true;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        setStochastic();
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null){
            throw new ArgumentException("Neither 1st not 2nd sprite can be EOS with Align interaction.");
        }
        if (orient) {
            sprite1.orientation = sprite2.orientation;
        }
        sprite1.rect = new Rect(sprite2.rect.x, sprite2.rect.y,
            sprite1.rect.width, sprite1.rect.height);
    }
}