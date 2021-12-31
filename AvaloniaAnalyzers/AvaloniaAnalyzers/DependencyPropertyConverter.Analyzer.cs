using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace AvaloniaAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DependencyPropertyConverterAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WpfDepdendencyProperty";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "AvaloniaConversion";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;
            if (fieldSymbol.Type.Name != "DependencyProperty" || !fieldSymbol.IsStatic || !fieldSymbol.IsReadOnly) return;
            var declaratorSynax = (VariableDeclaratorSyntax)fieldSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            if (!(declaratorSynax.Initializer?.Value is InvocationExpressionSyntax)) return;
            var invocation = (InvocationExpressionSyntax)declaratorSynax.Initializer.Value;
            var expression = invocation.Expression as MemberAccessExpressionSyntax;
            if (expression == null) return;
            if (expression.Expression.ToString() == "DependencyProperty" && expression.Name.ToString().StartsWith("Register"))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, declaratorSynax.GetLocation()));
            }
        }
    }
}
