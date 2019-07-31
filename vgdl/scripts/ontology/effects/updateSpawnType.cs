using System;
using UnityEngine;

public class updateSpawnType : VGDLEffect
{
    public string stype;
    public string spawnPoint;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        var exists = game.getRegisteredSpriteConstructor(stype);

        if (exists == null)
        {
            throw new Exception("Undefined sprite " + stype);
        }
        
        exists = game.getRegisteredSpriteConstructor(spawnPoint);

        if (exists == null)
        {
            throw new Exception("Undefined sprite " + spawnPoint);
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        var subtypes = game.getSubTypes(spawnPoint);
        foreach (var subtype in subtypes) {
            var sprites = game.getSprites(subtype);
            foreach(var sprite in sprites)
            {
                //NOTE: there's probably a more elegant way of handling this. Such as the below.
                //if(!(sprite is SpawnPoint)) continue;
                
                try {
                    var s = (SpawnPoint) sprite;
                    s.updateStype(sprite2.getType(), stype);
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }
    }
}