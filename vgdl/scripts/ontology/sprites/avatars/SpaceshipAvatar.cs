using System;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipAvatar : ShootAvatar
{
    public float angle_diff = 0.3f;

    public SpaceshipAvatar()
    {
        speed=5;
        orientation = VGDLUtils.VGDLDirections.NONE.getDirection();
    }

    public SpaceshipAvatar(SpaceshipAvatar from) : base(from)
    {
        angle_diff = from.angle_diff;
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
    
    protected override void shoot(VGDLGame game, int idx)
    {
        var newOne = game.addSprite(stypes[idx], new Vector2(rect.x + Mathf.Cos(rotation)*lastrect.width,rect.y + Mathf.Sin(rotation)*lastrect.height));

        if(newOne != null)
        {
            if(newOne.is_oriented)
                newOne.orientation = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
            reduceAmmo(idx);
            newOne.is_from_avatar = true;
        }
    }
    
    public override void applyMovement(VGDLGame game, Vector2 action)
    {
    	//this.physics.passiveMovement(this);
        if (!physicstype.CompareAndIgnoreCase(VGDLPhysics.GRID))
        {
	        base.updatePassive();
        }
    }

    
    private void aim(VGDLGame game)
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
    	this.updateRotation(angle);
    }
    
    public void move(VGDLGame game)
    {
    	var facing = new Vector2(0,0);

        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_UP) 
    	{
    		facing = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
    		physics.activeMovement(this, facing, speed);
    	}
    	else if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_DOWN) 
    	{
    		facing = new Vector2(Mathf.Cos(rotation+(Mathf.Deg2Rad * 180)), Mathf.Sin(rotation+(Mathf.Deg2Rad * 180)));
    		physics.activeMovement(this, facing, speed);
    	}
    }
}