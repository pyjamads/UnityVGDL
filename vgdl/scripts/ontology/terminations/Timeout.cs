using System;
using UnityEngine;

public class Timeout : VGDLTermination
{
    public bool use_counter;
    public bool compare;
    public string limits = "0";
    
    public override void Validate(VGDLGame game)
    {
        if (limit <= 0)
        {
            throw new ArgumentException("limit <= 0, for timeout");
        }
    }
    
    public override bool isDone(VGDLGame game)
    {
        if (base.isDone(game)) return true;

         if(game.getGameTick() >= limit) {
            countScore(game);

            if (use_counter) {
                //use the master game counter
                if (compare) {
                    //if comparing, the first player wins if they're not equal, the rest win otherwise
                    var first = game.getValueCounter(0);
                    var ok = true;
                    for (var i = 1; i < game.getNoCounters(); i++) {
                        if (game.getValueCounter(i) != first) {
                            ok = false;
                        }
                    }
                    if (ok) {
                        win = "False,";
                        for (var i = 1; i < game.getNoPlayers(); i++) {
                            if (i == game.no_players - 1) {
                                win += "True";
                            } else win += "True,";
                        }
                    } else {
                        win = "True,";
                        for (var i = 1; i < game.getNoPlayers(); i++) {
                            if (i == game.no_players - 1) {
                                win += "False";
                            } else win += "False,";
                        }

                    }
                } else {
                    //use the limits, split it and check each counter, idx corresponding to player ID
                    if (game.no_players != game.no_counters) {
                        win = "";
                        for (var i = 0; i < game.no_players; i++) {
                            if (i != game.no_players - 1) win += "False,";
                            else win += "False";
                        }
                    } else {
                        var split = limits.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
                        var intlimits = new int[split.Length];
                        for (var i = 0; i < intlimits.Length;i++)
                            intlimits[i] = int.Parse(split[i]);

                        for (var i = 0; i < game.no_players; i++) {
                            win = "";
                            if (game.getValueCounter(i) == intlimits[i]) {
                                win += "True";
                            } else
                                win += "False";
                            if (i != game.no_players - 1) win += ",";
                        }
                    }
                }
            }
            
             if (VGDLParser.verbose)
             {
                 Debug.Log("Timeout limit("+limit+") condition met: "+game.getGameTick());
             }

            return true;
        }

        return false;
    }
}