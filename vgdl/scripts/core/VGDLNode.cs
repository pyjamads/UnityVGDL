using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VGDLNode
{
    public VGDLNode parent;
    public string content;
    public int indent;
    public List<VGDLNode> children;
    public int lineNumber;

    public VGDLNode(string content, int indent, int lineNumber, VGDLNode parent = null)
    {
        this.children = new List<VGDLNode>();
        this.content = content;
        this.indent = indent;
        this.lineNumber = lineNumber;
        if (parent != null)
        {
            parent.insert(this);
        }
    }
    
    public void insert(VGDLNode node)
    {
        if (indent < node.indent)
        {
            if (children.Count > 0 && children[0].indent != node.indent)
            {
                Debug.Log("child indentation: "+children[0].indent+" vs node indentation: "+node.indent);
                throw new Exception("[Line "+node.lineNumber.ToString()+"] child indentations must match (tabs are expanded to 4 spaces by default)");
            }
            children.Add(node);
            node.parent = this;
        }
        else
        {
            if (parent == null)
            {
                throw new Exception("Root node too indented?");
            }
            parent.insert(node);
        }       
    }

    public string reproduce()
    {
        if (children.Count == 0) return content;

        return content + "\n"+(new string(' ', children[0].indent)) 
               + string.Join("\n"+(new string(' ', children[0].indent)),
                   children.Select(item => item.reproduce()).ToArray());
    }
    
    public VGDLNode getRoot()
    {
        return parent != null ? parent.getRoot() : this;
    }

}