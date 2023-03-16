namespace MaterialSkin2DotNet.Controls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    public sealed class MaterialDataTable : DataGridView, IMaterialControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        private bool showScrollBar { get; set; }

        [Browsable(true)]
        [DefaultValue(true)]
        [Category("MaterialSkin2Dotnet")]
        public bool ShowVerticalScroll
        {
            get => showScrollBar;
            set
            {
                showScrollBar = value;
                scrollBar.Visible = value;
                Invalidate();
            }
        }

        private bool showColorStripping { get; set; }

        [Browsable(true)]
        [DefaultValue(false)]
        [Category("MaterialSkin2Dotnet")]
        public bool ShowColorStripping
        {
            get => showColorStripping;
            set
            {
                showColorStripping = value;
                if (value == true)
                {
                    for (int i = 0; i < Rows.Count - 1; i++)
                    {
                        if (i % 2 != 0)
                        {
                            Rows[i].DefaultCellStyle.BackColor = ((int)colorStripColor).ToColor(); ;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Rows.Count - 1; i++)
                    {
                        if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
                        {
                            Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255, 255);
                        }
                        else
                        {
                            Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
                        }
                    }
                }
                NotifyPropertyChanged("showColorStripping");
                Invalidate();
            }
        }

        private Primary colorStripColor { get; set; }

        [Browsable(true)]
        [Category("MaterialSkin2Dotnet")]
        public Primary ColorStripColor
        {
            get => colorStripColor;
            set
            {
                colorStripColor = value;
                if (showColorStripping)
                {
                    for (int i = 0; i < Rows.Count - 1; i++)
                    {
                        if (i % 2 != 0)
                        {
                            Rows[i].DefaultCellStyle.BackColor = ((int)value).ToColor();
                        }
                    }
                }
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("MaterialSkin2Dotnet")]
        private Primary cellSelectionPrimaryColor { get; set; }

        [Browsable(true)]
        [Category("MaterialSkin2Dotnet")]
        public Primary CellSelectionPrimaryColor
        {
            get => cellSelectionPrimaryColor;
            set
            {
                cellSelectionPrimaryColor = value;
                DefaultCellStyle.SelectionBackColor = ((int)value).ToColor();
            }
        }

        //private const int PAD = 16;
        private bool _drawShadows;

        private bool _shadowDrawEventSubscribed = false;

        public MaterialDataTable()
        {
            DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DefaultCellStyle.SelectionBackColor = SkinManager.ColorScheme.PrimaryColor;
            ColumnHeadersDefaultCellStyle.BackColor = SkinManager.BackgroundColor;
            ColumnHeadersDefaultCellStyle.ForeColor = SkinManager.TextHighEmphasisColor;
            GridColor = Color.FromArgb(239, 239, 239);
            RowsDefaultCellStyle.BackColor = SkinManager.BackgroundColor;
            RowsDefaultCellStyle.ForeColor = SkinManager.TextHighEmphasisColor;
            ColumnHeadersDefaultCellStyle.Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Subtitle2);
            RowHeadersDefaultCellStyle.Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Body1);
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Raised;
            BackgroundColor = Color.FromArgb(255, 255, 255, 255);

            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            BorderStyle = BorderStyle.None;
            CellBorderStyle = DataGridViewCellBorderStyle.SunkenHorizontal;
            RowHeadersVisible = false;
            EnableHeadersVisualStyles = false;

            // User Restrictions
            AllowUserToDeleteRows = false;
            AllowUserToOrderColumns = false;
            AllowUserToResizeRows = false;

            ColumnHeadersHeight = 56;
            RowTemplate.Height = 52;
            AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
            ScrollBars = ScrollBars.None;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackgroundColor = SkinManager.BackgroundColor;
            ForeColor = SkinManager.TextHighEmphasisColor;

            // Padding = new Padding(24, 64, 24, 16);
            //  Margin = new Padding(3, 16, 3, 16);

            CellMouseMove += MaterialDataTable_MouseMove;
            CellMouseLeave += MaterialDataTable_CellMouseLeave;

            SkinManager.ThemeChanged += sender =>
            {
                ThemeChanged();
            };

            SkinManager.ColorSchemeChanged += sender =>
            {
                ColorSchemeChanged();
            };

            scrollBar.Height = Height;
            Controls.Add(scrollBar);
            scrollBar.Location = new Point((Location.X) + 2, Location.Y + Width + 20);
            scrollBar.Dock = DockStyle.Right;
            scrollBar.ValueChanged += ScrollBar_ValueChanged;

            //Pagination
            pagingPanel.Visible = false;
            pagingPanel.Dock = DockStyle.Bottom;
            pagingPanel.Parent = this;
            pagingPanel.BackColor = Color.Green;
            pagingPanel.Margin = new Padding(9);
            pagingPanel.Height = 50;
            MaterialFloatingActionButton btn = new MaterialFloatingActionButton();
            btn.Parent = pagingPanel;
            btn.Dock = DockStyle.Right;
            pagingPanel.Controls.Add(btn);
            Controls.Add(pagingPanel);
        }

        private MaterialScrollBar scrollBar = new MaterialScrollBar();
        private Panel pagingPanel = new Panel();

        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);
            if (DataSource != null)
            {
                if (showScrollBar == true)
                    scrollBar.Value = e.NewValue;
            }
        }

        private void ScrollBar_ValueChanged(object sender, int newValue)
        {
            // Rows.Count > 0 will take care of the System.ArgumentOutOfRange Exception that is thrown when the scrollbar is moved
            // Credit for finding and reporting this Exception goes to @Dr33v3s 👍
            if (Rows.Count > 0)
            {
                scrollBar.Maximum = Rows.Count - 1;
                FirstDisplayedScrollingRowIndex = newValue;
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (Height <= Rows.Count * RowTemplate.Height)
            {
                scrollBar.Visible = true;
            }
            else
            {
                scrollBar.Visible = false;
            }
        }

        private bool IsMouseOver;

        private void MaterialDataTable_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (showColorStripping != true)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
                {
                    Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255, 255);//SkinManager.BackgroundColor;
                }
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && SkinManager.Theme == MaterialSkinManager.Themes.DARK)
                {
                    Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);//SkinManager.BackgroundColor;
                }
            }
            IsMouseOver = false;
        }

        private void MaterialDataTable_MouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            IsMouseOver = true;
            if (showColorStripping != true)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
                {
                    Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                }
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && SkinManager.Theme == MaterialSkinManager.Themes.DARK)
                {
                    Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(95, 95, 95);
                }
            }
        }

        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            base.OnRowsAdded(e);
            if (showColorStripping)
            {
                for (int i = 0; i < Rows.Count - 1; i++)
                {
                    if (i % 2 != 0)
                    {
                        Rows[i].DefaultCellStyle.BackColor = ((int)colorStripColor).ToColor();
                        var brightness = Rows[i].DefaultCellStyle.BackColor.GetBrightness();
                        if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
                        {
                            if (brightness < 0.5)
                            {
                                Rows[i].DefaultCellStyle.ForeColor = SkinManager.BackgroundColor;
                            }
                            else
                            {
                                Rows[i].DefaultCellStyle.ForeColor = SkinManager.TextHighEmphasisColor;
                            }
                        }
                        if (SkinManager.Theme == MaterialSkinManager.Themes.DARK)
                        {
                            if (brightness < 0.5)
                            {
                                Rows[i].DefaultCellStyle.ForeColor = SkinManager.TextHighEmphasisColor;
                            }
                            else
                            {
                                Rows[i].DefaultCellStyle.ForeColor = SkinManager.BackgroundColor;
                            }
                        }
                    }
                }
            }
            if (SkinManager.Theme == MaterialSkinManager.Themes.DARK && RowsDefaultCellStyle.BackColor != Color.FromArgb(80, 80, 80))
                RowsDefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
            if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT && RowsDefaultCellStyle.BackColor != Color.White)
                RowsDefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255, 255);

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta > 0)
            {
                scrollBar.Value--;
            }
            else
            {
                scrollBar.Value++;
            }
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            base.OnDataSourceChanged(e);
            scrollBar.Maximum = RowCount - 1;
        }

        private void ColorSchemeChanged()
        {
            DefaultCellStyle.SelectionBackColor =
                SkinManager.ColorScheme.PrimaryColor;
        }

        private void ThemeChanged()
        {
            if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
            {
                RowsDefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255, 255);
                RowsDefaultCellStyle.ForeColor = Color.FromArgb(80, 80, 80);
                ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255, 255);
                ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(80, 80, 80);
                GridColor = SkinManager.BackgroundColor;
                BackgroundColor = Color.FromArgb(255, 255, 255, 255);

                Invalidate();
            }
            if (SkinManager.Theme == MaterialSkinManager.Themes.DARK)
            {
                RowsDefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
                RowsDefaultCellStyle.ForeColor = Color.FromArgb(255, 255, 255, 255);
                ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
                ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(255, 255, 255, 255);
                GridColor = Color.FromArgb(80, 80, 80);
                BackgroundColor = Color.FromArgb(80, 80, 80);

                #region RowsCells

                ColorStriper();

                #endregion RowsCells

                Update();
                UpdateStyles();
                Invalidate();
            }
            else
            {
                UpdateStyles();
                Invalidate();
            }
        }

        protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
        {
            base.OnColumnHeaderMouseClick(e);
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < Rows.Count - 1; i++)
                {
                    if (Rows[i].DefaultCellStyle.BackColor == ((int)colorStripColor).ToColor())
                    {
                        if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
                        {
                            Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255, 255);
                        }
                        else
                        {
                            Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
                        }
                    }
                }
                if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
                {
                    for (int i = 0; i < Rows.Count - 1; i++)
                    {
                        if (i % 2 != 0)
                        {
                            Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255, 255);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Rows.Count - 1; i++)
                    {
                        if (i % 2 != 0)
                        {
                            Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
                        }
                    }
                }
                for (int i = 0; i < Rows.Count - 1; i++)
                {
                    if (i % 2 != 0)
                    {
                        Rows[i].DefaultCellStyle.BackColor = ((int)colorStripColor).ToColor();
                    }
                }
            }
        }

        protected override void OnColumnDisplayIndexChanged(DataGridViewColumnEventArgs e)
        {
            base.OnColumnDisplayIndexChanged(e);
            if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
            {
                for (int i = 0; i < Rows.Count - 1; i++)
                {
                    if (i % 2 != 0)
                    {
                        Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255, 255);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Rows.Count - 1; i++)
                {
                    if (i % 2 != 0)
                    {
                        Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
                    }
                }
            }
            for (int i = 0; i < Rows.Count - 1; i++)
            {
                if (i % 2 != 0)
                {
                    Rows[i].DefaultCellStyle.BackColor = ((int)colorStripColor).ToColor();
                }
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (BackgroundColor != Color.FromArgb(255, 255, 255, 255))
            {
                BackgroundColor = Color.FromArgb(255, 255, 255, 255);
            }
            Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Body1);
            if (ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.EnableResizing || ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.AutoSize)
            {
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            }

            if (ColumnHeadersDefaultCellStyle.Font != SkinManager.getFontByType(MaterialSkinManager.fontType.Subtitle2))
            {
                ColumnHeadersDefaultCellStyle.Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Subtitle2);
            }

            if (RowHeadersDefaultCellStyle.Font != SkinManager.getFontByType(MaterialSkinManager.fontType.Body1
                ))
            {
                RowHeadersDefaultCellStyle.Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Body1);
            }
            if (ColumnHeadersHeight != 56)
            {
                ColumnHeadersHeight = 56;
            }
            foreach (var row in Rows)
            {
                RowsDefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
            }
            if (Height <= Rows.Count * RowTemplate.Height)
            {
                scrollBar.Visible = true;
            }
            else
            {
                scrollBar.Visible = false;
            }
            if (ShowColorStripping)
            {
                ColorStriper();
            }
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            base.OnColumnAdded(e);
            if (ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.EnableResizing || ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.AutoSize)
            {
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            }
            if (ColumnHeadersHeight != 56)
            {
                ColumnHeadersHeight = 56;
            }
        }

        protected override void InitLayout()
        {
            LocationChanged += (sender, e) =>
            {
                Parent?.Invalidate();
            };
            ForeColor = SkinManager.TextHighEmphasisColor;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null) AddShadowPaintEvent(Parent, drawShadowOnParent);
            if (_oldParent != null) RemoveShadowPaintEvent(_oldParent, drawShadowOnParent);
            _oldParent = Parent;
        }

        private Control _oldParent;

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Parent == null) return;
            if (Visible)
                AddShadowPaintEvent(Parent, drawShadowOnParent);
            else
                RemoveShadowPaintEvent(Parent, drawShadowOnParent);
        }

        private void drawShadowOnParent(object sender, PaintEventArgs e)
        {
            if (Parent == null)
            {
                RemoveShadowPaintEvent((Control)sender, drawShadowOnParent);
                return;
            }

            if (!_drawShadows || Parent == null) return;

            // paint shadow on parent
            Graphics gp = e.Graphics;
            Rectangle rect = new Rectangle(Location, ClientRectangle.Size);
            gp.SmoothingMode = SmoothingMode.AntiAlias;
            DrawHelper.DrawSquareShadow(gp, rect);
        }

        private void AddShadowPaintEvent(Control control, PaintEventHandler shadowPaintEvent)
        {
            if (_shadowDrawEventSubscribed) return;
            control.Paint += shadowPaintEvent;
            control.Invalidate();
            _shadowDrawEventSubscribed = true;
        }

        private void RemoveShadowPaintEvent(Control control, PaintEventHandler shadowPaintEvent)
        {
            if (!_shadowDrawEventSubscribed) return;
            control.Paint -= shadowPaintEvent;
            control.Invalidate();
            _shadowDrawEventSubscribed = false;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            BackColor = SkinManager.BackgroundColor;
            if (!showColorStripping)
            {
                for (int i = 0; i < Rows.Count - 1; i++)
                {
                    if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
                        Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255, 255);
                    else
                        Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(80, 80, 80);
                }
            }
        }

        public void ColorStriper()
        {
            if (showColorStripping)
            {
                for (int i = 0; i < Rows.Count - 1; i++)
                {
                    if (i % 2 != 0)
                    {
                        Rows[i].DefaultCellStyle.BackColor = ((int)colorStripColor).ToColor();
                        float brightness = Rows[i].DefaultCellStyle.BackColor.GetBrightness();
                        if (SkinManager.Theme == MaterialSkinManager.Themes.LIGHT)
                        {
                            if (brightness <= 0.5f)
                            {
                                Rows[i].DefaultCellStyle.ForeColor = Color.FromArgb(80, 80, 80);
                            }
                            else
                            {
                                Rows[i].DefaultCellStyle.ForeColor = Color.FromArgb(255, 255, 255, 255);
                            }
                        }
                        if (SkinManager.Theme == MaterialSkinManager.Themes.DARK)
                        {
                            if (brightness <= 0.5f)
                            {
                                Rows[i].DefaultCellStyle.ForeColor = Color.FromArgb(255, 255, 255, 255);
                            }
                            else
                            {
                                Rows[i].DefaultCellStyle.ForeColor = Color.FromArgb(80, 80, 80);
                            }
                        }
                    }
                }
            }
        }

        //Pagination

        /*        Paging pg;
                SQLQuery s;
                public void SetPagedDataSource(SQLQuery s, BindingNavigator bnav)
                {
                    this.s = s;
                    int count = DataPovi.ExecuteCount(s.CountQuery);
                    pg = new Paging(count, 5);
                    bnav.BindingSource = pg.BindingSource;
                    pg.BindingSource.PositionChanged += new EventHandler(bs_PositionChanged);
                    //first page
                    string q = s.GetPagingQuery(pg.GetStartRowNum(1), pg.GetEndRowNum(1), true);
                    DataTable dt = DataProvider.ExecuteDt(q);
                    DataSource = dt;
                }

                void bs_PositionChanged(object sender, EventArgs e)
                {
                    int pos = ((BindingSource)sender).Position + 1;
                    string q = s.GetPagingQuery(pg.GetStartRowNum(pos), pg.GetEndRowNum(pos), false);
                    DataTable dt = DataProvider.ExecuteDt(q);
                    DataSource = dt;
                }

                public void UpdateData()
                {
                    DataTable dt = (DataTable)DataSource;
                    using (SqlConnection con = new SqlConnection(DataProvider.conStr))
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(s.CompleteQuery, con);
                        SqlCommandBuilder cb = new SqlCommandBuilder(da);
                        da.UpdateCommand = cb.GetUpdateCommand();
                        da.InsertCommand = cb.GetInsertCommand();
                        da.DeleteCommand = cb.GetDeleteCommand();
                        da.Update(dt);
                    }
                    MessageBox.Show("The changes are committed to database!");
                }
            }

            /// <summary>
            /// Gives functionality of next page , etc for paging.
            /// </summary>
            public class Paging
            {
                public int _totalSize = 0;
                private int _pageSize = 0;

                public int TotalSize
                {
                    get
                    {
                        return _totalSize;
                    }
                    set
                    {
                        if (value <= 0)
                        {
                            throw new ArgumentException();
                        }
                        _totalSize = value;
                    }
                }

                public int PageSize
                {
                    get
                    {
                        return _pageSize;
                    }
                    set
                    {
                        if (value <= 0)
                        {
                            throw new ArgumentException();
                        }
                        _pageSize = value;
                    }
                }

                public Paging(int totalSize, int pageSize)
                {
                    this.TotalSize = totalSize;
                    this.PageSize = pageSize;
                }

                public int GetStartRowNum(int PageNum)
                {
                    if (PageNum < 1)
                    {
                        throw new Exception("Page number starts at 1");
                    }
                    if (PageNum > GetPageCount())
                    {
                        throw new Exception("Page number starts at " + GetPageCount().ToString());
                    }
                    return 1 + ((PageNum - 1) * _pageSize);
                }

                public int GetEndRowNum(int PageNum)
                {
                    if (PageNum < 1)
                    {
                        throw new Exception("Page number starts at 1");
                    }
                    if (PageNum > GetPageCount())
                    {
                        throw new Exception("Page number starts at " + GetPageCount().ToString());
                    }
                    return _pageSize + ((PageNum - 1) * _pageSize);
                }

                public int GetPageCount()
                {
                    return (int)Math.Ceiling(TotalSize / (decimal)PageSize);
                }

                public bool IsFirstPage(int PageNum)
                {
                    if (PageNum == 1)
                    {
                        return true;
                    }
                    return false;
                }

                public bool IsLastPage(int PageNum)
                {
                    if (PageNum == GetPageCount())
                    {
                        return true;
                    }
                    return false;
                }
                private int _currentPage = 1;
                public int CurrentPage
                {
                    get
                    {
                        return _currentPage;
                    }
                    set
                    {
                        _currentPage = value;
                    }
                }
                public int NextPage
                {
                    get
                    {
                        if (CurrentPage + 1 <= GetPageCount())
                        {
                            _currentPage = _currentPage + 1;
                        }
                        return _currentPage;
                    }
                }

                public int PreviousPage
                {
                    get
                    {
                        if (_currentPage - 1 >= 1)
                        {
                            _currentPage = _currentPage - 1;
                        }
                        return _currentPage;
                    }
                }
                private BindingSource _bindingSource = null;
                public BindingSource BindingSource
                {
                    get
                    {
                        if (_bindingSource == null)
                        {
                            _bindingSource = new BindingSource();
                            List<int> test = new List<int>();
                            for (int i = 0; i < GetPageCount(); i++)
                            {
                                test.Add(i);
                            }
                            _bindingSource.DataSource = test;
                        }
                        return _bindingSource;
                    }
                }
            }

            /// <summary>
            /// Query Helper of Paging
            /// </summary>
            public class SQLQuery
            {
                private string IDColumn = "";
                private string WherePart = " 1=1 ";
                private string FromPart = "";
                private string SelectPart = "";

                public SQLQuery(string SelectPart, string FromPart, string WherePart, string IDColumn)
                {
                    this.IDColumn = IDColumn;
                    this.WherePart = WherePart;
                    this.FromPart = FromPart;
                    this.SelectPart = SelectPart;
                }

                public string CompleteQuery
                {
                    get
                    {
                        if (WherePart.Trim().Length > 0)
                        {
                            return string.Format("Select {0} from {1} where {2} ", SelectPart, FromPart, WherePart);
                        }
                        else
                        {
                            return string.Format("Select {0} from {1} ", SelectPart, FromPart);
                        }
                    }
                }

                public string CountQuery
                {
                    get
                    {
                        if (WherePart.Trim().Length > 0)
                        {
                            return string.Format("Select count(*) from {0} where {1} ", FromPart, WherePart);
                        }
                        else
                        {
                            return string.Format("Select count(*) from {0} ", FromPart);
                        }
                    }
                }

                public string GetPagingQuery(int fromrow, int torow, bool isSerial)
                {
                    fromrow--;
                    if (isSerial)
                    {
                        return string.Format("{0} where {1} >= {2} and {1} <= {3}", CompleteQuery, IDColumn, fromrow, torow);
                    }
                    else
                    {
                        string select1 = "";
                        string select2 = "";
                        if (WherePart.Trim().Length > 0)
                        {
                            select1 = string.Format("Select top {3} {0} from {1} where {2} ", SelectPart, FromPart, WherePart, torow.ToString());
                            select2 = string.Format("Select top {3} {0} from {1} where {2} ", SelectPart, FromPart, WherePart, fromrow.ToString());
                        }
                        else
                        {
                            select1 = string.Format("Select top {2} {0} from {1} ", SelectPart, FromPart, torow.ToString());
                            select2 = string.Format("Select top {2} {0} from {1} ", SelectPart, FromPart, fromrow.ToString());
                        }
                        if (fromrow <= 1)
                        {
                            return select1;
                        }
                        else
                        {
                            return string.Format("{0} except {1} ", select1, select2);
                        }
                    }
                }
            }
        */
    }
}