using UnityEngine;

namespace UdonSharp.Tests
{
    [AddComponentMenu("Udon Sharp/Tests/AsExpressionTest")]
    public class AsExpressionTest : UdonSharpBehaviour
    {
        [System.NonSerialized]
        public IntegrationTestSuite tester;

        public void ExecuteTests()
        {
            Component componentRef = transform;
            Component thisComponent = this;

            Transform transformRef = componentRef as Transform;
            tester.TestAssertion("as cast success", transformRef != null);

            BoxCollider boxColliderRef = thisComponent as BoxCollider;
            tester.TestAssertion("as cast failure returns null", boxColliderRef == null);
        }
    }
}
