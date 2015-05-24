using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public interface IFibered { }

    public class Fibered : IEntityComponent
    {
        public class Fiber
        {
            public IEnumerator<float> enumerator;
            public float delayLeft;
            public bool isDead;           
        }

        List<Fiber> fibers;        

        public Fibered()
        {            
            this.fibers = new List<Fiber>();
        }

        public void OnDie()
        {
            foreach (var fib in fibers)
            {
                fib.isDead = true;
                fib.enumerator.Dispose();
            }
            
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
                    if (fib.isDead || !fib.enumerator.MoveNext())
                    {
                        fib.isDead = true;
                        fibers.RemoveAt(i--);
                    }
                    else
                        fib.delayLeft = fib.enumerator.Current;
                }
            }

            return true;
        }

        public Fiber StartFiber(IEnumerator<float> enumerator)
        {
            enumerator.MoveNext();
            float delay = enumerator.Current;
            var fiber = new Fiber() { delayLeft = delay, enumerator = enumerator };
            fibers.Add(fiber);
            return fiber;
        }

        public void StopFiber(Fiber fiber)
        {
            fiber.isDead = true;
            fiber.enumerator.Dispose();
        }
    }
}
