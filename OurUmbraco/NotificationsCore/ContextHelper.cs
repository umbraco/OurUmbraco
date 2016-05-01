using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace OurUmbraco.NotificationsCore
{
    // Copied from: https://github.com/kipusoep/UrlTracker/blob/master/Helpers/ContextHelper.cs
    public static class ContextHelper
    {
        public static IDisposable EnsureHttpContext()
        {
            return new HttpContextEnsurer();
        }

        class HttpContextEnsurer : IDisposable
        {
            readonly bool _fake;

            const string TempUri = "http://tempuri.org";
            static bool _umbracoContextTypeChecked;
            static Type _umbracoContextType;
            static Type _applicationContextType;
            static PropertyInfo _umbracoContextCurrentProperty;
            static MethodInfo _ensureContextMethodInfo;

            public HttpContextEnsurer()
            {
                _fake = HttpContext.Current == null;
                if (_fake)
                    HttpContext.Current = new HttpContext(new HttpRequest(string.Empty, TempUri, string.Empty), new HttpResponse(new StringWriter()));

                // V6
                if (!_umbracoContextTypeChecked)
                {
                    _umbracoContextType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                           where assembly.GetName().Name == "umbraco"
                                           from type in assembly.GetTypes()
                                           where type.Name == "UmbracoContext" && type.Namespace == "Umbraco.Web"
                                           select type).FirstOrDefault();
                    _umbracoContextTypeChecked = true;
                }
                if (_umbracoContextType != null)
                {
                    if (_umbracoContextCurrentProperty == null)
                        _umbracoContextCurrentProperty = _umbracoContextType.GetProperty("Current");
                    if (_umbracoContextCurrentProperty.GetValue(null, null) == null)
                    {
                        if (_applicationContextType == null)
                            _applicationContextType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                                       where assembly.GetName().Name == "Umbraco.Core"
                                                       from type in assembly.GetTypes()
                                                       where type.Name == "ApplicationContext"
                                                       select type).FirstOrDefault();

                        if (_ensureContextMethodInfo == null)
                            _ensureContextMethodInfo = _umbracoContextType.GetMethod("EnsureContext", new Type[] {
                                typeof(HttpContextBase),
                                _applicationContextType,
                                typeof(bool)
                            });
                        if (_ensureContextMethodInfo != null)
                        {
                            _ensureContextMethodInfo.Invoke(null, new object[]
                            {
                                new HttpContextWrapper(HttpContext.Current),
                                _applicationContextType.GetProperty("Current").GetValue(null, null),
                                true
                            });
                        }
                    }
                }
            }

            public void Dispose()
            {
                if (_fake)
                    HttpContext.Current = null;
            }
        }
    }
}