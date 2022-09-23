﻿// Copyright 2014 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.ComponentInterfaces;
using Microsoft.VisualStudio.Debugger.Evaluation;
using VsChromium.DkmIntegration.Visualizers;

namespace VsChromium.DkmIntegration.IdeComponent
{
  // Visual Studio's debugger component model is a layered one, similar to a driver filter chain,
  // whereby components are notified either from lowest to highest or highest to lowest depending
  // on the operation.  Components are further sub-categorized according to their level into
  // "server components" and "ide components".  This is done to support remote debugging, although
  // the same separation exists even for local debugging.  Certain debug engine operations 
  // (typically opeartions involving low level access to the debug engine) can only be performed in
  // the context of a server component (ComponentLevel < 100000), while other operations can only
  // be performed from the context of an IDE component.
  //
  // This class implements the server component.
  public class IdeComponent 
        : IDkmCustomVisualizer {
      static IdeComponent()
      {
        VisualizerRegistrar.Register<DateTimeVisualizer.Factory>(Guids.CustomVisualizer.BaseTime);
        VisualizerRegistrar.Register<TimeDeltaVisualizer.Factory>(Guids.CustomVisualizer.BaseTimeDelta);
      }

      public IdeComponent() {
      }

      private bool TryGetRegisteredVisualizer(DkmVisualizedExpression expression, out BasicVisualizer visualizer, out DkmFailedEvaluationResult failureResult)
      {
        visualizer = null;
        failureResult = null;

        if (VisualizerRegistrar.TryCreateVisualizer(expression, out visualizer))
          return true;

        string name = null;
        string fullName = null;
        Utility.GetExpressionName(expression, out name, out fullName);

        DkmFailedEvaluationResult failure = DkmFailedEvaluationResult.Create(
            expression.InspectionContext,
            expression.StackFrame,
            name,
            fullName,
            String.Format("No formatter is registered for VisualizerId {0}",
                expression.VisualizerId),
            DkmEvaluationResultFlags.Invalid,
            null);
        failureResult = failure;
        return false;
      }

      void IDkmCustomVisualizer.EvaluateVisualizedExpression(DkmVisualizedExpression expression, out DkmEvaluationResult resultObject)
      {
        BasicVisualizer visualizer = null;
        DkmFailedEvaluationResult failureResult = null;

        if (!TryGetRegisteredVisualizer(expression, out visualizer, out failureResult))
        {
          resultObject = failureResult;
          return;
        }

        DkmEvaluationResult evalResult = visualizer.EvaluationResult;
        EvaluationDataItem resultDataItem = new EvaluationDataItem(expression, evalResult);

        expression.SetDataItem(DkmDataCreationDisposition.CreateAlways, resultDataItem);

        string name = null;
        string fullName = null;
        Utility.GetExpressionName(expression, out name, out fullName);

        if (evalResult.TagValue == DkmEvaluationResult.Tag.SuccessResult) 
        {
          DkmSuccessEvaluationResult successResult = (DkmSuccessEvaluationResult)evalResult;
          resultObject = DkmSuccessEvaluationResult.Create(
              successResult.InspectionContext,
              successResult.StackFrame,
              name,
              successResult.FullName,
              successResult.Flags,
              successResult.Value,
              successResult.EditableValue,
              successResult.Type,
              successResult.Category,
              successResult.Access,
              successResult.StorageType,
              successResult.TypeModifierFlags,
              successResult.Address,
              successResult.CustomUIVisualizers,
              successResult.ExternalModules,
              resultDataItem);
        }
        else
        {
          DkmFailedEvaluationResult failResult = (DkmFailedEvaluationResult)evalResult;

          resultObject = DkmFailedEvaluationResult.Create(
              failResult.InspectionContext,
              failResult.StackFrame,
              name,
              fullName,
              failResult.ErrorMessage,
              failResult.Flags,
              null);
          return;
        }
      }

      void IDkmCustomVisualizer.GetChildren(DkmVisualizedExpression expression, int initialRequestSize, DkmInspectionContext inspectionContext, out DkmChildVisualizedExpression[] initialChildren, out DkmEvaluationResultEnumContext enumContext)
      {
        EvaluationDataItem dataItem = expression.GetDataItem<EvaluationDataItem>();
        if (dataItem == null)
        {
          Debug.Fail("DebugComponent.GetChildren passed a visualized expression that does not have an associated ExpressionDataItem.");
          throw new NotSupportedException();
        }

        initialChildren = new DkmChildVisualizedExpression[0];

        enumContext = DkmEvaluationResultEnumContext.Create(
            dataItem.Visualizer.ChildElementCount,
            expression.StackFrame,
            expression.InspectionContext,
            null);
      }

      void IDkmCustomVisualizer.GetItems(DkmVisualizedExpression expression, DkmEvaluationResultEnumContext enumContext, int startIndex, int count, out DkmChildVisualizedExpression[] items)
      {
        EvaluationDataItem dataItem = expression.GetDataItem<EvaluationDataItem>();
        if (dataItem == null)
        {
          Debug.Fail("DebugComponent.GetItems passed a visualized expression that does not have an associated ExpressionDataItem.");
          throw new NotSupportedException();
        }

        items = dataItem.Visualizer.GetChildItems(startIndex, count);
      }

      string IDkmCustomVisualizer.GetUnderlyingString(DkmVisualizedExpression visualizedExpression)
      {
        throw new NotImplementedException();
      }

      void IDkmCustomVisualizer.SetValueAsString(DkmVisualizedExpression visualizedExpression, string value, int timeout, out string errorText)
      {
        throw new NotImplementedException();
      }

      void IDkmCustomVisualizer.UseDefaultEvaluationBehavior(DkmVisualizedExpression expression, out bool useDefaultEvaluationBehavior, out DkmEvaluationResult defaultEvaluationResult)
      {
        BasicVisualizer visualizer = null;

        defaultEvaluationResult = null;
        useDefaultEvaluationBehavior = true;
        if (expression.VisualizerId != Guids.CustomVisualizer.ForceDefault &&
            VisualizerRegistrar.TryCreateVisualizer(expression, out visualizer))
        {
          // If this visualizer has custom fields, or displays default fields non-inline, don't use
          // the default evaluation behavior.
          ChildDisplayFlags flags = visualizer.ChildDisplayFlags;
          if (flags.HasFlag(ChildDisplayFlags.HasCustomFields) ||
              !flags.HasFlag(ChildDisplayFlags.DefaultFieldsInline))
            useDefaultEvaluationBehavior = false;
        }
        
        if (useDefaultEvaluationBehavior)
        {
          string name = null;
          string fullName = null;
          Utility.GetExpressionName(expression, out name, out fullName);
          fullName += ",!";
          defaultEvaluationResult = CppExpressionEvaluator.Evaluate(expression, fullName);
        }
      }
    }
}
