using System.Collections.Generic;

namespace Engine
{
    public static class Freelist<T> where T : new()
    {
        static Stack<T> list = new Stack<T>();
        public static int Count { get { return list.Count; } }

        public static T Allocate()
        {
            if (list.Count > 0)
                return list.Pop();
            else
                return new T();
        }

        public static void Deallocate(T inst)
        {
            list.Push(inst);
        }
    }   
}
