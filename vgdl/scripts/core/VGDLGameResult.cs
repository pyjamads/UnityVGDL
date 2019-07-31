using System.Collections.Generic;

/// <summary>
/// Game Results with scores and references to the actual avatar, index is player ID.
/// </summary>
public class VGDLGameResult
{   
    public List<VGDLPlayerOutcomes> playerOutcomes = new List<VGDLPlayerOutcomes>();
    public List<float> playerScores = new List<float>();
    public List<VGDLSprite> playerAvatars = new List<VGDLSprite>();
}

public enum VGDLPlayerOutcomes
{
    NO_WINNER = -1,
    PLAYER_LOSES = 0,
    PLAYER_WINS = 1,
    PLAYER_DISQ = -100,
}
