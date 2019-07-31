using System;
using UnityEngine;

public abstract class VGDLTermination
{
    public int limit;
    public string win;
    public bool count_score;
    public static bool canEnd = true;

    
    public abstract void Validate(VGDLGame game);
    
    public virtual bool isDone(VGDLGame game)
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    public virtual VGDLPlayerOutcomes getWin(int i)
    {
        if (string.IsNullOrEmpty(win))
        {
            Debug.LogError("Win values not defined, nobody wins :'(");
            return VGDLPlayerOutcomes.NO_WINNER;
        }
        
        var winners = win.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);

        if (winners.Length <= i)
        {
            Debug.LogError("Not enough Win values defined, using the first defined value!");
            i = 0;
        }
        
        var outcome = bool.Parse(winners[i]);
        return outcome ? VGDLPlayerOutcomes.PLAYER_WINS : VGDLPlayerOutcomes.PLAYER_LOSES;
    }
    
    public void countScore(VGDLGame game) {
        if (count_score) {
            double maxScore = game.avatars[0].score;
            for (int i = 1; i < game.no_players; i++) {
                double score = game.avatars[i].score;
                if (score > maxScore) {
                    maxScore = score;
                }
            }
            //give win to player/s with most number of points, rest lose
            win = "";
            bool winner = false;
            for (int i = 0; i < game.no_players; i++) {
                double s = game.avatars[i].score;
                if (s == maxScore) {
                    win += "True";
                } else {
                    win += "False";
                }
                if (i != game.no_players - 1) {
                    win += ",";
                }
                if (s != 0) winner = true;
            }
            if (!winner)
                for (int i = 0; i < game.no_players; i++) {
                    if (i != game.no_players - 1) win += "False,";
                    else win += "False";
                }
        }
    }
}