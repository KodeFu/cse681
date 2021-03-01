using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParsedData
{
    class TestClass1A
    {
        int a; // data member
        string b = ""; // data member


        void functionA()
        {
            b = "hello mudit"; // data member reference
        }

        void functionB()
        {
            b = "hello dolly"; // data member reference
        }
    }

    class TestClass1B
    {
        string classBDataMember = ""; // data member

        void functionA()
        {
            classBDataMember = "another test"; // data member reference
        }

        void functionB()
        {
            int a;
            int c;
        }
    }
}
