using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.SoundPropagationMode
{
    public partial class SoundEnvironmentPanel : UserControl
    {
        public event EventHandler OnShowWarningsOnlyChanged; //mxd
        public BufferedTreeView SoundEnvironments { get; set; }
        private readonly int warningiconindex; //mxd
        private bool treeisupdating; //mxd
        private int nodewarningscount; //mxd
        private const string shownodewarningstext = "Show nodes with warnings only (N)";
        private delegate void UpdateCallback(); //mxd

        public SoundEnvironmentPanel()
        {
            InitializeComponent();

            SoundEnvironments.ImageList = new ImageList();
            foreach (Bitmap icon in BuilderPlug.Me.DistinctIcons)
            {
                SoundEnvironments.ImageList.Images.Add(icon);
            }
            SoundEnvironments.ImageList.Images.Add(Properties.Resources.Warning);
            warningiconindex = SoundEnvironments.ImageList.Images.Count - 1; //mxd
        }

        //mxd
        public void BeginUpdate()
        {
            if (treeisupdating) return;

            if (SoundEnvironments.InvokeRequired)
            {
                UpdateCallback d = BeginUpdate;
                SoundEnvironments.Invoke(d);
            }
            else
            {
                SoundEnvironments.BeginUpdate();
                nodewarningscount = 0;
            }
        }

        //mxd
        public void EndUpdate()
        {
            if (SoundEnvironments.InvokeRequired)
            {
                UpdateCallback d = EndUpdate;
                SoundEnvironments.Invoke(d);
            }
            else
            {
                treeisupdating = false;
                SoundEnvironments.Sort();
                SoundEnvironments.EndUpdate();

                // Update the checkbox
                showwarningsonly.Text = shownodewarningstext.Replace("(N)", "(" + nodewarningscount + ")");
                showwarningsonly.Enabled = showwarningsonly.Checked || nodewarningscount > 0;
            }
        }

        public void AddSoundEnvironment(SoundEnvironment se)
        {
            TreeNode topnode = new TreeNode(se.Name);
            topnode.Tag = se; //mxd
            TreeNode thingsnode = new TreeNode("Things (" + se.Things.Count + ")");
            TreeNode linedefsnode = new TreeNode("Linedefs (" + se.Linedefs.Count + ")");
            int notdormant = 0;
            int iconindex = BuilderPlug.Me.DistinctColors.IndexOf(se.Color); //mxd
            int topindex = iconindex; //mxd
            bool nodehaswarnings = false; //mxd

            thingsnode.ImageIndex = iconindex; //mxd
            thingsnode.SelectedImageIndex = iconindex; //mxd
            linedefsnode.ImageIndex = iconindex; //mxd
            linedefsnode.SelectedImageIndex = iconindex; //mxd

            // Add things
            foreach (Thing t in se.Things)
            {
                TreeNode thingnode = new TreeNode("Thing " + t.Index);
                thingnode.Tag = t;
                thingnode.ImageIndex = iconindex; //mxd
                thingnode.SelectedImageIndex = iconindex; //mxd
                thingsnode.Nodes.Add(thingnode);

                if (!BuilderPlug.ThingDormant(t))
                {
                    notdormant++;
                }
                else
                {
                    thingnode.Text += " (dormant)";
                }
            }

            // Set the icon to warning sign and add the tooltip when there are more than 1 non-dormant things
            if (notdormant > 1)
            {
                thingsnode.ImageIndex = warningiconindex;
                thingsnode.SelectedImageIndex = warningiconindex;
                topindex = warningiconindex;

                foreach (TreeNode tn in thingsnode.Nodes)
                {
                    if (!BuilderPlug.ThingDormant((Thing)tn.Tag))
                    {
                        tn.ImageIndex = warningiconindex;
                        tn.SelectedImageIndex = warningiconindex;
                        tn.ToolTipText = "More than one thing in this\nsound environment is set to be\nactive. Set all but one thing\nto dormant.";
                        nodewarningscount++; //mxd
                        nodehaswarnings = true; //mxd
                    }
                }
            }

            // Add linedefs
            foreach (Linedef ld in se.Linedefs)
            {
                bool showwarning = false;
                TreeNode linedefnode = new TreeNode("Linedef " + ld.Index);
                linedefnode.Tag = ld;
                linedefnode.ImageIndex = iconindex; //mxd
                linedefnode.SelectedImageIndex = iconindex; //mxd

                if (ld.Back == null || ld.Front == null)
                {
                    showwarning = true;
                    linedefnode.ToolTipText = "This line is single-sided, but has\nthe sound boundary flag set.";
                }
                else if (se.Sectors.Contains(ld.Front.Sector) && se.Sectors.Contains(ld.Back.Sector))
                {
                    showwarning = true;
                    linedefnode.ToolTipText = "More than one thing in this\nThe sectors on both sides of\nthe line belong to the same\nsound environment.";
                }

                if (showwarning)
                {
                    linedefnode.ImageIndex = warningiconindex;
                    linedefnode.SelectedImageIndex = warningiconindex;

                    linedefsnode.ImageIndex = warningiconindex;
                    linedefsnode.SelectedImageIndex = warningiconindex;

                    topindex = warningiconindex;
                    nodewarningscount++; //mxd
                    nodehaswarnings = true; //mxd
                }

                linedefsnode.Nodes.Add(linedefnode);
            }

            //mxd
            if (!showwarningsonly.Checked || nodehaswarnings)
            {
                topnode.Nodes.Add(thingsnode);
                topnode.Nodes.Add(linedefsnode);

                topnode.Tag = se;
                topnode.ImageIndex = topindex;
                topnode.SelectedImageIndex = topindex;

                // Sound environments will no be added in consecutive order, so we'll have to find
                // out where in the tree to add the node
                int insertionplace = 0;

                foreach (TreeNode tn in SoundEnvironments.Nodes)
                {
                    if (se.ID < ((SoundEnvironment)tn.Tag).ID) break;
                    insertionplace++;
                }

                SoundEnvironments.Nodes.Insert(insertionplace, topnode);
            }
        }

        public void HighlightSoundEnvironment(SoundEnvironment se)
        {
            if (SoundEnvironments.SelectedNode != null) return; //mxd
            if (!treeisupdating) SoundEnvironments.BeginUpdate();
            SoundEnvironments.CollapseAll(); //mxd

            foreach (TreeNode tn in SoundEnvironments.Nodes)
            {
                if (se != null && ((SoundEnvironment)tn.Tag).ID == se.ID)
                {
                    if (tn.NodeFont == null || tn.NodeFont.Style != FontStyle.Bold)
                        tn.NodeFont = new Font(SoundEnvironments.Font.FontFamily, SoundEnvironments.Font.Size, FontStyle.Bold);

                    //mxd
                    tn.Expand();
                    if (SoundEnvironments.SelectedNode == null) tn.EnsureVisible();
                }
                else
                {
                    if (tn.NodeFont == null || tn.NodeFont.Style != FontStyle.Regular)
                        tn.NodeFont = new Font(SoundEnvironments.Font.FontFamily, SoundEnvironments.Font.Size);
                }
            }

            if (!treeisupdating) SoundEnvironments.EndUpdate();
        }

        //mxd. This (de)selects sound environment
        public void SelectSoundEnvironment(SoundEnvironment se)
        {
            if ((se == null && SoundEnvironments.SelectedNode != null) || (se != null && SoundEnvironments.SelectedNode != null && SoundEnvironments.SelectedNode.Tag is SoundEnvironment && ((SoundEnvironment)SoundEnvironments.SelectedNode.Tag).ID == se.ID))
            {
                SoundEnvironments.SelectedNode = null;
                return;
            }

            if (se == null) return;

            foreach (TreeNode tn in SoundEnvironments.Nodes)
            {
                if (((SoundEnvironment)tn.Tag).ID == se.ID)
                {
                    SoundEnvironments.SelectedNode = tn;
                    tn.EnsureVisible();
                    return;
                }
            }
        }

        private static bool IsClickOnText(TreeView treeView, TreeNode node, Point location)
        {
            var hitTest = treeView.HitTest(location);
            return hitTest.Node == node && (hitTest.Location == TreeViewHitTestLocations.Label || hitTest.Location == TreeViewHitTestLocations.Image);
        }

        private static void ProcessNodeClick(TreeNode node)
        {
            if (node == null) return;

            List<Vector2D> points = new List<Vector2D>();
            RectangleF area = MapSet.CreateEmptyArea();

            if (node.Parent == null)
            {
                if (node.Tag is SoundEnvironment)
                {
                    SoundEnvironment se = (SoundEnvironment)node.Tag;

                    foreach (Sector s in se.Sectors)
                    {
                        foreach (Sidedef sd in s.Sidedefs)
                        {
                            points.Add(sd.Line.Start.Position);
                            points.Add(sd.Line.End.Position);
                        }
                    }
                }
                else
                {
                    // Don't zoom if the wrong nodes are selected
                    return;
                }
            }
            else
            {
                if (node.Tag is Thing)
                {
                    Thing t = (Thing)node.Tag;

                    // We don't want to be zoomed too closely, so add somepadding
                    points.Add(t.Position - 200);
                    points.Add(t.Position + 200);
                }
                else if (node.Tag is Linedef)
                {
                    Linedef ld = (Linedef)node.Tag;

                    points.Add(ld.Start.Position);
                    points.Add(ld.End.Position);
                }
                else
                {
                    // Don't zoom if the wrong nodes are selected
                    return;
                }
            }

            area = MapSet.IncreaseArea(area, points);

            // Add padding
            area.Inflate(100f, 100f);

            // Zoom to area
            ClassicMode editmode = General.Editing.Mode as ClassicMode;
            editmode.CenterOnArea(area, 0.0f);
        }

        #region ================== Events

        private void soundenvironments_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (IsClickOnText(SoundEnvironments, e.Node, e.Location))
            {
                ProcessNodeClick(e.Node);
            }
        }

        private void soundenvironments_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Action != TreeViewAction.ByMouse) return;
            var position = SoundEnvironments.PointToClient(Cursor.Position);
            e.Cancel = !IsClickOnText(SoundEnvironments, e.Node, position);
        }

        //mxd
        private void showwarningsonly_CheckedChanged(object sender, EventArgs e)
        {
            if (OnShowWarningsOnlyChanged != null) OnShowWarningsOnlyChanged(this, EventArgs.Empty);
        }

        #endregion
    }
}
