using System;
using UnityEngine;

public class ShootOnlyAvatar : ShootAvatar
{   
    public ShootOnlyAvatar() { }

    public ShootOnlyAvatar(ShootOnlyAvatar from) : base(from) { }
    
    
    public override void init(Vector2 position, Vector2 size)
    {   
        //Define actions here first.
        if(actions.Count==0)
        {
            //NOTE: Order matters, due to the index lookup in updateUse
            actions.Add(VGDLAvatarActions.ACTION_LEFT);
            actions.Add(VGDLAvatarActions.ACTION_UP);
            actions.Add(VGDLAvatarActions.ACTION_RIGHT);
            actions.Add(VGDLAvatarActions.ACTION_DOWN);
            actions.Add(VGDLAvatarActions.ACTION_USE);
        }
        
        base.init(position, size);
    }
    
    public override void applyMovement(VGDLGame game, Vector2 action)
    {
        //No movement

        //this.physics.passiveMovement(this);
        if (!physicstype.CompareAndIgnoreCase(VGDLPhysics.GRID))
        {
            base.updatePassive();
        }
    }

    public override void updateUse(VGDLGame game)
    {
        int stypeToShoot;
        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_USE)
            stypeToShoot = actions.IndexOf(VGDLAvatarActions.ACTION_USE);
        else
        {
            stypeToShoot = actions.FindIndex(item => item == game.avatarLastAction[playerID]);
        }

        if (stypeToShoot != -1 && stypeToShoot < stypes.Length)
        {
            if (hasAmmo(stypeToShoot))
            {
                shoot(game, stypeToShoot);
            }
        }
    }
}