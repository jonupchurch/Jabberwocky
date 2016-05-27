using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jabberwocky.Autofac.CodeAnalysis.Attributes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InvalidNestedLifetimeScopeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JabberwockyAutofacCodeAnalysisInvalidNestedLifetimeScope";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.InvalidLifetimeScopeAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.InvalidLifetimeScopeAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.InvalidLifetimeScopeAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        private const string AutowireServiceAttributeTypeName = "Jabberwocky.Autofac.Attributes.AutowireServiceAttribute";
        private const string SingleInstanceLifetimeScopeValue = "4";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly string[] IgnoredScopes = { "0"/*"LifetimeScope.Default"*/, "3"/*"LifetimeScope.NoTracking"*/ };

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterCompilationStartAction(BeginCompilationAnalysis);
        }

        private void BeginCompilationAnalysis(CompilationStartAnalysisContext context)
        {
            var impls = FindAutowireClasses(context.Compilation);

            context.RegisterSyntaxNodeAction(c => AnalyzeSyntaxNode(c, impls), SyntaxKind.Parameter);
        }

        private ILookup<ITypeSymbol, INamedTypeSymbol> FindAutowireClasses(Compilation compilation)
        {
            IEnumerable<KeyValuePair<ITypeSymbol, INamedTypeSymbol>> types = Enumerable.Empty<KeyValuePair<ITypeSymbol, INamedTypeSymbol>>();

            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);

                var classDecls = tree.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();
                var concreteTypes = classDecls.Select(c => model.GetDeclaredSymbol(c));

                var typesWithAutowire =
                    concreteTypes.Where(
                        type =>
                            type.GetAttributes()
                                .Any(data => data.AttributeClass.ToDisplayString() == AutowireServiceAttributeTypeName));

                // Should this be 'AllInterfaces'?
                types = types.Concat(typesWithAutowire.SelectMany(type => type.Interfaces,
                    (concreteType, interfaceType) =>
                        new KeyValuePair<ITypeSymbol, INamedTypeSymbol>(interfaceType, concreteType)));

            }

            return types.ToLookup(pair => pair.Key, pair => pair.Value);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context, ILookup<ITypeSymbol, INamedTypeSymbol> interfaceToImplLookup)
        {
            // Check if this is a parameter from within a constructor (is 3 levels deep enough?)
            var argSyntax = (ParameterSyntax)context.Node;
            if (!argSyntax.Ancestors().Take(3).Any(node => node is ConstructorDeclarationSyntax)) return;
            
            IParameterSymbol paramSymbol = context.SemanticModel.GetDeclaredSymbol(argSyntax);
            var paramTypeAutowireAttribute = interfaceToImplLookup[paramSymbol.Type].First().GetAttributes() // TODO: This shouldn't be just 'First'... we should check every one
                .FirstOrDefault(data => data.AttributeClass.ToDisplayString() == AutowireServiceAttributeTypeName);

            INamedTypeSymbol parentTypeSymbol = paramSymbol.ContainingType;
            var parentTypeAutowireAttribute = parentTypeSymbol.GetAttributes()
                .FirstOrDefault(data => data.AttributeClass.ToDisplayString() == AutowireServiceAttributeTypeName);

            bool parentTypeIsAutowired = parentTypeAutowireAttribute != null;
            bool paramTypeIsAutowired = paramTypeAutowireAttribute != null;

            if (parentTypeIsAutowired && paramTypeIsAutowired)
            {
                // This is a valid analysis target:
                // * the parameter in question is a constructor arg,
                // * the parameter's Type is annotated with 'AutowireServiceAttribute',
                // * the containing Type is annotated with 'AutowireServiceAttribute'
                
                bool parentIsSingleton =
                    GetDeclaredAttributeParams(parentTypeAutowireAttribute)
                        .Any(constant => SingleInstanceLifetimeScopeValue == constant.Value.ToString());

                var paramAttributeArgs = GetDeclaredAttributeParams(paramTypeAutowireAttribute).ToImmutableArray();
                bool childHasNarrowerScope = !paramAttributeArgs.IsEmpty &&
                                             !paramAttributeArgs.Any(constant => IgnoredScopes.Contains(constant.Value.ToString()));

                if (parentIsSingleton && childHasNarrowerScope)
                {
                    var diagnostic = Diagnostic.Create(Rule, paramSymbol.Locations[0], paramSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static IEnumerable<TypedConstant> GetDeclaredAttributeParams(AttributeData attribute)
        {
            return attribute.ConstructorArguments.Concat(attribute.NamedArguments.Select(pair => pair.Value));
        }

        //private static IEnumerable<ITypeSymbol> GetImplementors(ITypeSymbol typeSymbol, SyntaxNodeAnalysisContext context)
        //{
        //    SymbolFinder.FindImplementationsAsync()

        //    context.SemanticModel.

        //    return typeSymbol.Interfaces.First().
        //}

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}