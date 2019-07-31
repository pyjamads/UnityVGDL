using System;

public class spawnIfCounterSubTypes : VGDLEffect
{
    public string stype; //Sprite to spawn
    public string estype; // Sprite to spawn if condition is not met
    public string stypeCount; //sprite to count
    public int limit; // number of total sprites
    public int subTypesNum = -1; //number of subtypes

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        var exists = game.getRegisteredSpriteConstructor(estype);

        if (exists == null)
        {
            throw new Exception("Undefined sprite " + estype);
        }
        
        exists = game.getRegisteredSpriteConstructor(stypeCount);

        if (exists == null)
        {
            throw new Exception("Undefined sprite " + stypeCount);
        }

        exists = game.getRegisteredSpriteConstructor(stype);
        if (exists == null)
        {
            throw new Exception("Undefined sprite " + stype);
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with SpawnIfCounterSubTypes interaction.");
        }
	
        applyScore = false;
        count = false;

        var countAcum = 0;

        countAcum += game.getNumberOfSprites(stypeCount, false);

        if(countAcum == limit) 
        {

            var subtypes = game.getSubTypes(stypeCount);
            var countAcumSubTypes = 0;
            foreach (var subtype in subtypes) 
            {

                var count = game.getNumberOfSprites(subtype, false);
                if(count > 0)
                {
                    //if(game.getSpriteGroup(subtype) != null) //This avoids non-terminal types
                    //NOTE: Don't understand the above line, I think it's to make sure that any sprites exists of that type
                    if(game.getSprites(subtype).Count > 0)
                    {
                        countAcumSubTypes += count > 0 ? 1 : 0;
                    }
                }
            }

            countAcumSubTypes /= 2;
            if(countAcumSubTypes == subTypesNum) 
            {
                game.addSprite(stype, sprite1.getPosition());
                applyScore = true;
                count=true;
            } 
            else 
            {
                game.addSprite(estype, sprite1.getPosition());
            }
        } 
        else 
        {
            game.addSprite(estype, sprite1.getPosition());
        }
    }
}