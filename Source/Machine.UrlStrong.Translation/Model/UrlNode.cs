﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Core;

namespace Machine.UrlStrong.Translation.Model
{
  public class UrlRootNode : UrlNode
  {
    public UrlRootNode()
      : base(new ParsedUrlPart(""))
    {
    }

    public override bool IsRoot { get { return true; } }
  }

  public class UrlNode
  {
    readonly string _name;

    public virtual bool IsRoot
    {
      get { return false; }
    }

    public string Name
    {
      get { return _name; }
    }

    public string ClassName
    {
      get { return _name.ReplaceDashes().ToPascalCase(); }
    }

    public string AccessorName
    {
      get { return _name.ReplaceDashes().ToCamelCase().EscapeReservedWords(); }
    }

    public ParsedUrl Url
    {
      get;
      set;
    }

    public bool HasParameters
    {
      get
      {
        return !IsOnlyParameter && _parameters.Any();
      }
    }

    readonly Dictionary<string, UrlNode> _children;
    readonly bool _isOnlyParameter;

    readonly IEnumerable<Parameter> _parameters;

    public IEnumerable<Parameter> Parameters
    {
      get { return _parameters; }
    }

    public string ImplementedInterfaces
    {
      get
      {
        if (Url == null) return string.Empty;

        return ", " + String.Join(", ", Url.AcceptedVerbs.Select(x => "ISupport" + x.ToString().ToPascalCase()).ToArray());
      }
    }

    public UrlNode(ParsedUrlPart part)
    {
      _name = part.PartName;
      if (string.IsNullOrEmpty(_name))
      {
        _name = "root";
      }

      FormatString = part.FormatString;
      _isOnlyParameter = part.IsOnlyParameter;
      _parameters = part.Parameters.Select(x => new Parameter(x, "object"));
      _children = new Dictionary<string, UrlNode>();
    }

    public string AdditionalConstructorArguments
    {
      get
      {
        if (_parameters.Any())
        {
          return ", " + FormalParameters;
        }
        else
        {
          return string.Empty;
        }
      }
    }

    public string FormalParameters
    {
      get
      {
        if (!_parameters.Any())
          return string.Empty;

        return String.Join(", ", _parameters.Select(x => x.FormalDeclaration).ToArray());
      }
    }

    public string ActualParameters
    {
      get
      {
        if (!_parameters.Any())
          return string.Empty;

        return String.Join(", ", _parameters.Select(x => x.FormalParameterName).ToArray());
      }
    }

    public string FormatStringArguments
    {
      get
      {
        if (!_parameters.Any())
          throw new Exception("There are no FormatStringArguments, this is a bug.");

        return String.Join(", ", _parameters.Select(x => x.FieldName).ToArray());
      }
    }

    public string ToFormattedNameString()
    {
      return string.Format(FormatString, FormatNameStringArguments);
    }

    public string[] FormatNameStringArguments
    {
      get
      {
        if (!_parameters.Any())
          throw new Exception("There are no FormatStringArguments, this is a bug.");

        return _parameters.Select(x => "{" + x.Name + "}").ToArray();
      }
    }

    public bool HasChildNamed(string name)
    {
      return _children.ContainsKey(name);
    }

    public void AddChild(UrlNode child)
    {
      _children[child.Name] = child;
    }

    public UrlNode GetChild(string name)
    {
      return _children[name];
    }

    public IEnumerable<UrlNode> Children
    {
      get { return _children.Values; }
    }

    public bool IsOnlyParameter
    {
      get { return _isOnlyParameter; }
    }

    public string FormatString
    {
      get;
      private set;
    }

    public override string ToString()
    {
      string ret = this.Name;

      if (_children.Any())
      {
        ret += "/\n";

        foreach (var child in _children)
        {
          foreach (var line in child.ToString().Split('\n'))
          {
            ret += "  " + line + "\n";
          }
        }
      }

      return ret;
    }
  }
}
