using System;
using UnityEngine;
using System.Linq;

public class BomberRandomMissile : SpawnPoint
{
    public string stypeMissile;
    public string[] stypesMissile;
    
    public BomberRandomMissile()
    {   
        color = VGDLColors.Orange;
        is_static = false;
        is_oriented = true;
        orientation = Vector2.right;
        is_npc = true;   
    }

    public BomberRandomMissile(BomberRandomMissile from) : base(from)
    {
        stypeMissile = from.stypeMissile;
        stypesMissile = from.stypesMissile.ToArray();
    }

    public override void init(Vector2 position, Vector2 size)
    {
        base.init(position, size);

        stypesMissile = stypeMissile.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
    }

    public override void update(VGDLGame game)
    {
        stype = stypesMissile.RandomElement();
        
        base.update(game);
    }

    public override void updateStype(string stype, string newstype)
    {
        var index = stypesMissile.ToList().IndexOf(stype);
        stypesMissile[index] = newstype;
    }
}