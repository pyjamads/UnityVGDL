

/**
 * Created with IntelliJ IDEA.
 * User: Diego
 * Date: 24/10/13
 * Time: 10:22
 * This is a Java port from Tom Schaul's VGDL - https://github.com/schaul/py-vgdl
 */
public class ResourcePack : Resource {

    public ResourcePack()
    {
        is_static = true;	
    }

    public ResourcePack(ResourcePack from) : base(from) { }
}