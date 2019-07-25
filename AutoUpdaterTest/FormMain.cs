using System;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using ViewDB.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using ViewDB;

namespace AutoUpdaterTest
{
    public partial class FormMain : Form
    {
        private string Connectionstring { get; set; }

        public FormMain()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            pgrProcess.MarqueeAnimationSpeed = 0;

        }
        static bool TestConnectionString(string connectionString)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    bool kq = false;
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        kq = true;
                    }
                    conn.Close();
                    return kq;
                }
                catch
                {
                    return false;
                }
            }

        }

        private void findDBname()
        {
            List<string> list = new List<string>();
            try
            {
                SqlConnection con = new SqlConnection(this.Connectionstring);
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT name FROM sys.databases", con);

                using (IDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(dr[0].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            cblistdb.Items.Clear();
            foreach (String namedb in list)
            {
                cblistdb.Items.Add(namedb);
            }
        }

        private void checkLogin()
        {
            if (TestConnectionString(this.Connectionstring))
            {
                this.findDBname();
                MessageBox.Show("Login success");
                btnlogin.Enabled = true;
                btnexcute.Enabled = true;
                btninsert.Enabled = true;
                btnviewcl.Enabled = true;
            }

            else
            {
                MessageBox.Show("Login failed");
                btnlogin.Enabled = true;
                btnexcute.Enabled = false;
                btninsert.Enabled = false;
                return;
            }

        }

        private void excuteQr()
        {
            try
            {
                SqlConnection con = new SqlConnection(this.Connectionstring);
                con.Open();
                SqlCommand cmd = new SqlCommand(txtquery.Text, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                con.Close();
                sda.Fill(ds);
                int count = ds.Tables.Count;


                List<DataColumn> columnsToConvert = new List<DataColumn>();
                if (count == 1)
                {
                    foreach (DataColumn column in ds.Tables[0].Columns)
                    {
                        if (column.DataType.Name == "Byte[]")
                        {
                            columnsToConvert.Add(column);

                        }

                    }
                    columnsToConvert.ForEach(col =>
                    {
                        ds.Tables[0].ConvertColumnType(col.ColumnName, typeof(string));
                    });


                    this.dataview.DataSource = ds.Tables[0];
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        foreach (DataColumn column in ds.Tables[0].Columns)
                        {
                            if (column.DataType.Name == "Byte[]")
                            {
                                columnsToConvert.Add(column);

                            }

                        }
                        columnsToConvert.ForEach(col =>
                        {
                            ds.Tables[0].ConvertColumnType(col.ColumnName, typeof(string));
                        });
                        Form2 form2 = new Form2(ds, i);
                        String title = "Table " + i;
                        form2.Text = title;
                        form2.Show();
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void excuteIns()
        {
            btninsert.Enabled = false;
            SqlConnection con = new SqlConnection(this.Connectionstring);
            con.Open();
            SqlCommand cmd = new SqlCommand(txtquery.Text, con);
            int result = cmd.ExecuteNonQuery();
            con.Close();
            if (result == 0)
            {
                MessageBox.Show("Query has completed ");
                btninsert.Enabled = true;
            }
            else if (result < 0)
            {
                MessageBox.Show("Fails...! ");
                btninsert.Enabled = true;
            }
            else
            {
                MessageBox.Show("Inserted: " + result + " colums");
                btninsert.Enabled = true;
            }
        }

        private string[] getTableName(String dbname)
        {
            List<string> colList = new List<string>();

            try
            {
                SqlConnection con = new SqlConnection(this.Connectionstring);
                con.Open();
                DataTable dt = con.GetSchema("Tables");
                foreach (DataRow row in dt.Rows)
                {
                    string tableName = row[2].ToString();
                    Console.WriteLine(tableName);
                    colList.Add(tableName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return colList.ToArray();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            //Uncomment below lines to handle parsing logic of non XML AppCast file.

            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.json");
            AutoUpdater.AppTitle = "ViewDB";
            //Uncomment below line to run update process using non administrator account.

            //AutoUpdater.RunUpdateAsAdmin = false;

            //Uncomment below line to see russian version

            //Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("ru");

            //If you want to open download page when user click on download button uncomment below line.

            //AutoUpdater.OpenDownloadPage = true;

            //Don't want user to select remind later time in AutoUpdater notification window then uncomment 3 lines below so default remind later time will be set to 2 days.

            //AutoUpdater.LetUserSelectRemindLater = false;
            //AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
            //AutoUpdater.RemindLaterAt = 1;

            //Don't want to show Skip button then uncomment below line.

            //AutoUpdater.ShowSkipButton = false;

            //Don't want to show Remind Later button then uncomment below line.

            //AutoUpdater.ShowRemindLaterButton = false;

            //Want to show custom application title then uncomment below line.

            //AutoUpdater.AppTitle = "My Custom Application Title";

            //Want to show errors then uncomment below line.

            //AutoUpdater.ReportErrors = true;

            //Want to handle how your application will exit when application finished downloading then uncomment below line.

            //AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;

            //Want to handle update logic yourself then uncomment below line.

            //AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

            //Want to use XML and Update file served only through Proxy.

            //var proxy = new WebProxy("localproxyIP:8080", true) {Credentials = new NetworkCredential("domain\\user", "password")};

            //AutoUpdater.Proxy = proxy;

            //Want to check for updates frequently then uncomment following lines.

            //System.Timers.Timer timer = new System.Timers.Timer
            //{
            //    Interval = 2 * 60 * 1000,
            //    SynchronizingObject = this
            //};
            //timer.Elapsed += delegate
            //{
            //    AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.xml");
            //};
            //timer.Start();

            //Uncomment following lines to provide basic authentication credetials to use.

            //BasicAuthentication basicAuthentication = new BasicAuthentication("myUserName", "myPassword");
            //AutoUpdater.BasicAuthXML = AutoUpdater.BasicAuthDownload = basicAuthentication;

            //Uncomment following lines to enable forced updates.

            //AutoUpdater.Mandatory = true;
            //AutoUpdater.UpdateMode = Mode.Forced;

            //Want to change update form size then uncomment below line.

            //AutoUpdater.UpdateFormSize = new System.Drawing.Size(800, 600);

            //Uncomment following if you want to update using FTP.
            //AutoUpdater.Start("ftp://rbsoft.org/updates/AutoUpdaterTest.xml", new NetworkCredential("FtpUserName", "FtpPassword"));

            //AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.xml");
        }

        private void AutoUpdater_ApplicationExitEvent()
        {
            Text = @"Closing application...";
            Thread.Sleep(3000);
            Application.Exit();
        }


        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = json.version,
                ChangelogURL = json.changelog,
                Mandatory = json.mandatory,
                DownloadURL = json.url
            };
        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null)
            {
                if (args.IsUpdateAvailable)
                {
                    DialogResult dialogResult;
                    if (args.Mandatory)
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {
                                        args.InstalledVersion
                                    }. This is required update. Press Ok to begin updating the application.",
                                @"Update Available",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                    }
                    else
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {
                                        args.InstalledVersion
                                    }. Do you want to update the application now?", @"Update Available",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                    }


                    if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                    {
                        try
                        {
                            //You can use Download Update dialog used by AutoUpdater.NET to download the update.

                            if (AutoUpdater.DownloadUpdate())
                            {
                                Application.Exit();
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(@"There is no update available. Please try again later.", @"Update Unavailable",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(
                    @"There is a problem reaching update server. Please check your internet connection and try again later.",
                    @"Update Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonCheckForUpdate_Click(object sender, EventArgs e)
        {
           
        }
        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoUpdater.Mandatory = true;
            AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTest.json");
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            String labelVersion = string.Format(Resources.CurrentVersion, Assembly.GetEntryAssembly().GetName().Version);//Version
            labelVersion += "\nDeverlopedVuChung\nUsing lib AutoUpdate.NTE";
            MessageBox.Show(labelVersion,"About me", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
        }

        private async void btnlogin_Click(object sender, EventArgs e)
        {
            btnlogin.Enabled = false;
            pgrProcess.Visible = true;
            pgrProcess.MarqueeAnimationSpeed = 4;
            this.Connectionstring = $"Server={txtservername.Text};User Id={txtusername.Text};Password = {txtpassword.Text}; ";
            await Task.Run(() =>
            {

                try
                {
                    checkLogin();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    btnlogin.Enabled = true;
                }
                pgrProcess.MarqueeAnimationSpeed = 0;
                pgrProcess.Refresh();
                pgrProcess.Visible = false;
            });
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
            var form2 = new FormMain();
            form2.ShowDialog();
            this.Close();
        }

        private async void btnexcute_Click(object sender, EventArgs e)
        {
            pgrProcess.MarqueeAnimationSpeed = 4;
            await Task.Run(() => {
                try
                {
                    excuteQr();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                pgrProcess.MarqueeAnimationSpeed = 0;
            });
        }

        private async void btninsert_Click(object sender, EventArgs e)
        {
            pgrProcess.MarqueeAnimationSpeed = 4;
            try
            {
                await Task.Run(() => {
                    excuteIns();
                });
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                btninsert.Enabled = true;
            }
            
            pgrProcess.MarqueeAnimationSpeed = 0;
        }

        private void btnviewcl_Click(object sender, EventArgs e)
        {
            try
            {
                if (cblistdb.SelectedIndex < 0)
                {
                    MessageBox.Show("Database name incorrect");
                    btnexcute.Enabled = false;
                    btninsert.Enabled = false;
                    btnlogin.Enabled = true;
                    btnviewcl.Enabled = false;
                    return;
                }
                this.Connectionstring = $"Server={txtservername.Text};Database= {cblistdb.Text};User Id={txtusername.Text};Password = {txtpassword.Text}; ";
                btnexcute.Enabled = true;
                btninsert.Enabled = true;
                btnviewcl.Enabled = true;
                ViewColums viewcl = new ViewColums(this.Connectionstring, getTableName(cblistdb.Text), cblistdb.Text);
                
                viewcl.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
