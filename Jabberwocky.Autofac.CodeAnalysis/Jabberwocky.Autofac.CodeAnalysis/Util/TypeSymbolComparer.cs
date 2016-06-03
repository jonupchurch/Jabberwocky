using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Jabberwocky.Autofac.CodeAnalysis.Util
{
    public class TypeSymbolComparer : IEqualityComparer<ITypeSymbol>
    {
        public bool Equals(ITypeSymbol x, ITypeSymbol y)
        {
            return x.ToDisplayString() == y.ToDisplayString()
                && (x.ContainingAssembly?.ToDisplayString() ?? string.Empty) == (y.ContainingAssembly?.ToDisplayString() ?? string.Empty);
        }

        public int GetHashCode(ITypeSymbol obj)
        {
            return 0; // Don't care
        }
    }
}
