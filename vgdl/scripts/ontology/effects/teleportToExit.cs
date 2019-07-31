using System;
using System.Collections.Generic;
using UnityEngine;

public class teleportToExit : VGDLEffect
{   
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither the 1st nor 2nd sprite can be EOS with TeleportToExit interaction.");
        }
	
        var portal = sprite2 as Portal;
        List<VGDLSprite> sprites;
        if(portal != null){
            sprites = game.getSprites(portal.stype);
        }
        else{
            Debug.Log("Ignoring TeleportToExit effect as " + sprite2.name + " isn't of type portal.");
            return;
        }

        if(sprites.Count > 0)
        {
            var destination = sprites.RandomElement();
            sprite1.rect = new Rect(destination.rect);
            sprite1.lastmove = 0;

            if(destination.is_oriented)
            {
                sprite1.orientation = new Vector2(destination.orientation.x, destination.orientation.y);
            }
        }
        else
        {
            //If there is no exit... kill the sprite
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(sprite1, false);
        }
    }
}