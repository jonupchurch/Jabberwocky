using Jabberwocky.Autofac.Attributes;
using Jabberwocky.Autofac.CodeAnalysis.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jabberwocky.Autofac.CodeAnalysis.Test
{
    [TestClass]
    public class InvariantTests
    {
        [TestMethod]
        public void AutowireServiceAttribute_EnumValueAssumptions_AreValid()
        {
            // Verify that the enum values for each of the LifetimeScopes has not changed

            Assert.AreEqual(InvalidNestedLifetimeScopeAnalyzer.DefaultLifetimeScopeValue, ((int) LifetimeScope.Default).ToString());
            Assert.AreEqual(InvalidNestedLifetimeScopeAnalyzer.NoTrackingLifetimeScopeValue, ((int) LifetimeScope.NoTracking).ToString());
            Assert.AreEqual(InvalidNestedLifetimeScopeAnalyzer.SingleInstanceLifetimeScopeValue, ((int) LifetimeScope.SingleInstance).ToString());
        }
    }
}
