using System.Collections.Generic;
using UnityEngine;

public class Flicker : VGDLSprite
{
    public int limit;
    public int age;

    public Flicker()
    {
        limit = 1;
        age = 0;
        color = VGDLColors.Red;
    }

    public Flicker(Flicker from) : base(from)
    {
        limit = from.limit;
        age = from.age;
    }

    public override void update(VGDLGame game)
    {
        base.update(game);

        if (age > limit)
        {
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(this, false);
        }
        age++;

    }
}