using Microsoft.Maui.Controls;

namespace ISOTankApp.Helpers;

public static class CursorHelper
{
    public static readonly BindableProperty IsPointerProperty =
        BindableProperty.CreateAttached("IsPointer", typeof(bool), typeof(CursorHelper), false, propertyChanged: OnIsPointerChanged);

    public static bool GetIsPointer(BindableObject view) => (bool)view.GetValue(IsPointerProperty);
    public static void SetIsPointer(BindableObject view, bool value) => view.SetValue(IsPointerProperty, value);

    private static void OnIsPointerChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VisualElement view && (bool)newValue)
        {
#if WINDOWS
            view.HandlerChanged += (s, e) =>
            {
                if (view.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement uiElement)
                {
                    var prop = typeof(Microsoft.UI.Xaml.UIElement).GetProperty("ProtectedCursor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    prop?.SetValue(uiElement, Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.Hand));
                }
            };
#elif MACCATALYST
            view.HandlerChanged += (s, e) =>
            {
                if (view.Handler?.PlatformView is UIKit.UIView uiView)
                {
                    uiView.AddGestureRecognizer(new AppKit.NSClickGestureRecognizer());
                }
            };
#endif
        }
    }
}
