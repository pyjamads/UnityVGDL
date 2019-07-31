using UnityEngine;

public class Spreader : Flicker
{
    public float spreadprob;
    public string stype;

    public Spreader()
    {
        spreadprob = 1f;
    }

    public Spreader(Spreader from) : base(from)
    {
        spreadprob = from.spreadprob;
        stype = from.stype;
    }

    public override void update(VGDLGame game)
    {
        base.update(game);

        if (age != 2) return;
        
        foreach(var dir in VGDLUtils.BASEDIRS)
        {
            if(Random.value < spreadprob)
            {
                var newType = (string.IsNullOrWhiteSpace(stype) ? getType() : stype);
                game.addSprite(newType, new Vector2(lastrect.x + dir.x*lastrect.width,lastrect.y + dir.y*lastrect.height));
            }
        }
    }
}