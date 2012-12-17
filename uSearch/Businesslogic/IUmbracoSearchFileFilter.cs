using System;

namespace uSearch.Businesslogic
{
	/// <summary>
	/// Summary description for IUmbracoSearchFileFilter.
	/// </summary>
	public interface IUmbracoSearchFileFilter
	{
		string[] extensions {get;}
		string returnText(string FullPathToFile);
	}
}
