using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class LList<T> : IEnumerable<T> where T : IEquatable<T>
    {
        Node head;
        int count;

        public int Count { get { return count; } }

        class Node
        {
            public Node next;
            public T payload;
            public bool isDead;

            public override string ToString()
            {
                return payload.ToString();
            }
        }

        class Enumerator : IEnumerator<T>
        {
            LList<T> list;
            Node node;

            bool isReset;
            public T Current { get { return node.payload; } }
            object IEnumerator.Current { get { return node.payload; } }

            public Enumerator(LList<T> list)
            {
                this.list = list;
                this.node = list.head;
                this.isReset = true;
            }

            public bool MoveNext()
            {
                if (isReset)
                {
                    isReset = false;                   
                }
                else
                {
                    do
                    {
                        node = node.next;
                    } while (node != null && node.isDead);                    
                }
                return node != null;
            }

            public void Reset()
            {
                node = list.head;
                isReset = true;
            }

            public void Dispose()
            {

            }
        }

        public void Add(T el)
        {
            var n = new Node() { payload = el };

            if (head == null)
            {
                head = n;
            }
            else
            {
                n.next = head;
                head = n;
            }

            ++count;
        }

        public bool Remove(T el)
        {
            if (head.payload.Equals(el))
            {
                head.isDead = true;
                head = head.next;
            }
            else
            {
                Node curr = head, prev = null;
                while (curr != null && !curr.payload.Equals(el))
                {
                    prev = curr;
                    curr = curr.next;
                }
                if (curr == null) return false;
                prev.next = curr.next;
                curr.isDead = true;
            }
            --count;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}