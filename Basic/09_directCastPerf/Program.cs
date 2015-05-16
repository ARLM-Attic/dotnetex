﻿namespace _09_directCastPerf
{
    using System;
    using System.Runtime.CLR;
    using System.Runtime.CompilerServices;
    using BenchmarkDotNet;

    class Program
    {
        static void Main(string[] args)
        {
            var competitionSwitch = new BenchmarkCompetitionSwitch(new[] {
                typeof(Competition)
            });
            competitionSwitch.Run(new[] { "Competition" });
            Console.ReadKey();
        }
    }

    internal class A { }
    internal class B : A { }
    internal class C : B { }
    internal class D : C { }
    internal class E : D { }

    public class Competition
    {
        const int Iter = 10000001;
        private object obj;

        [Benchmark("JIT provided casting to inherited class deep = 5")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void JitProvidedFar()
        {
            SetObject(new E());
            object obj = GetObject();
            for (int i = 0; i < Iter; i++)
            {
                SetObject((A) obj);
            }
        }

        [Benchmark("JetCast casting to inherited class deep = 5")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void CastRefProvidedFar()
        {
            SetObject(new E());
            object obj = GetObject();
            for (int i = 0; i < Iter; i++)
            { 
                SetObject(EntityPtr.CastRef<A>(obj));
            }
        }

        [Benchmark("JIT provided casting to inherited class deep = 2")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void JitProvidedNear()
        {
            SetObject(new B());
            object obj = GetObject();
            for (int i = 0; i < Iter; i++)
            {
                SetObject((A)obj);
            }
        }

        [Benchmark("JetCast casting to inherited class deep = 2")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void CastRefProvidedNear()
        {
            SetObject(new B());
            object obj = GetObject();
            for (int i = 0; i < Iter; i++)
            {
                SetObject(EntityPtr.CastRef<A>(obj));
            }
        }

        [Benchmark("JIT provided casting to inherited class deep = 1")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void JitProvidedEq()
        {
            SetObject(new A());
            object obj = GetObject();
            for (int i = 0; i < Iter; i++)
            {
                SetObject((A)obj);
            }
        }

        [Benchmark("JetCast casting to inherited class deep = 1")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void CastRefProvidedEq()
        {
            SetObject(new A());
            object obj = GetObject();
            for (int i = 0; i < Iter; i++)
            {
                SetObject(EntityPtr.CastRef<A>(obj));
            }
        }

        [Benchmark("No casting")]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void NoCast()
        {
            SetObject(new A());
            var obj = (A)GetObject();
            for (int i = 0; i < Iter; i++)
            {
                SetObject(obj);
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private object GetObject()
        {
            return obj;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void SetObject(A a)
        {
            obj = a;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void SetObject(B b)
        {
            obj = b;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void SetObject(E e)
        {
            obj = e;
        }
    }
}
