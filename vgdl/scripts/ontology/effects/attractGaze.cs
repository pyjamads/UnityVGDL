

using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class attractGaze : VGDLEffect
{
    public bool align;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        setStochastic();
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null){
            throw new ArgumentException("Neither 1st not 2nd sprite can be EOS with AttractGaze interaction.");
        }
	
        if(sprite1.is_oriented && sprite2.is_oriented)
        {
            if(Random.value < prob) {
                sprite1.orientation = sprite2.orientation;

                if(align)
                {
                    if(sprite1.orientation.Equals(VGDLUtils.VGDLDirections.LEFT.getDirection()) || sprite1.orientation.Equals(VGDLUtils.VGDLDirections.RIGHT.getDirection()))
                    {
                        //Need to align on the Y coordinate.
                        sprite1.rect = new Rect(sprite1.rect.x, sprite2.rect.y,
                            sprite1.rect.width, sprite1.rect.height);

                    }else{
                        //Need to align on the X coordinate.
                        sprite1.rect = new Rect(sprite2.rect.x, sprite1.rect.y,
                            sprite1.rect.width, sprite1.rect.height);
                    }
                }


            }
        }
    }
}