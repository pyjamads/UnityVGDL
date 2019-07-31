using System;
using System.Collections.Generic;
using UnityEngine;

public class ShootAvatar : OrientedAvatar
{
    protected const int MAX_WEAPONS = 5;
    
    //this is the resource I need to be able to shoot, if null no resource is needed.
    public string ammo;
    public string[] ammos;
    
    //This is the sprite I shoot
    public string stype;
    public string[] stypes;
    
    public ShootAvatar()
    {
        ammo = null;
        ammos = new string[MAX_WEAPONS];
        
        stype = null;
        stypes = new string[MAX_WEAPONS];
    }

    public ShootAvatar(ShootAvatar from) : base(from)
    {
        //Insert fields to be copied by copy constructor.
        ammo = from.ammo;
        ammos = from.ammos;

        stype = from.stype;
        stypes = from.stypes;
    }
    
    public override void init(Vector2 position, Vector2 size)
    {   
        //Define actions here first.
        if(actions.Count==0)
        {
            actions.Add(VGDLAvatarActions.ACTION_USE);
            actions.Add(VGDLAvatarActions.ACTION_LEFT);
            actions.Add(VGDLAvatarActions.ACTION_RIGHT);
            actions.Add(VGDLAvatarActions.ACTION_DOWN);
            actions.Add(VGDLAvatarActions.ACTION_UP);
        }
        
        base.init(position, size);
        
        stypes = stype.Split(new []{","}, StringSplitOptions.RemoveEmptyEntries);

        if (ammo != null)
        {
            ammos = ammo.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        base.updateAvatar(game, requestInput, actionMask);

        if (lastMovementType == VGDLMovementTypes.STILL)
        {
            updateUse(game);
        }
    }

    public override void updateUse(VGDLGame game)
    {
        //Actions have been processed by base.updateAvatar()
        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_USE)
        {
            for (int i = 0; i < stypes.Length; i++) {
                if (hasAmmo(i)) {
                    shoot(game, i);
                    break; // remove this to shoot all types of bullets at once; if here, shoots the first priority one only
                }
            }    
        }
    }
    
    protected virtual void shoot(VGDLGame game, int idx)
    {
        Vector2 dir = orientation;
        dir.Normalize();

        VGDLSprite newOne = game.addSprite(stypes[idx], new Vector2(rect.x + dir.x*lastrect.width,rect.y + dir.y*lastrect.height));

        if(newOne != null)
        {
            if(newOne.is_oriented)
                newOne.orientation = new Vector2(dir.x, dir.y);
            reduceAmmo(idx);
            newOne.is_from_avatar = true;
        }
    }

    protected bool hasAmmo(int idx) {
        if (ammo == null || idx >= ammos.Length)
            return true; //no ammo defined, I can shoot.

        //If I have ammo, I must have enough resource of ammo type to be able to shoot.
        return resources.ContainsKey(ammos[idx]) && resources[ammos[idx]] > 0;
    }

    protected void reduceAmmo(int idx)
    {
        if(ammo != null && idx < ammos.Length && resources.ContainsKey(ammos[idx]))
        {
            resources[ammos[idx]] = resources[ammos[idx]] - 1;
        }
    }
    
    
}