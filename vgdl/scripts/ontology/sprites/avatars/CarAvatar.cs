using System.Collections.Generic;
using UnityEngine;

public class CarAvatar : OrientedAvatar
{
    public float angle_diff = 0.15f;
    public float facing;

    public CarAvatar()
    {
        speed = 0;
    }
    
    public CarAvatar(CarAvatar from) : base (from)
    {
        
    }
    
    /**
    * This update call is for the game tick() loop.
    * @param game current state of the game.
    */
    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        base.updateAvatar(game, requestInput, actionMask);
        updateUse(game);
        aim(game);
        move(game);
    }
    
    public override void applyMovement(VGDLGame game, Vector2 action)
    {
        //this.physics.passiveMovement(this);
        if (!physicstype.CompareAndIgnoreCase(VGDLPhysics.GRID))
        {
            base.updatePassive();
        }
    }

    
    public void aim(VGDLGame game)
    {
        var angle = rotation;

        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_LEFT) 
        {
            angle -= angle_diff;
        }
        else if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_RIGHT) 
        {
            angle += angle_diff;
        }
        updateRotation(angle);
    }
    
    public void move(VGDLGame game)
    {
        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_UP) 
        {
            //this.orientation = new Direction(0,0);
            facing = 0;
        }
        else if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_DOWN)
        {
            //this.orientation = new Direction(0,0);
            facing = 1;
        }
        var direx =  new Vector2(Mathf.Cos(rotation+(facing*(Mathf.Deg2Rad *  180))), Mathf.Sin(rotation+(facing*(Mathf.Deg2Rad *180))));
        physics.activeMovement(this, direx, 5);
    }
}