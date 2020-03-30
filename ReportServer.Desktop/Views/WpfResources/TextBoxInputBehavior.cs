using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ReportServer.Desktop.Views.WpfResources
{
    public class TextBoxInputBehavior : Behavior<TextBox>
    {
        const NumberStyles ValidNumberStyles = NumberStyles.AllowDecimalPoint |
                                               //NumberStyles.AllowThousands |
                                               NumberStyles.AllowLeadingSign;

        public TextBoxInputMode InputMode { get; set; }

        public TextBoxInputBehavior()
        {
            InputMode = TextBoxInputMode.None;
            JustPositivDecimalInput = false;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += AssociatedObjectPreviewTextInput;
            AssociatedObject.PreviewKeyDown += AssociatedObjectPreviewKeyDown;

            DataObject.AddPastingHandler(AssociatedObject, Pasting);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= AssociatedObjectPreviewTextInput;
            AssociatedObject.PreviewKeyDown -= AssociatedObjectPreviewKeyDown;

            DataObject.RemovePastingHandler(AssociatedObject, Pasting);
        }

        public static readonly DependencyProperty JustPositivDecimalInputProperty =
            DependencyProperty.Register("JustPositivDecimalInput", typeof(bool),
                typeof(TextBoxInputBehavior), new FrameworkPropertyMetadata(false));

        public bool JustPositivDecimalInput
        {
            get => (bool) GetValue(JustPositivDecimalInputProperty);
            set => SetValue(JustPositivDecimalInputProperty, value);
        }

        private void Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var pastedText = (string) e.DataObject.GetData(typeof(string));

                if (IsValidInput(GetText(pastedText))) return;
                e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void AssociatedObjectPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space) return;
            e.Handled = true;
        }

        private void AssociatedObjectPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsValidInput(GetText(e.Text)))
            {
                e.Handled = true;
            }
        }

        private string GetText(string input)
        {
            var txt = AssociatedObject;

            int selectionStart = txt.SelectionStart;
            if (txt.Text.Length < selectionStart)
                selectionStart = txt.Text.Length;

            int selectionLength = txt.SelectionLength;
            if (txt.Text.Length < selectionStart + selectionLength)
                selectionLength = txt.Text.Length - selectionStart;

            var realtext = txt.Text.Remove(selectionStart, selectionLength);

            int caretIndex = txt.CaretIndex;
            if (realtext.Length < caretIndex)
                caretIndex = realtext.Length;

            var newtext = realtext.Insert(caretIndex, input);

            return newtext;
        }

        private bool IsValidInput(string input)
        {
            switch (InputMode)
            {
                case TextBoxInputMode.None:
                    return true;
                case TextBoxInputMode.DigitInput:
                    return CheckIsDigit(input) && input.Length < 7 && Convert.ToInt32(input) > 0;

                case TextBoxInputMode.NullableDigitInput:
                    return string.IsNullOrEmpty(input) ||
                           (CheckIsDigit(input) && input.Length < 7 && Convert.ToInt32(input) >= 0);

                case TextBoxInputMode.DecimalInput:
                    decimal d;

                    if (input.ToCharArray().Count(x => x == '.') > 1)
                        return false;

                    if (input.Contains("-"))
                    {
                        if (JustPositivDecimalInput)
                            return false;

                        if (input.IndexOf("-", StringComparison.Ordinal) > 0)
                            return false;

                        if (input.ToCharArray().Count(x => x == '-') > 1)
                            return false;

                        if (input.Length == 1)
                            return true;
                    }

                    var result = decimal.TryParse(input, ValidNumberStyles, new CultureInfo("en-US"), out d);
                    return result;

                default: throw new ArgumentException("Unknown TextBoxInputMode");

            }
        }

        private bool CheckIsDigit(string wert)
        {
            return wert.ToCharArray().All(Char.IsDigit);
        }
    }

    public enum TextBoxInputMode
    {
        None,
        DecimalInput,
        DigitInput,
        NullableDigitInput
    }
}
