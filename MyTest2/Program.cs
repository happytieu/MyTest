using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTest2
{
    class Father
    {
        public int i;
        protected virtual void Eat()
        {
            Console.Write("A");       
        }

        public Father()
        {
            A = 6;
        
        
        }
        public void SU()
        {
            Console.Write("D");
        }

        public static int B = 1;
        public readonly int A = B*5;
        static void Main(string[] args)
        {
            var a = new Father();
            Console.Write(a.A);

            Console.Write(a.A);
            Console.ReadKey();

        }
    }

    class Son : Father
    {
        protected  override void Eat()
        {
            Console.Write("B");
        }

        public void Drink()
        {
            Console.Write("C");
        }
    }
}
