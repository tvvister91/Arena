using System;
using UIKit;

namespace PE.Plugins.Dialogs.iOS
{
    class TimeSpanPickerViewController : UIViewController, IUIPickerViewDelegate
    {
        private TimeSpan _Timespan;
        private string _Message;
        private string _Title;
        private string _AffirmButton;
        private string _DenyButton;
        private Action<TimeSpan> _OnAffirm;
        private Action _OnDeny;

        public TimeSpanPickerViewController(TimeSpan timeSpan, string title, string message, string affirmButton,
            string denyButton, Action<TimeSpan> onAffirm, Action onDeny = null)
        {
            _Timespan = timeSpan;
            _Message = message;
            _Title = title;
            _AffirmButton = affirmButton;
            _DenyButton = denyButton;
            _OnAffirm = onAffirm;
            _OnDeny = onDeny;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            View.AddGestureRecognizer(new UITapGestureRecognizer(() => DismissViewController(true, null)));

            View.BackgroundColor = UIColor.Black.ColorWithAlpha(0.5f);

            UIView frame = new UIView
            {
                BackgroundColor = UIColor.White,
                Layer = {CornerRadius = 13},
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Pickers

            var hoursPicker = new UIPickerView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                DataSource = new NumberPickerViewDataSource(100),
                Delegate = new NumberPickerViewDelegate()
            };

            hoursPicker.Select((nint) Math.Floor(_Timespan.TotalHours), 0, false);
            
            var minutesPicker = new UIPickerView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                DataSource = new NumberPickerViewDataSource(60),
                Delegate = new NumberPickerViewDelegate()
            };

            minutesPicker.Select(_Timespan.Minutes, 0, false);

            UILabel titleLabel = null;

            if (!string.IsNullOrWhiteSpace(_Title))
            {
                titleLabel = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = _Title,
                    Font = UIFont.PreferredHeadline,
                    Lines = 2,
                    LineBreakMode = UILineBreakMode.WordWrap | UILineBreakMode.TailTruncation
                };
            }

            UILabel messageLabel = null;

            if (!string.IsNullOrWhiteSpace(_Message))
            {
                messageLabel = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = _Message,
                    Font = UIFont.PreferredSubheadline
                };
            }


            // Pickers labels

            var hoursLabel = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Hours",
                Font = UIFont.PreferredSubheadline,
                TextAlignment = UITextAlignment.Center
            };

            var minutesLabel = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Minutes",
                Font = UIFont.PreferredSubheadline,
                TextAlignment = UITextAlignment.Center
            };

            var affirmButton = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            affirmButton.SetTitle(_AffirmButton ?? "OK", UIControlState.Normal);

            affirmButton.TouchUpInside += (sender, args) =>
            {
                _OnAffirm?.Invoke(new TimeSpan((int)hoursPicker.SelectedRowInComponent(0), (int)minutesPicker.SelectedRowInComponent(0), 0));
                DismissViewController(true, null);
            };

            var denyButton = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            denyButton.SetTitle(_DenyButton ?? "Cancel", UIControlState.Normal);

            denyButton.TouchUpInside += (sender, args) =>
            {
                _OnDeny?.Invoke();
                DismissViewController(true, null);
            };

            // Separators

            var separatorColor = UIColor.FromRGB(235, 235, 240);

            var horizontalSeparator = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = separatorColor
            };

            var verticalSeparator = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = separatorColor
            };

            frame.AddSubview(hoursPicker);
            frame.AddSubview(minutesPicker);
            if (titleLabel != null) frame.AddSubview(titleLabel);
            if (messageLabel != null) frame.AddSubview(messageLabel);
            frame.AddSubview(hoursLabel);
            frame.AddSubview(minutesLabel);
            frame.AddSubview(affirmButton);
            frame.AddSubview(denyButton);
            frame.AddSubview(horizontalSeparator);
            frame.AddSubview(verticalSeparator);

            View.AddSubview(frame);

            if (titleLabel != null)
            {
                // Title constrains
                titleLabel.TopAnchor.ConstraintEqualTo(frame.TopAnchor, 20).Active = true;
                titleLabel.CenterXAnchor.ConstraintEqualTo(frame.CenterXAnchor).Active = true;
            }

            if (messageLabel != null)
            {
                if (titleLabel == null)
                {
                    messageLabel.TopAnchor.ConstraintEqualTo(frame.TopAnchor, 20).Active = true;
                }
                else
                {
                    messageLabel.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor, 10).Active = true;
                }
                
                messageLabel.CenterXAnchor.ConstraintEqualTo(frame.CenterXAnchor).Active = true;
            }

            // Hours, minutes labels constraints
            if (messageLabel != null)
            {
                hoursLabel.TopAnchor.ConstraintEqualTo(messageLabel.BottomAnchor, 20).Active = true;
            }
            else if (titleLabel != null)
            {
                hoursLabel.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor, 20).Active = true;
            }
            else
            {
                hoursLabel.TopAnchor.ConstraintEqualTo(frame.TopAnchor, 20).Active = true;
            }

            hoursLabel.LeftAnchor.ConstraintEqualTo(frame.LeftAnchor).Active = true;

            minutesLabel.TopAnchor.ConstraintEqualTo(hoursLabel.TopAnchor).Active = true;
            minutesLabel.RightAnchor.ConstraintEqualTo(frame.RightAnchor).Active = true;

            hoursLabel.RightAnchor.ConstraintEqualTo(minutesLabel.LeftAnchor).Active = true;
            minutesLabel.WidthAnchor.ConstraintEqualTo(hoursLabel.WidthAnchor).Active = true;

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                // pickers constrains
                hoursPicker.TopAnchor.ConstraintEqualTo(hoursLabel.BottomAnchor),
                hoursPicker.HeightAnchor.ConstraintEqualTo(100),
                hoursPicker.LeftAnchor.ConstraintEqualTo(frame.LeftAnchor),

                minutesPicker.HeightAnchor.ConstraintEqualTo(hoursPicker.HeightAnchor),
                minutesPicker.TopAnchor.ConstraintEqualTo(hoursPicker.TopAnchor),
                minutesPicker.RightAnchor.ConstraintEqualTo(frame.RightAnchor),

                hoursPicker.RightAnchor.ConstraintEqualTo(minutesPicker.LeftAnchor),
                hoursPicker.WidthAnchor.ConstraintEqualTo(minutesPicker.WidthAnchor),

                // Affirm, Deny button constrains and separators

                verticalSeparator.WidthAnchor.ConstraintEqualTo(1),
                horizontalSeparator.HeightAnchor.ConstraintEqualTo(1),

                horizontalSeparator.TopAnchor.ConstraintEqualTo(hoursPicker.BottomAnchor, 5),
                horizontalSeparator.LeftAnchor.ConstraintEqualTo(frame.LeftAnchor),
                horizontalSeparator.RightAnchor.ConstraintEqualTo(frame.RightAnchor),

                verticalSeparator.TopAnchor.ConstraintEqualTo(horizontalSeparator.BottomAnchor),
                verticalSeparator.CenterXAnchor.ConstraintEqualTo(frame.CenterXAnchor),
                verticalSeparator.BottomAnchor.ConstraintEqualTo(frame.BottomAnchor),

                affirmButton.TopAnchor.ConstraintEqualTo(horizontalSeparator.BottomAnchor),
                affirmButton.LeftAnchor.ConstraintEqualTo(verticalSeparator.RightAnchor),
                affirmButton.RightAnchor.ConstraintEqualTo(frame.RightAnchor),
                affirmButton.BottomAnchor.ConstraintEqualTo(frame.BottomAnchor),
                affirmButton.HeightAnchor.ConstraintEqualTo(40),

                denyButton.TopAnchor.ConstraintEqualTo(horizontalSeparator.BottomAnchor),
                denyButton.RightAnchor.ConstraintEqualTo(verticalSeparator.LeftAnchor),
                denyButton.LeftAnchor.ConstraintEqualTo(frame.LeftAnchor),
                denyButton.BottomAnchor.ConstraintEqualTo(frame.BottomAnchor),
                denyButton.HeightAnchor.ConstraintEqualTo(40),

                // Frame constrains

                frame.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                frame.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
                frame.WidthAnchor.ConstraintEqualTo(300)
            });
        }

    }
}