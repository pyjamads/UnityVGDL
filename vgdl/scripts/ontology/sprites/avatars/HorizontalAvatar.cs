using UnityEngine;

public class HorizontalAvatar : MovingAvatar
{
    public HorizontalAvatar()
    {
    }
    
    public HorizontalAvatar(HorizontalAvatar from) : base(from)
    {
    }

    public override void init(Vector2 position, Vector2 size)
    {
        //Define actions here first.
        if(actions.Count==0)
        {
            actions.Add(VGDLAvatarActions.ACTION_LEFT);
            actions.Add(VGDLAvatarActions.ACTION_RIGHT);
        }
        
        base.init(position, size);
    }
}