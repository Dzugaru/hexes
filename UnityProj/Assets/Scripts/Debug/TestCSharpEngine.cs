using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class TestCSharpEngine
{
    class Spell
    {
        public Fibered fibered;

        public Spell()
        {
            fibered = new Fibered();
        }

        class Something : IDisposable
        {
            public Something()
            {
                Debug.Log("Something constructed");
            }

            public void Dispose()
            {
                Debug.Log("Something disposed");
            }
        }

        public IEnumerator<float> Fiber()
        {
            Debug.Log("Fiber start");
            using (Something smth = new Something())
            {
                yield return 1;
                Debug.Log("Fiber end");

                yield break;
            }
        }

        public void Update(float dt)
        {
            fibered.OnUpdate(dt);
        }

        public void Die()
        {
            fibered.OnDie();
        }
    }
        
    public static void Test()
    {
        Spell s = Freelist<Spell>.Allocate();
        s.fibered.StartFiber(s.Fiber());

        for (int i = 0; i < 5; i++)
        {
            s.Update(0.1f);
            //Debug.WriteLine("Time: " + i * 0.1f);
        }

        s.Die();
    }
}

