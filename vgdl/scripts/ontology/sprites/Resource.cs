using UnityEngine;

public class Resource : Passive
{
    public int value;
    public int limit;
    public string resource_name;

    
    

    public override void init(Vector2 position, Vector2 size)
    {
        base.init(position, size);

        if (string.IsNullOrEmpty(resource_name))
        {
            //Resources are a bit special, we need the resource name
            resource_name = getType();    
        }
    }

    public Resource()
    {
        limit = 2;
        value = 1;
        color = VGDLColors.Yellow;
        is_resource = true;
    }
    
    public Resource(Resource from) : base(from){
        //Insert fields to be copied by copy constructor.
        limit = from.limit;
        value = from.value;
        resource_name = from.resource_name;
        
    }
}