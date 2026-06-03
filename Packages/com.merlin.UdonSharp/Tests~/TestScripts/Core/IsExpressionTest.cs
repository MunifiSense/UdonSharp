using UnityEngine;

namespace UdonSharp.Tests
{
    [AddComponentMenu("Udon Sharp/Tests/IsExpressionTest")]
    public class IsExpressionTest : UdonSharpBehaviour
    {
        [System.NonSerialized]
        public IntegrationTestSuite tester;

        public void ExecuteTests()
        {
            Component componentRef = transform;
            Component thisComponent = this;
            object boxedInt = 123;
            object boxedFloat = 123f;
            object boxedNull = null;

            bool isTransform = componentRef is Transform;
            tester.TestAssertion("is type check success", isTransform);

            bool isBoxCollider = thisComponent is BoxCollider;
            tester.TestAssertion("is type check failure", !isBoxCollider);

            tester.TestAssertion("is boxed int", boxedInt is int);
            tester.TestAssertion("is boxed int nullable", boxedInt is int?);
            tester.TestAssertion("is boxed int mismatch", !(boxedInt is float));
            tester.TestAssertion("is boxed float", boxedFloat is float);
            tester.TestAssertion("is null boxed int", !(boxedNull is int));
        }
    }
}
