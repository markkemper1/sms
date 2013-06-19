using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.RoutingService
{
     class Program
    {
        static void Main(string[] args)
        {
            var router = new RouterService();
            router.Start();

            Console.WriteLine("Press the any key to exit");
            Console.ReadKey();

            router.Stop();

            Console.WriteLine("Done");
        }
    }
}
