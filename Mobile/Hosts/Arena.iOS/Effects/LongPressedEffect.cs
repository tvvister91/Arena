using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Arena.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("Effects")]
[assembly: ExportEffect(typeof(LongPressedEffect), "LongPressedEffect")]
namespace Arena.iOS.Effects
{
    class LongPressedEffect: PlatformEffect
    {
        private bool _attached;
        private readonly UILongPressGestureRecognizer _longPressRecognizer;

        public LongPressedEffect()
        {
            _longPressRecognizer = new UILongPressGestureRecognizer(HandleLongClick);
        }

        protected override void OnAttached()
        {
            if (!_attached)
            {
                Container.AddGestureRecognizer(_longPressRecognizer);
                _attached = true;
            }
        }

        private void HandleLongClick()
        {
            var command = UI.Effects.LongPressedEffect.GetCommand(Element);
            command?.Execute(UI.Effects.LongPressedEffect.GetCommandParameter(Element));
        }
        protected override void OnDetached()
        {
            if (_attached)
            {
                Container.RemoveGestureRecognizer(_longPressRecognizer);
                _attached = false;
            }
        }
    }
}