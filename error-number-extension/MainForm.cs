using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace error_number_extension
{
    public partial class MainForm : Form, IValidate
    {
        #region I M P L E M E N T    I N T E R F A C E
        public string Username => textBoxUsername.Text;
        public string Email => textBoxEmail.Text;
        public string Password => textBoxPassword.Text;
        public string Confirm => textBoxConfirm.Text;
        public Control ErrorLabel => labelError;
        #endregion I M P L E M E N T    I N T E R F A C E

        public MainForm()
        {
            InitializeComponent();
            foreach (Control control in Controls)
            {
                if(control is TextBox textBox)
                {
                    textBox.KeyDown += onAnyTextboxKeyDown;
                    textBox.Validating += onAnyTextBoxValidating;
                }
            }
        }

        private void onAnyTextboxKeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                if (e.KeyData.Equals(Keys.Return))
                {
                    e.SuppressKeyPress = e.Handled = true;
                    textbox.BeginInvoke(() => textbox.SelectAll());

                    MethodInfo? validate = typeof(TextBox).GetMethod("OnValidating", BindingFlags.Instance | BindingFlags.NonPublic);
                    CancelEventArgs eCancel = new CancelEventArgs();
                    validate?.Invoke(textbox, new[] { eCancel });
                }
            }
        }

        private void onAnyTextBoxValidating(object? sender, CancelEventArgs e)
        {
            buttonLogin.Enabled = this.ValidateForm();
            if (sender is TextBox textbox)
            {
                this.SelectNextControl(
                    (Control)sender,
                    forward: true,
                    tabStopOnly: true,
                    nested: false,
                    wrap: true);
            }
        }
    }
    static class Extensions
    {
        public static bool ValidateForm(this IValidate @this)
        {
            bool isError = true;
            if (@this.Username.Length < 3)
            {
                @this.ErrorLabel.Text = "Username must be at least 3 characters.";
            }
            else if (!@this.Email.Contains("@"))
            {
                @this.ErrorLabel.Text = "Valid email is required.";
            }
            else if (@this.Password.Length < 7)
            {
                @this.ErrorLabel.Text = "Password must be at least 7 characters.";
            }
            else if (!@this.Password.Equals(@this.Confirm))
            {
                @this.ErrorLabel.Text = "Passwords must match.";
            }
            else isError = false;
            @this.ErrorLabel.Visible = isError;
            return isError;
        }
    }

    interface IValidate
    {
        public string Username { get; }
        public string Email { get; }
        public string Password { get; }
        public string Confirm { get; }
        public Control ErrorLabel { get; }
    }
}