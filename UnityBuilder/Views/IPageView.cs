using System;

namespace UnityBuilder.Views
{
    public interface IPageView
    {
        event EventHandler OnNextPage;
        event EventHandler OnPreviousPage;
    }
}
