using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParsedData
{
    class TestClass2A
    {
        int a;
        string b = "";
        TestClass1B localB;

        void functionA()
        {
            TestClass1B testClassB = new TestClass1B();

            b = "hello mudit";
        }

        void functionB()
        {
            TestClass1B testClassB = new TestClass1B();
            TestClass3B testClass3B = new TestClass3B();

            b = "hello dolly";
        }
    }

    class TestClass2B
    {
        string classBDataMember = "";

        void functionA()
        {
            TestClass1A a = new TestClass1A();

            classBDataMember = "another test";
        }

        void functionB()
        {
            int a;
            int c;
        }

        void functionC()
        {
            int a;
            int c;

            classBDataMember = "hello texas";
        }
    }
}
