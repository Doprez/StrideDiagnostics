﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Sources;

namespace StrideDiagnostics.PropertyFinders;
internal class DoubledAnnotationReporter : IViolationReporter, IPropertyFinder
{
    /// <summary>
    /// Is always Empty
    /// </summary>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public IEnumerable<IPropertySymbol> Find(ref INamedTypeSymbol baseType)
    {
        return Enumerable.Empty<IPropertySymbol>();
    }

    public void ReportViolations(ref INamedTypeSymbol baseType, ClassInfo classInfo)
    {
        if (baseType == null)
            return;
        var violations = baseType.GetMembers().OfType<IPropertySymbol>();

        var violationsFiltered = violations.Where(property => this.ShouldBeIgnored(property) && this.HasDataMemberAnnotation(property));
        foreach (var violation in violationsFiltered)
        {

            Report(violation, classInfo);


        }
    }


    private static void Report(IPropertySymbol property, ClassInfo classInfo)
    {
        DiagnosticDescriptor error = new DiagnosticDescriptor(
            id: ErrorCodes.DoubledAnnotation,
            title: "Invalid Annotations",
            category: NexGenerator.CompilerServicesDiagnosticCategory,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            messageFormat: $"The Property has a contradiction in the Annotations, there can't be [DataMember] and [DataMemberIgnore] on the same Property.",
            helpLinkUri: "https://www.stride3d.net"
        );
        Location location = Location.Create(classInfo.TypeSyntax.SyntaxTree, property.DeclaringSyntaxReferences.FirstOrDefault().Span);
        classInfo.ExecutionContext.ReportDiagnostic(Diagnostic.Create(error, location));
    }
}
