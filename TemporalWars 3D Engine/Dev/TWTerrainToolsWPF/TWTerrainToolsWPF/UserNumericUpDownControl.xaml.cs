using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF
{
    /// <summary>
    /// Interaction logic for UserNumUpDownControl.xaml
    /// </summary>
    public partial class UserNumUpDownControl : UserControl
    {
        private const int DefaultDecimalPlaces = 0;
        private const bool DefaultHexadecimal = false;
        private const decimal DefaultIncrement = 1M;
        private const decimal DefaultMaximum = 100M;
        private const decimal DefaultMinimum = 0M;
        private const bool DefaultThousandsSeparator = false;
        private const int InvalidValue = -1;
        private static readonly decimal DefaultValue;
        private readonly EventHandler _onValueChanged;
        private bool _changingText;
        private decimal _currentValue;
        private bool _currentValueChanged;
        private int _decimalPlaces;
        private bool _hexadecimal;
        private decimal _increment = DefaultIncrement;
        private bool _initializing;
        private decimal _maximum = DefaultMaximum;
        private decimal _minimum = DefaultMinimum;
        private bool _thousandsSeparator;
        private bool _userEdit;

        #region Events

        [DisplayName("ValueChanged")]
        [Description("Occurs when the value in the up-down control changes.")]
        public event EventHandler ValueChanged;

        #endregion

        #region Properties

        // 6/29/2010
        [DisplayName("Increment")]
        [DefaultValue(1)]
        [Description("Indicates the amount to increment or decrement on each button click.")]
        public decimal Increment
        {
            get { return _increment; }
            set
            {
                if (value < 0M)
                {
                    throw new ArgumentOutOfRangeException("Increment", @"Value of increment cannot be negative!");
                }
                _increment = value;
            }
        }


        [DisplayName("Maximum")]
        [DefaultValue(10)]
        [Description("Indicates the maximum value for the numeric up-down control.")]
        public decimal Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                if (_minimum > _maximum)
                {
                    _minimum = _maximum;
                }
                Value = Constrain(_currentValue);
            }
        }


        [DisplayName("Minimum")]
        [DefaultValue(0)]
        [Description("Indicates the minimum value for the numeric up-down control.")]
        public decimal Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                if (_minimum > _maximum)
                {
                    _maximum = value;
                }
                Value = Constrain(_currentValue);
            }
        }

        [DisplayName("ThousandsSeparator")]
        [DefaultValue(false)]
        [Description("Indicates whether the thousands separator will be inserted between every three decimal digits.")]
        public bool ThousandsSeparator
        {
            get { return _thousandsSeparator; }
            set
            {
                _thousandsSeparator = value;
                UpdateEditText();
            }
        }


        [DisplayName("Value")]
        [DefaultValue(0)]
        [Description("The current value to use in the numeric up-down control.")]
        public decimal Value
        {
            get
            {
                if (_userEdit)
                {
                    ValidateEditText();
                }
                return _currentValue;
            }
            set
            {
                if (value != _currentValue)
                {
                    if (!_initializing && ((value < _minimum) || (value > _maximum)))
                    {
                        throw new ArgumentOutOfRangeException("Value",
                                                              @"Value given must be in allowable range of 'Minimum' and 'Maximum'.");
                    }
                    _currentValue = value;
                    OnValueChanged(EventArgs.Empty);
                    _currentValueChanged = true;
                    UpdateEditText();
                }
            }
        }


        [DisplayName("DecimalPlaces")]
        [DefaultValue(0)]
        [Description("Indicates the number of decimal places to display.")]
        public int DecimalPlaces
        {
            get { return _decimalPlaces; }
            set
            {
                if ((value < 0) || (value > 99))
                {
                    throw new ArgumentOutOfRangeException("DecimalPlaces", @"InvalidBoundArgument; must be in range of 0-99.");
                }
                _decimalPlaces = value;
                UpdateEditText();
            }
        }

        [DisplayName("Hexadecimal")]
        [DefaultValue(false)]
        [Description("Indicates whether the numeric up-down should display its value in hexadecimal.")]
        public bool Hexadecimal
        {
            get { return _hexadecimal; }
            set
            {
                _hexadecimal = value;
                UpdateEditText();
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public UserNumUpDownControl()
        {
            InitializeComponent();
            txtNum.Text = "0";
        }

        /// <summary>
        /// Occurs when value is updated.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (_userEdit)
            {
                UpdateEditText();
            }
        }


        public new void BeginInit()
        {
            _initializing = true;

            
        }

        private decimal Constrain(decimal value)
        {
            if (value < _minimum)
            {
                value = _minimum;
            }
            if (value > _maximum)
            {
                value = _maximum;
            }
            return value;
        }

        public new void EndInit()
        {
            _initializing = false;
            Value = Constrain(_currentValue);
            UpdateEditText();

           
        }

        protected void ParseEditText()
        {
            try
            {
                if (!string.IsNullOrEmpty(txtNum.Text) && ((txtNum.Text.Length != 1) || (txtNum.Text != "-")))
                {
                    if (Hexadecimal)
                    {
                        Value = Constrain(Convert.ToDecimal(Convert.ToInt32(txtNum.Text, 0x10)));
                    }
                    else
                    {
                        Value = Constrain(decimal.Parse(txtNum.Text, CultureInfo.CurrentCulture));
                    }
                }
            }
            catch
            {
            }
            finally
            {
                _userEdit = false;
            }
        }


        protected void UpdateEditText()
        {
            if (_initializing) return;

            if (_userEdit)
            {
                ParseEditText();
            }
            if (!_currentValueChanged &&
                (string.IsNullOrEmpty(txtNum.Text) || ((txtNum.Text.Length == 1) && (txtNum.Text == "-")))) return;

            _currentValueChanged = false;
            _changingText = true;
            txtNum.Text = GetNumberText(_currentValue);
        }

        protected void ValidateEditText()
        {
            ParseEditText();
            UpdateEditText();
        }
       

        private string GetNumberText(decimal num)
        {
            if (Hexadecimal)
            {
                var num2 = (long) num;
                return num2.ToString("X", CultureInfo.InvariantCulture);
            }
            return num.ToString((ThousandsSeparator ? "N" : "F") + DecimalPlaces.ToString(CultureInfo.CurrentCulture),
                                CultureInfo.CurrentCulture);
        }

       

        /// <summary>
        /// Increments value
        /// </summary>
        private void CmdUpClick(object sender, RoutedEventArgs e)
        {
           
            if (_userEdit)
            {
                ParseEditText();
            }
            decimal currentValue = _currentValue;
            try
            {
                currentValue += Increment;
                if (currentValue > _maximum)
                {
                    currentValue = _maximum;
                }
            }
            catch (OverflowException)
            {
                currentValue = _maximum;
            }
            Value = currentValue;

        }

        /// <summary>
        /// Decrements value
        /// </summary>
        private void CmdDownClick(object sender, RoutedEventArgs e)
        {
            if (_userEdit)
            {
                ParseEditText();
            }
            var currentValue = _currentValue;
            try
            {
                currentValue -= Increment;
                if (currentValue < _minimum)
                {
                    currentValue = _minimum;
                }
            }
            catch (OverflowException)
            {
                currentValue = _minimum;
            }
            Value = currentValue;
        }

        /// <summary>
        /// Event handler to capture the text changed event.
        /// </summary>
        private void TxtNumTextChanged(object sender, TextChangedEventArgs e)
        {
            decimal newValue;
            if (!decimal.TryParse(txtNum.Text, out newValue))
                return;

            // 6/29/2010 - Check if outside allowable range
            if (newValue < Minimum || newValue > Maximum)
                throw new InvalidOperationException("Value given is outside Minimum and Maximum allowable range.");

            // otherwise ok, so set new value.
            Value = newValue;
            txtNum.Text = _currentValue.ToString();
        }
        
    }
}