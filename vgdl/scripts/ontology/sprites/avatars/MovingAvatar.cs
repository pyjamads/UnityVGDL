using UnityEngine;

public class MovingAvatar : VGDLAvatar
{   
    public MovingAvatar()
    {
        color = Color.white;
        speed = 1;
        is_avatar = true;
        is_disqualified = false;
    }

    public MovingAvatar(MovingAvatar from) : base(from)
    {
        //Insert fields to be copied by copy constructor.
        
    }

    public virtual void updateUse(VGDLGame game)
    {
        //Nothing to do by default.
    }
}