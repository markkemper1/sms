//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;
//using System.Text;

//namespace Sms.Internals
//{
//	internal static class GenericFactory
//	{
//		static readonly Type[] constructorArgs = new Type[0];

//		public static IEnumerable<T> FindAndBuild<T>()
//		{
//			foreach (var type in Find<T>())
//			{
//				yield return Build<T>(type);
//			}
//		}

//		private static DirectoryInfo GetBinPath()
//		{
//			var path = AppDomain.CurrentDomain.BaseDirectory;

//			bool isWeb = String.Compare(Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile), "web.config", StringComparison.OrdinalIgnoreCase) == 0;

//			if (isWeb)  path = Path.Combine(path, "bin");
//			return new DirectoryInfo(path);
//		}

//		private static IEnumerable<Type> Find<T>()
//		{
//			var dir = GetBinPath();
            
//			Type factoryType = typeof(T);

//			Assembly entry = Assembly.GetEntryAssembly();

//			foreach (var file in dir.EnumerateFiles("*.dll"))
//			{
//EVIL
//				var assembly = Assembly.LoadFile(file.FullName);

//				bool isEntry =entry != null && assembly.FullName == entry.FullName;

//				Type[] types = new Type[0];
//				try
//				{
//					types = assembly.GetTypes();
//				}
//				catch (ReflectionTypeLoadException ex)
//				{
//					StringBuilder sb = new StringBuilder();
//					foreach (Exception exSub in ex.LoaderExceptions)
//					{
//						sb.AppendLine(exSub.Message);
//						if (exSub is FileNotFoundException)
//						{
//							FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
//							if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
//							{
//								sb.AppendLine("Fusion Log:");
//								sb.AppendLine(exFileNotFound.FusionLog);
//							}
//						}
//						sb.AppendLine();
//					}
//					if (isEntry)
//						throw new ApplicationException(sb.ToString(), ex);
//					else
//					{
//						System.Diagnostics.Trace.WriteLine(sb.ToString());
//						System.Diagnostics.Trace.WriteLine(ex.ToString());
//					}
//					//Display or log the error based on your application.
//				}
//				foreach (var t in types)
//				{
//					if (factoryType.IsAssignableFrom(t) && t.GetConstructor(constructorArgs) != null)
//						yield return t;
//				}
//			}
//		}

//		public static T Build<T>(Type type) 
//		{
//			if (type == null) throw new ArgumentNullException("type");

//			var constructor = type.GetConstructor(constructorArgs);

//			return (T)constructor.Invoke(constructorArgs);
//		}
//	}
//}
