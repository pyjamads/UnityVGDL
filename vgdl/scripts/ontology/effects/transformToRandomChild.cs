using System;
using System.Collections.Generic;

public class transformToRandomChild : transformTo
{
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
            throw new ArgumentException("1st sprite can't be EOS with TransformToRandomChild interaction.");
        }
	
        var subtypes = game.getSubTypes(stype);
        
        var rndtype = subtypes.RandomElement();

        try
        {
            var newSprite = game.addSprite(rndtype, sprite1.getPosition());
            doTransformTo(newSprite, sprite1, sprite2, game);
        }
        catch (Exception)
        {
            throw new ArgumentException("Can't construct a parent node to the child " + stype + " sprite in TransformToRandomChild interaction.");
        }
    }
}