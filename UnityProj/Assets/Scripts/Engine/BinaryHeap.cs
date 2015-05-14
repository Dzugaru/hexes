using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class BinaryHeap<T> where T : IComparable<T>
    {      
        int end;        
        T[] heap;

        public int Count { get { return end - 1; } }

        public BinaryHeap(int levelsNum)
        {   
            end = 1;
            int elementsCount = 1 << levelsNum;
            heap = new T[elementsCount];            
        }

        public void Reset()
        {
            end = 1;
        }

        public void Enqueue(T item)
        {                     
            //TODO: resize if needed
            heap[end] = item;
            BubbleUp(end);
            ++end;   
        }

        public T Dequeue()
        {
            return RemoveElementAt(1);
        }

        public T Peek()
        {
            return heap[1];
        }       
      
        T RemoveElementAt(int i)
        {
            --end;
            T res = heap[i];   
            heap[i] = heap[end];            
            Fix(i);
            return res;
        }

        void Fix(int i)
        {
            if (i == 1)
            {
                BubbleDown(1);
            }
            else
            {               
                if (i << 1 >= end || heap[i >> 1].CompareTo(heap[i]) > 0)
                {
                    BubbleUp(i);
                }
                else
                {
                    BubbleDown(i);
                }
            }
        }

        void BubbleUp(int i)
        {
            while (i != 1)
            {
                int pi = i >> 1;
                //Important: equal items should go up, cause good implementation expand newest node first 
                //(even if node age is not used for comparison)
                if (heap[pi].CompareTo(heap[i]) >= 0)
                {
                    T tmp = heap[pi];
                    heap[pi] = heap[i];
                    heap[i] = tmp;
                }

                i = pi;
            }
        }

        void BubbleDown(int i)
        {
            for(;;)
            {
                int i1 = i << 1;
                if (i1 >= end) break;
                int i2 = i1 + 1;
                int swapi;

                T c = heap[i]; 
                T c1 = heap[i1];
                T c2 = heap[i2];

                bool g1 = c.CompareTo(c1) > 0;
                bool g2 = i2 != end && c.CompareTo(c2) > 0;

                if(!g1 && !g2) break;
                if (g1 && !g2)
                {
                    swapi = i1;
                }
                else if (!g1 && g2)
                {
                    swapi = i2;
                }
                else if (c1.CompareTo(c2) < 0 || i2 == end)
                {
                    swapi = i1;
                }
                else
                {
                    swapi = i2;
                }

                heap[i] = heap[swapi];
                heap[swapi] = c;

                i = swapi;
            }
        }  
    }  

}
