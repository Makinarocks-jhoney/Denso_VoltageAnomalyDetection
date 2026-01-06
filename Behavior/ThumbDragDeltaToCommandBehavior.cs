using Microsoft.Xaml.Behaviors;
using MVP_Voltage.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MVP_Voltage.Behavior
{
    public sealed class ThumbDragDeltaToCommandBehavior:Behavior<Thumb>
    {
        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ThumbDragDeltaToCommandBehavior));

        public static readonly DependencyProperty HandleProperty =
            DependencyProperty.Register(nameof(Handle), typeof(RoiHandle), typeof(ThumbDragDeltaToCommandBehavior),
                new PropertyMetadata(RoiHandle.Move));

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public RoiHandle Handle
        {
            get => (RoiHandle)GetValue(HandleProperty);
            set => SetValue(HandleProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DragDelta += OnDragDelta;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DragDelta -= OnDragDelta;
            base.OnDetaching();
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var cmd = Command;
            if (cmd is null) return;

            var args = new RoiDragArgs(Handle, e.HorizontalChange, e.VerticalChange);
            if (cmd.CanExecute(args))
                cmd.Execute(args);
        }
    }
}
