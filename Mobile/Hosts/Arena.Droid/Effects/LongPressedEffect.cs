using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Arena.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("Effects")]
[assembly: ExportEffect(typeof(LongPressedEffect), "LongPressedEffect")]
namespace Arena.Droid.Effects
{
    public class LongPressedEffect : PlatformEffect
    {
        private bool _Attached;

        protected override void OnAttached()
        {
            if (!_Attached)
            {
                if (Control != null)
                {
                    Control.LongClickable = true;
                    Control.LongClick += Control_LongClick;
                }
                else
                {
                    Container.LongClickable = true;
                    Container.LongClick += Control_LongClick;
                }
                _Attached = true;
            }
        }

        private void Control_LongClick(object sender, Android.Views.View.LongClickEventArgs e)
        {
            var command = UI.Effects.LongPressedEffect.GetCommand(Element);
            command?.Execute(UI.Effects.LongPressedEffect.GetCommandParameter(Element));
        }

        protected override void OnDetached()
        {
            if (_Attached)
            {
                if (Control != null)
                {
                    Control.LongClickable = true;
                    Control.LongClick -= Control_LongClick;
                }
                else
                {
                    Container.LongClickable = true;
                    Container.LongClick -= Control_LongClick;
                }
                _Attached = false;
            }
        }
    }
}