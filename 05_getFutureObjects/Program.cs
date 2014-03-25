﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CLR;
using System.Threading;

namespace _05_getFutureObjects
{
    public class AppDomainRunner : MarshalByRefObject
    {
        private void methodInsideAppDomain()
        {
            var startObj = new object();

            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    // What for Console.ReadKey().
                    Thread.Sleep(1000);

                    // Lookup our List'1, printing while looking up
                    List<int> catched = null;
                    foreach (var obj in GCEx.GetObjectsInSOH(startObj))
                    {
                        Console.WriteLine(" - object: {0}, type: {1}, size: {2}", obj, obj.GetType().Name, GCEx.SizeOf(obj));
                        if (obj is List<int>) catched = (List<int>)obj;
                    }

                    // Congrats if found
                    if (catched != null)
                    {
                        Console.WriteLine("Catched list size: {0}", catched.Count);
                    }
                }
            );
        }

        public static void Go()
        {
            // make appdomain
            var dom = AppDomain.CreateDomain("PseudoIsolated", null, new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            });

            // create object instance
            var p = (AppDomainRunner)dom.CreateInstanceAndUnwrap(typeof(AppDomainRunner).Assembly.FullName, typeof(AppDomainRunner).FullName);

            // enumerate objects from outside area to our appdomain area
            p.methodInsideAppDomain();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadKey();
            AppDomainRunner.Go();

            var list = new List<int>(10);
            list.AddRange(Enumerable.Range(0, 20));
            
            Console.ReadKey();

            Console.WriteLine(list);
        }
    }
}
