using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityBuilder.Services
{
    public static class Localizer
    {
        public static string Get(string key) => LanguageService.Instance.Get(key);
        public static string Get(string key, params object[] args) => LanguageService.Instance.Get(key, args);
    }
}
