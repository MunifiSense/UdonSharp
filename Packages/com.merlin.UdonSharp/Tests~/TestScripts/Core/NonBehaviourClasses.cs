
using System.Collections.Generic;
using UdonSharp;
using UdonSharp.Tests;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSharp.Tests
{
    internal class OtherClass<T>
    {
        public T Field;
        
        public OtherClass()
        {
            Field = default(T);
        }
        
        public OtherClass(T val)
        {
            Field = val;
        }
        
        public void TestMethod(T val)
        {
            Field = val;
        }
    }
    
    internal class MyNonBehaviourClass
    {
        public string AutoProp { get; set; } = "Hello";
        public int Field = 5;
        public OtherClass<float> OtherClassInstance = new OtherClass<float>();
        
        public int Property
        {
            get => Field;
            set => Field = value;
        }
        
        public bool TestMethod(int a, int b)
        {
            return a == b;
        }
        
        public T TestMethod2<T>(T a, T b)
        {
            return a;
        }
    }

    internal class DictionaryKeyClass
    {
        public int Id;

        public DictionaryKeyClass(int id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            DictionaryKeyClass other = obj as DictionaryKeyClass;
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
    
    [AddComponentMenu("Udon Sharp/Tests/NonBehaviourClasses")]
    public class NonBehaviourClasses : UdonSharpBehaviour
    {
        [System.NonSerialized] public IntegrationTestSuite tester;
        
        public List<int> list = new List<int>() { 1, 2, 3, 4, 5 };
        private Dictionary<int, string> dict = new Dictionary<int, string>();
        
        public void ExecuteTests()
        {
            TestBasicClass();
            TestLists();
            TestDictionary();
            ExternalReferencedClass();
            ReferencedBehaviourWithNonBehaviourClass();
            ArrayOfUserType();
        }

        private void TestBasicClass()
        {
            MyNonBehaviourClass testClass = new MyNonBehaviourClass();
            
            tester.TestAssertion("Imported Class Field", testClass.Field == 5);
            testClass.Field = 10;
            tester.TestAssertion("Imported Class Field2", testClass.Field == 10);
            
            tester.TestAssertion("Imported Class Auto Property", testClass.AutoProp == "Hello");
            testClass.AutoProp = "World";
            tester.TestAssertion("Imported Class Auto Property2", testClass.AutoProp == "World");
            
            tester.TestAssertion("Imported Class Property", testClass.Property == 10);
            testClass.Property = 20;
            tester.TestAssertion("Imported Class Property2", testClass.Property == 20);
            
            tester.TestAssertion("Imported Class Method", testClass.TestMethod(1, 1));
            tester.TestAssertion("Imported Class Method2", !testClass.TestMethod(2, 1));
            
            tester.TestAssertion("Imported Class Generic Method", testClass.TestMethod2(1, 1) == 1);
            tester.TestAssertion("Imported Class Generic Method2", testClass.TestMethod2(2, 1) == 2);
            
            tester.TestAssertion("Imported Class Generic Field", testClass.OtherClassInstance.Field == 0);
            testClass.OtherClassInstance.Field = 5;
            tester.TestAssertion("Imported Class Generic Field2", testClass.OtherClassInstance.Field == 5);
            
            testClass.OtherClassInstance = new OtherClass<float>(1.0f);
            tester.TestAssertion("Imported Class Generic Field3", testClass.OtherClassInstance.Field == 1.0f);
        }

        private void TestLists()
        {
            // Test list
            tester.TestAssertion("List Count", list.Count == 5);
            tester.TestAssertion("List Indexer", list[0] == 1);
            tester.TestAssertion("List Indexer2", list[1] == 2);
            tester.TestAssertion("List Indexer3", list[2] == 3);
            tester.TestAssertion("List Indexer4", list[3] == 4);
            tester.TestAssertion("List Indexer5", list[4] == 5);
            
            // iterator
            int sum = 0;
            foreach (int val in list)
            {
                sum += val;
            }
            
            tester.TestAssertion("List Iterator", sum == 15);
            
            list.Add(6);
            tester.TestAssertion("List Add", list[5] == 6);
            
            list.RemoveAt(0);
            tester.TestAssertion("List RemoveAt", list[0] == 2);
            
            list.Clear();
            
            tester.TestAssertion("List Clear", list.Count == 0);
            
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);
            
            list.Insert(2, 10);
            tester.TestAssertion("List Insert", list[2] == 10);
            tester.TestAssertion("List Insert2", list[3] == 3);
        }
        
        private void TestDictionary()
        {
            dict.Add(1, "Hello");
            dict.Add(2, "World");
            
            tester.TestAssertion("Dictionary Count", dict.Count == 2);
            tester.TestAssertion("Dictionary Indexer", dict[1] == "Hello");
            tester.TestAssertion("Dictionary Indexer2", dict[2] == "World");
            
            dict[1] = "Goodbye";
            tester.TestAssertion("Dictionary Indexer3", dict[1] == "Goodbye");
            
            dict.Remove(1);
            tester.TestAssertion("Dictionary Remove", dict.Count == 1);
            
            dict.Clear();
            
            tester.TestAssertion("Dictionary Clear", dict.Count == 0);
            
            dict.Add(1, "Hello");
            dict.Add(2, "World");
            
            tester.TestAssertion("Dictionary ContainsKey", dict.ContainsKey(1));
            tester.TestAssertion("Dictionary ContainsKey2", dict.ContainsKey(3) == false);
            
            tester.TestAssertion("Dictionary ContainsValue", dict.ContainsValue("Hello"));
            tester.TestAssertion("Dictionary ContainsValue2", dict.ContainsValue("Goodbye") == false);
            
            Dictionary<string, float> floatDict = new Dictionary<string, float>();
            
            floatDict.Add("Hello", 1.0f);
            floatDict.Add("World", 2.0f);
            
            tester.TestAssertion("Dictionary Generic Count", floatDict.Count == 2);
            tester.TestAssertion("Dictionary Generic Indexer", floatDict["Hello"] == 1.0f);
            tester.TestAssertion("Dictionary Generic Indexer2", floatDict["World"] == 2.0f);
            
            floatDict["Hello"] = 3.0f;
            tester.TestAssertion("Dictionary Generic Indexer3", floatDict["Hello"] == 3.0f);
            
            floatDict.Remove("Hello");
            tester.TestAssertion("Dictionary Generic Remove", floatDict.Count == 1);
            tester.TestAssertion("Dictionary Generic Remove2", floatDict.ContainsKey("Hello") == false);
            tester.TestAssertion("Dictionary Generic Remove3", floatDict["World"] == 2.0f);
            
            Dictionary<string, int> intDict = new Dictionary<string, int>();
            
            int sum = 0;
            
            for (int i = 0; i < 100; ++i)
            {
                intDict.Add(i.ToString(), i);
                sum += i;
            }
            
            int sum2 = 0;
            for (int i = 0; i < 100; ++i)
            {
                sum2 += intDict[i.ToString()];
            }
            
            tester.TestAssertion("Dictionary Generic Sum", sum == sum2);
            
            int sum3 = 0;
            foreach (KeyValuePair<string, int> pair in intDict)
            {
                sum3 += pair.Value;
            }
            
            tester.TestAssertion("Dictionary Generic Iterator", sum == sum3);
            
            tester.TestAssertion("Dictionary Generic TryGetValue", intDict.TryGetValue("50", out int val) && val == 50);

            List<KeyValuePair<string, int>> pairList = new List<KeyValuePair<string, int>>();
            foreach (KeyValuePair<string, int> pair in intDict)
            {
                pairList.Add(pair);
            }

            tester.TestAssertion("Dictionary KeyValuePair List Count", pairList.Count == 100);

            int pairListSum = 0;
            bool pairListDistinct = true;
            for (int i = 0; i < pairList.Count; ++i)
            {
                pairListSum += pairList[i].Value;
                if (pairList[i].Key != pairList[i].Value.ToString())
                    pairListDistinct = false;
            }

            tester.TestAssertion("Dictionary KeyValuePair List Sum", pairListSum == sum);
            tester.TestAssertion("Dictionary KeyValuePair List Distinct", pairListDistinct);
            tester.TestAssertion("Dictionary KeyValuePair List First", pairList[0].Key == pairList[0].Value.ToString());
            tester.TestAssertion("Dictionary KeyValuePair List Last", pairList[99].Key == pairList[99].Value.ToString());

            Dictionary<DictionaryKeyClass, string> customKeyDict = new Dictionary<DictionaryKeyClass, string>();
            DictionaryKeyClass keyA = new DictionaryKeyClass(1);
            DictionaryKeyClass keyB = new DictionaryKeyClass(1);
            DictionaryKeyClass keyC = new DictionaryKeyClass(2);

            customKeyDict.Add(keyA, "Hello");

            tester.TestAssertion("Dictionary Custom Key Same Instance", customKeyDict.ContainsKey(keyA));
            tester.TestAssertion("Dictionary Custom Key Equal Instance", customKeyDict.ContainsKey(keyB));
            tester.TestAssertion("Dictionary Custom Key Indexer", customKeyDict[keyB] == "Hello");
            tester.TestAssertion("Dictionary Custom Key TryGetValue", customKeyDict.TryGetValue(keyB, out string customVal) && customVal == "Hello");
            tester.TestAssertion("Dictionary Custom Key Missing", customKeyDict.ContainsKey(keyC) == false);

            customKeyDict[keyB] = "World";
            tester.TestAssertion("Dictionary Custom Key Update", customKeyDict[keyA] == "World");
            tester.TestAssertion("Dictionary Custom Key Count", customKeyDict.Count == 1);

            customKeyDict.Add(keyC, "Other");
            tester.TestAssertion("Dictionary Custom Key Add Distinct", customKeyDict.Count == 2);
            tester.TestAssertion("Dictionary Custom Key Remove", customKeyDict.Remove(new DictionaryKeyClass(1)));
            tester.TestAssertion("Dictionary Custom Key Remove Count", customKeyDict.Count == 1);
            tester.TestAssertion("Dictionary Custom Key Remaining", customKeyDict[keyC] == "Other");

            object boxedKey = keyC;
            tester.TestAssertion("User Class As Cast", (boxedKey as DictionaryKeyClass) != null);
            tester.TestAssertion("User Class As Cast Id", (boxedKey as DictionaryKeyClass).Id == 2);
            tester.TestAssertion("User Class Is Check", boxedKey is DictionaryKeyClass);
            tester.TestAssertion("User Class As Wrong Type", (boxedKey as MyNonBehaviourClass) == null);

            Dictionary<DictionaryKeyClass, int> customPairSource = new Dictionary<DictionaryKeyClass, int>();
            customPairSource.Add(new DictionaryKeyClass(10), 1);
            customPairSource.Add(new DictionaryKeyClass(20), 2);
            customPairSource.Add(new DictionaryKeyClass(30), 3);

            List<KeyValuePair<DictionaryKeyClass, int>> customPairList = new List<KeyValuePair<DictionaryKeyClass, int>>();
            foreach (KeyValuePair<DictionaryKeyClass, int> pair in customPairSource)
            {
                customPairList.Add(pair);
            }

            tester.TestAssertion("Custom KeyValuePair List Count", customPairList.Count == 3);

            int customPairIdSum = 0;
            int customPairValueSum = 0;
            for (int i = 0; i < customPairList.Count; ++i)
            {
                customPairIdSum += customPairList[i].Key.Id;
                customPairValueSum += customPairList[i].Value;
            }

            tester.TestAssertion("Custom KeyValuePair List Key Sum", customPairIdSum == 60);
            tester.TestAssertion("Custom KeyValuePair List Value Sum", customPairValueSum == 6);
        }

        private void ExternalReferencedClass()
        {
            NonBehaviourClassesReferencedClass testClass = new NonBehaviourClassesReferencedClass();
            
            tester.TestAssertion("Imported Class Field", testClass.Field == 5);
            testClass.Field = 10;
            tester.TestAssertion("Imported Class Field2", testClass.Field == 10);
            
            tester.TestAssertion("Imported Class Auto Property", testClass.AutoProp == "Hello");
            testClass.AutoProp = "World";
            tester.TestAssertion("Imported Class Auto Property2", testClass.AutoProp == "World");
            
            tester.TestAssertion("Imported Class Property", testClass.Property == 10);
            testClass.Property = 20;
            tester.TestAssertion("Imported Class Property2", testClass.Property == 20);
            
            tester.TestAssertion("Imported Class Method", testClass.TestMethod(1, 1));
            tester.TestAssertion("Imported Class Method2", !testClass.TestMethod(2, 1));
            
            tester.TestAssertion("Imported Class Generic Method", testClass.TestMethod2(1, 1) == 1);
            tester.TestAssertion("Imported Class Generic Method2", testClass.TestMethod2(2, 1) == 2);
            tester.TestAssertion("Imported Class Generic Method3", testClass.TestMethod2("Hello", "World") == "Hello");
            
            tester.TestAssertion("Imported Class Generic Field", testClass.OtherClassInstance.Field == 0);
            testClass.OtherClassInstance.Field = 5;
            tester.TestAssertion("Imported Class Generic Field2", testClass.OtherClassInstance.Field == 5);
            
            testClass.OtherClassInstance = new OtherClass<float>(1.0f);
            tester.TestAssertion("Imported Class Generic Field3", testClass.OtherClassInstance.Field == 1.0f);
            
            testClass.SetOtherClassData(10.0f);
            tester.TestAssertion("Imported Class Generic Field4", testClass.GetOtherClassData() == 10.0f);
        }

        private void ReferencedBehaviourWithNonBehaviourClass()
        {
            NonBehaviourClassesPassBehaviour testBehaviour = GetComponentInChildren<NonBehaviourClassesPassBehaviour>();

            tester.TestAssertion("Imported Class Field", testBehaviour.referencedClass.Field == 5);
            testBehaviour.referencedClass.Field = 10;
            tester.TestAssertion("Imported Class Field2", testBehaviour.referencedClass.Field == 10);

            tester.TestAssertion("Imported Class Auto Property", testBehaviour.referencedClass.AutoProp == "Hello");
            testBehaviour.referencedClass.AutoProp = "World";
            tester.TestAssertion("Imported Class Auto Property2", testBehaviour.referencedClass.AutoProp == "World");

            tester.TestAssertion("Imported Class Property", testBehaviour.referencedClass.Property == 10);
        }

        private void ArrayOfUserType()
        {
            MyNonBehaviourClass[] arr = new MyNonBehaviourClass[1];
            
            arr[0] = new MyNonBehaviourClass();
            
            tester.TestAssertion("Array of User Type Field", arr[0].Field == 5);
            arr[0].Field = 10;
            tester.TestAssertion("Array of User Type Field2", arr[0].Field == 10);
            
            tester.TestAssertion("Array of User Type Auto Property", arr[0].AutoProp == "Hello");
            arr[0].AutoProp = "World";
            tester.TestAssertion("Array of User Type Auto Property2", arr[0].AutoProp == "World");
            
            tester.TestAssertion("Array of User Type Property", arr[0].Property == 10);
            arr[0].Property = 20;
            tester.TestAssertion("Array of User Type Property2", arr[0].Property == 20);
            
            tester.TestAssertion("Array of User Type Method", arr[0].TestMethod(1, 1));
            tester.TestAssertion("Array of User Type Method2", !arr[0].TestMethod(2, 1));
        }
    }
}