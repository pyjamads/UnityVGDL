using System;

public class transformIfCount : transformTo
{
    //This effect transforms sprite1 into stype if
    // * num(stypeCount) >= GEQ
    // * num(stypeCount) <= LEQ
    //Otherwise it'll turn into estype
    public string stypeCount;
    public string estype;
    public int geq;
    public int leq;

    public transformIfCount()
    {
        geq = 0;
        leq = VGDLGame.MAX_SPRITES;
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with TransformIfCount interaction.");
        }
	
        var numSpritesCheck = game.getNumberOfSprites(stypeCount);
        applyScore = false;
        count = false;
        countElse = false;
        if(numSpritesCheck <= leq && numSpritesCheck >= geq)
        {
            var newSprite = game.addSprite(stype, sprite1.getPosition(), true);
            doTransformTo(newSprite, sprite1, sprite2, game);
            applyScore = true;
            count = true;
        } 
        else if (!string.IsNullOrEmpty(estype)) 
        {
            var newSprite = game.addSprite(estype, sprite1.getPosition(), true);
            doTransformTo(newSprite, sprite1, sprite2, game);
            countElse=true;
        }
    }
}