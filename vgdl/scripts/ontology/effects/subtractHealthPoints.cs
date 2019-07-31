using System;

public class subtractHealthPoints : VGDLEffect
{
    public string stype = string.Empty;
    public int value = 1;
    public int limit = 0;
    public string scoreChangeIfKilled = "0";
    private string defScoreChange;
    
    public subtractHealthPoints()
    {
        is_kill_effect = true;
    }

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        if (!string.IsNullOrEmpty(stype)){
            var exists = game.getRegisteredSpriteConstructor(stype);

            if (exists == null)
            {
                throw new Exception("Undefined sprite " + stype);
            }
        }
        
        defScoreChange = scoreChange;
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        var s = sprite1;
        if (!string.IsNullOrEmpty(stype)) {
            var subtypes = game.getSubTypes(stype);
            foreach (var type in subtypes)
            {
                var spriteIt = game.getSprites(type);
                if (spriteIt != null)
                    foreach (var sprite in spriteIt)
                    {
                        s = sprite;
                        break;
                    }
            }
        }
        else{
            if(sprite1 == null){
                throw new ArgumentException("1st sprite can't be EOS with SubtractHealthPoints interaction.");
            }
        }
        s.healthPoints -= value;
        if(s.healthPoints <= limit)
        {
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(s, false);
            scoreChange = scoreChangeIfKilled;
        } else {
            scoreChange = defScoreChange;
        }
    }
}