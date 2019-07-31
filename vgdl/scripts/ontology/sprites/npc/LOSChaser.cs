using System;

public class LOSChaser : Chaser
{
    public LOSChaser()
    {
        
    }

    public LOSChaser(LOSChaser from) : base(from)
    {
        
    }
    
    /**
     * Sets a list with the closest targets (sprites with the type 'stype'), by distance
     * @param game game to access all sprites
     */
    protected override void closestTargets(VGDLGame game)
    {
        targets.Clear();
        var bestDist = float.MaxValue;

        var sprites = game.getSprites(stype);
        if(sprites.Count == 0) sprites = game.getAllSubSprites(stype); //Try subtypes

        foreach(var sprite in sprites)
        {
            var distance = physics.distance(rect, sprite.rect);

            //check if I can see this sprite
            var canSee = false;

            if (prevAction == VGDLUtils.VGDLDirections.NONE.getDirection() || prevAction == VGDLUtils.VGDLDirections.NIL.getDirection()) {
                break;
            }

            if (prevAction.Equals(VGDLUtils.VGDLDirections.DOWN.getDirection())) {
                if ((sprite.rect.x == rect.x && sprite.rect.y >= rect.y)) {
                    canSee = true;
                }
            } else if (prevAction.Equals(VGDLUtils.VGDLDirections.UP.getDirection())) {
                if ((sprite.rect.x == rect.x && sprite.rect.y <= rect.y)) {
                    canSee = true;
                }
            } else if (prevAction.Equals(VGDLUtils.VGDLDirections.LEFT.getDirection())) {
                if ((sprite.rect.x <= rect.x && sprite.rect.y == rect.y)) {
                    canSee = true;
                }
            } else if (prevAction.Equals(VGDLUtils.VGDLDirections.RIGHT.getDirection())) {
                if ((sprite.rect.x >= rect.x && sprite.rect.y == rect.y)) {
                    canSee = true;
                }
            }

            if (canSee) {
                if (distance < bestDist) {
                    bestDist = distance;
                    targets.Clear();
                    targets.Add(sprite);
                } else if (Math.Abs(distance - bestDist) < 0.01f) {
                    targets.Add(sprite);
                }
            }
        }
    }
}