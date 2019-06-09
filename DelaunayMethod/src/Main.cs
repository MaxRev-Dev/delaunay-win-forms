using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DelaunayMethod.Algorithm;
using DelaunayMethod.Algorithm.Interfaces;
using DelaunayMethod.PoissonDiscSampler;
using DelaunayMethod.Utils;
using DelaunayMethod.Utils.RazorPainterGDI;
using Point = DelaunayMethod.Algorithm.Models.Point;
using Timer = System.Threading.Timer;

namespace DelaunayMethod
{
    using GrTF = Func<Graphics, Task>;
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            InitializeGraphicsGPU();
            InitializeDefaults();
            InitializeMisc();
        }

        #region Extern Funcs
        [DllImport("user32")]
        internal static extern bool HideCaret(IntPtr hWnd);

        [DllImport("user32")]
        internal static extern bool ShowCaret(IntPtr hWnd);
        [DllImport("user32")]
        internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32", SetLastError = true)]
        internal static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, int lParam);
        #endregion

        #region Enums
        enum FillSpeed
        {
            Turtle = 200,
            Slow = 100,
            Fast = 30,
            Ultra = 0,
        }

        enum ColorSite
        {
            Points,
            Delanay_Mesh,
            Delanay_Cell_Fill,
            Voronoi_Mesh,
            Voronoi_Cell_Fill,
            Hull_Points,
            Hull_Edges
        }
        #endregion

        #region Vars

        private Dictionary<ColorSite, Color> _colorMap;
        readonly ImageResolutionConverter irc = new ImageResolutionConverter();
        private readonly object paintLock = new object();
        public int DefaultPointDist => 50;
        private CancellationToken _drawInlineCancellationToken;
        private Graphics graphics { get; set; }

        private RazorPainter _painter;
        private DelaunayEngine _delaunayEngine;
        private FillSpeed _selectedFillSpeed;
        private string importFileName;
        private CancellationTokenSource _resizeCancellationTokenSource;
        private CancellationTokenSource _drawCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource renderTCS = new CancellationTokenSource();
        private Bitmap _graphicsBmp;
        private HandleRef _hRefGraphics;
        private float _padding = 5;
        private bool _drag;
        private List<IPoint> _pointsHeap;
        private List<IPoint> _userPointHeap;
        private List<GeoPoint> _importCache;
        private readonly TaskRegistry<Graphics> _registry = new TaskRegistry<Graphics>();
        private const uint EM_SETREADONLY = 0x00CF;
        private bool _refreshContext;
        private bool _immediate;
        //scale & drag related - NotImplemented
        private IPoint _oldPoint;
        readonly System.Windows.Forms.Timer _dragDelayerRelay = new System.Windows.Forms.Timer();
        private System.Drawing.Point drag;
        private float factor = 0.2f;
        private float _scale = 1;
        private float _translateX;
        private float _translateY;
        #endregion Vars

        #region Initialization
        private void InitializeDefaults()
        {
            dPoints.Text = DefaultPointDist.ToString();

            GraphicsLocked(graphics, g =>
            {
                using (var br = new SolidBrush(Color.AntiqueWhite))
                    g.FillRegion(br, g.Clip);
            });

            _selectedFillSpeed = FillSpeed.Fast;
            speedBox.DataSource = Enum.GetNames(typeof(FillSpeed));
            speedBox.SelectedIndex = 0;
            _oldPoint = new Point(layer.Width, layer.Height);

            InitializeColorSite();
        }

        private void InitializeColorSite()
        {
            colorSelectorCB.SelectedIndexChanged -= MeshColor_SelectedIndexChanged;
            _colorMap = new Dictionary<ColorSite, Color>
            {
                { ColorSite.Points, Color.Black},
                { ColorSite.Delanay_Mesh, Color.Green},
                { ColorSite.Delanay_Cell_Fill, Color.Transparent},
                { ColorSite.Voronoi_Mesh, Color.DeepSkyBlue},
                { ColorSite.Voronoi_Cell_Fill, Color.Transparent},
                { ColorSite.Hull_Edges, Color.Red},
                { ColorSite.Hull_Points, Color.DarkRed},
            };
            colorSiteCB.DataSource =
                _colorMap.Keys.Select(x => x.ToString())
                    .Select(x => x.Replace('_', ' ')).ToList();
            var propInfos = typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            colorSelectorCB.DataSource =
                propInfos.Select(x => x.Name)
                    .Where(x =>
                        !x.Contains("Panel") &&
                        !x.Contains("Control") &&
                        !x.Contains("Accent")).ToList();
            colorSelectorCB.SelectedIndexChanged += MeshColor_SelectedIndexChanged;
            colorSelectorCB.SelectedItem = _colorMap[GetCurrentColorSite()].Name;

        }

        private void InitializeMisc()
        {
            ReadOnlyCombobox(aspectRatioCB);
            ReadOnlyCombobox(resolutionCB);
            ReadOnlyCombobox(colorSelectorCB);
            ReadOnlyCombobox(colorSiteCB);
            ReadOnlyCombobox(speedBox);

            aspectRatioCB.DataSource = Enum.GetNames(typeof(AspectRatio))
                .Select(x => x.Trim('_'))
                .Select(x => x.Replace("_", ":")).ToArray();
            aspectRatioCB.SelectedIndex = 0;
            _registry.OnEvent += s =>
            {
                TryInvoke(() => status.Text = s);
                Application.DoEvents();
            };
            _registry.OnCompleted += s =>
            {
                // ReSharper disable once LocalizableElement
                TryInvoke(() => status.Text += $"\n{s}");
                Application.DoEvents();
            };
            layer.MouseWheel += Layer_MouseWheel;
            dPoints.MouseLeave += (sender, args) => status.Focus();
            _registry.PreAction += () =>
            {
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();
            };
            _registry.AfterAction += () =>
            {
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();
            };
            //typeof(Panel).InvokeMember("DoubleBuffered",
            //    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
            //    null, layer, new object[] { true });
        }

        private void InitializeGraphicsGPU()
        {
            _graphicsBmp = new Bitmap(layer.Width, layer.Height, PixelFormat.Format32bppArgb);
            graphics = Graphics.FromImage(_graphicsBmp);
            _painter = new RazorPainter();
            var wrapper = layer.CreateGraphics();
            _hRefGraphics = new HandleRef(wrapper, wrapper.GetHdc());
        }

        #endregion

        #region Main tasks

        private void ClearGraphics(Graphics context)
        {
            DrawTokenCleanUp();

            GraphicsLocked(context,
              g =>
              {
                  using (var t = new SolidBrush(Color.White))
                      g.FillRegion(t, g.Clip);
              });


            _registry.Clear();
        }

        private IEnumerable<IPoint> GeneratePoints(RectangleF g)
        {
            if (!int.TryParse(dPoints.Text, out var d))
            {
                d = DefaultPointDist;
            }

            if (d < 0 || d > 200) d = 3;
            return UniformPoissonDiskSampler.SampleRectangle(
                    new Vector2(g.Top + _padding, g.Left + _padding),
                    new Vector2(g.Right - _padding, g.Bottom - _padding),
                    d).Select(x => new Point(x.X, x.Y)).Cast<IPoint>()
                .Concat(GetUserPoints());
        }

        private IEnumerable<IPoint> GetAllPoints()
        {
            IEnumerable<IPoint> userEnu = GetUserPoints();
            if (_pointsHeap == default)
                _pointsHeap = new List<IPoint>();
            return _pointsHeap.Concat(userEnu);
        }

        private void RefreshEngine(RectangleF bounds)
        {
            if (_pointsHeap != default && _pointsHeap.Count < 3)
                _pointsHeap = default;
            var co = GetBoundsLocked(graphics);

            var mainEnu = GetAllPoints();
            if (_refreshContext)
            {
                mainEnu = mainEnu.Select(x => Project(x, co.Width, co.Height, bounds));

            }

            // regenerate if empty??

            try
            {
                _delaunayEngine?.Dispose();
                _delaunayEngine = new DelaunayEngine(mainEnu.Select(x => (IPoint)new Point(x.X, x.Y)));
            }
            catch
            {
                // ignored
            }
        }

        private async Task GenerateAndDrawPoints(Graphics context)
        {
            ClearGraphics(context);
            DrawTokenCleanUp();
            GraphicsLocked(context,
              g =>
              {
                  using (var b = new SolidBrush(Color.White))
                      g.FillRectangle(b, layer.ClientRectangle);
              });
            var bounds = context.VisibleClipBounds;
            lock (paintLock)
            {
                _pointsHeap = GeneratePoints(bounds).ToList();
                RefreshEngine(bounds);
            }
            await RegisterAndExecuteAsync(DrawPoints, context);
        }

        private async Task DrawVoironoiCells(Graphics context)
        {
            var bounds = GetBoundsLocked(context);
            RefreshEngine(bounds);
            if (_delaunayEngine == default)
                return;

            var token = _drawInlineCancellationToken;
            var voronoiCells = _delaunayEngine.GetVoronoiCells();
            var voronoiEdges = _delaunayEngine.GetVoronoiEdges();
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var cv = _colorMap[ColorSite.Voronoi_Cell_Fill];
                    if (cv != Color.Transparent)
                        GraphicsLocked(context,
                            (g, paint, iter) =>
                            {
                                using (var solidBrush = new SolidBrush(cv))
                                    foreach (var tr in voronoiCells.Select(c => c.Points.Select(x => new PointF(x.X, x.Y)).ToArray()))
                                    {
                                        g.FillPolygon(solidBrush, tr);
                                        if (TryPaintAndCheckCancellation(token, paint))
                                            break;
                                        iter();
                                    }
                            });

                    GraphicsLocked(context, (g, paint, iter) =>
                    {
                        using (var solidBrush = new SolidBrush(_colorMap[ColorSite.Voronoi_Mesh]))
                        using (var p = new Pen(solidBrush))
                            foreach (var tr in voronoiEdges)
                            {
                                g.DrawLine(p, tr.P.X, tr.P.Y, tr.Q.X, tr.Q.Y);
                                if (TryPaintAndCheckCancellation(token, paint))
                                    break;
                                iter();
                            }
                    });
                }, token);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private async Task DrawHull(Graphics context)
        {
            var bounds = GetBoundsLocked(context);

            RefreshEngine(bounds);
            try
            {
                var token = _drawInlineCancellationToken;
                await Task.Factory.StartNew(() =>
                {
                    GraphicsLocked(context, (g, paint, iter) =>
                    {
                        using (var pen = new SolidBrush(_colorMap[ColorSite.Hull_Points]))
                            foreach (var tr in _delaunayEngine.GetHullPoints())
                            {
                                g.FillEllipse(pen, tr.X, tr.Y, 3, 3);
                                if (TryPaintAndCheckCancellation(token, paint))
                                    break;
                                iter();
                            }
                    });
                    GraphicsLocked(context, (g, paint, iter) =>
                    {
                        using (var pen = new SolidBrush(_colorMap[ColorSite.Hull_Edges]))
                        using (var p = new Pen(pen))
                            foreach (var tr in _delaunayEngine.GetHullEdges())
                            {
                                g.DrawLine(p, tr.P.X, tr.P.Y, tr.Q.X, tr.Q.Y);
                                if (TryPaintAndCheckCancellation(token, paint))
                                    break;
                                iter();
                            }
                    });
                }, token);
            }
            catch (TaskCanceledException) { }
        }

        private Task DrawDelaunayCells(Graphics context)
        {
            var bounds = GetBoundsLocked(context);
            if (!_refreshContext)
                DrawTokenCleanUp();
            var w = new Stopwatch();
            w.Start();
            RefreshEngine(bounds);
            var edges = _delaunayEngine.GetEdges().ToArray();
            w.Stop();

            TryInvoke(() => status.Text = $@"Mesh generated in: {w.Elapsed.TotalSeconds}s for {GetAllPoints().Count()} points");
            var token = _drawInlineCancellationToken;

            var color = _colorMap[ColorSite.Delanay_Cell_Fill];
            if (color != Color.Transparent)
                GraphicsLocked(context, (g, paint, iter) =>
                {
                    using (var solidBrush = new SolidBrush(_colorMap[ColorSite.Delanay_Cell_Fill]))
                        foreach (var triangle in _delaunayEngine.GetTriangles())
                        {
                            g.FillPolygon(solidBrush, triangle.Points.Select(x => new PointF(x.X, x.Y)).ToArray());
                            if (TryPaintAndCheckCancellation(token, paint))
                                break;
                            iter();
                        }
                });
            GraphicsLocked(context, (g, paint, iter) =>
            {
                using (var pen = new Pen(_colorMap[ColorSite.Delanay_Mesh]))
                    foreach (var edge in edges)
                    {
                        g.DrawLine(pen, edge.P.X, edge.P.Y, edge.Q.X, edge.Q.Y);
                        if (TryPaintAndCheckCancellation(token, paint))
                            break;
                        iter();
                    }
            });

            return Task.CompletedTask;

        }

        private async Task DrawPoints(Graphics context)
        {
            try
            {
                RefreshEngine(GetBoundsLocked(context));
                var token = _drawInlineCancellationToken;
                await Task.Factory.StartNew(() =>
                {
                    GraphicsLocked(context, (g, paint, iter) =>
                    {
                        using (var pen = new Pen(_colorMap[ColorSite.Points]))
                            foreach (var p in _delaunayEngine.Points)
                            {
                                g.DrawEllipse(pen, p.X - 1, p.Y - 1, 3, 3);
                                if (TryPaintAndCheckCancellation(token, paint))
                                    break;
                                iter();
                            }
                    });
                }, token);
            }
            catch (TaskCanceledException) { }
        }

        private async Task ProcessImport(Graphics context)
        {
            try
            {
                var token = _drawInlineCancellationToken;
                float xR;
                float yR;
                lock (paintLock)
                {
                    xR = context.VisibleClipBounds.Right;
                    yR = context.VisibleClipBounds.Bottom;
                }

                if (_importCache == default || !_importCache.Any())
                {
                    _importCache = GeoImporter.ImportCsv(importFileName).ToList();
                }
                _pointsHeap = await _importCache
                    .Select(x => GeoImporter.ProjectLatLonToXY(x, xR, yR))
                    .ToListAsync(token);
                _delaunayEngine?.Dispose();
                _delaunayEngine = new DelaunayEngine(GetAllPoints());
                _refreshContext = false;
                await DrawPoints(context);
            }
            catch (Exception ex)
            {
                TryInvoke(() => status.Text = $@"Failed to import file: {ex.Message}");
            }
        }

        private IEnumerable<IPoint> GetUserPoints()
        {
            return (IEnumerable<IPoint>)_userPointHeap ?? Array.Empty<IPoint>();
        }

        private void DrawTokenCleanUp()
        {
            _drawCancellationTokenSource?.Cancel();
            _drawCancellationTokenSource = new CancellationTokenSource();
            _drawInlineCancellationToken = _drawCancellationTokenSource.Token;
        }

        private void Export()
        {
            GraphicsLocked(graphics, g => g.ResetTransform());
            var props = typeof(ImageFormat).GetProperties(BindingFlags.Public | BindingFlags.Static);
            var formats = props.Where(f => f.Name != "MemoryBmp").Select(s => "|" + s.Name + " media file|*." + s.Name.ToLower());

            using (var fd = new SaveFileDialog { Filter = $@"{string.Join("", formats).Trim('|')}" })
            {
                if (fd.ShowDialog(Owner) == DialogResult.OK)
                {
                    var fileName = fd.FileName;

                    var extension = Path.GetExtension(fd.FileName)?.Substring(1);
                    var format = (ImageFormat)props.First(x => x.Name.ToLower().Equals(extension)).GetValue(default);
                    fd.Disposed += (s, e) =>
                       {

                           var ar = ((string)resolutionCB.SelectedItem).Split('×', 'x').Select(int.Parse).ToArray();
                           var w = ar[0];
                           var h = ar[1];
                           var token = _drawInlineCancellationToken;
                           _ = Task.Factory.StartNew(async () =>
                           {
                               try
                               {
                                   using (var bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                                   {
                                       bitmap.SetResolution(1500f, 1500f);
                                       using (var g = Graphics.FromImage(bitmap))
                                       {
                                           Cursor.Current = Cursors.WaitCursor;
                                           Application.DoEvents();
                                           lock (paintLock)
                                               g.FillRegion(new SolidBrush(Color.White), g.Clip);
                                           var pback = _pointsHeap.ToList();
                                           _refreshContext = true;
                                           var tmpImm = _immediate;
                                           _immediate = true;

                                           await _registry.ExecuteAll(g, token);
                                           _pointsHeap = pback;
                                           _immediate = tmpImm;
                                           _refreshContext = false;
                                           lock (paintLock)
                                           {
                                               //restore & paint
                                               RefreshEngine(graphics.VisibleClipBounds);
                                               _painter.Paint(new HandleRef(g, g.GetHdc()), bitmap);
                                           }
                                       }

                                       bitmap.Save(fileName, format);
                                       TryInvoke(() =>
                                           status.Text = @"Exported successfully");
                                   }
                               }
                               catch
                               {
                                   TryInvoke(() =>
                                       status.Text = @"Failed to export");
                               }
                               finally
                               {
                                   Cursor.Current = Cursors.Default;
                                   Application.DoEvents();
                               }
                           }, token);

                       };
                }
            }

        }

        void ClearAll()
        {
            _userPointHeap?.Clear();
            _importCache?.Clear();
            _pointsHeap?.Clear();
            DrawTokenCleanUp();
            ClearGraphics(graphics);
        }
        #endregion

        #region Helpers
        private ColorSite GetCurrentColorSite()
        {
            var site = (string)colorSiteCB.SelectedItem;
            var s = string.Join("_", site.Split(' '));
            return (ColorSite)Enum.Parse(typeof(ColorSite), s);
        }

        private RectangleF GetBoundsLocked(Graphics context)
        {
            RectangleF bounds;
            lock (paintLock)
            {
                bounds = context.VisibleClipBounds;
            }

            return bounds;
        }

        private bool TryPaintAndCheckCancellation(CancellationToken token, Action paint)
        {
            if (!_immediate)
            {
                paint();
                try
                {
                    Task.Delay((int)_selectedFillSpeed, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    return true; //cancelled
                }
            }
            return token.IsCancellationRequested;
        }

        private IPoint Project(IPoint point, float x, float y, RectangleF cn)
        {
            var xNew = cn.Width;
            var yNew = cn.Height;
            return new Point(Math.Min(point.X / x * xNew, xNew), Math.Min(point.Y / y * yNew, yNew));
        }

        private Task RegisterAndExecuteAsync(GrTF v, Graphics context, bool prepend = false)
        {
            Register(v, prepend);
            var token = _drawInlineCancellationToken;
            _ = Task.Factory.StartNew(async () => await v(context), token);
            return Task.CompletedTask;
        }

        private void TryInvoke(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else action();
        }

        private void ReadOnlyCombobox(ComboBox box)
        {
            var t1 = FindWindowEx(box.Handle, IntPtr.Zero, "Edit", null);
            HideCaret(t1);
            SendMessage(t1, EM_SETREADONLY, 1, 0);
        }

        private void Register(GrTF item, bool prepend)
        {
            if (prepend)
                _registry.Prepend(item);
            else
                _registry.Add(item);
        }

        private void GraphicsLocked(Graphics context, Action<Graphics> action)
        {
            GraphicsLocked(context, (g, _) => action(g));
        }

        private void GraphicsLocked(Graphics context, Action<Graphics, Action> action)
        {
            GraphicsLocked(context, (g, p, _) => action(g, p));
        }
        private void GraphicsLocked(Graphics context, Action<Graphics, Action, Action> action)
        {
            Application.DoEvents();

            lock (paintLock)
            {
                int overflowCounter = 0;
                void iter()
                {
                    overflowCounter++;
                    if (overflowCounter > 10000)
                    {
                        paint();
                        overflowCounter = 0;
                    }
                }
                void paint()
                {
                    _painter.Paint(_hRefGraphics, _graphicsBmp);
                    Application.DoEvents();
                }

                try
                {
                    action(context, paint, iter);
                }
                catch (ArgumentException) { }
                catch (OperationCanceledException) { }
                paint();
            }
        }

        private void HandleResolutionForRatio()
        {
            var ratio = (string)aspectRatioCB.SelectedItem;
            var raw = string.Join("", ratio.Replace(":", "_").Prepend('_'));
            var r = (AspectRatio)Enum.Parse(typeof(AspectRatio), raw);
            resolutionCB.DataSource = irc.GetForRatio(r).ToList();
            resolutionCB.SelectedIndex = 0;
        }

        private Task DrawPoint(Graphics context, Color color, IPoint point)
        {
            GraphicsLocked(context, g =>
            {
                if (_refreshContext)
                {
                    var b = GetBoundsLocked(graphics);
                    point = Project(point, b.Width, b.Height, context.VisibleClipBounds);
                }

                using (var pen = new Pen(color))
                    context.DrawEllipse(pen, point.X - 1, point.Y - 1, 3, 3);
            });

            return Task.CompletedTask;
        }
        private void OnLayerResize()
        {
            _resizeCancellationTokenSource?.Cancel();
            _resizeCancellationTokenSource = new CancellationTokenSource();
            var token = _resizeCancellationTokenSource.Token;
            try
            {
                Task.Factory.StartNew(() =>
               {
                   try
                   {

                       if (layer.Width == 0 || token.IsCancellationRequested) return;

                       var tmpImm = _immediate;
                       _immediate = true;
                       lock (paintLock)
                       {
                           graphics?.Dispose();
                           _graphicsBmp?.Dispose();
                           _graphicsBmp = new Bitmap(layer.Width, layer.Height, PixelFormat.Format32bppArgb);
                           graphics = Graphics.FromImage(_graphicsBmp);

                           graphics.Clear(Color.White);

                       }

                       var bounds = GetBoundsLocked(graphics);

                       if (_pointsHeap != default)
                       {
                           RefreshEngine(bounds);
                       }

                       lock (paintLock)
                       {
                           _oldPoint = new Point(bounds.X, bounds.Y);
                       }

                       Application.DoEvents();
                       _immediate = tmpImm;
                   }
                   catch (ArgumentException)
                   {
                   }
               }, token);
            }
            catch (TaskCanceledException) { }
        }

        #endregion  

        #region Event Handlers

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            Func<Graphics, Task> task = default;
            var context = graphics;
            bool handled = true;
            switch (e.KeyCode)
            {
                case Keys.G:
                    {
                        task = GenerateAndDrawPoints;
                        break;
                    }
                case Keys.C:
                    {
                        ClearAll();
                        break;
                    }
                case Keys.E:
                    {
                        Export();
                        return;
                    }

                case Keys.D:
                    {
                        task = DrawDelaunayCells;
                        break;
                    }

                case Keys.V:
                    {
                        task = DrawVoironoiCells;
                        break;
                    }
                case Keys.H:
                    {
                        task = DrawHull;
                        break;
                    }
                default:
                    {
                        handled = false;
                        break;
                    }
            }

            if (handled && task != default)
            {
                _registry.Add(task);
                var token = _drawInlineCancellationToken;
                _ = Task.Factory.StartNew(async () => await task(context), token);
            }
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            Export();
        }

        private void GeneratePointsBtn_Click(object sender, EventArgs e)
        {
            _ = RegisterAndExecuteAsync(GenerateAndDrawPoints, graphics);
        }

        private void FillDelaunayBtn_Click(object sender, EventArgs e)
        {
            _ = RegisterAndExecuteAsync(DrawDelaunayCells, graphics);
        }

        private void Layer_Click(object sender, EventArgs e)
        {
            var panel = (Panel)sender;
            panel.Focus();
        }

        private void Main_Click(object sender, EventArgs e)
        {
            Focus();
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog
            {
                Filter = @"Comma-separated values|*.csv"
            };
            if (fd.ShowDialog(Owner) == DialogResult.OK)
            {
                ClearGraphics(graphics);
                importFileName = fd.FileName;
                _ = RegisterAndExecuteAsync(ProcessImport, graphics);
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(ClearAll, _drawInlineCancellationToken);
        }

        private async void Layer_Paint(object sender, PaintEventArgs e)
        {
            var token = _resizeCancellationTokenSource?.Token ?? CancellationToken.None;

            GraphicsLocked(graphics,
                g =>
                {
                    using (var t = new SolidBrush(Color.White))
                        g.FillRegion(t, g.Clip);
                });
            try
            {
                await Task.Factory.StartNew(async () =>
                    await _registry.ExecuteAll(graphics, token), token);
            }
            catch (TaskCanceledException) { }

            // graphics.ScaleTransform(_scale, _scale);
            // graphics.TranslateTransform(_translateX, _translateY);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (paintLock)
            {
                renderTCS.Cancel();
                _painter.Dispose();
                _graphicsBmp?.Dispose();
                graphics?.Dispose();
            }
        }

        private void Layer_MouseMove(object sender, MouseEventArgs e)
        {
            mposition.Text = $@"X:{e.X} Y:{e.Y}";
            if (_drag)
            {
                //var offX = e.Location.X - drag.X;
                //var offY = e.Location.Y - drag.Y;
                //graphics.TranslateTransform(
                //    offX * 1f,
                //    offY * 1f);
                drag = e.Location;
            }
        }

        private void Layer_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                _ = Task.Factory.StartNew(() =>
                {
                    float x;
                    float y;
                    lock (paintLock)
                    {
                        x = e.Location.X + graphics.Transform.OffsetX;
                        y = e.Location.Y + graphics.Transform.OffsetY;
                    }


                    var p = new Point(x, y);
                    _dragDelayerRelay.Start();
                    _dragDelayerRelay.Interval = 300;
                    _dragDelayerRelay.Tick += (e0, e1) =>
                    {
                        if (!_drag)
                        {
                            _userPointHeap.Remove(p);
                            _drag = true;
                            _dragDelayerRelay.Stop();
                        }
                    };
                    if (_userPointHeap == default || e.Button == MouseButtons.Right)
                    {
                        _userPointHeap = new List<IPoint>();
                        ClearGraphics(graphics);
                    }

                    if (e.Button == MouseButtons.Middle)
                    {
                        lock (paintLock)
                            graphics.ResetTransform();
                    }

                    if (!_drag)
                    {
                        _userPointHeap.Add(p);
                        drag = e.Location;
                        var token = _drawInlineCancellationToken;
                        try
                        {
                            RegisterAndExecuteAsync(g =>
                                        DrawPoint(g, _colorMap[ColorSite.Points], new Point(x, y)),
                                    graphics, true)
                                .Wait(token);
                            layer.Invalidate(); /*new Rectangle(new System.Drawing.Point((int) p.X, (int) p.Y), new Size(9,9))*/
                        }
                        catch (OperationCanceledException)
                        {
                        }

                    }
                }, _drawInlineCancellationToken);
            }
            catch (TaskCanceledException) { }
        }

        private void Layer_MouseWheel(object sender, MouseEventArgs e)
        {
            var t = .1f;
            if (e.Delta > 0)
            {
                _scale += t;
                _translateX = -e.X * t;
                _translateY = -e.Y * t;
                factor++;
            }
            else
            {
                _scale -= t;
                _translateX = e.X * t;
                _translateY = e.Y * t;
                factor--;
            }

            try
            {
                lock (paintLock)
                    graphics.PageScale = _scale;
                //  graphics.TranslateTransform(_translateX, _translateY);
            }
            catch (ArgumentException) { }

            //  layer.Invalidate();
        }

        private void Layer_MouseUp(object sender, MouseEventArgs e)
        {
            if (_drag) layer.Invalidate();
            _dragDelayerRelay.Stop();
            _drag = false;
        }

        private void VoronoiFillBtn_Click(object sender, EventArgs e)
        {
            var token = _drawInlineCancellationToken;
            Task.Factory.StartNew(async () =>
                await RegisterAndExecuteAsync(DrawVoironoiCells, graphics), token);
        }

        private void SpeedBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Enum.TryParse((string)speedBox.SelectedItem, out FillSpeed s))
                _selectedFillSpeed = s;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            _immediate = immediateChk.Checked;
        }

        private void AspectRatioCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleResolutionForRatio();
        }

        private void MeshColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = Enum.Parse(typeof(KnownColor), (string)colorSelectorCB.SelectedItem);
            _colorMap[GetCurrentColorSite()] = Color.FromKnownColor((KnownColor)t);
            status.Focus();
        }

        private void ColorSiteCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            colorSelectorCB.SelectedItem = _colorMap[GetCurrentColorSite()].Name;
            status.Focus();
        }

        private void Layer_SizeChanged(object sender, EventArgs e)
        {
            OnLayerResize();
        }

        private void ColorSiteCB_DropDownClosed(object sender, EventArgs e)
        {
            status.Focus();
        }
        #endregion

    }
}
