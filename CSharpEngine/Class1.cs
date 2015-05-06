using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine
{
    public static class Sandbox
    {
        class A
        {
            public Vector3 v;
            public void Construct(Vector3 v)
            {
                this.v = v;
            }
        }

        public static Vector3 Test()
        {
            var a = Freelist<A>.Allocate();
            a.Construct(new Vector3(1, 2, 3));
            Freelist<A>.Deallocate(a);
            return a.v;
        }
    }
}
