/* Matthew Delapasse
 * September 21, 2021
 * This project builds off of example 5-9 in the previous chapter
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace _6_8
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //level declarations that will be used in the frmPublishers_Load
        SqlConnection booksConnection;
        SqlCommand publishersCommand;
        SqlDataAdapter publishersAdapter;
        DataTable publishersTable;
        CurrencyManager publishersManager;
        string myState;
        int myBookmark;

        private void frmPublishers_Load(object sender, EventArgs e)
        {
            try
            {
                //point to help file
                hlpPublishers.HelpNamespace = Application.StartupPath + "\\publishers.chm";

                //connect to the books database (this will lead to successful connection)
                string fullfile = Path.GetFullPath("SQLBooksDB.mdf");

                //Connect to the books database (this will lead to an unsuccessful connection)
                //string fullfile = Path.GetFullPath("SQLBooksDB.accdb");

                booksConnection = new SqlConnection("Data Source=.\\SQLEXPRESS; AttachDbFilename=" + fullfile + ";Integrated Security=True; Connect Timeout=30; User Instance=True");
                booksConnection.Open();

                //This tested to see if the connection worked
                //MessageBox.Show("the connection was successfull");

                //establish command object
                publishersCommand = new SqlCommand("SELECT * FROM Publishers ORDER BY Name", booksConnection);

                ////connection object established
                //MessageBox.Show("The connection object established.");

                //esablish data adapter/data table
                publishersAdapter = new SqlDataAdapter();
                publishersAdapter.SelectCommand = publishersCommand;
                publishersTable = new DataTable();
                publishersAdapter.Fill(publishersTable);

                //bind controls to data table
                txtPubID.DataBindings.Add("Text", publishersTable, "PubID");
                txtPubName.DataBindings.Add("Text", publishersTable, "Name");
                txtCompanyName.DataBindings.Add("Text", publishersTable, "Company_Name");
                txtPubAddress.DataBindings.Add("Text", publishersTable, "Address");
                txtPubCity.DataBindings.Add("Text", publishersTable, "City");
                txtPubState.DataBindings.Add("Text", publishersTable, "State");
                txtPubZip.DataBindings.Add("Text", publishersTable, "Zip");
                txtPubTelephone.DataBindings.Add("Text", publishersTable, "Telephone");
                txtPubFAX.DataBindings.Add("Text", publishersTable, "FAX");
                txtPubComments.DataBindings.Add("Text", publishersTable, "Comments");

                //establish currency manager
                publishersManager = (CurrencyManager)this.BindingContext[publishersTable];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error establishing Publishers table.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //When the applicaiton starts it will be in view state
            this.Show();
            SetState("View");
        }

        private void frmPublishers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myState.Equals("Edit") || myState.Equals("Add"))
            {
                MessageBox.Show("You must finish the current edit before stopping the application.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            else
            {
                try
                {
                    //save changes to database
                    SqlCommandBuilder publishersAdapterCommands = new SqlCommandBuilder(publishersAdapter);
                    publishersAdapter.Update(publishersTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving database to file: \r\n" + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // close the connection 
                booksConnection.Close();

                //dispose of the objects
                booksConnection.Dispose();
                publishersCommand.Dispose();
                publishersAdapter.Dispose();
                publishersTable.Dispose();
            }
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            try
            {
                myBookmark = publishersManager.Position;
                publishersManager.AddNew();
                SetState("Add");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (publishersManager.Position == 0)
            {
                Console.Beep();
            }
            publishersManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (publishersManager.Position == publishersManager.Count - 1)
            {
                Console.Beep();
            }
            publishersManager.Position++;
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            publishersManager.Position = 0;
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            publishersManager.Position = publishersManager.Count - 1;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateData())
            {
                return;
            }
            string savedName = txtPubName.Text;
            int savedRow;
            try
            {
                publishersManager.EndCurrentEdit();
                publishersTable.DefaultView.Sort = "Name";
                savedRow = publishersTable.DefaultView.Find(savedName);
                publishersManager.Position = savedRow;
                MessageBox.Show("Record saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetState("View");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            publishersManager.CancelCurrentEdit();
            if (myState.Equals("Add"))
            {
                publishersManager.Position = myBookmark;
            }
            SetState("View");
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure you want to delete this record?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (response == DialogResult.No)
            {
                return;
            }
            try
            {
                publishersManager.RemoveAt(publishersManager.Position);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, hlpPublishers.HelpNamespace);
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtInput_KeyPress(Object sender, KeyPressEventArgs e)
        {
            TextBox whichBox = (TextBox)sender;
            if ((int)e.KeyChar == 13)
            {
                switch (whichBox.Name)
                {
                    case "txtPubName":
                        txtCompanyName.Focus();
                        break;
                    case "txtCompanyName":
                        txtPubAddress.Focus();
                        break;
                    case "txtPubAddress":
                        txtPubCity.Focus();
                        break;
                    case "txtPubCity":
                        txtPubState.Focus();
                        break;
                    case "txtPubState":
                        txtPubZip.Focus();
                        break;
                    case "txtPubZip":
                        txtPubTelephone.Focus();
                        break;
                    case "txtPubTelephone":
                        txtPubFAX.Focus();
                        break;
                    case "txtPubFAX":
                        txtPubComments.Focus();
                        break;
                    case "txtPubComments":
                        txtPubName.Focus();
                        break;
                }
            }
        }

        private void SetState(string appState)
        {
            myState = appState;
            switch (appState)
            {
                case "View":
                    txtPubID.BackColor = Color.White;
                    txtPubID.ForeColor = Color.Black;
                    txtPubName.ReadOnly = true;
                    txtCompanyName.ReadOnly = true;
                    txtPubAddress.ReadOnly = true;
                    txtPubCity.ReadOnly = true;
                    txtPubState.ReadOnly = true;
                    txtPubZip.ReadOnly = true;
                    txtPubTelephone.ReadOnly = true;
                    txtPubFAX.ReadOnly = true;
                    txtPubComments.ReadOnly = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnAddNew.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnEdit.Enabled = true;
                    btnDelete.Enabled = true;
                    txtPubName.Focus();
                    break;
                default: // Add or Edit if not View;
                    txtPubID.BackColor = Color.Red;
                    txtPubID.ForeColor = Color.White;
                    txtPubName.ReadOnly = false;
                    txtCompanyName.ReadOnly = false;
                    txtPubAddress.ReadOnly = false;
                    txtPubCity.ReadOnly = false;
                    txtPubState.ReadOnly = false;
                    txtPubZip.ReadOnly = false;
                    txtPubTelephone.ReadOnly = false;
                    txtPubFAX.ReadOnly = false;
                    txtPubComments.ReadOnly = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnAddNew.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    txtPubName.Focus();
                    break;
            }
        }

        private bool ValidateData()
        {
            string message = "";
            bool allOK = true;

            // Check for name
            if (txtPubName.Text.Trim().Equals(""))
            {
                message = "You must enter an Author Name." + "\r\n";
                txtPubName.Focus();
                allOK = false;
            }

            if (!allOK)
            {
                MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return (allOK);
        }
    }
}
