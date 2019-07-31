using UnityEngine;

public class VerticalAvatar : MovingAvatar
{
    public VerticalAvatar()
    {
    }
    
    public VerticalAvatar(VerticalAvatar from) : base(from)
    {
    }

    public override void init(Vector2 position, Vector2 size)
    {
        //Define actions here first.
        if(actions.Count==0)
        {
            actions.Add(VGDLAvatarActions.ACTION_UP);
            actions.Add(VGDLAvatarActions.ACTION_DOWN);
        }
        
        base.init(position, size);
    }
}