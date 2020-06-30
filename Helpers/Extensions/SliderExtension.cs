using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Ducky.Helpers
{
    class SliderExtension
    {
        public static readonly DependencyProperty DragCompletedCommandProperty = DependencyProperty.RegisterAttached(
            "DragCompletedCommand",
            typeof(ICommand),
            typeof(SliderExtension),
            new PropertyMetadata(default(ICommand), OnDragCompletedCommandChanged));

        public static readonly DependencyProperty DragStartedCommandProperty = DependencyProperty.RegisterAttached(
            "DragStartedCommand",
            typeof(ICommand),
            typeof(SliderExtension),
            new PropertyMetadata(default(ICommand), OnDragStartedCommandChanged));


        private static void SliderOnLoaded(object sender, RoutedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider == null)
            {
                return;
            }
            slider.Loaded -= SliderOnLoaded;

            Track track = slider.Template.FindName("PART_Track", slider) as Track;
            if (track == null)
            {
                return;
            }
            track.Thumb.DragCompleted += (dragCompletedSender, dragCompletedArgs) =>
            {
                ICommand command = GetDragCompletedCommand(slider);
                command.Execute(null);
            };
            track.Thumb.DragStarted += (dragStartedSender, dragStartedArgs) =>
            {
                ICommand commandSt = GetDragStartedCommand(slider);
                commandSt?.Execute(null);
            };
        }
       
        public static ICommand GetDragCompletedCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(DragCompletedCommandProperty);
        }
        public static void SetDragCompletedCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DragCompletedCommandProperty, value);
        }
        private static void OnDragCompletedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slider slider = d as Slider;
            if (slider == null)
            {
                return;
            }

            if (e.NewValue is ICommand)
            {
                slider.Loaded += SliderOnLoaded;
            }
        }

        public static ICommand GetDragStartedCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(DragStartedCommandProperty);
        }
        public static void SetDragStartedCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(DragStartedCommandProperty, value);
        }
        private static void OnDragStartedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slider slider = d as Slider;
            if (slider == null)
            {
                return;
            }

            if (e.NewValue is ICommand)
            {
                slider.Loaded += SliderOnLoaded;
            }
        }

    }
}

