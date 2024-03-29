﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using University_advisor.Models;
using University_advisor.Services;
using University_advisor.Tools;
using University_advisor.Constants;

namespace University_advisor.Forms
{
    public partial class SignupForm : Form
    {
        ArrayList statusList = new ArrayList() { "Student", "Graduate", "Lecturer" };
        public SignupForm()
        {
            InitializeComponent();
            CenterToScreen();
            SetValues();
        }
        private void SetValues()
        {
            UserEditingService service = new UserEditingService();
            universityBox.DataSource = service.GetAllUniversities();
            statusBox.DataSource = statusList;
        }

        private void CreateAccountButton_Click(object sender, EventArgs e)
        {
            if (ValidateFields())
            {
                var newUser = new UserModel()
                {
                    Username = usernameText.Text,
                    FirstName = firstNameText.Text,
                    LastName = lastNameText.Text,
                    Email = emailText.Text,
                    University = universityBox.SelectedItem.ToString(),
                    Status = statusBox.SelectedItem.ToString(),
                    Password = PasswordHasher.CreateMD5(passwordText.Text),
                };
                SendUserToDb(newUser);

                Hide();
                var loginForm = new LoginForm();
                loginForm.Closed += (s, args) => this.Close();
                loginForm.ShowDialog();
            }

        }

        private void SendUserToDb(UserModel newUser)
        {
            string txtSqlQuery = "INSERT INTO users (username, first_name, last_name, email, universityId, status, password) VALUES ";
            txtSqlQuery += $"('{newUser.Username}', '{newUser.FirstName}', '{newUser.LastName}', '{newUser.Email}', (SELECT universityId FROM universities WHERE name = '{newUser.University}'), '{newUser.Status}', '{newUser.Password}');";
            try
            {
                if (SqlDriver.Execute(txtSqlQuery))
                {
                    MessageBox.Show(Messages.userCreateSuccess);
                    Logger.Log(Messages.userCreateSuccess);
                    Hide();
                }
                else
                {
                    MessageBox.Show(Messages.userCreateFailed);
                    Logger.Log(Messages.userCreateFailed);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }

        private bool ValidateFields()
        {
            if (String.IsNullOrEmpty(usernameText.Text) || String.IsNullOrEmpty(firstNameText.Text) ||
                String.IsNullOrEmpty(lastNameText.Text) || String.IsNullOrEmpty(emailText.Text) ||
                String.IsNullOrEmpty(passwordText.Text) || String.IsNullOrEmpty(confirmPasswordText.Text))
            {
                MessageBox.Show(Messages.emptyFields);
                return false;
            }

            var regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            if (!regex.Match(emailText.Text).Success)
            {
                MessageBox.Show(Messages.badEmailFormat);
                return false;
            }

            if (!passwordText.Text.Equals(confirmPasswordText.Text))
            {
                MessageBox.Show(Messages.passwordsDoNotMatch);
                return false;
            }

            if (passwordText.Text.Length < 6)
            {
                MessageBox.Show(Messages.passwordTooShort);
                return false;
            }
            return true;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            Hide();
            var loginForm = new LoginForm();
            loginForm.Closed += (s, args) => this.Close();
            loginForm.ShowDialog();
        }
    }
}
