using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnPointMultiSprite : SpriteProducer
{
    public float prob;
    public int total;
    public int counter;
    public Vector2 spawnorientation;

    private int start;

    public string stypes;
    private string[] stypesList;

    public SpawnPointMultiSprite()
    {
        prob = 1.0f;
        total = 0;
        start = -1;
        color = VGDLColors.Black;
        cooldown = 1;
        is_static = true;
        spawnorientation = Vector2.zero;
        is_oriented = true;
        orientation = Vector2.right;
        is_npc = true;
        stypesList = new string[0];
    }
    
    public SpawnPointMultiSprite(SpawnPointMultiSprite from) : base(from)
    {
        //Insert fields to be copied by copy constructor.
        prob = from.prob;
        total = from.total;
        counter = from.counter;
        spawnorientation = from.spawnorientation;
        start = from.start;
        stypes = from.stypes;
        stypesList = from.stypesList.ToArray();
    }


    public override void init(Vector2 position, Vector2 size)
    {
        base.init(position, size);

        stypesList = stypes.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
        
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
        if((start+game.getGameTick()) % cooldown == 0 && rollDie < prob)
        {
            foreach (var stype in stypesList) {
                var newSprite = game.addSprite(stype, getPosition());
                if (newSprite == null) continue;
                
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

        if(total > 0 && counter >= total)
        {
            //boolean variable set to false to indicate the sprite was not transformed
            game.killSprite(this, false);
        }
    }
}