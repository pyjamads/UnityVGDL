using System;

public class transformToAll : transformTo
{
    public string stypeTo;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);

        var exists = game.getRegisteredSpriteConstructor(stypeTo);

        if (exists == null)
        {
            throw new Exception("Undefined sprite " + stypeTo);
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        //First, we need to get all sprites of type stype.
        var sprites = game.getSprites(stype);
        
        foreach(var sprite in sprites)
        {
            //Last argument: forces the creation. This could be a parameter of the effect too, if needed.
            var newSprite = game.addSprite(stypeTo, sprite.getPosition(), true);
            //newSprite inherits things from 's'. Maybe sprite2 gets killed in the process.
            doTransformTo(newSprite, sprite, sprite2, game);
        }
    }
}