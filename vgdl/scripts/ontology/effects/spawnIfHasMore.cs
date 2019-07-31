using System;
using Random =  UnityEngine.Random;

public class spawnIfHasMore : VGDLEffect
{
    public string stype;
    public string resource;
    public int spend;
    public int limit;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        var exists = game.getRegisteredSpriteConstructor(stype);

        if (exists == null)
        {
            throw new Exception("Undefined sprite " + stype);
        }
        
        exists = game.getRegisteredSpriteConstructor(resource);

        if (exists == null)
        {
            throw new Exception("Undefined resource " + resource);
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with SpawnIfHasMore interaction.");
        }

        applyScore = false;

        if (Random.value >= prob) return;

        if (sprite1.getAmountResource(resource) >= limit)
        {
            game.addSprite(stype, sprite1.getPosition());
            applyScore = true;

            sprite1.modifyResource(resource, -spend); //0 by default.
        }
    }
}