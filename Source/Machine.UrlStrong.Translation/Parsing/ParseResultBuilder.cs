﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.UrlStrong.Translation.Model;

namespace Machine.UrlStrong.Translation.Parsing
{
  public class ParseResultBuilder : IParseListener
  {
    int _currentLineNumber;
    string _currentLine;
    string _namespace = "";
    string _className = "Urls";
    List<ParsedUrl> _urls = new List<ParsedUrl>();
    List<string> _namespaces = new List<string>();
    List<ParseError> _errors = new List<ParseError>();

    public void BeginLine(int lineNumber, string line)
    {
      _currentLine = line;
      _currentLineNumber = lineNumber;
    }

    public void AddError(string error)
    {
      var parseError = new ParseError(_currentLineNumber, _currentLine, error);
      _errors.Add(parseError);
    }

    public void OnUrl(IEnumerable<string> verbs, string url, string hash, string comment)
    {
      var parsedVerbs = ParseVerbs(verbs);
      var parsedUrlParts = ParseUrl(url);

      _urls.Add(new ParsedUrl(parsedVerbs, parsedUrlParts, hash, comment));
    }

    static IEnumerable<ParsedUrlPart> ParseUrl(string url)
    {
      List<ParsedUrlPart> parts = new List<ParsedUrlPart>();
      foreach (var part in url.Split(new [] {'/'}, StringSplitOptions.RemoveEmptyEntries))
      {
        parts.Add(new ParsedUrlPart(part));
      }

      return parts;
    }

    IEnumerable<HttpVerbs> ParseVerbs(IEnumerable<string> verbs)
    {
      if (verbs.Contains("*"))
      {
        return Enum.GetValues(typeof(HttpVerbs)).Cast<HttpVerbs>();
      }

      List<HttpVerbs> parsedVerbs = new List<HttpVerbs>();

      foreach (var verb in verbs)
      {
        try
        {
          HttpVerbs parsedVerb = (HttpVerbs)Enum.Parse(typeof(HttpVerbs), verb, true);

          parsedVerbs.Add(parsedVerb);
        }
        catch (ArgumentException)
        {
          AddError(String.Format("Unknown verb: {0}, try one of these: {1}", verb,
            String.Join(", ", Enum.GetNames(typeof(HttpVerbs)).Select(x => x.ToUpper()).ToArray())));
        }
      }

      return parsedVerbs;
    }

    public void OnUsingNamespace(string @namespace)
    {
      _namespaces.Add(@namespace);
    }

    public void OnNamespace(string value)
    {
      if (!String.IsNullOrEmpty(_namespace))
      {
        throw new Exception("You can only have one namespace per url file.");
      }
      _namespace = value;
    }

    public void OnClassName(string value)
    {
      if (!String.IsNullOrEmpty(_className))
      {
        throw new Exception("You can only have one class per url file.");
      }
      _namespace = value;
    }

    public ParseResult GetResult()
    {
      var urlConfig = new UrlStrongModel(_urls, _namespaces, _namespace, _className);

      return new ParseResult(urlConfig, _errors);
    }
  }

  public class ParseError
  {
    readonly int _lineNumber;
    readonly string _line;
    readonly string _error;

    public ParseError(int lineNumber, string line, string error)
    {
      _lineNumber = lineNumber;
      _line = line;
      _error = error;
    }
  }
}
