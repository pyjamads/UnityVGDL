using System;
using System.Collections.Generic;
using System.Linq;

public class undoAll : VGDLEffect
{
    /**
     * List of sprites that do NOT respond to UndoAll. This list can be specified
     * with sprite string identifiers separated by commas.
     */
    public string notStype = string.Empty;

    //List of stypes not affected by UndoAll. Array for efficiency.
    //can be set directly with array [stype1, stype2, stype3...]
    public string[] notStypes;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        if (!string.IsNullOrEmpty(notStype))
        {
            notStypes = notStype.Split(',');
        }
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        var gameSpriteOrder = game.getSpriteOrder();
        foreach (var stype in gameSpriteOrder)
        {
            if (notStypes != null && notStypes.Any(item => item.ContainsAndIgnoreCase(stype)))
            {
                continue;
            }

            var sprites = game.getSprites(stype);
            
            foreach (var vgdlSprite in sprites)
            {
                vgdlSprite.rect = vgdlSprite.lastrect;    
            }   
        }
    }
}

