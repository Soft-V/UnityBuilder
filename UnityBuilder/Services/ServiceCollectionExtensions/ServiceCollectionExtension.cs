using CraftHub.Services;
using Hypocrite.Container.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityBuilder.Services.ServiceCollectionExtensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddCommonServices(this ILightContainer collection)
        {
            collection.RegisterSingleton<ThemeService, ThemeService>();
        }
    }
}
