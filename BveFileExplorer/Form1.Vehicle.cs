using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BveFileExplorer
{
    /// <summary>
    /// 車両ファイル
    /// </summary>
    public partial class Form1
    {
        /// <summary>車両ファイルから参照される各ファイルのタブ</summary>
        private sealed class VehicleTab
        {
            public TabPage Page;
            public TextBox PathBox;
            public ComboBox EncodingBox;
            public DataGridView Grid;
            /// <summary>パネルファイル(4列)として読むか</summary>
            public bool IsPanel;
            /// <summary>先頭行(ファイルヘッダ)を表示から除くか</summary>
            public bool RemoveHeader;
        }

        private VehicleTab[] _vehicleTabs;

        private void InitVehicleTabs()
        {
            _vehicleTabs = new[]
            {
                new VehicleTab { Page = tpPerformanceCurve, PathBox = tbPerfoemanceCurve, EncodingBox = cbxPerformanceCurveEnc, Grid = dgvPerformanceCurve, RemoveHeader = false },
                new VehicleTab { Page = tpParameters, PathBox = tbParameters, EncodingBox = cbxParametersEnc, Grid = dgvParameters, RemoveHeader = true },
                new VehicleTab { Page = tpPanel, PathBox = tbPanel, EncodingBox = cbxPanelEnc, Grid = dgvPanel, RemoveHeader = true, IsPanel = true },
                new VehicleTab { Page = tpSound, PathBox = tbSound, EncodingBox = cbxSoundEnc, Grid = dgvSound, RemoveHeader = true },
                new VehicleTab { Page = tpMotorNoise, PathBox = tbMotorNoise, EncodingBox = cbxMotorNoiseEnc, Grid = dgvMotorNoise, RemoveHeader = true },
            };
        }

        #region 車両ファイルの読込

        private void OpenNewVehicleFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                btnOpenVehicleFile.Enabled = false;
                btnOpenVehicleDirectory.Enabled = false;
                cbxVehicleEnc.Enabled = false;
                strDisp += "車両ファイルがないか指定されていません\n";
                FlushMessages();
                return;
            }

            btnOpenVehicleFile.Enabled = true;
            btnOpenVehicleDirectory.Enabled = true;

            // 手動指定があればそのエンコードで、なければヘッダから判定して読み込む
            vehicle = new Vehicle(filePath, false, GetEncodingOverride(filePath));
            SetEncIndex(cbxVehicleEnc, vehicle.encMode);
            cbxVehicleEnc.Enabled = true;
            if (vehicle.HeaderWarning != "")
            {
                strDisp += "車両ファイル: " + vehicle.HeaderWarning + "\n";
            }
            strVehicleFilePath = filePath;

            int error = 0;

            if (vehicle.FileVersion < 2.00)
            {
                ShowBve5OnlyVehicleLayout();
            }
            else
            {
                ShowBve6VehicleLayout();
            }

            // Ats(32bit)
            int iRet = ShowPathStatus(tbAts32, btnAts32Open, btnAts32Open, vehicle.Ats32, out strAts32DetailManagerFilePath);
            btnAts32Open2.Enabled = btnAts32Open.Enabled;
            if (vehicle.Ats32 != null)
            {
                if (vehicle.Ats32.FilePath.IndexOf("DetailManager", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    strAts32SettingTextFilePath = PathUtil.CombineAbs(vehicle.Ats32.FilePath, "detailmodules.txt");
                    tbAts32DetailModules.Text = strAts32SettingTextFilePath;
                    OpenNewAtsPluginFile(strAts32SettingTextFilePath, BVE_Version.BVE5);
                    btnAts32Open.Enabled = true;
                    btnAts32Open2.Enabled = true;
                }
                else
                {
                    strDisp += "BVE5用(32bit)ATSプラグインが見つからないか、対応していません(DetailManager以外)\n";
                    btnAts32Open.Enabled = false;
                    btnAts32Open2.Enabled = false;
                }
            }
            if (iRet <= 0)
            {
                error++;
                btnAts32Check.Enabled = false;
            }
            else
            {
                btnAts32OpenDirectory.Enabled = true;
                btnAts32Check.Enabled = true;
            }

            error += (ShowPathStatus(tbPerfoemanceCurve, btnPerfoemanceCurveOpen, btnPerfoemanceCurveDirectory, vehicle.PerformanceCurve, out _) <= 0) ? 1 : 0;
            error += (ShowPathStatus(tbParameters, btnParametersOpen, btnParametersDirectory, vehicle.Parameters, out _) <= 0) ? 1 : 0;
            error += (ShowPathStatus(tbPanel, btnPanelOpen, btnPanelDirectory, vehicle.Panel, out _) <= 0) ? 1 : 0;
            error += (ShowPathStatus(tbSound, btnSoundOpen, btnSoundDirectory, vehicle.Sound, out _) <= 0) ? 1 : 0;
            error += (ShowPathStatus(tbMotorNoise, btnMotorNoiseOpen, btnMotorNoiseDirectory, vehicle.MotorNoise, out _) <= 0) ? 1 : 0;

            // Ats64
            int ret = ShowPathStatus(tbAts64, btnAts64Open, btnAts64Open, vehicle.Ats64, out strAts64DetailManagerFilePath);
            btnAts64Open2.Enabled = btnAts64Open.Enabled;
            if (ret <= 0)
            {
                error++;
                btnBootBVE6.Enabled = false;
                SetAts64ControlsVisible(false);
            }
            else
            {
                btnBootBVE6.Enabled = true;
                btnAts64Check.Enabled = true;
                btnAts64OpenDirectory.Enabled = true;
                SetAts64ControlsVisible(true);
                ShowAts64Plugin();
            }

            if (error > 0)
            {
                strDisp += "いくつかのファイルにエラーがあるか、読込未対応ファイル形式ですm(_ _)m\n";
            }

            RefreshCurrentVehicleTab();
            FlushMessages();
        }

        /// <summary>BVE5専用車両(Vehicle 2.00未満)の表示</summary>
        private void ShowBve5OnlyVehicleLayout()
        {
            lblVehicleVer.Text = "Vehicle File Ver : " + vehicle.FileVersion.ToString("0.00") + " BVE5 専用車両ファイル";
            lblAts32.Text = "Ats";

            foreach (Control c in new Control[] {
                btnAts64Check, btnAts64Open, btnAts64Open2, btnAts64Reset, btnAts64OpenDirectory,
                tbAts64, lblAts64, label9, btnAts64RelatePathGen, btnAts64Add, tbAts64RelatePath })
            {
                c.Visible = false;
            }

            // BVE6コンバータ
            foreach (Control c in new Control[] {
                lblDetailModuleStore, tbDetailModuleStoreDetail, btnDetailModuleSelect,
                btnDetailManagerBve6Clear, btnDetailModuleStoreClear,
                lblDetailManager, tbDetailManagerDetail, btnDetailManagerBve6Select, btnBve6Convert })
            {
                c.Visible = true;
            }
            tbDetailModuleStoreDetail.Enabled = true;
            if (strAts64SettingTextFilePath == "")
            {
                btnBve6Convert.Enabled = false;
            }
            btnBve5Recovery.Visible = false;
        }

        /// <summary>BVE6対応車両(Vehicle 2.00以上)の表示</summary>
        private void ShowBve6VehicleLayout()
        {
            lblVehicleVer.Text = "Vehicle File Ver : " + vehicle.FileVersion.ToString("0.00") + " BVE6 対応車両ファイル";
            lblAts32.Text = "Ats32";

            // BVE6コンバータ
            foreach (Control c in new Control[] {
                lblDetailModuleStore, tbDetailModuleStoreDetail, btnDetailModuleSelect,
                lblDetailManager, tbDetailManagerDetail, btnDetailManagerBve6Select,
                btnDetailManagerBve6Clear, btnBve6Convert, btnDetailModuleStoreClear })
            {
                c.Visible = false;
            }
            lblConvert.Text = "既にBVE6に対応している車両かコンバート済みです。";

            string current = CurrentVehicleFilePath();
            if (current != null && File.Exists(current + ".bak"))
            {
                btnBve5Recovery.Visible = true;
                btnBve5Recovery.Enabled = true;
            }
        }

        private void SetAts64ControlsVisible(bool visible)
        {
            foreach (Control c in new Control[] {
                btnAts64Check, btnAts64Open, btnAts64Open2, tbAts64DetailModules,
                btnAts64OpenDirectory, tbAts64, lblAts64 })
            {
                c.Visible = visible;
            }

            if (visible)
            {
                label9.Visible = true;
                btnAts64RelatePathGen.Visible = true;
                btnAts64Add.Visible = true;
                tbAts64RelatePath.Visible = true;
            }
            else
            {
                btnAts64Open.Enabled = false;
                btnAts64Open2.Enabled = false;
                tbAts64DetailModules.Enabled = false;
                btnAts64OpenDirectory.Enabled = false;
                btnAts64Check.Enabled = false;
            }
        }

        /// <summary>Ats64 の種類(bve-plugin-bridge / DetailManager)を判定して表示する</summary>
        private void ShowAts64Plugin()
        {
            if (vehicle.FileVersion < 2.0)
            {
                strDisp += "BVE6用(64bit)ATSプラグインが見つからないか、対応していません(DetailManager以外)\n";
                SetAts64DetailModulesEnabled(false);
                return;
            }

            if (strAts64DetailManagerFilePath.Contains("_.dll"))
            {
                // bve-plugin-bridge で変換されている
                const string bridgeMessage = "bve-plugin-bridgeによって変換されています";
                strAts64SettingTextFilePath = bridgeMessage;
                tbAts64DetailModules.Text = bridgeMessage;
                SetAts64DetailModulesEnabled(false);
            }
            else if (strAts64DetailManagerFilePath.IndexOf("DetailManager", StringComparison.OrdinalIgnoreCase) > 0)
            {
                strAts64SettingTextFilePath = PathUtil.CombineAbs(strAts64DetailManagerFilePath, "detailmodules.txt");
                tbAts64DetailModules.Text = strAts64SettingTextFilePath;
                OpenNewAtsPluginFile(strAts64SettingTextFilePath, BVE_Version.BVE6);
                SetAts64DetailModulesEnabled(true);
            }
        }

        private void SetAts64DetailModulesEnabled(bool enabled)
        {
            btnAts64Open.Enabled = enabled;
            btnAts64Open2.Enabled = enabled;
            tbAts64DetailModules.Enabled = enabled;
        }

        #endregion

        #region 車両の選択

        /// <summary>車両ファイルのリンク切れを黄色で表示する</summary>
        private void cbxVehicle_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            Brush backBrush = Brushes.White;
            if (scenario != null && e.Index < scenario.VehicleFilesExists.Count && !scenario.VehicleFilesExists[e.Index])
            {
                backBrush = Brushes.Yellow;
            }
            e.Graphics.FillRectangle(backBrush, e.Bounds);
            e.Graphics.DrawString(cbxVehicle.Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void cbxVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearVehicleFileView();
            SetEncIndex(cbxVehicleEnc, 0);

            if (cbxVehicle.SelectedIndex < 0) return;

            cbxVehicleIndex = cbxVehicle.SelectedIndex;
            OpenNewVehicleFile(CurrentVehicleFilePath());
        }

        /// <summary>車両ファイルから読み取った表示をクリアする</summary>
        private void ClearVehicleFileView()
        {
            foreach (Control c in new Control[] {
                tbPerfoemanceCurve, tbParameters, tbPanel, tbSound, tbMotorNoise,
                tbAts32, tbAts32DetailModules, tbAts64, tbAts64DetailModules })
            {
                c.Text = "";
            }
            dgvAts32.Rows.Clear();
            dgvAts64.Rows.Clear();
            foreach (VehicleTab tab in _vehicleTabs)
            {
                tab.Grid.DataSource = null;
                SetEncIndex(tab.EncodingBox, 0);
            }
        }

        /// <summary>
        /// 車両ファイルのエンコードを手動で切り替える(ヘッダの shift_jis 記載漏れ・保存ミス対策)。
        /// 指定はファイルごとに保持され、タブ切替等で読み直しても維持される。
        /// </summary>
        private void cbxVehicleEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsUserEncodingChange(cbxVehicleEnc)) return;

            string path = CurrentVehicleFilePath();
            if (!File.Exists(path)) return;

            _encodingOverrides[path] = EncodingDetector.FromMode(cbxVehicleEnc.SelectedIndex);
            ClearVehicleFileView();
            OpenNewVehicleFile(path);
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (cbxVehicle.Items.Count > 0)
            {
                if (cbxVehicle.SelectedIndex == -1)
                {
                    cbxVehicle.SelectedIndex = 0;
                }
                cbxVehicleIndex = cbxVehicle.SelectedIndex;
                OpenNewVehicleFile(CurrentVehicleFilePath());
            }
        }

        private void dgvVehicle_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < cbxVehicle.Items.Count)
            {
                cbxVehicle.SelectedIndex = e.RowIndex;
            }
        }

        private void dgvVehicle_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFileFromGrid(dgvVehicle, e.RowIndex, e.ColumnIndex, 0, tbSeinarioFileName.Text);
        }

        #endregion

        #region 車両の各ファイル(性能曲線・パラメータ・パネル・サウンド・モーター音)

        private void tabControlVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshCurrentVehicleTab();
        }

        /// <summary>
        /// 選択中の車両タブの内容を読み込む。
        /// 車両ファイルを開き直してもタブ位置は保持するため、読込後にこれを呼んで表示を更新する。
        /// </summary>
        private void RefreshCurrentVehicleTab()
        {
            VehicleTab tab = _vehicleTabs.FirstOrDefault(t => t.Page == tabControlVehicle.SelectedTab);
            if (tab != null)
            {
                LoadVehicleTab(tab, null);
            }
        }

        /// <summary>
        /// 車両の各ファイルをグリッドに展開する
        /// </summary>
        /// <param name="enc">null の場合はファイルヘッダから判定してコンボボックスに表示</param>
        private void LoadVehicleTab(VehicleTab tab, Encoding enc)
        {
            string path = tab.PathBox.Text;
            if (!File.Exists(path)) return;

            TryLoad(path, () =>
            {
                if (enc == null)
                {
                    enc = DetectEncoding(path, tab.EncodingBox);
                }
                DataTable dt = tab.IsPanel
                    ? IniTableLoader.LoadPanel(path, enc, tab.RemoveHeader)
                    : IniTableLoader.LoadKeyValue(path, enc, tab.RemoveHeader);

                GridHelper.Bind(tab.Grid, dt);
                GridHelper.ColorRows(tab.Grid, sectionColor: Color.PaleGreen);
            });
        }

        private void OnVehicleEncodingChanged(ComboBox cbx)
        {
            if (!IsUserEncodingChange(cbx)) return;

            VehicleTab tab = _vehicleTabs.First(t => t.EncodingBox == cbx);
            LoadVehicleTab(tab, Encoding.GetEncoding(cbx.Text));
        }

        private void cbxPerformanceCurveEnc_SelectedIndexChanged(object sender, EventArgs e) => OnVehicleEncodingChanged(cbxPerformanceCurveEnc);

        private void cbxParametersEnc_SelectedIndexChanged(object sender, EventArgs e) => OnVehicleEncodingChanged(cbxParametersEnc);

        private void cbxPanelEnc_SelectedIndexChanged(object sender, EventArgs e) => OnVehicleEncodingChanged(cbxPanelEnc);

        private void cbxSoundEnc_SelectedIndexChanged(object sender, EventArgs e) => OnVehicleEncodingChanged(cbxSoundEnc);

        private void cbxMotorNoiseEnc_SelectedIndexChanged(object sender, EventArgs e) => OnVehicleEncodingChanged(cbxMotorNoiseEnc);

        private void dgvPerformanceCurve_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFileFromGrid(dgvPerformanceCurve, e.RowIndex, e.ColumnIndex, 1, tbPerfoemanceCurve.Text);
        }

        private void dgvPanel_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFileFromGrid(dgvPanel, e.RowIndex, e.ColumnIndex, 1, tbPanel.Text);
        }

        private void dgvSound_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFileFromGrid(dgvSound, e.RowIndex, e.ColumnIndex, 1, tbSound.Text);
        }

        private void dgvMotorNoise_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFileFromGrid(dgvMotorNoise, e.RowIndex, e.ColumnIndex, 1, tbMotorNoise.Text);
        }

        /// <summary>車両サウンドのwavをクリックで再生</summary>
        private void dgvSound_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            PlayWav(GridHelper.ResolveCellPath(dgvSound, e.RowIndex, e.ColumnIndex, 1, tbSound.Text));
        }

        #endregion

        /// <summary>グリッドのセル(相対パス)が指すファイルを開く</summary>
        private static void OpenFileFromGrid(DataGridView dgv, int rowIndex, int columnIndex, int fileColumn, string baseFile)
        {
            string path = GridHelper.ResolveCellPath(dgv, rowIndex, columnIndex, fileColumn, baseFile);
            if (path != null)
            {
                System.Diagnostics.Process.Start(path);
            }
        }
    }
}
