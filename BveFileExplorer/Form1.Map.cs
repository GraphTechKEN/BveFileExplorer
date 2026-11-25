using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BveFileExplorer
{
    /// <summary>
    /// マップファイル・サウンド再生
    /// </summary>
    public partial class Form1
    {
        private static readonly string[] SoundListColumns = { "SoundKey", "FilePath", "BufferSize", "Remarks" };
        private static readonly string[] SoundListToolTips =
        {
            "任意の文字列。このサウンド名は、マップファイル、停車場リストファイル、他列車ファイルで使用します。",
            "wavファイルの相対パス(ダブルクリックで開きます)",
            "その音を同時に再生できる数。省略した場合は 1 になります。",
        };

        private static readonly string[] StructureColumns = { "StructureName", "FilePath", "Remarks" };
        private static readonly string[] StructureToolTips =
        {
            "任意の文字列。このストラクチャー名は、マップファイル、信号現示リストファイル、他列車ファイルで使用します。",
            "ストラクチャーファイルの相対パス。ダブルクリックで3Dプレビュー、Shift+ダブルクリックで関連付けアプリで開きます。ストラクチャビューワ5にドラッグ＆ドロップで表示できます。",
            "備考",
        };

        private static readonly string[] StationColumns =
        {
            "stationKey", "stationName", "arrivalTime", "depertureTime", "stoppageTime", "defaultTime", "signalFlag",
            "alightingTime", "passengers", "arrivalSoundKey", "depertureSoundKey", "doorReopen", "stuckInDoor"
        };
        private static readonly string[] StationToolTips =
        {
            "停車場名 (任意の文字列)", "時刻表に表示する停車場名", "到着時刻 (HH:mm:ss) (p: 通過駅)",
            "出発時刻または通過時刻 (HH:mm:ss) (t: 終着駅)", "標準停車時間 [s]", "駅にジャンプしたときの時刻 (HH:mm:ss)",
            "出発信号 (0: 最高現示 | 1: depertureTime の stoppageTime 前の時刻まで停止現示)", "降車時間 [s]", "出発時の乗車率",
            "ドアが開いたときに再生される音 (サウンドリストファイルで定義した soundKey)",
            "depertureTime の stoppageTime 前の時刻に再生される音 (サウンドリストファイルで定義した soundKey)",
            "ドアが再開閉される確率", "旅客がドアに挟まる時間 [s]"
        };

        /// <summary>停車場リストのうちサウンドキーを持つ列(arrivalSoundKey, depertureSoundKey)</summary>
        private const int StationArrivalSoundColumn = 9;
        private const int StationDepartureSoundColumn = 10;

        private SoundPlayer player;

        /// <summary>ストラクチャ簡易ビューア(開いていなければ null)</summary>
        private StructureViewerForm _structureViewer;
        private DateTime _lastClickTime = DateTime.MinValue;
        private DataGridViewCell _lastClickedCell = null;

        #region マップファイルの読込

        private string CurrentMapFilePath() => map?.FilePath;

        private void OpenNewMapFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                btnMapOpen.Enabled = true;
                btnMapDirectory.Enabled = true;
                cbxMapFilePath.Text = filePath;
                cbxTrain.Items.Clear();

                // 手動指定があればそのエンコードで、なければヘッダから判定して読み込む
                map = new Map(filePath, false, GetEncodingOverride(filePath));
                SetEncIndex(cbxMapEnc, map.encMode);
                cbxMapEnc.Enabled = true;
                if (map.HeaderWarning != "")
                {
                    strDisp += "マップファイル: " + map.HeaderWarning + "\n";
                }

                ShowPathStatus(tbStructure, btnStructureOpen, btnStructureDirectory, map.Structure, out _);
                ShowPathStatus(tbStation, btnStationOpen, btnStationDirectory, map.Station, out _);
                ShowPathStatus(tbSignal, btnSignalOpen, btnSignalDirectory, map.Signal, out _);
                ShowPathStatus(tbSoundList, btnSoundListOpen, btnSoundListDirectory, map.SoundList, out _);
                ShowPathStatus(tbSound3DList, btnSound3DListOpen, btnSound3DListDirectory, map.Sound3DList, out _);
                ShowTrainStatus(map.Train);

                // タブ位置は保持しているので、選択中のタブの内容を読み直す
                RefreshCurrentMapTab();
            }
            else
            {
                map = null; // 前のマップの内容を表示し続けないように
                cbxMapEnc.Enabled = false;
                strDisp += "マップファイルが指定されていません\n";
            }
            FlushMessages();
        }

        /// <summary>他列車ファイル一覧をコンボボックスに表示する</summary>
        private void ShowTrainStatus(List<Contents_Map> trains)
        {
            foreach (Contents_Map train in trains)
            {
                cbxTrain.Items.Add(train.FilePathAbs);
            }

            if (trains.Count > 0)
            {
                cbxTrain.Text = trains[0].FilePathAbs;
                btnTrainOpen.Enabled = true;
                btnTrainDirectory.Enabled = true;
                cbxTrain.BackColor = SystemColors.Window;
            }
            else
            {
                cbxTrain.Text = "Not Found : Not Declared";
                cbxTrain.BackColor = Color.LightYellow;
            }
        }

        private void cbxMapFilePath_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearMapFileView();
            SetEncIndex(cbxMapEnc, 0);

            cbxMapIndex = cbxMapFilePath.SelectedIndex;
            if (cbxMapIndex >= 0 && cbxMapIndex < listMapFilePath.Count)
            {
                OpenNewMapFile(listMapFilePath[cbxMapIndex]);
                IncludeFileExtract();
            }
        }

        /// <summary>マップファイルから読み取った表示をクリアする</summary>
        private void ClearMapFileView()
        {
            foreach (TextBox tb in new[] { tbStructure, tbStation, tbSignal, tbSoundList, tbSound3DList })
            {
                tb.Text = "";
            }
            foreach (Button btn in new[] {
                btnStructureOpen, btnStructureDirectory, btnStationOpen, btnStationDirectory,
                btnSignalOpen, btnSignalDirectory, btnSoundListOpen, btnSoundListDirectory,
                btnSound3DListOpen, btnSound3DListDirectory, btnTrainOpen, btnTrainDirectory })
            {
                btn.Enabled = false;
            }
            foreach (DataGridView dgv in new[] { dgvIncludeList, dgvStructure, dgvStation, dgvSignal, dgvSoundList, dgvSound3DList, dgvTrain, dgvTrainFileList })
            {
                dgv.DataSource = null;
            }
            foreach (ComboBox cbx in new[] { cbxStructureEnc, cbxStationEnc, cbxSignalEnc, cbxSoundListEnc, cbxSound3DListEnc, cbxTrainFileEnc })
            {
                SetEncIndex(cbx, 0);
            }
        }

        /// <summary>
        /// マップファイルのエンコードを手動で切り替える(ヘッダの shift_jis 記載漏れ・保存ミス対策)。
        /// 参照ファイルのパス・include 一覧・表示中のタブを読み直す。
        /// </summary>
        private void cbxMapEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsUserEncodingChange(cbxMapEnc)) return;

            string path = map?.FilePath;
            if (!File.Exists(path)) return;

            _encodingOverrides[path] = EncodingDetector.FromMode(cbxMapEnc.SelectedIndex);
            ClearMapFileView();
            OpenNewMapFile(path);
            IncludeFileExtract();
        }

        /// <summary>一覧の行をクリックすると、そのマップを表示する(パスで対応付け)</summary>
        private void dgvRoute_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            SelectEntryFile(_routeList, e.RowIndex);
        }

        private void dgvRoute_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFileFromGrid(dgvRoute, e.RowIndex, e.ColumnIndex, 0, tbSeinarioFileName.Text);
        }

        private void tabControlMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshCurrentMapTab();
        }

        /// <summary>
        /// 選択中のマップタブの内容を読み込む。
        /// マップファイルを開き直してもタブ位置は保持するため、読込後にこれを呼んで表示を更新する。
        /// </summary>
        private void RefreshCurrentMapTab()
        {
            if (map == null) return;

            TabPage currentTab = tabControlMaps.SelectedTab;
            if (currentTab == tpStructure) StructureFileExtract();
            else if (currentTab == tpStation) StationFileExtract();
            else if (currentTab == tpSignal) SignalFileExtract();
            else if (currentTab == tpSoundList) SoundListFileExtract();
            else if (currentTab == tpSound3D) Sound3DListExtract();
            else if (currentTab == tpTrain)
            {
                TrainFileListExtract();
                TrainFileExtract();
            }
        }

        #endregion

        #region 導入ファイル(Include)

        private void IncludeFileExtract()
        {
            string path = map?.FilePath;
            if (!File.Exists(path))
            {
                dgvIncludeList.DataSource = null;
                return;
            }

            TryLoad(path, () =>
            {
                // マップファイル本体と同じエンコード(手動指定を含む)で読む
                Encoding enc = map.FileEncoding ?? EncodingDetector.Detect(path, out _);
                GridHelper.Bind(dgvIncludeList, IniTableLoader.LoadIncludes(path, enc));
                GridHelper.ColorRows(dgvIncludeList, sectionColor: Color.LightGreen);
            });
        }

        private void dgvIncludeList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFileFromGrid(dgvIncludeList, e.RowIndex, e.ColumnIndex, 0, map?.FilePath);
        }

        #endregion

        #region ストラクチャ(Structure)

        private void StructureFileExtract(Encoding enc = null)
        {
            string path = map?.Structure?.FilePathAbs;
            if (!File.Exists(path)) return;

            TryLoad(path, () =>
            {
                if (enc == null) enc = DetectEncoding(path, cbxStructureEnc);

                GridHelper.Bind(dgvStructure, ListFileLoader.LoadWithRemarks(path, enc, StructureColumns));
                GridHelper.SetHeaderToolTips(dgvStructure, StructureToolTips);
                GridHelper.ColorRows(dgvStructure, pathBaseFile: path);
            });
        }

        private void cbxStructureEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsUserEncodingChange(cbxStructureEnc)) StructureFileExtract(Encoding.GetEncoding(cbxStructureEnc.Text));
        }

        /// <summary>ダブルクリックで3Dプレビュー(Shift+ダブルクリックで関連付けアプリで開く)</summary>
        private void dgvStructure_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) ActivateStructureRow(e.RowIndex);
        }

        /// <summary>同じダブルクリックを MouseDown 側と二重に処理しないための時刻</summary>
        private DateTime _lastStructureActivate = DateTime.MinValue;

        /// <summary>
        /// ストラクチャ一覧の行をダブルクリックしたときの処理。
        /// 通常は3Dプレビュー、Shift を押しながらなら関連付けアプリで .x を開く。
        /// </summary>
        private void ActivateStructureRow(int rowIndex)
        {
            if ((DateTime.Now - _lastStructureActivate).TotalMilliseconds < SystemInformation.DoubleClickTime) return;
            _lastStructureActivate = DateTime.Now;

            if ((Control.ModifierKeys & Keys.Shift) != 0)
            {
                OpenFileFromGrid(dgvStructure, rowIndex, 1, 1, map?.Structure?.FilePathAbs);
            }
            else
            {
                ShowStructurePreview(rowIndex, openIfClosed: true);
            }
        }

        /// <summary>
        /// ストラクチャファイルをドラッグ(ストラクチャビューワへのドロップ用)。
        /// DoDragDrop 中は通常のダブルクリックイベントが届かないため、ダブルクリックもここで判定する。
        /// </summary>
        private void dgvStructure_MouseDown(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo hit = dgvStructure.HitTest(e.X, e.Y);
            if (hit.RowIndex >= 0)
            {
                // プレビューを開いていれば、クリックした行のストラクチャを表示する
                ShowStructurePreview(hit.RowIndex, openIfClosed: false);
            }
            if (hit.RowIndex < 0 || hit.ColumnIndex != 1) return;

            DataGridViewCell clickedCell = dgvStructure.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
            TimeSpan duration = DateTime.Now - _lastClickTime;
            string filePath = PathUtil.CombineAbs(tbStructure.Text, clickedCell.Value?.ToString());

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                // FileDrop 形式(CF_HDROP)でドラッグ
                DataObject data = new DataObject();
                data.SetData(DataFormats.FileDrop, true, new[] { filePath });
                this.DoDragDrop(data, DragDropEffects.Copy);
            }

            if (duration.TotalMilliseconds < SystemInformation.DoubleClickTime && _lastClickedCell == clickedCell)
            {
                ActivateStructureRow(hit.RowIndex);
                _lastClickTime = DateTime.MinValue;
                _lastClickedCell = null;
            }
            else
            {
                _lastClickTime = DateTime.Now;
                _lastClickedCell = clickedCell;
            }
        }

        #endregion

        #region ストラクチャ簡易ビューア

        private void btnStructurePreview_Click(object sender, EventArgs e)
        {
            int row = dgvStructure.CurrentCell?.RowIndex ?? -1;
            if (row >= 0)
            {
                ShowStructurePreview(row, openIfClosed: true);
            }
            else
            {
                OpenStructureViewer();
            }
        }

        /// <summary>キー操作で行を移動したときもプレビューを更新する</summary>
        private void dgvStructure_CurrentCellChanged(object sender, EventArgs e)
        {
            int row = dgvStructure.CurrentCell?.RowIndex ?? -1;
            if (row >= 0) ShowStructurePreview(row, openIfClosed: false);
        }

        private StructureViewerForm OpenStructureViewer()
        {
            if (_structureViewer == null || _structureViewer.IsDisposed)
            {
                _structureViewer = new StructureViewerForm();
                _structureViewer.Show(this); // メインフォームの手前に表示(一緒に閉じる)
            }
            else if (!_structureViewer.Visible)
            {
                _structureViewer.Show(this);
            }
            if (_structureViewer.WindowState == FormWindowState.Minimized)
            {
                _structureViewer.WindowState = FormWindowState.Normal;
            }
            return _structureViewer;
        }

        /// <summary>
        /// ストラクチャ一覧の行のファイルをビューアに表示する。
        /// </summary>
        /// <param name="openIfClosed">ビューアが閉じていれば開くか(false の場合は開いているときだけ更新)</param>
        private void ShowStructurePreview(int rowIndex, bool openIfClosed)
        {
            bool isOpen = _structureViewer != null && !_structureViewer.IsDisposed && _structureViewer.Visible;
            if (!isOpen && !openIfClosed) return;
            if (rowIndex < 0 || rowIndex >= dgvStructure.Rows.Count || dgvStructure.Columns.Count < 2) return;

            string relative = GridHelper.CellText(dgvStructure.Rows[rowIndex], 1);
            string path = PathUtil.CombineAbs(map?.Structure?.FilePathAbs, relative);

            StructureViewerForm viewer = OpenStructureViewer();
            if (path != "") viewer.LoadFile(path);
        }

        #endregion

        #region 停車場(Station)

        private void StationFileExtract(Encoding enc = null)
        {
            string path = map?.Station?.FilePathAbs;
            if (!File.Exists(path)) return;

            TryLoad(path, () =>
            {
                if (enc == null) enc = DetectEncoding(path, cbxStationEnc);

                GridHelper.Bind(dgvStation, ListFileLoader.LoadFixedColumns(path, enc, StationColumns));
                GridHelper.SetHeaderToolTips(dgvStation, StationToolTips);
                GridHelper.ColorRows(dgvStation);
            });
        }

        private void cbxStationEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsUserEncodingChange(cbxStationEnc)) StationFileExtract(Encoding.GetEncoding(cbxStationEnc.Text));
        }

        private static bool IsStationSoundColumn(int columnIndex)
        {
            return columnIndex == StationArrivalSoundColumn || columnIndex == StationDepartureSoundColumn;
        }

        /// <summary>停車場リストのサウンドキーから、サウンドリストに定義された wav の絶対パスを探す</summary>
        private string FindStationSoundFile(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || !IsStationSoundColumn(e.ColumnIndex)) return null;

            string soundKey = dgvStation[e.ColumnIndex, e.RowIndex].Value?.ToString();
            if (string.IsNullOrEmpty(soundKey)) return null;

            if (dgvSoundList.RowCount == 0)
            {
                SoundListFileExtract();
            }

            foreach (DataGridViewRow row in dgvSoundList.Rows)
            {
                if (row.IsNewRow) continue;
                if (GridHelper.CellText(row, 0) == soundKey)
                {
                    return PathUtil.CombineAbs(tbSoundList.Text, GridHelper.CellText(row, 1));
                }
            }
            return null;
        }

        /// <summary>サウンドキーのセルをクリックで再生</summary>
        private void dgvStation_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            PlayWav(FindStationSoundFile(e));
        }

        /// <summary>サウンドキーのセルをダブルクリックで wav を開く</summary>
        private void dgvStation_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || !IsStationSoundColumn(e.ColumnIndex)) return;

            string path = FindStationSoundFile(e);
            if (path != null) OpenFile(path);
        }

        /// <summary>停車場リストの時刻列</summary>
        private const int StationArrivalTimeColumn = 2;   // arrivalTime   (p: 通過)
        private const int StationDepartureTimeColumn = 3; // depertureTime (t: 終着)
        private const int StationDefaultTimeColumn = 5;   // defaultTime

        /// <summary>時刻 H:mm:ss / HH:mm:ss (24時以降の深夜帯も許可)</summary>
        private static readonly Regex StationTimeRegex = new Regex(@"^\d{1,2}:[0-5]?\d:[0-5]?\d$", RegexOptions.CultureInvariant);

        /// <summary>停車場リストの時刻列の値が正しいか(空白は常に可)</summary>
        private static bool IsValidStationTime(int column, string input)
        {
            if (string.IsNullOrEmpty(input)) return true;

            // 到着時刻は p (通過)、出発時刻は t (終着) のみ許可。大文字小文字は区別しない
            if (column == StationArrivalTimeColumn && input.Equals("p", StringComparison.OrdinalIgnoreCase)) return true;
            if (column == StationDepartureTimeColumn && input.Equals("t", StringComparison.OrdinalIgnoreCase)) return true;

            return StationTimeRegex.IsMatch(input);
        }

        private void dgvStation_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex != StationArrivalTimeColumn &&
                e.ColumnIndex != StationDepartureTimeColumn &&
                e.ColumnIndex != StationDefaultTimeColumn) return;

            // 編集していないセルはフォーカス移動・再読込時に検証しない(既存データで閉じ込めない)
            DataGridView grid = (DataGridView)sender;
            if (!grid.IsCurrentCellInEditMode) return;

            string input = Convert.ToString(e.FormattedValue)?.Trim();
            if (IsValidStationTime(e.ColumnIndex, input)) return;

            string msg;
            switch (e.ColumnIndex)
            {
                case StationArrivalTimeColumn:
                    msg = "arrivalTime は「空白」「P」(通過駅) または「HH:MM:SS」形式で入力してください。";
                    break;
                case StationDepartureTimeColumn:
                    msg = "depertureTime は「空白」「T」(終着駅) または「HH:MM:SS」形式で入力してください。";
                    break;
                default:
                    msg = "defaultTime は「空白」または「HH:MM:SS」形式で入力してください。";
                    break;
            }
            MessageBox.Show(msg, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            e.Cancel = true;
        }

        #endregion

        #region 信号(Signal)

        private void SignalFileExtract(Encoding enc = null)
        {
            string path = map?.Signal?.FilePathAbs;
            if (!File.Exists(path)) return;

            TryLoad(path, () =>
            {
                if (enc == null) enc = DetectEncoding(path, cbxSignalEnc);

                GridHelper.Bind(dgvSignal, ListFileLoader.LoadVariableColumns(path, enc));
            });
        }

        private void cbxSignalEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsUserEncodingChange(cbxSignalEnc)) SignalFileExtract(Encoding.GetEncoding(cbxSignalEnc.Text));
        }

        #endregion

        #region サウンド(Sound) / 固定音源(Sound3D)

        private void SoundListFileExtract(Encoding enc = null)
        {
            string path = map?.SoundList?.FilePathAbs;
            if (!File.Exists(path)) return;

            TryLoad(path, () =>
            {
                if (enc == null) enc = DetectEncoding(path, cbxSoundListEnc);

                GridHelper.Bind(dgvSoundList, ListFileLoader.LoadWithRemarks(path, enc, SoundListColumns));
                GridHelper.SetHeaderToolTips(dgvSoundList, SoundListToolTips);
                GridHelper.ColorRows(dgvSoundList, pathBaseFile: path);
            });
        }

        private void cbxSoundListEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsUserEncodingChange(cbxSoundListEnc)) SoundListFileExtract(Encoding.GetEncoding(cbxSoundListEnc.Text));
        }

        private void dgvSoundList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            PlayWav(GridHelper.ResolveCellPath(dgvSoundList, e.RowIndex, e.ColumnIndex, 1, tbSoundList.Text));
        }

        private void dgvSoundList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFileFromGrid(dgvSoundList, e.RowIndex, e.ColumnIndex, 1, map?.SoundList?.FilePathAbs);
        }

        private void Sound3DListExtract(Encoding enc = null)
        {
            string path = map?.Sound3DList?.FilePathAbs;
            if (!File.Exists(path)) return;

            TryLoad(path, () =>
            {
                if (enc == null) enc = DetectEncoding(path, cbxSound3DListEnc);

                GridHelper.Bind(dgvSound3DList, ListFileLoader.LoadWithRemarks(path, enc, SoundListColumns));
                GridHelper.SetHeaderToolTips(dgvSound3DList, SoundListToolTips);
                GridHelper.ColorRows(dgvSound3DList, pathBaseFile: path);
            });
        }

        private void cbxSound3DListEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsUserEncodingChange(cbxSound3DListEnc)) Sound3DListExtract(Encoding.GetEncoding(cbxSound3DListEnc.Text));
        }

        private void dgvSound3DList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            PlayWav(GridHelper.ResolveCellPath(dgvSound3DList, e.RowIndex, e.ColumnIndex, 1, tbSound3DList.Text));
        }

        private void dgvSound3DList_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFileFromGrid(dgvSound3DList, e.RowIndex, e.ColumnIndex, 1, tbSound3DList.Text);
        }

        #endregion

        #region 他列車(Train)

        /// <summary>マップファイルの Train.Add 一覧を表示する</summary>
        private void TrainFileListExtract()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TrainKey");
            dt.Columns.Add("FilePath");
            dt.Columns.Add("TrackKey");
            dt.Columns.Add("Direction");

            foreach (Contents_Map trainContent in map.Train)
            {
                foreach (var train in trainContent.TrainFilesList)
                {
                    dt.Rows.Add(train.trainKey, train.filePath, train.trackKey, train.direction);
                }
            }
            dgvTrainFileList.DataSource = dt;
        }

        /// <summary>選択中の他列車ファイルの内容を表示する</summary>
        private void TrainFileExtract()
        {
            string path = cbxTrain.Text;
            if (!File.Exists(path))
            {
                dgvTrain.DataSource = null;
                return;
            }

            TryLoad(path, () =>
            {
                Encoding enc = DetectEncoding(path, cbxTrainFileEnc);
                GridHelper.Bind(dgvTrain, IniTableLoader.LoadKeyValue(path, enc, removeFirstRow: true));
                GridHelper.ColorRows(dgvTrain, sectionColor: Color.LightGreen);
            });
        }

        private void dgvTrainFileList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= cbxTrain.Items.Count) return;

            cbxTrain.SelectedIndex = e.RowIndex;
            TrainFileExtract();
        }

        private void dgvTrainFileList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OpenFile(cbxTrain.Text);
        }

        #endregion

        #region サウンド再生

        private void PlayWav(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            StopSound();
            try
            {
                player = new SoundPlayer(filePath);
                player.Play(); // 非同期再生
                btnPlayStop.Visible = true;
            }
            catch (InvalidOperationException ex)
            {
                // wav 以外・壊れたファイル
                StopSound();
                tsslDisp.Text = "再生できません: " + filePath + " (" + ex.Message + ")";
            }
        }

        private void StopSound()
        {
            if (player == null) return;
            player.Stop();
            player.Dispose();
            player = null;
        }

        private void BtnPlayStop_Click(object sender, EventArgs e)
        {
            StopSound();
            btnPlayStop.Visible = false;
        }

        #endregion
    }
}
