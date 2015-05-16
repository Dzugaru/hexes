using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface IFibered { }

    public class Fibered : IEntityComponent
    {
        class Fiber
        {
            public IEnumerator<float> enumerator;
            public float delayLeft;            
        }

        List<Fiber> fibers;        

        public Fibered()
        {            
            this.fibers = new List<Fiber>();
        }

        public void OnDie()
        {
            foreach (var fib in fibers)
                fib.enumerator.Dispose();
            
            fibers.Clear();
        }

        public void OnSpawn(HexXY pos)
        {
            
        }

        public bool OnUpdate(float dt)
        {
            for(int i = 0; i < fibers.Count; i++)
            {
                Fiber fib = fibers[i];
                if (fib.delayLeft > 0)
                    fib.delayLeft -= dt;

                if (fib.delayLeft <= 0)
                {
                    if (!fib.enumerator.MoveNext())
                        fibers.RemoveAt(i--);
                    else
                        fib.delayLeft = fib.enumerator.Current;
                }
            }

            return true;
        }

        public void StartFiber(IEnumerator<float> enumerator)
        {
            enumerator.MoveNext();
            float delay = enumerator.Current;
            fibers.Add(new Fiber() { delayLeft = delay, enumerator = enumerator });
        }
    }
}
