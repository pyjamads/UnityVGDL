using System;

public class setSpeedForAll : VGDLEffect
{
    public string stype;
    public float value = 0;

    public setSpeedForAll()
    {
        is_stochastic = true;
    }

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        var exists = game.getRegisteredSpriteConstructor(stype);

        if (exists == null)
        {
            throw new Exception("Undefined sprite " + stype);
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        var subtypes = game.getSubTypes(stype);
        foreach (var s in subtypes)
        {
            var sprites = game.getSprites(s);
            foreach (var sprite in sprites)
            {
                sprite.speed = value;
            }
        }
    }
}