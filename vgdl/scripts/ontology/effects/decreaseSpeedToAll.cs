using System;

public class decreaseSpeedToAll : VGDLEffect
{
    public string stype;
    public float value = 0.1f;

    public decreaseSpeedToAll()
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
        foreach (var type in subtypes) {
            var sprites = game.getSprites(type);
            foreach (var sprite in sprites)
            {
                if (sprite.speed - value < 0) sprite.speed = 0;
                else sprite.speed -= value;
            }
        }
    }
}