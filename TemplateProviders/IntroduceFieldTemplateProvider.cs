﻿using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.ControlFlow.PostfixCompletion.LookupItems;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Refactorings.IntroduceField;
using JetBrains.ReSharper.Refactorings.WorkflowNew;
using JetBrains.TextControl;
using JetBrains.TextControl.Coords;
using JetBrains.Util;
using DataConstants = JetBrains.DocumentModel.DataContext.DataConstants;

namespace JetBrains.ReSharper.ControlFlow.PostfixCompletion.TemplateProviders
{
  [PostfixTemplateProvider("field", "Introduces field for expression in constructors")]
  public class IntroduceFieldTemplateProvider : IPostfixTemplateProvider
  {
    public void CreateItems(PostfixTemplateAcceptanceContext context, ICollection<ILookupItem> consumer)
    {
      if (context.CanBeStatement)
      {
        var declaration = context.ContainingFunction;
        if (declaration == null) return;

        // only in constructors by default
        if (context.LooseChecks || declaration.DeclaredElement is IConstructor)
        {
          consumer.Add(new IntroduceFieldLookupItem(context));
        }
      }
    }

    private class IntroduceFieldLookupItem : ProcessExpressionPostfixLookupItem
    {
      public IntroduceFieldLookupItem([NotNull] PostfixTemplateAcceptanceContext context)
        : base(context, "field") { }

      protected override void AcceptExpression(
        ITextControl textControl, ISolution solution, TextRange resultRange, ICSharpExpression expression)
      {
        // set selection for introduce field
        textControl.Selection.SetRanges(new[] {TextControlPosRange.FromDocRange(textControl, resultRange)});

        const string name = "IntroFieldAction";
        var rules = DataRules
          .AddRule(name, ProjectModel.DataContext.DataConstants.SOLUTION, solution)
          .AddRule(name, DataConstants.DOCUMENT, textControl.Document)
          .AddRule(name, TextControl.DataContext.DataConstants.TEXT_CONTROL, textControl)
          .AddRule(name, Psi.Services.DataConstants.SELECTED_EXPRESSION, expression);

        Lifetimes.Using(lifetime =>
          WorkflowExecuter.ExecuteBatch(
            Shell.Instance.GetComponent<DataContexts>().CreateWithDataRules(lifetime, rules),
            new IntroFieldWorkflow(solution, null)));

        // todo: rename hotspots

        var ranges = textControl.Selection.Ranges.Value;
        if (ranges.Count == 1) // reset selection
        {
          var endPos = ranges[0].End;
          textControl.Selection.SetRanges(new[] {new TextControlPosRange(endPos, endPos)});
        }
      }
    }
  }
}