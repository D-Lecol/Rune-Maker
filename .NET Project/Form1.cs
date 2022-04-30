using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Management;
using System.Data.SQLite;

namespace rune_maker
{
    public partial class Form1 : Form
    {
        private RunePage runepage1;
        private LcuConnection currentPageConn;
        private int chosenStyle;

        private int[] secondStylesArray = new int[4];
        private int chosenSecondStyle;

        private int[] orderSecondPerk = new int[2];
        private int disabledSecondPerk;
        private string pathGame = "";
        private SQLiteConnection con;

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        static readonly IntPtr HWND_TOP = new IntPtr(0);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        const UInt32 SWP_NOSIZE = 0x0001;

        const UInt32 SWP_NOMOVE = 0x0002;

        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;



        [DllImport("user32.dll")]

        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public Form1()
        {

            chosenStyle = 0;
            chosenSecondStyle = 1;
            disabledSecondPerk = 2;
            for (int i = 0; i < 4; i++)
            {
                secondStylesArray[i] = i + 1;
            }
            orderSecondPerk[0] = 0;
            orderSecondPerk[1] = 1;
            string path = Directory.GetCurrentDirectory();
            string cs = "URI=file:"+path+"\\savedPages.db";

            con = new SQLiteConnection(cs);
            con.Open();
            
            InitWMI();
            var auths = GetAuthPassword();
            InitializeComponent();

            runepage1 = new(auths[0], auths[1]);
            InitRunes();

            currentPageConn = new LcuConnection(auths[0], auths[1]);
            GetIdsPages();
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }

        private void InitWMI()
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process WHERE name='LeagueClientUx.exe'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection results = searcher.Get();
            int x = 0;
            string[] list = new string[results.Count];
            foreach(ManagementObject values in results)
            {
                list[x] = values["commandline"].ToString()!;
                x++;
            }
            string[] splitCommandLine = list[0].Split("\" \"");

            pathGame = splitCommandLine[11].Split("--install-directory=")[1];
        }

        private string[] GetAuthPassword()
        {
            var lockfileStream = new FileStream(pathGame + @"\lockfile", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var streamReader = new StreamReader(lockfileStream);
            var lockfile = streamReader.ReadToEnd();
            string[] lockfileArray = lockfile.Split(':');
            lockfileStream.Close();
            var hostClient = "https://127.0.0.1:" + lockfileArray[2];
            var authorization = "riot:" + lockfileArray[3];
            return new string[] { authorization, hostClient };
        }

        private void style_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (sender as RadioButton)!;
            char nbString = radioButton.Name[radioButton.Name.Length - 1];
            this.chosenStyle = int.Parse(nbString.ToString()) - 1;
            int p = 1;
            for (int i = 0; i < 5; i++)
            {
                if (i != this.chosenStyle)
                {
                    this.secondStylesArray[p - 1] = i;
                    p++;
                }
            }
            RadioButton radioButton_2 = secondStyles.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked)!;
            char nbString_2 = radioButton_2.Name[radioButton_2.Name.Length - 1];
            this.chosenSecondStyle = secondStylesArray[int.Parse(nbString_2.ToString()) - 1];

            this.ChangeStyle(); 
            this.ChangeSecondStyle();
        }

        private void SecondStyle_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (sender as RadioButton)!;
            char nbString = radioButton.Name[radioButton.Name.Length - 1];
            this.chosenSecondStyle = secondStylesArray[int.Parse(nbString.ToString()) - 1];
            this.ChangeSecondStyle();
        }

        private void secondPerks_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (sender as RadioButton)!;
            int nbRow = int.Parse(radioButton.Name[radioButton.Name.Length-3].ToString())-1;
            if(nbRow == disabledSecondPerk)
            {
                Control ctn = this.Controls["secondPerk_"+ (orderSecondPerk[0]+1)];
                RadioButton todisabled = ctn.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked)!;
                todisabled.Checked = false;
                disabledSecondPerk = orderSecondPerk[0];
                orderSecondPerk[0] = orderSecondPerk[1];
                orderSecondPerk[1] = nbRow;
            }

            
        }

        private void ChangeStyle()
        {
            for (int i = 0; i < 5; i++)
            {
                RadioButton button = (RadioButton)styles.Controls["style" + (i + 1).ToString()];
                button.Image = runepage1.styleIconBitmap[i, 0, 0];
                toolTip.SetToolTip(button, runepage1.styleNames[i, 0, 0]);
            }

            int x = this.chosenStyle;
            //int x = 3;
            for (int i = 0; i < 4; i++)
            {
                RadioButton button = (RadioButton)keystones.Controls["keystone" + (i + 1).ToString()];
                if (runepage1.styleNames[x, 1, i] != "")
                {
                    button.Enabled = true;
                    button.Visible = true;
                    button.Image = runepage1.styleIconBitmap[x, 1, i];
                    toolTip.SetToolTip(button, runepage1.styleNames[x, 1, i]);
                }
                else
                {
                    button.Enabled = false;
                    button.Hide();
                }
            }

            for (int i = 2; i < 5; i++)
            {
                for (int y = 0; y < 4; y++)
                {
                    String nameControl = "perk_" + (i - 1);
                    Control ctn = this.Controls[nameControl];
                    RadioButton button = (RadioButton)ctn.Controls[nameControl + "_" + (y + 1).ToString()];
                    if (runepage1.styleNames[x, i, y] != "")
                    {
                        button.Enabled = true;
                        button.Visible = true;
                        button.Image = runepage1.styleIconBitmap[x, i, y];
                        toolTip.SetToolTip(button, runepage1.styleNames[x, i, y]);
                    }
                    else
                    {
                        button.Enabled = false;
                        button.Hide();
                    }
                }
            }
            this.ChangeSecondStyle();
        }

        private void ChangeSecondStyle()
        {
            int z = this.chosenStyle;
            int p = 1;

            int x = this.chosenSecondStyle;
            for (int i = 0; i < 5; i++)
            {
                if (i != z)
                {
                    RadioButton button = (RadioButton)secondStyles.Controls["secondStyle" + p];
                    button.Image = runepage1.styleIconBitmap[i, 0, 0];
                    toolTip.SetToolTip(button, runepage1.styleNames[i, 0, 0]);
                    this.secondStylesArray[p-1] = i;
                    p++;
                }
            }

            for (int i = 2; i < 5; i++)
            {
                    for (int y = 0; y < 4; y++)
                    {
                        String nameControl = "secondPerk_" + (i - 1);
                        Control ctn = this.Controls[nameControl];
                        RadioButton button = (RadioButton)ctn.Controls[nameControl + "_" + (y + 1).ToString()];
                        if (runepage1.styleNames[x, i, y] != "")
                        {
                            button.Enabled = true;
                            button.Visible = true;
                            button.Image = runepage1.styleIconBitmap[x, i, y];
                            toolTip.SetToolTip(button, runepage1.styleNames[x, i, y]);
                        }
                        else
                        {
                            button.Enabled = false;
                            button.Hide();
                        }
                    }
            }

        }

        private async void InitRunes()
        {
            await runepage1.InitRunePage();

            for (int i = 0; i < 3; i++)
            {
                for (int y = 0; y < 3; y++)
                {
                    String nameControl = "stats" + (i + 1);
                    Control ctn = this.Controls[nameControl];
                    RadioButton button = (RadioButton)ctn.Controls[nameControl + "_" + (y + 1)];
                    button.Image = runepage1.statsIconBitmap[i, y];
                    toolTip.SetToolTip(button, runepage1.statsNames[i, y]);
                }
                for(int y = 0; y < 4; y++)
                {
                    String nameControl_2 = "secondPerk_" + (i + 1);
                    Control ctn_2 = this.Controls[nameControl_2];
                    RadioButton button_2 = (RadioButton)ctn_2.Controls[nameControl_2 + "_" + (y + 1)];
                    button_2.CheckedChanged += secondPerks_CheckedChanged!;
                }
            }
            this.ChangeStyle();
            this.ChangeSecondStyle();
            for(int i = 0; i < 5; i++)
            {
                RadioButton button = (RadioButton)styles.Controls["style"+ (i+1)];
                button.CheckedChanged += style_CheckedChanged!;
            }
            for(int i = 0; i < 4; i++)
            {
                RadioButton button = (RadioButton)secondStyles.Controls["secondStyle" + (i + 1)];
                button.CheckedChanged += SecondStyle_CheckedChanged!;
            }

        }

        private async void prepareJson()
        {
            statusUpload.Text = "";
            JsonNode currentPageNode = await currentPageConn.GetCurrentPerks();
            currentPageConn.DeletePage(currentPageNode["id"]!.ToString());
            currentPageNode["name"] = comboBox1.Text;
            currentPageNode["primaryStyleId"] = int.Parse(runepage1.styleIds[chosenStyle, 0, 0]);
            currentPageNode["subStyleId"] = int.Parse(runepage1.styleIds[chosenSecondStyle, 0, 0]);
            currentPageNode["selectedPerkIds"]![0] = int.Parse(runepage1.styleIds[chosenStyle, 1, isChecked("keystones")]);

            DateTime foo = DateTime.Now;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            currentPageNode["lastModified"] = unixTime;

            for (int i = 1; i < 4; i++)
            {
                currentPageNode["selectedPerkIds"]![i] = int.Parse(runepage1.styleIds[chosenStyle, i+1, isChecked("perk_"+i)]);
            }
            for (int i = 4; i < 6; i++)
            {
                currentPageNode["selectedPerkIds"]![i] = int.Parse(runepage1.styleIds[chosenSecondStyle, orderSecondPerk[i-4]+2, isChecked("secondPerk_" + (orderSecondPerk[i - 4]+1))]);
            }
            for (int i = 6; i < 9; i++)
            {
                currentPageNode["selectedPerkIds"]![i] = int.Parse(runepage1.statsIds[i - 6, isChecked("stats" + (i-5))]);
            }
            var reponse = await currentPageConn.PostPage(currentPageNode);
            statusUpload.Text = "OK!";
            await Task.Delay(1000);
            statusUpload.Text = "";
        }

        private async void SavePage()
        {

            using var cmd = new SQLiteCommand(con);
            cmd.CommandText = "INSERT INTO pages(name, primaryStyle, subStyle, keystone," +
                "primary_perk1, primary_perk2, primary_perk3, sub_perk1, sub_perk2," +
                "stats1, stats2, stats3) " +
                "VALUES(@name, @primaryStyle, @subStyle, @keystone, " +
                "@primary_perk1, @primary_perk2, @primary_perk3, @sub_perk1, @sub_perk2," +
                "@stats1, @stats2, @stats3)";

            cmd.Parameters.AddWithValue("@name", comboBox1.Text);
            cmd.Parameters.AddWithValue("@primaryStyle", chosenStyle);
            cmd.Parameters.AddWithValue("@subStyle", isChecked("secondStyles"));
            cmd.Parameters.AddWithValue("@keystone", isChecked("keystones"));
            cmd.Parameters.AddWithValue("@primary_perk1", isChecked("perk_1"));
            cmd.Parameters.AddWithValue("@primary_perk2", isChecked("perk_2"));
            cmd.Parameters.AddWithValue("@primary_perk3", isChecked("perk_3"));
            cmd.Parameters.AddWithValue("@sub_perk1", orderSecondPerk[0]+"_" + isChecked("secondPerk_" + (orderSecondPerk[0] + 1)));
            cmd.Parameters.AddWithValue("@sub_perk2", orderSecondPerk[1] + "_" + isChecked("secondPerk_" + (orderSecondPerk[1]+1)));
            cmd.Parameters.AddWithValue("@stats1", isChecked("stats1"));
            cmd.Parameters.AddWithValue("@stats2", isChecked("stats2"));
            cmd.Parameters.AddWithValue("@stats3", isChecked("stats3"));

            cmd.Prepare();

            await cmd.ExecuteNonQueryAsync();
            comboBox1.Items.Add(comboBox1.Text);
            statusUpload.Text = "Saved!";
            await Task.Delay(1000);
            statusUpload.Text = "";

        }

        private async void GetIdsPages()
        {

            string stm = "SELECT * FROM pages";

            var cmd = new SQLiteCommand(stm, con);
            SQLiteDataReader rdr = (SQLiteDataReader)await cmd.ExecuteReaderAsync();

            while (rdr.Read())
            {
               comboBox1.Items.Add(rdr.GetString(1));
            }
        }

        private async void LoadPage(string pageName)
        {
            string stm = "SELECT * FROM pages WHERE name='"+pageName+"'";

            var cmd = new SQLiteCommand(stm, con);
            SQLiteDataReader rdr = (SQLiteDataReader)await cmd.ExecuteReaderAsync();

            while (rdr.Read())
            {
                //comboBox1.Items.Add(rdr.GetString(1));
                RadioButton btn = (RadioButton)styles.Controls["style" + (rdr.GetInt32(2) + 1).ToString()];
                btn.Checked = true;

                btn = (RadioButton)secondStyles.Controls["secondStyle" + (rdr.GetInt32(3) + 1).ToString()];
                btn.Checked = true;

                btn = (RadioButton)keystones.Controls["keystone" + (rdr.GetInt32(4) + 1).ToString()];
                btn.Checked=true;

                btn = (RadioButton)perk_1.Controls["perk_1_" + (rdr.GetInt32(5) + 1).ToString()];
                btn.Checked = true;

                btn = (RadioButton)perk_2.Controls["perk_2_" + (rdr.GetInt32(6) + 1).ToString()];
                btn.Checked = true;

                btn = (RadioButton)perk_3.Controls["perk_3_" + (rdr.GetInt32(7) + 1).ToString()];
                btn.Checked = true;

                string[] split = rdr.GetString(8).Split("_");
                Control ctn = this.Controls["secondPerk_" + (int.Parse(split[0]) + 1)];
                btn = (RadioButton)ctn.Controls["secondPerk_"+ (int.Parse(split[0]) + 1) + "_" + (int.Parse(split[1]) + 1)];
                btn.Checked = true;

                split = rdr.GetString(9).Split("_");
                ctn = this.Controls["secondPerk_" + (int.Parse(split[0]) + 1)];
                btn = (RadioButton)ctn.Controls["secondPerk_" + (int.Parse(split[0]) + 1) + "_" + (int.Parse(split[1]) + 1)];
                btn.Checked = true;

                btn = (RadioButton)stats1.Controls["stats1_" + (rdr.GetInt32(10) + 1).ToString()];
                btn.Checked = true;

                btn = (RadioButton)stats2.Controls["stats2_" + (rdr.GetInt32(11) + 1).ToString()];
                btn.Checked = true;

                btn = (RadioButton)stats3.Controls["stats3_" + (rdr.GetInt32(12) + 1).ToString()];
                btn.Checked = true;
            }
        }

        private async void DeletePage(string name)
        {
            string stm = "DELETE FROM pages WHERE name='" + name + "'";
            var cmd = new SQLiteCommand(stm, con);
            await cmd.ExecuteNonQueryAsync();
            statusUpload.Text = "Deleted!";
            await Task.Delay(1000);
            statusUpload.Text = "";
        }

        private int isChecked(string panel)
        {
            Control ctn = this.Controls[panel];
            RadioButton btn = ctn.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked)!;
            int nb = int.Parse(btn.Name[btn.Name.Length-1].ToString())-1;
            return nb;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void style2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void perk_2_3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void perk_3_1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void perk_1_1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.prepareJson();
        }

        private void style1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            var count = comboBox1.Items.Count;
            bool exist = false;
            for(int i = 0; i < count; i++)
            {
                if(comboBox1.Items[i].ToString() == comboBox1.Text)
                {
                    exist = true;
                }
            }
            if (!exist)
            {
                SavePage();
            }
            else
            {
                PageExistMessage();
            }
            
        }

        private async void PageExistMessage()
        {
            statusUpload.Text = "Already exist X";
            await Task.Delay(1000);
            statusUpload.Text = "";
        }

        private async void PageDontExistMessage()
        {
            statusUpload.Text = "Doesn't exist X";
            await Task.Delay(1000);
            statusUpload.Text = "";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPage(comboBox1.Text);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            var count = comboBox1.Items.Count;
            bool exist = false;
            string name = "";
            int index = -1;
            for (int i = 0; i < count; i++)
            {
                if (comboBox1.Items[i].ToString() == comboBox1.Text)
                {
                    exist = true;
                    name = comboBox1.Text;
                    index = i;
                }
            }
            if (exist)
            {
                comboBox1.Items.RemoveAt(index);
                DeletePage(name);
                comboBox1.Text = "";
            }
            else
            {
                PageDontExistMessage();
            }
        }
    }
}