using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParsedData
{
    class TestClass3A
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

            b = "hello dolly";
        }
    }

    class TestClass3B
    {
        string classBDataMember = "";

        void functionA()
        {
            TestClass1A a = new TestClass1A();
            TestClass2A b = new TestClass2A();

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

            TestClass2B couplingClass = new TestClass2B();

            classBDataMember = "hello texas";
        }
    }
}

