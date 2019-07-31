using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : SpriteProducer
{
    public float prob;
    public int total;
    public int counter;
    public Vector2 spawnorientation;

    private int start;
    
    public SpawnPoint()
    {   
        prob = 1.0f;
        total = 0;
        start = -1;
        color = VGDLColors.Black;
        cooldown = 1;
        is_static = true;
        spawnorientation = Vector2.zero;
    }

    public SpawnPoint(SpawnPoint from) : base(from)
    {
        //Insert fields to be copied by copy constructor.
        prob = from.prob;
        total = from.total;
        counter = from.counter;
        spawnorientation = from.spawnorientation;
    }

    public override void init(Vector2 position, Vector2 size)
    {
        base.init(position, size);
        
        is_stochastic = (prob > 0 && prob < 1);
        counter = 0;
    }

    public override void update(VGDLGame game)
    {
        if (start == -1)
        {
            start = game.getGameTick();
        }

        var rollDie = Random.value;
        if ((start + game.getGameTick()) % cooldown == 0 && rollDie < prob)
        {
            var newSprite = game.addSprite(stype, rect.position);
            if (newSprite != null)
            {
                counter++;

                //We set the orientation given by default it this was passed.
                if (spawnorientation != Vector2.zero)
                {
                    newSprite.orientation = spawnorientation.copy();
                }
                //If no orientation given, we set the one from the spawner.
                else if (newSprite.orientation == Vector2.zero)
                {
                    newSprite.orientation = orientation.copy();
                }
            }
        }

        base.update(game);

        if (total > 0 && counter >= total)
        {
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(this, false);
        }
    }

    /**
     * Updates spawn itype with newitype
     * @param itype - current spawn type
     * @param newitype - new spawn type to replace the first
     */
    public virtual void updateStype(string stype, string newstype) {
        this.stype = newstype;
    }
}