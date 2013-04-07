﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrayVisualizerExt.TypeParsers
{
  public class FSharpMatrixParser : ITypeParser
  {
    private enum ExpressionType
    {
      UnKnown,
      Matrix,
      Vector
    }

    private ExpressionType GetExpressionType(EnvDTE.Expression expression)
    {
      string expressionName = expression.Type + new string(' ', 28);
      switch (expressionName.Substring(0, 28))
      {
        case "Microsoft.FSharp.Math.Matrix":
          return ExpressionType.Matrix;
        case "Microsoft.FSharp.Math.Vector":
          return ExpressionType.Vector;
        default:
          return ExpressionType.UnKnown;
      }
    }

    #region ITypeParser Members

    public char LeftBracket { get; set; }
    public char RightBracket { get; set; }

    public bool IsExpressionTypeSupported(EnvDTE.Expression expression)
    {
      return GetExpressionType(expression) != ExpressionType.UnKnown;
    }

    public string GetDisplayName(EnvDTE.Expression expression)
    {
      string formatter;
      int[] dimensions = GetDimensions(expression);
      switch (dimensions.Length)
      {
        case 2:
          formatter = "Matrix{0}{1},{2}{3}";
          return string.Format(formatter, this.LeftBracket, dimensions[0], dimensions[1], this.RightBracket);
        case 1:
          formatter = "Vector{0}{1}{2}";
          return string.Format(formatter, this.LeftBracket, dimensions[0], this.RightBracket);
        default:
          throw new NotSupportedException(string.Format("'{0} is not supported.'", expression.Type));
      }
    }

    public int[] GetDimensions(EnvDTE.Expression expression)
    {
      int[] dims;
      switch (GetExpressionType(expression))
      {
        case ExpressionType.Matrix:
          dims = new int[2];
          EnvDTE.Expressions members = expression.DataMembers.Item("Item").DataMembers;
          dims[0] = int.Parse(members.Item("NumRows").Value);
          dims[1] = int.Parse(members.Item("NumCols").Value);
          break;
        case ExpressionType.Vector:
          dims = new int[1];
          dims[0] = expression.DataMembers.Count - 1;
          break;
        default:
          throw new NotSupportedException(string.Format("'{0} is not supported.'", expression.Type));
      }
      return dims;
    }

    public int GetMembersCount(EnvDTE.Expression expression)
    {
      int[] dims = GetDimensions(expression);
      if (dims.Length == 1)
        return dims[0];
      else
        return dims[0] * dims[1];
    }

    public object[] GetValues(EnvDTE.Expression expression)
    {
      switch (GetExpressionType(expression))
      {
        case ExpressionType.Matrix:
          return expression.DataMembers.Item("Item").DataMembers.Item("Values").DataMembers.Cast<EnvDTE.Expression>().Select(e => e.Value).ToArray();
        case ExpressionType.Vector:
          int count = expression.DataMembers.Count - 1;
          return expression.DataMembers.Cast<EnvDTE.Expression>().Take(count).Select(e => e.Value).ToArray();
        default:
          throw new NotSupportedException(string.Format("'{0} is not supported.'", expression.Type));
      }
    }

    #endregion
  }
}
