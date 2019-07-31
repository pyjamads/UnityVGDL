using System;

public class addTimer : VGDLTimeEffect
{
    //Name of the effect this TimeEffect inserts in the queue of time effects.
    public string ftype;
    
    public override void Validate(VGDLGame game)
    {
        base.Validate(game);

        if (string.IsNullOrEmpty(ftype))
        {
            throw new ArgumentException("Missing ftype in addTimer definition!");
        }
        
        //Also check whether the type exists!
        VGDLParser.eval(ftype);
        
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {        
        var timeEffect = new VGDLTimeEffect(this);
        timeEffect.planExecution(game);
        game.addTimeEffect(timeEffect);
    }
}

