using System;
using UnityEngine;

public class transformToSingleton : VGDLEffect
{
    //new type to be transormed to
    public string stype;

    //true if orientation of sprite 2 should be inherited. Otherwise, orientation is kept.
    public bool takeOrientation;
    
    //The other sprite of type stype (if any) is transformed back to stype_other:
    // type the sprites of type stype are transormed back to
    public string stype_other;

    public transformToSingleton()
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
        
        exists = game.getRegisteredSpriteConstructor(stype_other);

        if (exists == null)
        {
            throw new Exception("Undefined sprite " + stype_other);
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with TransformToSingleton interaction.");
        }
	
        //First, transform all sprites in the game to the stype_other type.
        // (in theory, there should be only 1 or none).
        var sprites = game.getSprites(stype);
        foreach(var sprite in sprites)
        {
            var otherSprite = game.addSprite(stype_other, sprite.getPosition());
            if(otherSprite != null)
                setSpriteFields(game, otherSprite, sprite);
        }

        //Now, make the transformTo normal operation.
        var newSprite = game.addSprite(stype, sprite1.getPosition());
        if(newSprite != null)
        {
            setSpriteFields(game, newSprite, sprite1);

            if(takeOrientation) {
                var orientation = new Vector2(-sprite2.orientation.x, -sprite2.orientation.y);
                newSprite.is_oriented = true;
                newSprite.orientation = orientation;
            }
        }
    }
    
    private void setSpriteFields(VGDLGame game, VGDLSprite newSprite, VGDLSprite oldSprite)
    {
        //Orientation
        if(newSprite.is_oriented && oldSprite.is_oriented)
        {
            newSprite.orientation = oldSprite.orientation;
        }

        //Last position of the avatar.
        newSprite.lastrect =  new Rect(oldSprite.lastrect.x, oldSprite.lastrect.y,
            oldSprite.lastrect.width, oldSprite.lastrect.height);

        //Copy resources
        if(oldSprite.resources.Count > 0)
        {
            foreach(var entry in oldSprite.resources)
            {
                var resType = entry.Key;
                var resValue = entry.Value;
                newSprite.modifyResource(resType, resValue);
            }
        }


        //Avatar handling (I think considering avatars here is weird...)
        var transformed = true;
        if(oldSprite.is_avatar)
        {
            try {
                var id = ((MovingAvatar)oldSprite).playerID;
                var p = game.avatars[id].player;
                var score = game.avatars[id].score;
                var win = game.avatars[id].winState;
                game.avatars[id] = (MovingAvatar) newSprite;
                game.avatars[id].player = p;
                game.avatars[id].setKeyHandler(game.inputHandler);
                game.avatars[id].score = score;
                game.avatars[id].winState = win;
                game.avatars[id].playerID = id;
                transformed = true;
            } catch (Exception) {
                transformed = false; // new sprite is not an avatar, don't kill the current one}
            }
        }

        //boolean variable set to true to indicate the sprite was transformed
        game.killSprite(oldSprite, transformed);
    }
}

