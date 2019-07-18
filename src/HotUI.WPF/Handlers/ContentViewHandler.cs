﻿using System.Windows;

namespace HotUI.WPF.Handlers
{
	public class ContentViewHandler : AbstractHandler<ContentView, UIElement>
    {
        protected override UIElement CreateView()
        {
            return VirtualView?.Content.ToView();
        }
    }
}
