﻿using System;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace JetBrains.ReSharper.ControlFlow.PostfixCompletion
{
  [Language(typeof(CSharpLanguage))]
  public class CSharpPostfixItemProviderSmart : CSharpItemsProviderBase<CSharpCodeCompletionContext>
  {
    [NotNull] private readonly PostfixTemplatesManager myTemplatesManager;

    public CSharpPostfixItemProviderSmart([NotNull] PostfixTemplatesManager templatesManager)
    {
      myTemplatesManager = templatesManager;
    }

    protected override bool IsAvailable(CSharpCodeCompletionContext context)
    {
      var completionType = context.BasicContext.CodeCompletionType;
      return completionType == CodeCompletionType.AutomaticCompletion
          || completionType == CodeCompletionType.BasicCompletion;
    }

    protected override bool AddLookupItems(CSharpCodeCompletionContext context, GroupedItemsCollector collector)
    {
      var node = context.NodeInFile;
      if (node == null) return false;

      ReparsedCodeCompletionContext completionContext = null;

      // foo.{caret}  var bar = ...; fuck my life
      var referenceName = node.Parent as IReferenceName;
      if (referenceName != null)
      {
        completionContext = context.UnterminatedContext;
        node = completionContext.TreeNode;
        if (node == null) return false;
      }

      var looseChecks = (context.BasicContext.CodeCompletionType == CodeCompletionType.BasicCompletion);

      var items = myTemplatesManager.GetAvailableItems(node, looseChecks, completionContext);
      foreach (var lookupItem in items)
        collector.AddAtDefaultPlace(lookupItem);

      return (items.Count > 0);
    }

    // todo: transform?
    // todo: can items be hidden by .ForEach and friends?
  }
}