using System;
using UnityEngine;

public class transformTo : VGDLEffect
{
    public string stype;
    public bool killSecond;
    public bool forceOrientation;

    public transformTo()
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
        if (sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with spawn interaction.");
        }

        if (!sprite1.is_disabled()){
            VGDLSprite newSprite = game.addSprite(stype, sprite1.getPosition());
            doTransformTo(newSprite, sprite1,  sprite2,  game);
        }
    }
    
    protected void doTransformTo(VGDLSprite newSprite, VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(newSprite != null)
        {
            //Orientation
            if(forceOrientation || newSprite.is_oriented && sprite1.is_oriented 
                && newSprite.orientation.Equals(VGDLUtils.VGDLDirections.NONE.getDirection()))
            {
                newSprite.orientation = sprite1.orientation;
            }

            //Last position of the avatar.
            newSprite.lastrect = 
                new Rect(sprite1.lastrect.x, sprite1.lastrect.y,
                         sprite1.lastrect.width, sprite1.lastrect.height);

            //Copy resources
            if(sprite1.resources.Count > 0)
            {
                foreach(var entry in sprite1.resources)
                {
                    newSprite.resources.Add(entry.Key, entry.Value);
                }
            }


            //Avatar handling.
            var transformed = true;
            if(sprite1.is_avatar)
            {
                try{
                    int id = (sprite1 as VGDLAvatar).playerID;
                    
                    var p = game.avatars[id].player;
                    var score = game.avatars[id].score;
                    var win = game.avatars[id].winState;
                    game.avatars[id] = newSprite as VGDLAvatar;
                    game.avatars[id].player = p;
                    game.avatars[id].setKeyHandler(game.inputHandler);
                    game.avatars[id].score = score;
                    game.avatars[id].winState = win;
                    game.avatars[id].playerID = id;
                    transformed = true;
                }catch (Exception e) {
                    transformed = false; // new sprite is not an avatar, don't kill the current one
                }
            }

            //Health points should be copied too.
            newSprite.healthPoints = sprite1.healthPoints;

            //boolean variable in method call set to true
            //to indicate the sprite was transformed
            game.killSprite(sprite1, transformed);

            if(killSecond && sprite2 != null)
                game.killSprite(sprite2, true);
        }
    }
}