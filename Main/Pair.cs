using System;
using System.Collections.Generic;
using System.Text;

namespace TESVSnip
{
    struct Pair<A, B>
    {
        public A a;
        public B b;

        public Pair(A a, B b) { this.a = a; this.b = b; }

        public A Key { get { return a; } set { a = value; } }
        public B Value { get { return b; } set { b = value; } }

        public override string ToString()
        {
            return a.ToString();
        }
    }
}
