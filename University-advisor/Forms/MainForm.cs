﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;
using University_advisor.Tools;
using System.Diagnostics;
using University_advisor.Services;
using University_advisor.Models;
using Control = System.Windows.Forms.Control;
using RadioButton = System.Windows.Forms.RadioButton;
using Panel = System.Windows.Forms.Panel;
using University_advisor.Constants;

namespace University_advisor.Forms
{
    public partial class MainForm : Form
    {
        string currentUser;
        ArrayList statusList = new ArrayList() { "Student", "Graduate", "Lecturer" };
        ControlGMap map;
        int selectedCourse;
        int selectedUniversity;

        public MainForm(string username)
        {
            InitializeComponent();
            CenterToScreen();
            InstantiateGrid();
            currentUser = username;
            SetValues();
            tabsController.Appearance = TabAppearance.FlatButtons;
            tabsController.ItemSize = new Size(0, 1);
            tabsController.SizeMode = TabSizeMode.Fixed;
        }

        private void LogoButton_Click(object sender, EventArgs e)
        {
            tabsController.SelectTab(homeTab);
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            tabsController.SelectTab(homeTab);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            tabsController.SelectTab(settingsTab);
            SetValues();
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            tabsController.SelectTab(aboutTab);
            var service = new UserEditingService(currentUser);
            var userInfo = service.GetUserInfo();

            currentUserLabel.Text = currentUser;
            currentEmailLabel.Text = userInfo[0];
            currentFirstnameLabel.Text = userInfo[1];
            currentLastnameLabel.Text = userInfo[2];
            currentUniversityLabel.Text = userInfo[3];
            currentStatusLabel.Text = userInfo[4];
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            tabsController.SelectTab(logOutTab);
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            tabsController.SelectTab(homeTab);
        }

        private void ChangePassword_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(currentPassword.Text) ||
                 String.IsNullOrEmpty(newPassword.Text) || String.IsNullOrEmpty(newPassword2.Text))
            {
                MessageBox.Show(Messages.emptyFields);
                Logger.Log(Messages.emptyFields);
            }
            else
            {
                if (newPassword.Text.Length >= 6 && newPassword2.Text.Length >= 6)
                {
                    new UserEditingService(currentUser)
                    .UpdateSetting("password", PasswordHasher.CreateMD5(currentPassword.Text),
                    PasswordHasher.CreateMD5(newPassword.Text), PasswordHasher.CreateMD5(newPassword2.Text),
                    Messages.newPasswordSameAsOld, Messages.passwordUpdateSuccess,
                    Messages.passwordUpdateFailed, Messages.passwordsDoNotMatch, Messages.passwordIncorrect);
                    ClearValues();
                }
                else
                {
                    MessageBox.Show(Messages.passwordTooShort);
                    Logger.Log(Messages.passwordTooShort);
                }
            }
        }

        private void ChangeEmail_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(currentEmail.Text) ||
                 String.IsNullOrEmpty(newEmail.Text) || String.IsNullOrEmpty(newEmail2.Text))
            {
                MessageBox.Show(Messages.emptyFields);
                Logger.Log(Messages.emptyFields);
            }
            else
            {
                UserEditingService service = new UserEditingService(currentUser);
                if (service.CheckEmailFormat(newEmail.Text) && service.CheckEmailFormat(newEmail2.Text))
                {
                    service.UpdateSetting("email", currentEmail.Text,
                    newEmail.Text, newEmail2.Text, Messages.newEmailSameAsOld, Messages.emailUpdateSuccess,
                    Messages.emailUpdateFailed, Messages.emailsDontMatch, Messages.emailIncorrect);
                    ClearValues();
                }
            }
        }

        private void ChangeUniversity_Click(object sender, EventArgs e)
        {
            new UserEditingService(currentUser).ChangeUniversity(universityBox.SelectedItem.ToString());
            ClearValues();
        }

        private void ChangeStatus_Click(object sender, EventArgs e)
        {
            new UserEditingService(currentUser).ChangeStatus(statusBox.SelectedItem.ToString());
            ClearValues();
        }

        private void SetValues()
        {
            var service = new UserEditingService(currentUser);
            universityBox.DataSource = service.GetAllUniversities();
            universityBox.SelectedItem = service.GetCurrentUniversity();
            statusBox.DataSource = statusList;
            statusBox.SelectedItem = service.GetCurrentSetting("status");
        }

        private void ClearValues()
        {
            currentPassword.Clear();
            newPassword.Clear();
            newPassword2.Clear();
            currentEmail.Clear();
            newEmail.Clear();
            newEmail2.Clear();
        }

        private void InstantiateGrid()
        {
            var table = new DataTable();

            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Variety of courses", typeof(string));
            table.Columns.Add("Availability of extracurricular activities ", typeof(string));
            table.Columns.Add("Access to faculty", typeof(string));
            table.Columns.Add("Quality of academic facilities (library, PCs, etc.)", typeof(string));
            table.Columns.Add("Student unions", typeof(string));
            table.Columns.Add("Cost of studying", typeof(string));

            var universities = SqlDriver.Fetch("SELECT u.universityId,name,avg(variety) as variety,avg(availability) as availability,avg(accessability) as accessability,avg(quality) as quality,avg(unions) as unions,avg(cost) as cost " +
                "FROM universities u LEFT JOIN universityReviews ur ON u.universityId=ur.universityId " +
                "GROUP BY u.universityId,name");

            foreach (Dictionary<string, object> row in universities)
            {
                table.Rows.Add(row["universityId"], row["name"], row["variety"], row["availability"], row["accessability"], row["quality"], row["unions"], row["cost"]);
            }

            universitiesGrid.DataSource = null;
            universitiesGrid.Rows.Clear();
            universitiesGrid.DataSource = table;
        }
        private void InstantiateProgramsGrid(int universityId)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Group", typeof(string));
            table.Columns.Add("Direction", typeof(string));
            table.Columns.Add("Program", typeof(string));
            table.Columns.Add("Presentation of content", typeof(string));
            table.Columns.Add("Clarity of expectations", typeof(string));
            table.Columns.Add("Clear feedback on performance", typeof(string));
            table.Columns.Add("Encouragment of participation/discussion", typeof(string));
            table.Columns.Add("Overall teaching effectiveness", typeof(string));
            table.Columns.Add("How satisfied were you with this course", typeof(string));

            var programmes = SqlDriver.Fetch($"SELECT studyProgramId,[group],direction,program,city," +
                $"avg(presentation) as presentation,avg(clarity) as clarity,avg(feedback) as feedback, avg(encouragement) as encouragement,avg(effectiveness) as effectiveness,avg(satisfaction) as satisfaction " +
                $"FROM studyProgrammes left join courseReviews on studyProgramId=courseId" +
                $" WHERE universityId = {universityId} group by studyProgramId,[group], direction, program, city");

            foreach (Dictionary<string, object> row in programmes)
            {
                table.Rows.Add(row["studyProgramId"], row["group"], row["direction"], row["program"], row["presentation"], row["clarity"], row["feedback"], row["encouragement"], row["effectiveness"], row["satisfaction"]);
            }

            programmesGrid.DataSource = null;
            programmesGrid.Rows.Clear();
            programmesGrid.DataSource = table;
        }

        private void UniversitiesGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;

            if (dgv == null)
            {
                return;
            }

            var selectedId = (int)dgv.CurrentRow.Cells["Id"].Value;
            var selectedName = dgv.CurrentRow.Cells["Name"].Value.ToString();

            universityName.Text = selectedName;
            InstantiateProgramsGrid(selectedId);
            selectedUniversity = selectedId;
            tabsController.SelectTab(universityTab);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            int[] rangeValues = new int[] { 50, 100, 250, 500, 1000, 2000 };
            foreach (var integer in rangeValues) rangeComboBox.Items.Add(integer);

            searchButton.TabStop = false;
            searchButton.FlatStyle = FlatStyle.Flat;
            searchButton.FlatAppearance.BorderSize = 0;

            tabsController.SelectTab(findSchoolsTab);
        }
        private void gMap_Load(object sender, EventArgs e)
        {
            map = new ControlGMap(gMap);
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            map.UpdateMap(rangeComboBox.Text, addressTextBox.Text, incorrectDataLabel);
        }

        private void ProgrammesGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;

            if (dgv == null)
            {
                return;
            }

            int selectedId = (int)dgv.CurrentRow.Cells["Id"].Value;
            selectedCourse = selectedId;
            string userId = GetUserId(currentUser);

            if (SqlDriver.Exists($"SELECT * FROM coursereviews WHERE userId = {userId} AND courseId = {selectedCourse} LIMIT 1"))
            {
                MessageBox.Show(Messages.reviewAlreadySubmitted);
                return;
            }
            tabsController.SelectTab(courseReview);
        }

        private void ReviewSubmit_Click(object sender, EventArgs e)
        {
            var userId = GetUserId(currentUser);
            if (SqlDriver.Exists($"SELECT * FROM universityReviews WHERE userId = {userId} AND universityId = {selectedUniversity} LIMIT 1"))
            {
                MessageBox.Show(Messages.reviewAlreadySubmitted);
                return;
            }
            tabsController.SelectTab(universityReview);
        }

        private void SubmitCourseReview_Click(object sender, EventArgs e)
        {
            var panels = new List<Panel> { presentation, clarity, feedback, encouragement, effectiveness, satisfaction };
            var result = ExtractReviews(panels);
            string insert = "INSERT INTO coursereviews (";
            string values = "VALUES (";
            foreach (var item in result)
            {
                insert += item.Key + ",";
                values += item.Value + ",";
            }
            insert += "userId,courseId)";
            values += GetUserId(currentUser) + "," + selectedCourse + ")";
            SqlDriver.Execute(insert + values);
            // Renew grid after submitting a review.
            InstantiateGrid();
            InstantiateProgramsGrid(selectedUniversity);
            tabsController.SelectTab(universityTab);
        }

        private void SubmitUniversityReview_Click(object sender, EventArgs e)
        {
            var panels = new List<Panel> { variety, availability, accessability, quality, unions, cost };
            var result = ExtractReviews(panels);
            var insert = "INSERT INTO universityReviews (";
            var values = "VALUES (";
            foreach (var item in result)
            {
                insert += item.Key + ",";
                values += item.Value + ",";
            }
            insert += "userId,universityId)";
            values += GetUserId(currentUser) + "," + selectedUniversity + ")";
            SqlDriver.Execute(insert + values);
            // Renew grid after submitting a review.
            InstantiateGrid();
            InstantiateProgramsGrid(selectedUniversity);
            tabsController.SelectTab(universityTab);
        }

        private string GetUserId(string username)
        {
            var user = (Dictionary<string, object>)SqlDriver.Fetch($"SELECT userId FROM users WHERE username = '{username}'")[0];
            return user["userId"].ToString();
        }

        private Dictionary<string, string> ExtractReviews(List<Panel> panels)
        {
            var result = new Dictionary<string, string>();
            foreach (Panel p in panels)
            {
                foreach (Control c in p.Controls)
                {
                    if (c.GetType() == typeof(RadioButton))
                    {
                        RadioButton btn = (RadioButton)c;
                        if (btn.Checked == true)
                        {
                            result.Add(p.Name, btn.Text);
                        }
                    }
                }
            }
            return result;
        }
    }
}
