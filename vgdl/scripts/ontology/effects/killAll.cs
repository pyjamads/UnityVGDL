using System;

public class killAll : VGDLEffect
{
    public string stype;

    public killAll()
    {
        is_kill_effect = true;
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
        var spriteGroup = game.getSprites(stype);

        foreach (var sprite in spriteGroup)
        {
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(sprite, false);
        }
    }
}