using System;
using UnityEngine;

public class bounceDirection : VGDLEffect
{
    public float maxBounceAngleDeg = 60;
    private float maxBounceAngleRad;

    public override void Validate(VGDLGame game)
    {
        base.Validate(game);
        
        maxBounceAngleRad = Mathf.Deg2Rad * maxBounceAngleDeg;
    }

    public override void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game)
    {
        if (sprite1 == null || sprite2 == null)
        {
            throw new ArgumentException("Neither 1st not 2nd sprite can be EOS with BounceDirection interaction.");
        }

        //We need the actual intersection:
        
        //var interRect = sprite1.rect.intersection(sprite2.rect);
        var padLength = sprite2.rect.height;
        float diff, travelDir;
        var vel = sprite1.getVelocity();

//        boolean verticalBounce = interRect.width > interRect.height;
//        boolean horizontalBounce = interRect.width < interRect.height;

        var distX =  Mathf.Min(Mathf.Abs (sprite1.lastrect.x - (sprite2.rect.x + sprite2.rect.width)) ,
                Mathf.Abs ((sprite1.lastrect.x + sprite1.rect.width) - sprite2.rect.x));

        var distY =  Mathf.Min(Mathf.Abs (sprite1.lastrect.y - (sprite2.rect.y + sprite2.rect.height)) ,
                Mathf.Abs ((sprite1.lastrect.y + sprite1.rect.height) - sprite2.rect.y));


        var tX = Mathf.Abs(distX / vel.x);
        var tY = Mathf.Abs(distY / vel.y);
        var horizontalBounce = (tX < tY);
        var verticalBounce = (tY < tX);

        if(verticalBounce)
        {
            //Bouncing vertically
            padLength = sprite2.rect.width;
            diff = sprite1.rect.center.x - sprite2.rect.center.x;
            travelDir = (sprite1.rect.center.y < sprite2.rect.center.y)? 1 : -1;
        }
        else if(horizontalBounce){
            diff = sprite2.rect.center.y  - sprite1.rect.center.y;
            travelDir = (sprite1.rect.center.x > sprite2.rect.center.x)? 1 : -1;
        }else{
            sprite1.orientation = new Vector2(-sprite1.orientation.x, sprite1.orientation.y);
            //System.out.println("DIAGONAL");
            return;
        }

        //Calculate bouncing angle relative to the position where sprite1 hit sprite2.
        var relHit = diff / (0.5f * padLength);
        if(relHit < 0) relHit = Mathf.Max(relHit, -1); else relHit = Mathf.Min(relHit, 1); //Keeping it in [-1,1]
        var bounceAngle = relHit * maxBounceAngleRad;

        float xDir, yDir;
        if(verticalBounce)
        {
            xDir = Mathf.Sin(bounceAngle);
            yDir = -Mathf.Cos(bounceAngle) * travelDir;
        }else{
            xDir = Mathf.Cos(bounceAngle) * travelDir;
            yDir = -Mathf.Sin(bounceAngle);
        }

        //Assign new orientation with the calculated new direction.
        var outDir = new Vector2(xDir, yDir);
        outDir.Normalize();
        sprite1.orientation = new Vector2(outDir.x, outDir.y);
    }
}