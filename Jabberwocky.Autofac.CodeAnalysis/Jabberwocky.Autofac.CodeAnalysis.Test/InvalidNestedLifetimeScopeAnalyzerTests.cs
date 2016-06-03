using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestHelper;
using Jabberwocky.Autofac.CodeAnalysis.Attributes;

namespace Jabberwocky.Autofac.CodeAnalysis.Test
{
    [TestClass]
    public class InvalidNestedLifetimeScopeAnalyzerTests : CodeFixVerifier
    {

        private const string AutowireServiceFilePath = "References/AutowireServiceAttribute.cs";

        #region Test Compilation Source

        private const string SingletonRegistration_HasInvalidDependencyLifetime_Source = @"

	using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Jabberwocky.Autofac.Attributes;
    
    [AutowireService(LifetimeScope.SingleInstance)]
	public class MyClass : IMyClass {
		private readonly IMyDep _myDep;

		public MyClass(IMyDep myDep) {
			_myDep = myDep;
		}
	}

    public interface IMyClass {}

    [AutowireService(LifetimeScope.PerRequest)]
    public class MyDepClass : IMyDep {

    }

    public interface IMyDep {}

";

        private const string SingletonRegistration_HasValidDependencyLifetime_Source = @"

	using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Jabberwocky.Autofac.Attributes;
    
    [AutowireService(LifetimeScope.SingleInstance)]
	public class MyClass : IMyClass {
		private readonly IMyDep _myDep;

		public MyClass(IMyDep myDep) {
			_myDep = myDep;
		}
	}

    public interface IMyClass {}

    [AutowireService] // This is default, 'Transient'
    public class MyDepClass : IMyDep {

    }

    public interface IMyDep {}

";

        private const string SingletonRegistration_HasInvalidNestedDependencyLifetime_Source = @"

	using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Jabberwocky.Autofac.Attributes;
    
    [AutowireService(LifetimeScope.SingleInstance)]
	public class MyClass : IMyClass {
		private readonly IMyDep _myDep;

		public MyClass(IMyDep myDep) {
			_myDep = myDep;
		}
	}

    public interface IMyClass {}

    [AutowireService] // This is default, 'Transient'
    public class MyDepClass : IMyDep {
        public MyDepClass(IMyOtherDep myOtherDep) {
        }
    }
    
    [AutowireService(LifetimeScope.PerRequest)]
    public class MyOtherDepClass : IMyOtherDep {
    
    }

    public interface IMyDep {}

    public interface IMyOtherDep {}

";

        #endregion

        //No diagnostics expected to show up
        [TestMethod]
        public void SingletonRegistration_HasValidDependencyLifetime_Analysis()
        {
            var test = SingletonRegistration_HasValidDependencyLifetime_Source;

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void SingletonRegistration_HasInvalidDependencyLifetime_Analysis()
        {
            var source = SingletonRegistration_HasInvalidDependencyLifetime_Source;

            var expected = new DiagnosticResult
            {
                Id = "JabberwockyAutofacCodeAnalysisInvalidNestedLifetimeScope",
                Message = String.Format("Type name '{0}' contains lowercase letters", "myDep"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 15, 25)
                        }
            };

            VerifyCSharpDiagnostic(source, expected);
        }

        [TestMethod]
        public void SingletonRegistration_HasInvalidNestedDependencyLifetime_Analysis()
        {
            var source = SingletonRegistration_HasInvalidNestedDependencyLifetime_Source;

            var expected = new DiagnosticResult
            {
                Id = "JabberwockyAutofacCodeAnalysisInvalidNestedLifetimeScope",
                Message = String.Format("Type name '{0}' contains lowercase letters", "myDep"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 24, 39)
                        }
            };

            VerifyCSharpDiagnostic(source, expected);
        }

        #region Test Helpers

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new JabberwockyAutofacCodeAnalysisCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new InvalidNestedLifetimeScopeAnalyzer();
        }

        protected override IEnumerable<string> GetReferencedFiles()
        {
            return new[] { AutowireServiceFilePath }
                .Select(File.ReadAllText);
        }

        #endregion
    }
}