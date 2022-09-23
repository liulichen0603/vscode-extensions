﻿// Copyright 2014 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Evaluation;

namespace VsChromium.DkmIntegration.IdeComponent
{
  public class EvaluationDataItem : DkmDataItem
  {
    private DkmVisualizedExpression expression_;
    private DkmEvaluationResult evalResult_;
    private BasicVisualizer visualizer_;

    public EvaluationDataItem(DkmVisualizedExpression expression, DkmEvaluationResult evalResult)
    {
      expression_ = expression;
      evalResult_ = evalResult;

      VisualizerRegistrar.TryCreateVisualizer(expression, out visualizer_);
    }

    protected override void OnClose()
    {
      base.OnClose();
      //evalResult_.Close();
    }

    public DkmVisualizedExpression Expression
    {
      get { return expression_; }
    }

    public DkmEvaluationResult EvaluationResult
    {
      get { return evalResult_; }
    }

    public BasicVisualizer Visualizer
    {
      get { return visualizer_; }
    }
  }
}
