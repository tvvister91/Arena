using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Arena.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(TappedEffect), "TappedEffect")]
namespace Arena.iOS.Effects
{
    class TappedEffect: PlatformEffect
    {
        private bool _attached;
        private readonly UITapGestureRecognizer _tapRecognizer;

        public TappedEffect()
        {
            _tapRecognizer = new UITapGestureRecognizer(HandleTap);
        }

        protected override void OnAttached()
        {
            if (!_attached)
            {
                Container.AddGestureRecognizer(_tapRecognizer);
                _attached = true;
            }
        }

        private void HandleTap()
        {
            var command = UI.Effects.TappedEffect.GetCommand(Element);
            command?.Execute(UI.Effects.TappedEffect.GetCommandParameter(Element));
        }
        protected override void OnDetached()
        {
            if (_attached)
            {
                Container.RemoveGestureRecognizer(_tapRecognizer);
                _attached = false;
            }
        }
    }
}