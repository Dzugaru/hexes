using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engine;

namespace CSharpEngineTest
{
    [TestClass]
    public class UnitTest1
    {
        class A
        {          
            int[] data = new int[128];

            public void Construct(int seed)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = i + seed;
                }
            }
        }

        class B
        {
            public int x;
            public void Construct(int x)
            {
                this.x = x;
            }
        }

        [TestMethod]
        public void TestFreelist()
        {
            List<A> someAs = new List<A>();
            List<B> someBs = new List<B>();


            for (int i = 0; i < 5; i++)
            {
                A a = Freelist<A>.Allocate();
                B b = Freelist<B>.Allocate();
                a.Construct(5);
                b.Construct(42);
                someAs.Add(a);
                someBs.Add(b);
            }

            foreach (var a in someAs)
            {
                Freelist<A>.Deallocate(a);
            }

            foreach (var b in someBs)
            {
                Freelist<B>.Deallocate(b);
            }

            someAs.Clear();
            someBs.Clear();

            A a1 = Freelist<A>.Allocate();            

            Assert.IsTrue(Freelist<A>.Count == 4);
            Assert.IsTrue(Freelist<B>.Count == 5);

        }

        [TestMethod]
        public void TestBinaryHeap()
        {
            int levels = 10;
            BinaryHeap<int> heap = new BinaryHeap<int>(levels);
            Random rng = new Random(1);
            for (int k = 0; k < 10; k++)
            {
                heap.Reset();
                for (int i = 0; i < 1000; i++)
                {
                    if (heap.Count > 0 && rng.NextDouble() < 0.4)
                    {
                        heap.Dequeue();
                    }
                    else if (heap.Count < (1 << levels))
                    {
                        heap.Enqueue(rng.Next(100));
                    }
                }

                List<int> sortedByHeap = new List<int>();
                while (heap.Count > 0)
                {
                    sortedByHeap.Add(heap.Dequeue());
                }

                for (int i = 0; i < sortedByHeap.Count - 1; i++)
                {
                    Assert.IsTrue(sortedByHeap[i] <= sortedByHeap[i + 1]);
                }
            }
            
        }
    }
}
