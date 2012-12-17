using System;
using System.IO;
using System.Reflection;
using System.Collections;

namespace uSearch.Businesslogic
{
	/// <summary>
	/// Summary description for FileFilterFactory.
	/// </summary>
	public class FileFilterFactory
	{
		private static bool _isInitialized = false;
		private static System.Collections.Hashtable _filters = new System.Collections.Hashtable();

		public FileFilterFactory()
		{
		}
	
		public static IUmbracoSearchFileFilter GetFilter(string extension) 
		{
			if (!_isInitialized)
				init();

			IDictionaryEnumerator ide = _filters.GetEnumerator();
			while (ide.MoveNext()) 
			{
				if (ide.Key.ToString().IndexOf(extension) > -1) 
					return Activator.CreateInstance(ide.Value.GetType()) as IUmbracoSearchFileFilter;
			}

			return null;

		}

		private static void init() 
		{

			string _pluginFolder = umbraco.GlobalSettings.Path + "/../bin";

			// Loop through plugin-folder and try to create types in assemblies as an IDataField
			// if the type is IDataField, then add it to factory
			foreach (string assembly in System.IO.Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath(_pluginFolder), "*.dll")) 
			{
				try 
				{
					System.Web.HttpContext.Current.Trace.Write("umbracoUtilities.Search.FileFilterFactory", "Reading assembly " + assembly);
					Assembly asm = System.Reflection.Assembly.LoadFrom(assembly);
					try 
					{
						foreach (Type t in asm.GetTypes()) 
						{
							Type hasInterface = t.GetInterface("umbracoUtilities.Search.IUmbracoSearchFileFilter", true);

							if (hasInterface != null && !t.IsInterface) 
							{
							
								IUmbracoSearchFileFilter typeInstance = Activator.CreateInstance(t) as IUmbracoSearchFileFilter;
								string extensions = ",";
								foreach(string ext in typeInstance.extensions)
									extensions += ext + ",";
								_filters.Add(extensions,typeInstance);
								System.Web.HttpContext.Current.Trace.Write("umbracoUtilities.Search.FileFilterFactory", " + Adding searchfilter for extensions '" + extensions + "'");
							} 
						}
					} 
					catch (Exception factoryE) 
					{
						System.Web.HttpContext.Current.Trace.Warn("umbracoUtilities.Search.FileFilterFactory", "error", factoryE);
					}
				} 
				catch 
				{
					System.Web.HttpContext.Current.Trace.Warn("umbracoUtilities.Search.FileFilterFactory", "Couldn't load " + assembly);
				}
			}
			_isInitialized = true;

		}
	}
}
