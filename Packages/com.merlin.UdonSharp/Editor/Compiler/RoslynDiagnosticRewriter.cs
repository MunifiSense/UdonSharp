
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UdonSharp.Compiler
{
    internal static class RoslynDiagnosticRewriter
    {
        private const string TMPDropdownAddOptionsHint =
            "TMP_Dropdown.AddOptions with string[], Sprite[], or TMP_Dropdown.OptionData[] is a VRChat extension method. Add 'using VRC.SDK3.Components;' at the top of this file.";

        private static readonly HashSet<string> _tmpDropdownAddOptionsDiagnosticIds = new HashSet<string>
        {
            "CS1501", "CS1503", "CS1615", "CS7036", "CS1061",
        };

        public static string FormatDiagnostic(CSharpCompilation compilation, Diagnostic diagnostic)
        {
            string rewrittenMessage = TryGetTMPDropdownAddOptionsHint(compilation, diagnostic);
            if (rewrittenMessage != null)
                return $"error USHARP_TMPDropdown: {rewrittenMessage}";

            return $"{diagnostic.Severity.ToString().ToLower()} {diagnostic.Id}: {diagnostic.GetMessage()}";
        }

        private static string TryGetTMPDropdownAddOptionsHint(CSharpCompilation compilation, Diagnostic diagnostic)
        {
            if (!_tmpDropdownAddOptionsDiagnosticIds.Contains(diagnostic.Id))
                return null;

            Location location = diagnostic.Location;
            if (location == null || !location.IsInSource)
                return null;

            SyntaxTree syntaxTree = location.SourceTree;
            if (syntaxTree == null)
                return null;

            if (syntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>()
                .Any(usingDirective => usingDirective.Name?.ToString() == "VRC.SDK3.Components"))
                return null;

            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            SyntaxNode node = syntaxTree.GetRoot().FindNode(location.SourceSpan, getInnermostNodeForTie: true);
            InvocationExpressionSyntax invocation = FindEnclosingInvocation(node);
            if (invocation == null)
                return null;

            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess) ||
                memberAccess.Name.Identifier.Text != "AddOptions")
                return null;

            ITypeSymbol receiverType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (!IsTMPDropdownType(receiverType))
                return null;

            if (!invocation.ArgumentList.Arguments.Any(argument =>
                    IsVRCTMPDropdownAddOptionsArgumentType(semanticModel.GetTypeInfo(argument.Expression).Type)))
                return null;

            return TMPDropdownAddOptionsHint;
        }

        private static InvocationExpressionSyntax FindEnclosingInvocation(SyntaxNode node)
        {
            while (node != null)
            {
                if (node is InvocationExpressionSyntax invocation)
                    return invocation;

                node = node.Parent;
            }

            return null;
        }

        private static bool IsTMPDropdownType(ITypeSymbol typeSymbol)
        {
            for (ITypeSymbol current = typeSymbol; current != null; current = current.BaseType)
            {
                if (current.Name == "TMP_Dropdown" && current.ContainingNamespace?.ToString() == "TMPro")
                    return true;
            }

            return false;
        }

        private static bool IsVRCTMPDropdownAddOptionsArgumentType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null || typeSymbol.TypeKind != TypeKind.Array)
                return false;

            ITypeSymbol elementType = ((IArrayTypeSymbol)typeSymbol).ElementType;

            if (elementType.SpecialType == SpecialType.System_String)
                return true;

            if (elementType.Name == "Sprite" && elementType.ContainingNamespace?.ToString() == "UnityEngine")
                return true;

            return elementType.Name == "OptionData" &&
                   elementType.ContainingType?.Name == "TMP_Dropdown" &&
                   elementType.ContainingType.ContainingNamespace?.ToString() == "TMPro";
        }
    }
}
