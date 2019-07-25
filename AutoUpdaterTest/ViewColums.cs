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
using System.Threading;

namespace ViewDB
{
    public partial class ViewColums : Form
    {
        public ViewColums()
        {
            InitializeComponent();
        }

        private String[] columnames { get; set; }
        private String tbname { get; set; }
        private String connect { get; set; }
        public ViewColums(String cm, String[] list, String tbname)
        {
            this.columnames = list;
            this.tbname = tbname;
            this.connect = cm;
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        
        private async void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            await Task.Run(() => {
                 getAllDB(lbTable.SelectedItem.ToString());
            });
           
                
        }
        private void ViewColums_Load(object sender, EventArgs e)
        {
            txttbname.Text = "Database:" + tbname;
            lbTable.Items.Clear();
            lbTable.DataSource = columnames;
            
        }
        

        private async void getAllDB(String itemtbname)
        {
            try
                {
                    SqlConnection con = new SqlConnection(this.connect);
                    String sql = " Select top 10 * from " + itemtbname;
                    con.Open();
                    SqlCommand cmd = new SqlCommand(sql, con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    con.Close();
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    List<DataColumn> columnsToConvert = new List<DataColumn>();

                     await Task.Run(() => {
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
                        dataview.ClearSelection();
                        dataview.DataSource = ds.Tables[0];
                    });
                    

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            
        }

        private void ViewColums_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
    }
}
