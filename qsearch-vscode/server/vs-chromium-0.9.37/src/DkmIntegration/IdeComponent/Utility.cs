﻿// Copyright 2014 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using Microsoft.VisualStudio.Debugger.Evaluation;

namespace VsChromium.DkmIntegration.IdeComponent
{
  public static class Utility
  {
    public static void GetExpressionName(DkmVisualizedExpression expression, out string name, out string fullName)
    {
      if (expression.TagValue == DkmVisualizedExpression.Tag.RootVisualizedExpression)
      {
        DkmRootVisualizedExpression rootExpr = (DkmRootVisualizedExpression)expression;
        name = rootExpr.Name;
        fullName = rootExpr.FullName;
      }
      else
      {
        DkmChildVisualizedExpression childExpr = (DkmChildVisualizedExpression)expression;
        name = childExpr.EvaluationResult.Name;
        fullName = childExpr.EvaluationResult.FullName;
      }
    }
  }
}
