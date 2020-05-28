using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Lab_informNo3
{
    public partial class Form1 : Form
    {
        private String currentFolder;
        public Color graphicsColor = Color.Crimson;
        public Color officeColor = Color.DodgerBlue;
        public Color archiveColor = Color.Gold;
        public Color executableColor = Color.MediumVioletRed;
        public long largeFileThreshold = 1000000;
        public long smallFileThreshold = 10000;
        public static string treeInText = String.Empty;
        public ulong totalSize = 0, totalFiles = 0, totalSelected = 0;
        public Form1()
        {
            InitializeComponent();
            this.listView1.Items.Clear();
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            this.treeView1.Nodes.Clear();
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\Users\hikuma\Documents\IR");
            SizeLastColumn(listView1);
            listView1.Columns[0].Width = listView1.Width * 5 / 10;
            listView1.Columns[1].Width = listView1.Width * 2 / 10;
            listView1.Columns[2].Width = listView1.Width * 3 / 10;
        }
        private void listView1_Resize(object sender, System.EventArgs e)
        {
            SizeLastColumn((ListView)sender);
        }
        private void SizeLastColumn(ListView lv)
        {
            lv.Columns[lv.Columns.Count - 1].Width = -2;
        }
        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                    GetDirectories(subSubDirs, aNode);
                nodeToAddTo.Nodes.Add(aNode);
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                currentFolder = folderBrowserDialog1.SelectedPath;
                listView1.Items.Clear();
                this.treeView1.Nodes.Clear();
                this.treeView1.Nodes.Add(CreateDirectoryNode(new DirectoryInfo(currentFolder)));
            }
        }
        private void BuildTree(DirectoryInfo directoryInfo, TreeNodeCollection addInMe)
        {
            TreeNode curNode = addInMe.Add(directoryInfo.Name);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                curNode.Nodes.Add(file.FullName, file.Name);
            }
            foreach (DirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                BuildTree(subdir, curNode.Nodes);
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                DefaultExt = ".txt",
                Filter = "Test files | *.txt*"
            };
            if (saveFile.ShowDialog() == DialogResult.OK && saveFile.FileName.Length > 0)
            {
                using (TextWriter tw = new StreamWriter(saveFile.FileName, false))
                {
                    foreach (ListViewItem item in listView1.Items)
                        tw.WriteLine(item.SubItems[0].Text + "\t|" + item.SubItems[1].Text + "\t|" + item.SubItems[2].Text);
                    MessageBox.Show("Your data has been succesfully exported.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void SaveClicked(object sender, EventArgs e)
        {
            if (this.saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                btnCreateTreeData(saveFileDialog1.FileName);
        }
        private void btnCreateTreeData(String filePath)
        {
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            foreach (TreeNode rootNode in treeView1.Nodes)
                BuildTreeString(rootNode, buffer);
            System.IO.File.WriteAllText(filePath, buffer.ToString());
        }
        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            int large = 0, medium = 0, small = 0;
            this.listView1.Items.Clear();
            string filepath = currentFolder.Substring(0, currentFolder.LastIndexOf('\\') + 1) + treeView1.SelectedNode.FullPath;
            string[] files = Directory.GetFiles(filepath);
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                string name = info.Name;
                string size = info.Length.ToString();
                totalSize += (ulong)info.Length;
                if (info.Length > largeFileThreshold)
                    large++;
                else if (info.Length < smallFileThreshold)
                    small++;
                else
                    medium++;
                string type = name.Substring(name.LastIndexOf('.'), name.Length - name.LastIndexOf('.'));
                Console.WriteLine(name + size + type);
                ListViewItem item = new ListViewItem(name);
                item.SubItems.Add(size);
                item.SubItems.Add(type);
                item.Checked = true;
                listView1.Items.Add(item);
            }
            List<ChartItem> items = new List<ChartItem>();
            items.Add(new ChartItem("Large", large));
            items.Add(new ChartItem("Medium", medium));
            items.Add(new ChartItem("Small", small));
            changeColors();
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.Columns[0].Width = listView1.Width * 5 / 10;
            listView1.Columns[1].Width = listView1.Width * 2 / 10;
            listView1.Columns[2].Width = listView1.Width * 3 / 10;
            SizeLastColumn(listView1);
            totalFiles = (ulong)listView1.Items.Count;
            drawCharts(items);
        }
        private void drawCharts(List<ChartItem> items)
        {
            chart1.Series[0].Points.Clear();
            chart1.Visible = true;
            List<int> values = new List<int>();
            foreach (var item in items)
            {
                chart1.Series[0].Points.AddXY(item.Name, item.Number);
                values.Add(item.Number);
            }
            chart1.ChartAreas[0].AxisY.Maximum = values.Max() + (int)(values.Max() / 20);
        }
        public void drawCheckedItems()
        {
            int large = 0, medium = 0, small = 0;
            List<string> filepaths = new List<string>();
            foreach (ListViewItem item in listView1.CheckedItems)
                filepaths.Add(currentFolder + "\\" + item.Text);
            string[] files = filepaths.ToArray();
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                string size = info.Length.ToString();
                if (info.Length > largeFileThreshold)
                    large++;
                else if (info.Length < smallFileThreshold)
                    small++;
                else
                    medium++;
            }
            List<ChartItem> items = new List<ChartItem>();
            items.Add(new ChartItem("Large", large));
            items.Add(new ChartItem("Medium", medium));
            items.Add(new ChartItem("Small", small));
            drawCharts(items);
        }
        public void changeColors()
        {
                foreach (ListViewItem item in listView1.Items)
                {
                    String type = item.SubItems[2].Text;
                    if (type == ".png" || type == ".jpg" || type == ".bmp" || type == ".gif")
                        item.BackColor = graphicsColor;
                    else if (type == ".docx" || type == ".xlsx" || type == ".pdf" || type == ".txt")
                        item.BackColor = officeColor;
                    else if (type == ".zip" || type == ".rar" || type == ".7z")
                        item.BackColor = archiveColor;
                    else if (type == ".exe" || type == ".dll")
                        item.BackColor = executableColor;
                }
        }
        private void changeColorsButLight()
        {
            float lowestColor = 0.85F;
            Color newGraphicsColor = graphicsColor;
            Color newOfficeColor = officeColor;
            Color newArchiveColor = archiveColor;
            Color newExecutableColor = executableColor;
            foreach (ListViewItem item in listView1.Items)
            {
                String type = item.SubItems[2].Text;
                if (type == ".png" || type == ".jpg" || type == ".bmp" || type == ".gif")
                    item.BackColor = newGraphicsColor;
                else if (type == ".docx" || type == ".xlsx" || type == ".pdf" || type == ".txt")
                    item.BackColor = newOfficeColor;
                else if (type == ".zip" || type == ".rar" || type == ".7z")
                    item.BackColor = newArchiveColor;
                else if (type == ".exe" || type == ".dll")
                    item.BackColor = newExecutableColor;
            }
        }
        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            toolStripStatusLabel2.Text = listView1.CheckedItems.Count + " of " + (listView1.Items.Count + " items selected");
            Dictionary<string, int> chartSource = new Dictionary<string, int>();
            int val = 0;
            string title;
            bool flag = false;
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedColumn;
            chart1.Series[0].Points.Clear();
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                flag = true;
                title = item.SubItems[2].Text;
                if (chartSource.TryGetValue(title, out val))
                {
                    if (int.Parse(item.SubItems[1].Text) > chartSource[title])
                        chartSource[title] = int.Parse(item.SubItems[1].Text);
                }
                else
                    chartSource.Add(title, int.Parse(item.SubItems[1].Text));

            }
            if (flag)
            {
                foreach (var r in chartSource)
                    chart1.Series[0].Points.AddXY(r.Key, r.Value);
            }
        }

        private void TableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void ToolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void TableLayoutPanel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void OpenFileDialog1_FileOk_1(object sender, CancelEventArgs e)
        {

        }

        private void ListView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
        private void BuildTreeString(TreeNode rootNode, System.Text.StringBuilder buffer)
        {
            buffer.Append(rootNode.Text);
            buffer.Append(Environment.NewLine);
            foreach (TreeNode childNode in rootNode.Nodes)
                BuildTreeString(childNode, buffer);
        }
        private static TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name);
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            return directoryNode;
        }
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Contact your support for help");
        }
        private void справкаToolStripButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Made by Zipchenko");
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        void AddDirectories(TreeNode node)
        {
            string path = node.FullPath;
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo[] arrDirInfo;
            try
            {
                arrDirInfo = dirInfo.GetDirectories();
            }
            catch
            {
                return;
            }
            foreach (DirectoryInfo dir in arrDirInfo)
            {
                TreeNode nodeDir = new TreeNode(dir.Name);
                node.Nodes.Add(nodeDir);
                AddDirectories(nodeDir);
            }
        }
        private void openToolStripMenuItem_click(object sender, System.EventArgs e)
        {

        }
    }
}
