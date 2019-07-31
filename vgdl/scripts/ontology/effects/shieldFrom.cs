using System;

public class shieldFrom : VGDLEffect
{
    public string stype;
    public string ftype;

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
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with shieldFrom interaction.");
        }

        game.addShield(sprite1.getType(), stype, ftype.GetHashCode());
    }
}