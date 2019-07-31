using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class VGDLEffect
{
    //Indicates if this effect kills any sprite
    public bool is_kill_effect = false;

    //Indicates if this effect has some random element.
    public bool is_stochastic = false;

    // indicates whether the interactions of this effect should be carried out sequentially or simultaneously
    public bool sequential = false;

    //Change of the score this effect makes.
    public string scoreChange = "0";

    //Count something
    public bool count = true;
    public string counter = "0";
    
    //Count something else
    public bool countElse = true;
    public string counterElse = "0";

    //Probabilty for stochastic effects.
    public float prob = 1;

    //Indicates if this effects changes the score.
    public bool applyScore = true;

    //Indicates the number of repetitions of this effect. This affects how many times this
    // effect is taken into account at each step. This is useful for chain effects (i.e. pushing
    // boxes in a chain - thecitadel, enemycitadel).
    public int repeat = 1;

    /**
     * 'Unique' hashcode for this effect
     */
    public int hashCode;

    /**
     * Indicates if this effect is enabled or not (default: true)
     */
    public bool enabled = true;

    /**
     * Indicates if the effect wishes to take into account all sprites of the second type at once.
     */
    public bool inBatch = false;

    /**
     * Collision for batches
     */
    protected Rect collision;

    /// <summary>
    /// Used for post initialization/reflection validation and initialization. 
    /// </summary>
    public virtual void Validate(VGDLGame game) { }

    /// <summary>
    /// Execute the effect on the two colliding sprites
    /// </summary>
    /// <param name="sprite1"></param>
    /// <param name="sprite2"></param>
    /// <param name="game"></param>
    /// <param name="sprite1 first sprite of the collision"></param>
    /// <param name="sprite2 second sprite of the collision"></param>
    /// <param name="game reference to the game object with the current state."></param>
    public abstract void execute(VGDLSprite sprite1, VGDLSprite sprite2, VGDLGame game);

    /// <summary>
    /// Executes the effect on to all second sprites at once.
    /// </summary>
    /// <param name="sprite1"></param>
    /// <param name="sprite2list"></param>
    /// <param name="game"></param>
    /// <returns> the number of sprites considered in the collision </returns>
    public virtual int executeBatch(VGDLSprite sprite1, List<VGDLSprite> sprite2list, VGDLGame game)
    {
        return -1;
    }

    /// <summary>
    /// Takes a batch of sprites to collide against at once, and builds the collision boundaries
    /// </summary>
    /// <param name="sprite1"></param>
    /// <param name="sprite2list"></param>
    /// <param name="game"></param>
    /// <returns> the number of sprites in the list to collide with, List is returned sorted by proximity</returns>
    public virtual int sortBatch(VGDLSprite sprite1, ref List<VGDLSprite> sprite2list, VGDLGame game)
    {
        if(sprite2list.Count == 1) {
            //execute(sprite1, sprite2list.get(0), game);
            collision = sprite2list[0].rect;
            return 1;

        }else if(sprite2list.Count > 2)
        {
            //More than 2, sort by proximity and return the first.
            sortByProximity(sprite1, ref sprite2list);
            //execute(sprite1, sprite2list.get(0), game);
            collision = sprite2list[0].rect;
            return 1;
        }

        //Else, do a cleverer collision with a wall of 2
        VGDLSprite b1 = sprite2list[0];
        VGDLSprite b2 = sprite2list[1];

        if(b1.rect.center.x == b2.rect.center.x)
        {
            bool b1Above = b1.rect.y < b2.rect.y;

            double x = b1.rect.x;
            double y = b1Above ? b1.rect.y : b2.rect.y;
            double w = b1.rect.width;
            double h = b1.rect.height * 2;
            collision = new Rect((int)x,(int)y,(int)w,(int)h);

        }else if (b1.rect.center.y == b2.rect.center.y)
        {
            bool b1Left = b1.rect.x < b2.rect.x;

            double x = b1Left ? b1.rect.x : b2.rect.x;
            double y = b1.rect.y;
            double w = b1.rect.width * 2;
            double h = b1.rect.height;
            collision = new Rect((int)x,(int)y,(int)w,(int)h);
        }else {
            //Not aligned, better to use the closest one.
            sortByProximity(sprite1, ref sprite2list);
            //execute(sprite1, sprite2list.get(0), game);
            collision = sprite2list[0].rect;
            return 1;
        }

        return 2;
    }

    private void sortByProximity(VGDLSprite sprite1, ref List<VGDLSprite> sprite2list)
    {
        var spCompareCenter = new Vector2(sprite1.lastrect.center.x, sprite1.lastrect.center.y);
        sprite2list.Sort((o1, o2) =>
        {
            var s1Center = new Vector2(o1.lastrect.center.x, o1.lastrect.center.y);
            var s2Center = new Vector2(o2.lastrect.center.x, o2.lastrect.center.y);

            if (Vector2.Distance(spCompareCenter, s1Center) < Vector2.Distance(spCompareCenter, s2Center)) return -1;
            
            if(Vector2.Distance(spCompareCenter, s1Center) > Vector2.Distance(spCompareCenter, s2Center))	return 1;
            
            return 0;
        });
    }
    
    /**
    * Determines if the collision is horizontal and/or vertical
    * @param sprite1 Sprite colliding
    * @param s2rect Collision colliding against.
    * @param g game
    * @return An array indicating if the collision is []{horizontal, vertical}.
    */
    protected bool[] determineCollision(VGDLSprite sprite1, Rect s2rect, VGDLGame g)
    {
        var intersec = sprite1.rect.intersection(s2rect);
        var horizontalBounce = (sprite1.rect.height == intersec.height);
        var verticalBounce =   (sprite1.rect.width == intersec.width);

        if(!horizontalBounce && !verticalBounce)
        {
            Vector2 vel = sprite1.getVelocity();

            //Distance on X, according to the direction of travel
            var distX = (vel.x == 0.0f) ?  Mathf.Abs (sprite1.lastrect.x - s2rect.x) :                         //Travelling vertically
                ((vel.x > 0.0f) ?  Mathf.Abs ((sprite1.lastrect.x + sprite1.rect.width) - s2rect.x) :  //Going right
                    Mathf.Abs ((s2rect.x + s2rect.width) - sprite1.lastrect.x));        //Going left


            //Distance on Y, according to the direction of travel
            var distY = (vel.y == 0.0f) ?  Mathf.Abs (sprite1.lastrect.y - s2rect.y) :                          //Travelling laterally
                ((vel.y > 0.0f) ?  Mathf.Abs ((sprite1.lastrect.y + sprite1.rect.height) - s2rect.y) :  //Going downwards
                    Mathf.Abs (sprite1.lastrect.y - (s2rect.y + s2rect.height)));        //Going upwards


            var tX = Mathf.Abs(distX / vel.x);
            var tY = Mathf.Abs(distY / vel.y);
            horizontalBounce = (tX < tY);
            verticalBounce = (tY < tX);
        }

        return new[]{horizontalBounce, verticalBounce};
    }
    
    public float getScoreChange(int playerID)
    {
        try
        {
            var scores = scoreChange.Split(',');
            return playerID < scores.Length ? float.Parse(scores[playerID]) : float.Parse(scores[0]);
        }
        catch (Exception e)
        {
            Debug.LogWarning("scoreChange must be an integer or float number not [" + scoreChange + "].");
        }
        
        return 0f;
    }
    
    public int getCounter(int idx) {
        try
        {
            var counters = counter.Split(',');
            return idx < counters.Length ? int.Parse(counters[idx]) : int.Parse(counters[0]);
        }
        catch (Exception e)
        {
            Debug.LogWarning("counter must be an integer number not [" + counter + "].");
        }
        
        return 0;
    }
    
    public int getCounterElse(int idx) {		
        try
        {
            var counters = counterElse.Split(',');
            return idx < counters.Length ? int.Parse(counters[idx]) : int.Parse(counters[0]);
        }
        catch (Exception e)
        {
            Debug.LogWarning("counterElse must be an integer number not [" + counterElse + "].");
        }
        
        return 0;
    }
    
     protected Rect calculatePixelPerfect(VGDLSprite sprite1, VGDLSprite sprite2)
    {
        var spriteVector = new Vector2(sprite1.rect.center.x - sprite1.lastrect.center.x,
                sprite1.rect.center.y - sprite1.lastrect.center.y);

        spriteVector.Normalize();

        if(spriteVector.Equals(VGDLUtils.VGDLDirections.DOWN.getDirection()))
        {
            return adjustDown(sprite1, sprite2);
        }
        else if(spriteVector.Equals(VGDLUtils.VGDLDirections.RIGHT.getDirection()))
        {
            return adjustRight(sprite1, sprite2);
        }
        else if(spriteVector.Equals(VGDLUtils.VGDLDirections.UP.getDirection()))
        {
            return adjustUp(sprite1, sprite2);
        }
        else if(spriteVector.Equals(VGDLUtils.VGDLDirections.LEFT.getDirection()))
        {
            return adjustLeft(sprite1, sprite2);
        }

        return sprite1.lastrect;

    }

    private Rect adjustRight(VGDLSprite sprite1, VGDLSprite sprite2)
    {
        //Sprite RIGHT adjusts for overlap.
        var overlay = (sprite1.rect.x + sprite1.rect.width) - sprite2.rect.x;
        return new Rect(sprite1.rect.x - overlay, sprite1.rect.y,
                sprite1.rect.width, sprite1.rect.height);
    }

    private Rect adjustLeft(VGDLSprite sprite1, VGDLSprite sprite2)
    {
        return new Rect(sprite2.rect.x + sprite2.rect.width, sprite1.rect.y,
                sprite1.rect.width, sprite1.rect.height);
    }

    private Rect adjustUp(VGDLSprite sprite1, VGDLSprite sprite2)
    {
        return new Rect(sprite1.rect.x, sprite2.rect.y + sprite2.rect.height,
                sprite1.rect.width, sprite1.rect.height);
    }

    private Rect adjustDown(VGDLSprite sprite1, VGDLSprite sprite2)
    {
        //Sprite DOWN adjusts for overlap.
        var overlay = (sprite1.rect.y + sprite1.rect.height) - sprite2.rect.y;
        return new Rect(sprite1.rect.x, sprite1.rect.y - overlay,
                sprite1.rect.width, sprite1.rect.height);
    }

    public void setStochastic()
    {
        if (prob > 0 && prob < 1)
            is_stochastic = true;
    }
}