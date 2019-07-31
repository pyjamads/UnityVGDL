using UnityEngine;

public class NullAvatar : HorizontalAvatar
{
    public NullAvatar()
    {
        
        color = VGDLColors.Green;
    }
    
    public NullAvatar(NullAvatar from) : base(from)
    {
    }

    public override void init(Vector2 position, Vector2 size)
    {
        //Define actions here first.
        if(actions.Count==0)
        {
            actions.Add(VGDLAvatarActions.ACTION_NIL);
        }
        
        base.init(position, size);
    }
}