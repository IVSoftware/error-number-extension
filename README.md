Making a static [Extension Method](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
is one way to **"create global class (so I have to write it only once...)"**. This answer will explore this option step-by-step.

[![screenshots][1]][1]

***
First ask "what info is needed to determine a valid form?" Define these requirements in an [interface](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface):

    interface IValidate
    {
        // Inputs needed
        public string Username { get; }
        public string Email { get; }
        public string Password { get; }
        public string Confirm { get; }
    }

***
Now declare an Extension Method for "any" form that implements `IValidate` (this is the **global class** you asked about).

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

***
**Example of Form Validation**

For each of the your 3 forms, implement the interface. This just means that the 3 form classes are making a promise or contract to provide the  information that the interface requires (in this case by retrieving the text from the textboxes). 

    public partial class MainForm : Form, IValidate
    {
        #region I M P L E M E N T    I N T E R F A C E
        public string Username => textBoxUsername.Text;
        public string Email => textBoxEmail.Text;
        public string Password => textBoxPassword.Text;
        public string Confirm => textBoxConfirm.Text;
        #endregion I M P L E M E N T    I N T E R F A C E
        .
        .
        .
    }

***
Then call the extension method when any textbox loses focus or receives an Enter key.

    public partial class MainForm : Form, IValidate
    {        
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
                    buttonLogin.Enabled = true;
                    return;
                }
                else if (textBox.Modified)
                {
                    buttonLogin.Enabled = false;
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
        .
        .
        .
    }   

Where:

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


  [1]: https://i.stack.imgur.com/6YRLR.png