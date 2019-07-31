public class Door : Immovable {

	public Door()
	{
		portal = true;
	}
	
	public Door(Door from) : base(from) { }
}