using System;

public class removeScore : VGDLEffect
{
    public bool killSecond;
    public string stype = string.Empty;
    
    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if(sprite1 == null)
        {
            throw new ArgumentException("1st sprite can't be EOS with RemoveScore interaction.");
        }
	
        if (string.IsNullOrEmpty(stype)) {
            if (sprite1.is_avatar) {
                var a = (MovingAvatar) sprite1;
                a.score = 0;
                if (killSecond && sprite2 != null)
                {
                    game.killSprite(sprite2, true);
                }
            }
        } else {
            var sprites = game.getSprites(stype);

            foreach(var sprite in sprites)
            {
                if (!sprite.is_avatar) continue;
                
                var a = (MovingAvatar) sprite;
                a.score = 0;
                if (killSecond && sprite2 != null)
                {
                    game.killSprite(sprite2, true);
                }
            }
        }
    }
}