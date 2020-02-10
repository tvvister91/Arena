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

[assembly: ExportEffect(typeof(TappedEffect), "TappedEffect")]
namespace Arena.Droid.Effects
{
    public class TappedEffect: PlatformEffect
    {
        private bool _Attached;

        protected override void OnAttached()
        {
            if (!_Attached)
            {
                if (Control != null)
                {
                    Control.Click += Control_Click;
                }
                else
                {
                    Container.Click += Control_Click;
                }
                _Attached = true;
            }
        }

        private void Control_Click(object sender, EventArgs e)
        {
            var command = UI.Effects.TappedEffect.GetCommand(Element);
            command?.Execute(UI.Effects.TappedEffect.GetCommandParameter(Element));
        }

        protected override void OnDetached()
        {
            if (_Attached)
            {
                if (Control != null)
                {
                    Control.Click -= Control_Click;
                }
                else
                {
                    Container.Click -= Control_Click;
                }
                _Attached = false;
            }
        }
    }
}