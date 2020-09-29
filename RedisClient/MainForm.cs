using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace RedisClient
{
    public partial class MainForm : Form
    {
        private ScrollablePanel pnlLnkKeys;
        private ScrollablePanel pnlTreatRedisKeys;
        private Label _lblCurrentKey;
        private Label _lblDisplayValue;
        private string _portVal = string.Empty;
        private RedisKey _currentRedisKey;
        private Thread _nonBlockinglyPastingTextToControlThread;

        private bool _isRedisConnected;

        private Dictionary<string, Link> _redisKeys = new Dictionary<string, Link>();

        public MainForm()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            RedisDataAccess.URL = "localhost";
            RedisDataAccess.Port = 6379;
            _portVal = txtPortRedisServer.Text;

            txtUrlRedisServer.TextChanged += (object sender, EventArgs e) => 
            {
                RedisDataAccess.URL = (sender as TextBox).Text;
            };
            
            txtPortRedisServer.TextChanged += (object sender, EventArgs e) =>
            {
                if (String.IsNullOrEmpty((sender as TextBox).Text))
                    (sender as TextBox).Text = _portVal;

                if (Int32.TryParse((sender as TextBox).Text, out int port))
                {
                    RedisDataAccess.Port = port;
                    _portVal = port.ToString();
                }
                else
                {
                    string strVal = "";
                    foreach (char s in txtPortRedisServer.Text)
                    {
                        if (int.TryParse(s.ToString(), out int digit))
                            strVal += s;
                    }
                    txtPortRedisServer.Text = strVal;
                    _portVal = strVal;
                }
            };

            lblInfo.Padding = new Padding(5);
            lblInfo.BackColor = Color.FromArgb(245, 229, 228);
            lblInfo.drawBorder(3, Color.DarkBlue);
            lblInfo.Text = string.Empty;

            Label lblpnlLnkKeysBackground = new Label();
            lblpnlLnkKeysBackground.AutoSize = false;
            lblpnlLnkKeysBackground.Location = new Point(lblInfo.Location.X, lblInfo.Location.Y + lblInfo.Height + 10);
            lblpnlLnkKeysBackground.Size = new Size(357, 355);
            lblpnlLnkKeysBackground.drawBorder(3, Color.DarkBlue);
            this.Controls.Add(lblpnlLnkKeysBackground);
            this.Controls.SetChildIndex(lblpnlLnkKeysBackground, -1);

            pnlLnkKeys = new ScrollablePanel();
            pnlLnkKeys.BackColor = Color.Bisque;
            pnlLnkKeys.Padding = new Padding(5);
            pnlLnkKeys.Location = new Point(lblpnlLnkKeysBackground.Location.X + 3, lblpnlLnkKeysBackground.Location.Y + 3);
            pnlLnkKeys.Size = new Size(lblpnlLnkKeysBackground.Size.Width - 6, lblpnlLnkKeysBackground.Size.Height - 6);
            pnlLnkKeys.Text = string.Empty;

            //here you can add additional functionality to mouse events of ScrollablePanel
            pnlLnkKeys.ScrollMouseWheel += (object sender, MouseEventArgs me) => { };
            pnlLnkKeys.ScrollHorizontal += (object sender, ScrollEventArgs se) => { };
            pnlLnkKeys.ScrollVertical += (object sender, ScrollEventArgs se) => { };
            this.Controls.Add(pnlLnkKeys);
            this.Controls.SetChildIndex(pnlLnkKeys, 1);

            lblTreatKeysPnlBack.drawBorder(3, Color.DarkBlue);
            lblTreatKeysPnlBack.Text = string.Empty;
            this.Controls.SetChildIndex(lblTreatKeysPnlBack, -1);

            pnlTreatRedisKeys = new ScrollablePanel();
            pnlTreatRedisKeys.Location = new Point(lblTreatKeysPnlBack.Location.X + 3, lblTreatKeysPnlBack.Location.Y + 3);
            pnlTreatRedisKeys.Size = new Size(lblTreatKeysPnlBack.Width - 6, lblTreatKeysPnlBack.Height - 6);
            pnlTreatRedisKeys.BackColor = Color.Bisque;
            pnlTreatRedisKeys.ScrollMouseWheel += (object sender, MouseEventArgs me) => { };
            pnlTreatRedisKeys.ScrollHorizontal += (object sender, ScrollEventArgs se) => { };
            pnlTreatRedisKeys.ScrollVertical += (object sender, ScrollEventArgs se) => { };
            this.Controls.Add(pnlTreatRedisKeys);
            this.Controls.SetChildIndex(pnlTreatRedisKeys, 1);

            Label lblCurrKeyConstntMessage = new Label();
            lblCurrKeyConstntMessage.Location = new Point(0);
            lblCurrKeyConstntMessage.AutoSize = true;
            lblCurrKeyConstntMessage.Padding = new Padding(3);
            lblCurrKeyConstntMessage.Font = new Font(new FontFamily("Arial"), 15, FontStyle.Bold, GraphicsUnit.Pixel);
            lblCurrKeyConstntMessage.Text = "redis key:";
            pnlTreatRedisKeys.Controls.Add(lblCurrKeyConstntMessage);

            Button btnViewValue = new Button();
            btnViewValue.Location = new Point(lblCurrKeyConstntMessage.Location.X, lblCurrKeyConstntMessage.Location.Y + lblCurrKeyConstntMessage.Height);
            btnViewValue.Click += async(object sender, EventArgs e) => 
            {
                _nonBlockinglyPastingTextToControlThread?.Abort();
                _lblDisplayValue.Text = "";

                byte[] currenKeyValue = (byte[])_currentRedisKey.GetType().GetProperty("KeyValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_currentRedisKey);
                if (currenKeyValue == null)
                    return;

                await TryToConnectAndRestoreRedisObjects();

                RedisValue value = await RedisDataAccess.RedisDb.StringGetAsync(_currentRedisKey);

                try
                {
                    object serialized = JsonConvert.DeserializeObject(value.ToString());
                }
                catch(JsonReaderException)
                {
                    _lblDisplayValue.Text = value.ToString();
                    return;
                }

                DialogResult rez = DialogResult.Cancel;
                if(value.ToString().Length > 10000)
                {
                    string title = "value is too long!";
                    string message = $"Yor value is a JSON object, but it is fairly lengthy ({System.Text.ASCIIEncoding.Unicode.GetByteCount(value.ToString()).FormatSize()} bytes exactly), so prettifying if may take a long time. Maybe you prefer to view it raw instead? \n\nPress OK to raw view\nor Cancel ro prettyfied view";

                    rez = MessageBox.Show(message, title, MessageBoxButtons.OKCancel);
                }

                if (rez == DialogResult.OK)
                {
                    PassTextToControlNonBlockingly(value.ToString(), _lblDisplayValue);
                    return;
                }


                var conv = new JsonToDictionaryConverter();
                conv.RawStringToConsume = value.ToString();
                Dictionary<string, object> valueDict = await conv.ProvideAPIDataFromJSON();
                Task<string> tsk = conv.ProvidePrettyString("    ", valueDict);
                DisplayAwaiting(tsk, lblInfo);
                PassTextToControlNonBlockingly(await tsk, _lblDisplayValue);


            };
            btnViewValue.Text = "View value";

            pnlTreatRedisKeys.Controls.Add(btnViewValue);

            Button btnDeleteKey = new Button();
            btnDeleteKey.Location = new Point(btnViewValue.Location.X + btnViewValue.Width + 5, btnViewValue.Location.Y);
            btnDeleteKey.Click += async(object sender, EventArgs e) => 
            {
                await TryToConnectAndRestoreRedisObjects();

                var val = await RedisDataAccess.RedisDb.StringGetAsync(_currentRedisKey);

                await RedisDataAccess.RedisDb.KeyDeleteAsync(_currentRedisKey);

                val = await RedisDataAccess.RedisDb.StringGetAsync(_currentRedisKey);

                if (val.IsNull)
                {

                    _lblCurrentKey.Text = "No key";
                    _lblDisplayValue.Text = "Nokey, so no data";
                }

                await btnGetKeys_Click();
            };
            btnDeleteKey.Text = "Remove";
            pnlTreatRedisKeys.Controls.Add(btnDeleteKey);

            _lblCurrentKey = new Label();
            _lblCurrentKey.Location = new Point(lblCurrKeyConstntMessage.Location.X + lblCurrKeyConstntMessage.Width, 0);
            _lblCurrentKey.AutoSize = true;
            _lblCurrentKey.Padding = new Padding(3);
            _lblCurrentKey.Font = new Font(new FontFamily("Arial"), 15, new FontStyle(), new GraphicsUnit());
            _lblCurrentKey.Text = "";
            pnlTreatRedisKeys.Controls.Add(_lblCurrentKey);

            lblOutputScrlPanBack.drawBorder(3, Color.DarkBlue);
            lblOutputScrlPanBack.Text = string.Empty;
            this.Controls.SetChildIndex(lblOutputScrlPanBack, -1);

            ScrollablePanel pnlOutput = new ScrollablePanel();
            pnlOutput.Location = new Point(lblOutputScrlPanBack.Location.X + 3, lblOutputScrlPanBack.Location.Y + 3);
            pnlOutput.Size = new Size(lblOutputScrlPanBack.Width - 6, lblOutputScrlPanBack.Height - 6);
            pnlOutput.BackColor = Color.LightCyan;
            pnlOutput.ScrollMouseWheel += (object sender, MouseEventArgs me) => { };
            pnlOutput.ScrollHorizontal += (object sender, ScrollEventArgs se) => { };
            pnlOutput.ScrollVertical += (object sender, ScrollEventArgs se) => { };
            this.Controls.Add(pnlOutput);
            this.Controls.SetChildIndex(pnlOutput, 1);

            _lblDisplayValue = new Label();
            _lblDisplayValue.Location = new Point(0);
            _lblDisplayValue.Padding = new Padding(5);
            _lblDisplayValue.AutoSize = true;
            pnlOutput.Controls.Add(_lblDisplayValue);




            btnStartRedis.Click += StartRedis;
            btnKillRedis.Click += (object sender, EventArgs e) => 
            {
                string pathToRedisServer = providePathToRedisServer("Please provide path to Redis server in order to be killed");
                if (pathToRedisServer != null)
                    RedisDataAccess.KillRedis(pathToRedisServer);
                else
                    MessageBox.Show("There is no path to Redis server");
            };

            btnisConnected.Click += async(object sender, EventArgs e) => {
                Task<bool> isConnectedTask = RedisDataAccess.isRedisConnected();
                lblInfo.Text = "Please wait ";
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 100;
                timer.Tick += (object sender, EventArgs e) => {
                    lblInfo.Text += ".";
                };
                timer.Start();
                bool isConnected = await isConnectedTask;
                timer.Stop();
                if (isConnected) lblInfo.Text = "Redis is running.";
                else lblInfo.Text = "Redis is not running";
            };

            btnGetKeys.Click += async(object sender, EventArgs e) => 
            {
                Task tsk = btnGetKeys_Click();
                DisplayAwaiting(tsk, lblInfo);
                await tsk;
            };

            btnNewKey.Click += (object sender, EventArgs e) => 
            {
                Form frmAddNewKey = new Form();
                frmAddNewKey.Size = new Size(this.Width - 100, this.Height - 100);
                frmAddNewKey.AutoSize = false;
                frmAddNewKey.AutoScroll = false;
                frmAddNewKey.VerticalScroll.Visible = true;
                frmAddNewKey.HorizontalScroll.Visible = false;

                Label lblAddNewkeyValue = new Label();
                lblAddNewkeyValue.Text = "Add new key and value:";
                lblAddNewkeyValue.Location = new Point(0);
                lblAddNewkeyValue.Padding = new Padding(5);
                lblAddNewkeyValue.Font = new Font("Arial", 15, FontStyle.Bold, GraphicsUnit.Pixel);
                lblAddNewkeyValue.AutoSize = true;
                frmAddNewKey.Controls.Add(lblAddNewkeyValue);

                TextBox txtNewKeyName = new TextBox();
                txtNewKeyName.Location = new Point(lblAddNewkeyValue.Location.X + 10, lblAddNewkeyValue.Location.Y + lblAddNewkeyValue.Height);
                txtNewKeyName.Width = frmAddNewKey.ClientRectangle.Width - 100;
                txtNewKeyName.Text = "[key name]";
                frmAddNewKey.Controls.Add(txtNewKeyName);

                RichTextBox rtbNewKeyValue = new RichTextBox();
                rtbNewKeyValue.Location = new Point(txtNewKeyName.Location.X, txtNewKeyName.Location.Y + txtNewKeyName.Height + 10);
                rtbNewKeyValue.Size = new Size(txtNewKeyName.Width, 300);
                rtbNewKeyValue.Text = "[key value]";
                frmAddNewKey.Controls.Add(rtbNewKeyValue);

                Button btnAddNewkeyValue = new Button();
                btnAddNewkeyValue.Location = new Point(rtbNewKeyValue.Location.X, rtbNewKeyValue.Location.Y + rtbNewKeyValue.Height + 10);
                btnAddNewkeyValue.Text = "Add";
                btnAddNewkeyValue.Click += async(object sender, EventArgs e) => 
                {
                    string keyName = txtNewKeyName.Text;
                    string keyValue = rtbNewKeyValue.Text;

                    await TryToConnectAndRestoreRedisObjects();

                    await RedisDataAccess.RedisDb.StringSetAsync(keyName, keyValue);


                    await btnGetKeys_Click();

                    frmAddNewKey.Close();
                };
                frmAddNewKey.Controls.Add(btnAddNewkeyValue);
                frmAddNewKey.Show();
            };


            RedisDataAccess.CallBackEvent += (GlobalEventArgs messageCarrier) => 
            {                
                if (messageCarrier.clearDisplay)
                    lblInfo.Text = messageCarrier.Message;
                else
                    lblInfo.Text += messageCarrier.Message;
            };

     
        }




        private async Task TryToConnectAndRestoreRedisObjects()
        {
            if (RedisDataAccess.RedisDb == null)
            {
                string pathToRedisServer = providePathToRedisServer("Please provide path to \"redis-server.exe\" file");
                if (pathToRedisServer != null)
                    await RedisDataAccess.TryToConnect(pathToRedisServer);
            }
        }

        private async Task btnGetKeys_Click()
        {


            string pathToRedisServer = providePathToRedisServer("Please provide path to redis server file on your computer");
            if (pathToRedisServer != null)
            {
                pnlLnkKeys.Controls.Clear();

                string lnkKeysInitialText = "Redis keys:\n\n";
                Label lblPnlLnkKeysInitialText = new Label();
                lblPnlLnkKeysInitialText.Location = new Point(0);
                lblPnlLnkKeysInitialText.Text = lnkKeysInitialText;
                pnlLnkKeys.Controls.Add(lblPnlLnkKeysInitialText);
                int lnkKeyLicationY = 0;
                
                await foreach (RedisKey s in RedisDataAccess.GetKeys(pathToRedisServer))
                {
                    LinkLabel lnkKey = new LinkLabel();
                    lnkKey.AutoSize = true;
                    lnkKeyLicationY += lnkKey.Height;
                    lnkKey.Location = new Point(lblPnlLnkKeysInitialText.Location.X, lblPnlLnkKeysInitialText.Location.Y + lnkKeyLicationY + 1);
                    lnkKey.Text += s.ToString();

                    Link link = new Link(0, s.ToString().Length, s);                    
                    lnkKey.Links.Add(link);
                    lnkKey.LinkArea = new LinkArea(0, s.ToString().Length);
                    pnlLnkKeys.Controls.Add(lnkKey);
                    if(!_redisKeys.ContainsKey(s.ToString()))
                        _redisKeys.Add(s.ToString(), link);

                    lnkKey.LinkClicked += (object sender, LinkLabelLinkClickedEventArgs e) =>
                    {
                        _nonBlockinglyPastingTextToControlThread?.Abort();
                        _lblDisplayValue.Text = "";

                        _lblDisplayValue.Text = string.Empty;

                        string dictKey = (e.Link.GetType().GetProperty("Owner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(e.Link) as LinkLabel).Text;

                        if (_redisKeys.ContainsKey(dictKey))
                            _redisKeys[dictKey].Visited = true;

                        _currentRedisKey = (RedisKey)_redisKeys[dictKey].LinkData;
                        _lblCurrentKey.Text = _currentRedisKey;
                    };

                    
                }
                


            }
            else
                MessageBox.Show("There is no path to Redis server");
            
        }

        private string VerifyRedisServerPath()
        {
            if (File.Exists("_pathToRedisServer.txt"))
            {
                string[] supposedPath = File.ReadAllLines("_pathToRedisServer.txt", Encoding.Default);
                if (File.Exists(supposedPath[0]) && supposedPath[1].Equals("redis-server.exe"))
                    return supposedPath[0];
            }
            return null;
        }
        
        private bool flag_providePathToRedisServer = false;
        private string providePathToRedisServer(string titleMessage)
        {
            string supposedPath = VerifyRedisServerPath();
            if (supposedPath != null)
                return supposedPath;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = titleMessage;
            dialog.Multiselect = false;

            DialogResult rez = dialog.ShowDialog();

            File.WriteAllLines("_pathToRedisServer.txt", new string[] { dialog.FileName, dialog.SafeFileName }, Encoding.Default);
            supposedPath = VerifyRedisServerPath();
            if (supposedPath == null)
            {
                while (supposedPath == null && rez == DialogResult.OK)
                {
                    if (flag_providePathToRedisServer)
                        break;
                    supposedPath = providePathToRedisServer("The file you're just selected isn't Redis Server file.\nThe file name must be \"redis-server.exe\"!\nPlease tryanother time!");
                }
            }

            dialog.Dispose();
            flag_providePathToRedisServer = true;
            return supposedPath;
        }

        private async void StartRedis(object sender, EventArgs e)
        {
            string pathToRedisServer = providePathToRedisServer("Please provide the path to your Redis server file");



            Task<bool> isRedisConnectedTask = RedisDataAccess.TryToConnect(pathToRedisServer);

            lblInfo.Text = "Redis not started. Please wait\n";
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += (object sender, EventArgs e) =>
            {
                lblInfo.Text += ".";
            };
            timer.Start();
            _isRedisConnected = await isRedisConnectedTask;
            timer.Stop();
            if (_isRedisConnected)
            {
                lblInfo.Text = "Redis is started";
            }
            else lblInfo.Text = "Something went wrong, redis isn't started";
        }



        private void PassTextToControlNonBlockingly(string text, Control control)
        {
            _nonBlockinglyPastingTextToControlThread = new Thread(() =>
            {
                foreach (char s in text)
                {
                    Thread.Sleep(10);

                    Action act = () => { control.Text += s; };
                    SafeInvoke(act, control);
                }
            });
            _nonBlockinglyPastingTextToControlThread.IsBackground = true;
            _nonBlockinglyPastingTextToControlThread.Start();
        }
        private void SafeInvoke(Action work, Control control)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(work);
            }
            else work();
        }

        private void DisplayAwaiting(Task task, Label label)
        {
            label.Text = "";
            ContentAlignment textAlign = label.TextAlign;
            Font font = label.Font;
            Padding padding = label.Padding;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            label.Padding = new Padding(10);
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            int count = 0;
            timer.Tick += (object sender, EventArgs e) =>
            {
                if (task.IsCompleted)
                {
                    timer.Stop();
                    label.TextAlign = textAlign;
                    label.Padding = padding;
                    label.Font = font;
                }

                label.Text = count.ToString();
                count++;

            };
            timer.Start();
        }
    }
}
