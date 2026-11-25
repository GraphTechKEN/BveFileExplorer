using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
// WPF の型は WinForms と名前が重なるため別名で使う
using Wpf = System.Windows;
using WpfControls = System.Windows.Controls;
using WpfInput = System.Windows.Input;
using Media = System.Windows.Media;
using Media3D = System.Windows.Media.Media3D;
using Imaging = System.Windows.Media.Imaging;

namespace BveFileExplorer
{
    /// <summary>
    /// ストラクチャ(.x)の簡易ビューア。WPF の Viewport3D を ElementHost で埋め込んで表示する。
    /// <para>操作: 左ドラッグ=回転 / 右(中)ドラッグ=平行移動 / ホイール=拡大縮小 / ダブルクリック=視点リセット</para>
    /// </summary>
    public sealed class StructureViewerForm : Form
    {
        // WinForms 部品
        private readonly ElementHost _host;
        private readonly CheckBox _chkTexture;
        private readonly CheckBox _chkBothSides;
        private readonly CheckBox _chkGrid;
        private readonly Label _lblInfo;

        // WPF 部品
        private readonly WpfControls.Border _border;
        private readonly WpfControls.Viewport3D _viewport;
        private readonly Media3D.PerspectiveCamera _camera;
        private readonly Media3D.ModelVisual3D _modelVisual;
        private readonly Media3D.ModelVisual3D _gridVisual;

        // 表示中のモデル
        private XModel _model;
        private string _currentPath;
        private readonly Dictionary<string, Media.ImageBrush> _textureCache = new Dictionary<string, Media.ImageBrush>(StringComparer.OrdinalIgnoreCase);

        // カメラ(注視点を中心に回る)
        private Media3D.Point3D _target = new Media3D.Point3D(0, 0, 0);
        private double _yaw, _pitch, _distance = 10, _modelRadius = 5;
        private double _gridExtent = 5;

        /// <summary>グリッドの高さ。Y=0 の面(レール面など)と重なってちらつかないよう少し下げる[m]</summary>
        private const double GridY = -0.01;
        private Wpf.Point _lastMouse;
        private bool _rotating, _panning;

        private const double DefaultYaw = Math.PI / 6;     // 右前から
        private const double DefaultPitch = Math.PI / 9;   // やや上から

        public StructureViewerForm()
        {
            Text = "ストラクチャ簡易ビューア";
            Width = 900;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;

            // 上部の操作パネル
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 32,
                Padding = new Padding(4, 4, 4, 0),
                WrapContents = false,
            };
            var btnOpen = new Button { Text = "開く...", Width = 70, Height = 24 };
            btnOpen.Click += (s, e) => OpenWithDialog();
            var btnReset = new Button { Text = "視点リセット", Width = 90, Height = 24 };
            btnReset.Click += (s, e) => ResetView();
            _chkTexture = new CheckBox { Text = "テクスチャ", Checked = true, AutoSize = true, Margin = new Padding(8, 6, 0, 0) };
            _chkTexture.CheckedChanged += (s, e) => RebuildModel();
            _chkBothSides = new CheckBox { Text = "裏面も表示", Checked = true, AutoSize = true, Margin = new Padding(8, 6, 0, 0) };
            _chkBothSides.CheckedChanged += (s, e) => RebuildModel();
            _chkGrid = new CheckBox { Text = "グリッド", Checked = true, AutoSize = true, Margin = new Padding(8, 6, 0, 0) };
            _chkGrid.CheckedChanged += (s, e) => UpdateGrid();
            _lblInfo = new Label { AutoSize = true, Margin = new Padding(12, 7, 0, 0) };
            panel.Controls.AddRange(new Control[] { btnOpen, btnReset, _chkTexture, _chkBothSides, _chkGrid, _lblInfo });

            // WPF 3D ビュー
            _camera = new Media3D.PerspectiveCamera { FieldOfView = 45, UpDirection = new Media3D.Vector3D(0, 1, 0) };
            _viewport = new WpfControls.Viewport3D { Camera = _camera, ClipToBounds = true };

            var lights = new Media3D.Model3DGroup();
            lights.Children.Add(new Media3D.AmbientLight(Media.Color.FromRgb(0x60, 0x60, 0x60)));
            lights.Children.Add(new Media3D.DirectionalLight(Media.Color.FromRgb(0xC0, 0xC0, 0xC0), new Media3D.Vector3D(-0.5, -1, -0.7)));
            lights.Children.Add(new Media3D.DirectionalLight(Media.Color.FromRgb(0x50, 0x50, 0x50), new Media3D.Vector3D(0.6, 0.3, 0.8)));
            _viewport.Children.Add(new Media3D.ModelVisual3D { Content = lights });

            // 半透明のグリッドは不透明なモデルの後に描く(WPF は半透明の前後を並べ替えないため)
            _modelVisual = new Media3D.ModelVisual3D();
            _viewport.Children.Add(_modelVisual);
            _gridVisual = new Media3D.ModelVisual3D();
            _viewport.Children.Add(_gridVisual);

            // Viewport3D はモデル上しかマウスを拾わないので、背景付きの Border で受ける
            _border = new WpfControls.Border
            {
                Background = new Media.SolidColorBrush(Media.Color.FromRgb(0x40, 0x48, 0x50)),
                Child = _viewport,
                AllowDrop = true,
            };
            _border.MouseDown += Border_MouseDown;
            _border.MouseUp += Border_MouseUp;
            _border.MouseMove += Border_MouseMove;
            _border.MouseWheel += Border_MouseWheel;
            _border.Drop += Border_Drop;

            _host = new ElementHost { Dock = DockStyle.Fill, Child = _border };

            Controls.Add(_host);
            Controls.Add(panel);

            ShowMessage("ストラクチャ一覧の行を選択するか、.x ファイルをドロップしてください。");
            ResetView();
        }

        #region 読込・表示

        /// <summary>.x ファイルを表示する(同じファイルなら何もしない)</summary>
        public void LoadFile(string filePath, bool force = false)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            if (!force && string.Equals(filePath, _currentPath, StringComparison.OrdinalIgnoreCase)) return;

            _currentPath = filePath;
            Text = "ストラクチャ簡易ビューア - " + Path.GetFileName(filePath);

            if (!File.Exists(filePath))
            {
                ClearModel();
                ShowMessage("ファイルが見つかりません: " + filePath);
                return;
            }
            if (!string.Equals(Path.GetExtension(filePath), ".x", StringComparison.OrdinalIgnoreCase))
            {
                ClearModel();
                ShowMessage(".x ファイルではありません: " + Path.GetFileName(filePath));
                return;
            }

            try
            {
                _model = XFileParser.Load(filePath);
            }
            catch (XFileFormatException ex)
            {
                ClearModel();
                ShowMessage("読み込めません: " + ex.Message);
                return;
            }
            catch (IOException ex)
            {
                ClearModel();
                ShowMessage("読み込めません: " + ex.Message);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                ClearModel();
                ShowMessage("読み込めません: " + ex.Message);
                return;
            }

            _textureCache.Clear();
            RebuildModel();
            FitView();
        }

        private void ClearModel()
        {
            _model = null;
            _modelVisual.Content = null;
            UpdateGrid();
        }

        private void ShowMessage(string message)
        {
            _lblInfo.Text = message;
        }

        /// <summary>読み込んだモデルから WPF のモデルを作り直す(テクスチャ・裏面の切替時も)</summary>
        private void RebuildModel()
        {
            if (_model == null) return;

            var group = new Media3D.Model3DGroup();
            int vertexCount = 0, faceCount = 0, materialCount = 0;
            var missingTextures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (XMesh mesh in _model.Meshes)
            {
                vertexCount += mesh.Positions.Count;
                faceCount += mesh.Faces.Count;

                List<XMaterial> materials = mesh.Materials.Count > 0 ? mesh.Materials : new List<XMaterial> { new XMaterial() };
                materialCount += materials.Count;

                for (int m = 0; m < materials.Count; m++)
                {
                    Media3D.MeshGeometry3D geometry = BuildGeometry(mesh, m);
                    if (geometry.TriangleIndices.Count == 0) continue;
                    geometry.Freeze();

                    Media3D.Material material = BuildMaterial(materials[m], mesh.TexCoords.Count > 0, missingTextures);
                    var model = new Media3D.GeometryModel3D(geometry, material);
                    if (_chkBothSides.Checked) model.BackMaterial = material;
                    group.Children.Add(model);
                }
            }

            _modelVisual.Content = group;

            // 情報表示
            Media3D.Rect3D b = group.Bounds;
            string size = b.IsEmpty ? "" : string.Format(" | 大きさ {0:0.##} × {1:0.##} × {2:0.##} m", b.SizeX, b.SizeY, b.SizeZ);
            var warnings = new List<string>(_model.Warnings);
            if (missingTextures.Count > 0) warnings.Add("表示できないテクスチャ: " + string.Join(", ", missingTextures));
            ShowMessage(string.Format("頂点 {0} / 面 {1} / マテリアル {2}{3}{4}",
                vertexCount, faceCount, materialCount, size,
                warnings.Count > 0 ? " | ⚠ " + string.Join(" ", warnings) : ""));

            UpdateGrid();
        }

        /// <summary>
        /// メッシュのうちマテリアル m の面だけを3角形に分割してジオメトリにする。
        /// DirectX(左手系)→ WPF(右手系)の変換として Z を反転する(表面の向きはこれで WPF の反時計回りになる)。
        /// </summary>
        private static Media3D.MeshGeometry3D BuildGeometry(XMesh mesh, int materialIndex)
        {
            var geometry = new Media3D.MeshGeometry3D();
            var map = new Dictionary<int, int>();
            bool hasUv = mesh.TexCoords.Count > 0;
            bool hasMaterials = mesh.FaceMaterials.Count == mesh.Faces.Count && mesh.Materials.Count > 0;

            int Add(int src)
            {
                if (map.TryGetValue(src, out int dst)) return dst;
                XVector3 p = mesh.Positions[src];
                geometry.Positions.Add(new Media3D.Point3D(p.X, p.Y, -p.Z));
                if (hasUv)
                {
                    var uv = mesh.TexCoords[src];
                    geometry.TextureCoordinates.Add(new Wpf.Point(uv.U, uv.V));
                }
                dst = geometry.Positions.Count - 1;
                map[src] = dst;
                return dst;
            }

            for (int f = 0; f < mesh.Faces.Count; f++)
            {
                if (hasMaterials && mesh.FaceMaterials[f] != materialIndex) continue;
                if (!hasMaterials && materialIndex != 0) continue;

                int[] face = mesh.Faces[f];
                int a = Add(face[0]);
                for (int k = 1; k + 1 < face.Length; k++)
                {
                    geometry.TriangleIndices.Add(a);
                    geometry.TriangleIndices.Add(Add(face[k]));
                    geometry.TriangleIndices.Add(Add(face[k + 1]));
                }
            }
            return geometry;
        }

        private Media3D.Material BuildMaterial(XMaterial xm, bool hasUv, HashSet<string> missingTextures)
        {
            Media.Brush brush = null;
            if (_chkTexture.Checked && hasUv && !string.IsNullOrEmpty(xm.TextureFile))
            {
                brush = LoadTexture(xm.TextureFile);
                if (brush == null) missingTextures.Add(xm.TextureFile);
            }
            if (brush == null)
            {
                var color = Media.Color.FromScRgb((float)Clamp01(xm.A), (float)Clamp01(xm.R), (float)Clamp01(xm.G), (float)Clamp01(xm.B));
                var solid = new Media.SolidColorBrush(color);
                solid.Freeze();
                brush = solid;
            }

            var group = new Media3D.MaterialGroup();
            group.Children.Add(new Media3D.DiffuseMaterial(brush));
            if (xm.EmissiveR > 0 || xm.EmissiveG > 0 || xm.EmissiveB > 0)
            {
                var emissive = Media.Color.FromScRgb(1f, (float)Clamp01(xm.EmissiveR), (float)Clamp01(xm.EmissiveG), (float)Clamp01(xm.EmissiveB));
                group.Children.Add(new Media3D.EmissiveMaterial(new Media.SolidColorBrush(emissive)));
            }
            group.Freeze();
            return group;
        }

        /// <summary>テクスチャを読み込む(.x ファイルからの相対パス)。読めない形式(dds 等)は null</summary>
        private Media.ImageBrush LoadTexture(string textureFile)
        {
            string path = PathUtil.CombineAbs(_model.FilePath, textureFile);
            if (path == "" || !File.Exists(path)) return null;
            if (_textureCache.TryGetValue(path, out Media.ImageBrush cached)) return cached;

            Media.ImageBrush brush = null;
            try
            {
                var bmp = new Imaging.BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = Imaging.BitmapCacheOption.OnLoad; // ファイルをロックしない
                bmp.UriSource = new Uri(path);
                bmp.EndInit();
                bmp.Freeze();

                brush = new Media.ImageBrush(bmp)
                {
                    // テクスチャ座標をそのまま使い、1 を超える座標は繰り返す
                    ViewportUnits = Media.BrushMappingMode.Absolute,
                    Viewport = new Wpf.Rect(0, 0, 1, 1),
                    TileMode = Media.TileMode.Tile,
                };
                brush.Freeze();
            }
            catch (NotSupportedException) { brush = null; }
            catch (IOException) { brush = null; }
            catch (UnauthorizedAccessException) { brush = null; }
            catch (ArgumentException) { brush = null; }

            _textureCache[path] = brush;
            return brush;
        }

        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        /// <summary>地面(Y=0)のグリッド。1m 間隔の細い帯で描く(WPF 3D には線がないため)</summary>
        private void UpdateGrid()
        {
            if (!_chkGrid.Checked)
            {
                _gridVisual.Content = null;
                return;
            }

            // モデルの大きさに合わせて範囲と間隔を決める
            double extent = Math.Max(5, Math.Ceiling(_modelRadius * 1.5));
            double step = extent > 50 ? 10 : (extent > 20 ? 5 : 1);
            extent = Math.Ceiling(extent / step) * step;
            _gridExtent = extent;
            double w = step * 0.02;

            var mesh = new Media3D.MeshGeometry3D();
            void Quad(double x1, double z1, double x2, double z2)
            {
                int i = mesh.Positions.Count;
                mesh.Positions.Add(new Media3D.Point3D(x1, GridY, z1));
                mesh.Positions.Add(new Media3D.Point3D(x2, GridY, z1));
                mesh.Positions.Add(new Media3D.Point3D(x2, GridY, z2));
                mesh.Positions.Add(new Media3D.Point3D(x1, GridY, z2));
                mesh.TriangleIndices.Add(i); mesh.TriangleIndices.Add(i + 2); mesh.TriangleIndices.Add(i + 1);
                mesh.TriangleIndices.Add(i); mesh.TriangleIndices.Add(i + 3); mesh.TriangleIndices.Add(i + 2);
            }
            for (double v = -extent; v <= extent + 1e-9; v += step)
            {
                Quad(-extent, v - w, extent, v + w);   // X 方向の線
                Quad(v - w, -extent, v + w, extent);   // Z 方向の線
            }
            mesh.Freeze();

            var material = new Media3D.EmissiveMaterial(new Media.SolidColorBrush(Media.Color.FromArgb(0x80, 0xA0, 0xA0, 0xA0)));
            material.Freeze();
            var model = new Media3D.GeometryModel3D(mesh, material) { BackMaterial = material };
            _gridVisual.Content = model;
            UpdateCamera();
        }

        #endregion

        #region カメラ操作

        /// <summary>モデル全体が見える位置に注視点と距離を合わせる</summary>
        private void FitView()
        {
            Media3D.Rect3D b = (_modelVisual.Content as Media3D.Model3DGroup)?.Bounds ?? Media3D.Rect3D.Empty;
            if (b.IsEmpty)
            {
                _target = new Media3D.Point3D(0, 0, 0);
                _modelRadius = 5;
            }
            else
            {
                _target = new Media3D.Point3D(b.X + b.SizeX / 2, b.Y + b.SizeY / 2, b.Z + b.SizeZ / 2);
                _modelRadius = Math.Max(0.1, Math.Sqrt(b.SizeX * b.SizeX + b.SizeY * b.SizeY + b.SizeZ * b.SizeZ) / 2);
            }
            _yaw = DefaultYaw;
            _pitch = DefaultPitch;
            _distance = _modelRadius / Math.Tan(_camera.FieldOfView * Math.PI / 360) * 1.1;
            UpdateGrid();
            UpdateCamera();
        }

        private void ResetView() => FitView();

        private void UpdateCamera()
        {
            double cp = Math.Cos(_pitch);
            var offset = new Media3D.Vector3D(
                _distance * cp * Math.Sin(_yaw),
                _distance * Math.Sin(_pitch),
                _distance * cp * Math.Cos(_yaw));
            _camera.Position = _target + offset;
            _camera.LookDirection = -offset;
            // 奥行きの範囲(Far/Near の比)が大きいと前後判定の精度が落ち、
            // 視点を動かしたときにグリッドと物体の前後関係が乱れるので、必要な範囲に絞る
            double sceneRadius = Math.Max(_modelRadius, _gridExtent * 1.5);
            double targetDistance = (_target - new Media3D.Point3D(0, 0, 0)).Length;
            _camera.NearPlaneDistance = Math.Max(0.005, _distance * 0.01);
            _camera.FarPlaneDistance = _distance + targetDistance + sceneRadius * 2;
        }

        private void Border_MouseDown(object sender, WpfInput.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == WpfInput.MouseButton.Left && e.ClickCount == 2)
            {
                ResetView();
                return;
            }

            _lastMouse = e.GetPosition(_border);
            if (e.ChangedButton == WpfInput.MouseButton.Left) _rotating = true;
            else _panning = true;
            _border.CaptureMouse();
        }

        private void Border_MouseUp(object sender, WpfInput.MouseButtonEventArgs e)
        {
            _rotating = false;
            _panning = false;
            _border.ReleaseMouseCapture();
        }

        private void Border_MouseMove(object sender, WpfInput.MouseEventArgs e)
        {
            if (!_rotating && !_panning) return;

            Wpf.Point p = e.GetPosition(_border);
            double dx = p.X - _lastMouse.X;
            double dy = p.Y - _lastMouse.Y;
            _lastMouse = p;

            if (_rotating)
            {
                _yaw -= dx * 0.01;
                _pitch = Math.Max(-1.55, Math.Min(1.55, _pitch + dy * 0.01));
            }
            else
            {
                // カメラの右方向・上方向に注視点を動かす
                Media3D.Vector3D look = _camera.LookDirection;
                look.Normalize();
                Media3D.Vector3D right = Media3D.Vector3D.CrossProduct(look, new Media3D.Vector3D(0, 1, 0));
                if (right.LengthSquared < 1e-9) right = new Media3D.Vector3D(1, 0, 0);
                right.Normalize();
                Media3D.Vector3D up = Media3D.Vector3D.CrossProduct(right, look);
                double scale = _distance * 0.0015;
                _target += (-dx * scale) * right + (dy * scale) * up;
            }
            UpdateCamera();
        }

        private void Border_MouseWheel(object sender, WpfInput.MouseWheelEventArgs e)
        {
            _distance *= e.Delta > 0 ? 0.85 : 1 / 0.85;
            _distance = Math.Max(_modelRadius * 0.05, Math.Min(_distance, _modelRadius * 200 + 100));
            UpdateCamera();
        }

        #endregion

        #region ファイルを開く

        private void OpenWithDialog()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "ストラクチャ(*.x)|*.x|すべてのファイル(*.*)|*.*";
                if (!string.IsNullOrEmpty(_currentPath))
                {
                    try { ofd.InitialDirectory = Path.GetDirectoryName(_currentPath); }
                    catch (ArgumentException) { }
                }
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    LoadFile(ofd.FileName, force: true);
                }
            }
        }

        private void Border_Drop(object sender, Wpf.DragEventArgs e)
        {
            if (e.Data.GetData(Wpf.DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                LoadFile(files[0], force: true);
            }
        }

        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape) Close();
            else if (e.KeyCode == Keys.F5 && _currentPath != null) LoadFile(_currentPath, force: true);
        }
    }
}
