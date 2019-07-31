using System.Collections.Generic;
using UnityEngine;

public class FlakAvatar : HorizontalAvatar
{
    public string stype;
    
    //This is the resource I need, to be able to shoot.
    public string ammo; //If ammo is null, no resource needed to shoot.
    public int minAmmo; //-1 if not used. minimum amount of ammo needed for shooting.
    public int ammoCost; //1 if not used. amount of ammo to subtract after shooting once.
    

    public FlakAvatar()
    {
        ammo = null;
        minAmmo = -1;
        ammoCost = 1;
        color = VGDLColors.Green;
    }
    
    public FlakAvatar(FlakAvatar from) : base(from)
    {
        //Insert fields to be copied by copy constructor.
        stype = from.stype;
        ammo = from.ammo;
        minAmmo = from.minAmmo;
        ammoCost = from.ammoCost;
    }


    public override void init(Vector2 position, Vector2 size)
    {
        //Define actions here first, if we do this everywhere, the actions are only defined once.
        if (actions.Count == 0)
        {
            actions.Add(VGDLAvatarActions.ACTION_USE);
            actions.Add(VGDLAvatarActions.ACTION_LEFT);
            actions.Add(VGDLAvatarActions.ACTION_RIGHT);
        }

        //Then do base.
        base.init(position, size);
    }

    public override void updateAvatar(VGDLGame game, bool requestInput, List<VGDLAvatarActions> actionMask = null)
    {
        base.updateAvatar(game, requestInput, actionMask);

        if (lastMovementType == VGDLMovementTypes.STILL)
            updateUse(game);
    }

    public override void updateUse(VGDLGame game)
    {
        if (game.avatarLastAction[playerID] == VGDLAvatarActions.ACTION_USE && hasAmmo()) //use primary set of keys, idx = 0
        {
            var added = game.addSprite(stype, new Vector2(rect.x, rect.y));
            
            //singleton or max sprites could not add anything here.
            if (added == null) return;
            
            reduceAmmo();
            added.is_from_avatar = true;
        }
    }

    private bool hasAmmo()
    {
        if (string.IsNullOrEmpty(ammo))
        {
            return true; //no ammo defined, I can shoot.
        }

        //If I have ammo, I must have enough resource of ammo type to be able to shoot.
        if (resources.ContainsKey(ammo))
        {
            if (minAmmo > -1)
                return resources[ammo] > minAmmo;
            
            return resources[ammo] > 0;
        }

        return false;
    }

    private void reduceAmmo()
    {
        if (ammo != null && resources.ContainsKey(ammo))
        {
            resources[ammo] = resources[ammo] - ammoCost;
        }
    }
}