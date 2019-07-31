using System.Collections.Generic;
using UnityEngine;

public class BirdAvatar : OrientedAvatar
{
    public BirdAvatar()
    {
        draw_arrow = true;
        jump_strength = 10;
    }

    public BirdAvatar(BirdAvatar from) : base(from)
    {
        
    }

    public override void init(Vector2 position, Vector2 size)
    {
        //Define actions here first.
        if(actions.Count==0)
        {
            actions.Add(VGDLAvatarActions.ACTION_USE);
        }
        
        base.init(position, size);
    }

    /**
     * This update call is for the game tick() loop.
     * @param game current state of the game.
     */
    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        base.updateAvatar(game, requestInput, actionMask);
        
        var action2 = new Vector2 (0.0f,0.0f);
        
        if (Mathf.Abs(orientation.x) < 0.5f)
            action2 = new Vector2 (1.0f,0.0f);

        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_USE)
        {
            var action = new Vector2(0, -jump_strength);
            orientation = new Vector2(orientation.x, 0.0f);
            physics.activeMovement(this, action, speed);
        }

        physics.activeMovement(this, action2, speed);
        
        updateRotation(Mathf.Atan2(orientation.y,orientation.x));
    }
   

}