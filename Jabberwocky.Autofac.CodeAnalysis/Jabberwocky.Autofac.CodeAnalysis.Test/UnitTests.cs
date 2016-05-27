using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestHelper;
using Jabberwocky.Autofac.CodeAnalysis;
using Jabberwocky.Autofac.CodeAnalysis.Attributes;

namespace Jabberwocky.Autofac.CodeAnalysis.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
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

        #endregion

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestMethod2()
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
    }
}