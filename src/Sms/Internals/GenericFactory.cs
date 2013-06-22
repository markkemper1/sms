using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sms.Internals
{
    internal static class GenericFactory
    {
        static readonly Type[] constructorArgs = new Type[0];

        public static IEnumerable<T> FindAndBuild<T>()
        {
            foreach (var type in Find<T>())
            {
                yield return Build<T>(type);
            }
        }

        private static IEnumerable<Type> Find<T>()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            
            Type factoryType = typeof(T);

            foreach (var file in dir.EnumerateFiles("*.dll"))
            {
                var assembly = Assembly.LoadFile(file.FullName);
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Exception exSub in ex.LoaderExceptions)
                    {
                        sb.AppendLine(exSub.Message);
                        if (exSub is FileNotFoundException)
                        {
                            FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                            if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                            {
                                sb.AppendLine("Fusion Log:");
                                sb.AppendLine(exFileNotFound.FusionLog);
                            }
                        }
                        sb.AppendLine();
                    }
                    throw new ApplicationException(sb.ToString(), ex);
                    //Display or log the error based on your application.
                }
                foreach (var t in types)
                {
                    if (factoryType.IsAssignableFrom(t) && t.GetConstructor(constructorArgs) != null)
                        yield return t;
                }
            }
        }

        public static T Build<T>(Type type) 
        {
            if (type == null) throw new ArgumentNullException("type");

            var constructor = type.GetConstructor(constructorArgs);

            return (T)constructor.Invoke(constructorArgs);
        }
    }
}
