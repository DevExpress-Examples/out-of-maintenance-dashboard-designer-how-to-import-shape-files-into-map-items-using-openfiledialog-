using DevExpress.DashboardCommon;
using DevExpress.DashboardWin;
using DevExpress.DashboardWin.Bars;
using DevExpress.XtraBars;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace T566254 {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            dashboardDesigner1.CreateRibbon();
            dashboardDesigner1.Dashboard.Items.Add(new GeoPointMapDashboardItem());
        }

        private void dashboardDesigner1_PopupMenuShowing(object sender, DashboardPopupMenuShowingEventArgs e) {
            var designer = (DashboardDesigner)sender;
            var mapItem = designer.SelectedDashboardItem as MapDashboardItem;

            //Load shapes using File Path...
            if (e.DashboardItemArea == DashboardItemArea.DashboardItem && mapItem != null) {
                for (int i = e.Menu.ItemLinks.Count - 1; i >= 0; i--) {
                    if (e.Menu.ItemLinks[i].Item is MapImportBarItem || e.Menu.ItemLinks[i].Item is MapLoadBarItem)
                        e.Menu.ItemLinks.RemoveAt(i);
                }
                BarButtonItem bbi1 = new BarButtonItem() { Caption = "Load Custom Map", Name = "mapCustomShapeFile" };
                bbi1.ItemClick += (s, args) => {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.InitialDirectory = Path.Combine(Application.StartupPath, "ShapeFiles");
                    dialog.Filter = "Shape files|*.shp";
                    if (dialog.ShowDialog() == DialogResult.OK) {
                        try {
                            mapItem.CustomShapefile.Url = dialog.FileName;
                            mapItem.Area = ShapefileArea.Custom;
                        } catch (Exception ex) {
                            MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                        }
                    }
                };

                //Load shapes using Stream...
                BarButtonItem bbi2 = new BarButtonItem() { Caption = "Load Custom Map (Stream approach)", Name = "mapCustomShapeFile" };
                bbi2.ItemClick += (s, args) => {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.InitialDirectory = Path.Combine(Application.StartupPath, "ShapeFiles");
                    dialog.Filter = "Shape files|*.shp";
                    if (dialog.ShowDialog() == DialogResult.OK) {
                        try {
                           var fileName = Path.Combine(System.IO.Path.GetDirectoryName(dialog.FileName), Path.GetFileNameWithoutExtension(dialog.FileName));
                            byte[] shapeData = File.ReadAllBytes(string.Format("{0}.{1}", fileName, "shp"));
                            byte[] attributeData = File.ReadAllBytes(string.Format("{0}.{1}", fileName, "dbf"));
                            mapItem.CustomShapefile.Data = new CustomShapefileData(shapeData, attributeData);
                            mapItem.Area = ShapefileArea.Custom;
                        } catch (Exception ex) {
                            MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                        }
                    }
                };

                if (e.Menu.ItemLinks.OfType<BarButtonItemLink>().Where(i => i.Item.Name == "mapCustomShapeFile").Count() == 0) {
                    var mapDefaultShapefile = e.Menu.ItemLinks.FirstOrDefault(l => l.Item is MapFullExtentBarItem);
                    e.Menu.ItemLinks.Insert(mapDefaultShapefile, bbi1);
                    e.Menu.ItemLinks.Insert(mapDefaultShapefile, bbi2);
                }
            }
        }
    }
}
