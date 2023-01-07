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
        #endregion I M P L E M E N T    I N T E R F A C E

        public MainForm()
        {
            InitializeComponent();
            foreach (Control control in Controls)
            {
                if(control is TextBox textBox)
                {
                    textBox.TabStop = false;
                    textBox.KeyDown += onAnyTextboxKeyDown;
                    textBox.Validating += onAnyTextBoxValidating;
                    textBox.TextChanged += (sender, e) =>
                    {
                        if (sender is TextBox textbox) textbox.Modified = true;
                    };
                }
            }
        }
        private void onAnyTextBoxValidating(object? sender, CancelEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Call the extension method to validate.
                ErrorInt @int = (ErrorInt)this.ValidateForm(e);
                if (@int.Equals(ErrorInt.None))
                {
                    labelError.Visible = false; 
                    return;
                }
                else if (textBox.Modified)
                {
                    BeginInvoke(() =>
                    {
                        switch (@int)
                        {
                            case ErrorInt.Username: textBoxUsername.Focus(); break;
                            case ErrorInt.Email: textBoxEmail.Focus(); break;
                            case ErrorInt.Password: textBoxPassword.Focus(); break;
                            case ErrorInt.Confirm: textBoxConfirm.Focus(); break;
                        }
                        labelError.Visible = true;
                        labelError.Text = typeof(ErrorInt)
                            .GetMember(@int.ToString())
                            .First()?
                            .GetCustomAttribute<DescriptionAttribute>()
                            .Description;
                        textBox.Modified = false;
                        textBox.SelectAll();
                    });
                }
            }
        }
        private void onAnyTextboxKeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                if (e.KeyData.Equals(Keys.Return))
                {
                    // Handle the Enter key.
                    e.SuppressKeyPress = e.Handled = true;

                    MethodInfo? validate = typeof(TextBox).GetMethod("OnValidating", BindingFlags.Instance | BindingFlags.NonPublic);
                    CancelEventArgs eCancel = new CancelEventArgs();
                    validate?.Invoke(textbox, new[] { eCancel });
                }
            }
        }
    }
    static class Extensions
    {
        public static int ValidateForm(this IValidate @this, CancelEventArgs e)
        {
            if (@this.Username.Length < 3) return 1;
            if (!@this.Email.Contains("@")) return 2;
            if (@this.Password.Length < 7) return 3;
            if (!@this.Password.Equals(@this.Confirm)) return 4;
            return 0;
        }
    }

    enum ErrorInt
    {
        None = 0,
        [Description("Username must be at least 3 characters.")]
        Username = 1,
        [Description("Valid email is required.")]
        Email = 2,
        [Description("Password must be at least 7 characters.")]
        Password = 3,
        [Description("Passwords must match.")]
        Confirm = 4,
    }
    interface IValidate
    {
        public string Username { get; }
        public string Email { get; }
        public string Password { get; }
        public string Confirm { get; }
    }
}