using System.Collections.Generic;

public class VGDLSpriteInfo
{
    public string identifier;
    public string sclass;
    public Dictionary<string, string> args;
    public List<string> stypes;
    //public List<string> subtypes; //Maybe I don't need it?

    public VGDLSpriteInfo(string identifier, string sclass, Dictionary<string, string> args, List<string> stypes)
    {
        this.identifier = identifier;
        this.sclass = sclass;
        this.args = args;
        this.stypes = stypes;
    }
}