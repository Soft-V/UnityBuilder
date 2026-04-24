using Hypocrite.Container.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityBuilder.ViewModels;
using UnityBuilder.Views;

namespace UnityBuilder.Services.ServiceCollectionExtensions
{
    public static class ViewsAndViewModelsCollectionExtension
    {
        public static void AddViews(this ILightContainer collection)
        {
            collection.RegisterSingleton<FirstPage, FirstPage>();
            collection.RegisterSingleton<SecondPage, SecondPage>();
            collection.RegisterSingleton<ThirdPage, ThirdPage>();
        }

        public static void AddViewModels(this ILightContainer collection)
        {
            collection.RegisterSingleton<MainViewModel, MainViewModel>();
            collection.RegisterSingleton<PagesViewModel, PagesViewModel>();
        }
    }
}
