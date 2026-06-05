// ======================================================================================================
// Encryphix - File and Folder Encryption Software
// © Copyright 2025-2026, Eray Türkay.
// Publisher: Türkaysoft
// Project Type: Open Source
// License: MIT License
// Website: https://turkaysoft.com
// GitHub: https://github.com/turkaysoft/encryphix
// ======================================================================================================

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
// TS MODULES
using static Encryphix.TSModules;
using static Encryphix.TSProtection;

namespace Encryphix{
    public partial class EncryphixMain : Form{
        public EncryphixMain(){
            InitializeComponent();
            // LANGUAGE SET MODES
            // ==================
            arabicToolStripMenuItem.Tag = "ar";
            chineseToolStripMenuItem.Tag = "zh";
            englishToolStripMenuItem.Tag = "en";
            dutchToolStripMenuItem.Tag = "nl";
            frenchToolStripMenuItem.Tag = "fr";
            germanToolStripMenuItem.Tag = "de";
            hindiToolStripMenuItem.Tag = "hi";
            italianToolStripMenuItem.Tag = "it";
            japaneseToolStripMenuItem.Tag = "ja";
            koreanToolStripMenuItem.Tag = "ko";
            polishToolStripMenuItem.Tag = "pl";
            portugueseToolStripMenuItem.Tag = "pt";
            russianToolStripMenuItem.Tag = "ru";
            spanishToolStripMenuItem.Tag = "es";
            turkishToolStripMenuItem.Tag = "tr";
            // LANGUAGE SET EVENTS
            // ==================
            arabicToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            chineseToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            englishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            dutchToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            frenchToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            germanToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            hindiToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            italianToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            japaneseToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            koreanToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            polishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            portugueseToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            russianToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            spanishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            turkishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            //
            SystemEvents.UserPreferenceChanged += (s, e) => TSUseSystemTheme();
        }
        // GLOBAL VARIABLES
        // ======================================================================================================
        public static string lang, lang_path;
        public static int theme;
        // TS PROTECTION ERROR MESSAGES
        // ======================================================================================================
        public static class TSProtectionErrorMessages{
            public static Dictionary<string, string> Messages = new Dictionary<string, string>(){
                { "FolderEncryptionError", "An error occurred while encrypting the folder" },
                { "SaltReadError", "Failed to read salt data from the encrypted file" },
                { "FileTypeReadError", "Failed to read file type information" },
                { "ExtLengthReadError", "Failed to read extension length information" },
                { "InvalidExtensionLength", "The extension length value is invalid or corrupted" },
                { "ExtensionReadError", "Failed to read original file extension" },
                { "IVReadError", "Failed to read initialization vector data" },
                { "InvalidPasswordOrCorruptFile", "Invalid password or corrupted encrypted file" },
                { "AccessError", "Access denied while processing the file or directory" },
                { "UnknownError", "An unknown error occurred" },
                { "InvalidFolderPath", "Folder path cannot be null or empty" },
                { "InvalidPassword", "Password cannot be null or empty" },
                { "FolderNotFound", "The specified folder could not be found" },
                { "InvalidInputFile", "Input file path cannot be null or empty" },
                { "InvalidOutputFile", "Output file path cannot be null or empty" },
                { "FileNotFound", "The specified file could not be found" },
                { "CorruptedFileOrTampered", "The file has been corrupted or tampered with. Integrity check failed." },
                { "HMACReadError", "Failed to read data integrity check value from the encrypted file" },
            };
        }
        // LOCAL VARIABLES
        // ======================================================================================================
        int themeSystem, startup_status, safety_warnings_status;
        bool p_mode, p_visible = true;
        CompressionLevel compress_level = CompressionLevel.NoCompression;
        // ======================================================================================================
        // COLOR MODES
        static readonly List<Color> header_colors = new List<Color>() { Color.Transparent, Color.Transparent, Color.Transparent };
        // HEADER SETTINGS
        // ======================================================================================================
        private class HeaderMenuColors : ToolStripProfessionalRenderer{
            public HeaderMenuColors() : base(new HeaderColors()) { }
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) { e.ArrowColor = header_colors[1]; base.OnRenderArrow(e); }
            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e){
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                float dpiScale = g.DpiX / 96f;
                Rectangle rect = e.ImageRectangle;
                using (Pen anti_alias_pen = new Pen(header_colors[2], 2.2f * dpiScale)){
                    anti_alias_pen.StartCap = LineCap.Round;
                    anti_alias_pen.EndCap = LineCap.Round;
                    anti_alias_pen.LineJoin = LineJoin.Round;
                    PointF p1 = new PointF(rect.Left + rect.Width * 0.18f, rect.Top + rect.Height * 0.52f);
                    PointF p2 = new PointF(rect.Left + rect.Width * 0.38f, rect.Top + rect.Height * 0.72f);
                    PointF p3 = new PointF(rect.Left + rect.Width * 0.78f, rect.Top + rect.Height * 0.28f);
                    g.DrawLines(anti_alias_pen, new[] { p1, p2, p3 });
                }
            }
        }
        private class HeaderColors : ProfessionalColorTable{
            public override Color MenuItemSelected => header_colors[0];
            public override Color ToolStripDropDownBackground => header_colors[0];
            public override Color ImageMarginGradientBegin => header_colors[0];
            public override Color ImageMarginGradientEnd => header_colors[0];
            public override Color ImageMarginGradientMiddle => header_colors[0];
            public override Color MenuItemSelectedGradientBegin => header_colors[0];
            public override Color MenuItemSelectedGradientEnd => header_colors[0];
            public override Color MenuItemPressedGradientBegin => header_colors[0];
            public override Color MenuItemPressedGradientMiddle => header_colors[0];
            public override Color MenuItemPressedGradientEnd => header_colors[0];
            public override Color MenuItemBorder => header_colors[0];
            public override Color CheckBackground => header_colors[0];
            public override Color ButtonSelectedBorder => header_colors[0];
            public override Color CheckSelectedBackground => header_colors[0];
            public override Color CheckPressedBackground => header_colors[0];
            public override Color MenuBorder => header_colors[0];
            public override Color SeparatorLight => header_colors[1];
            public override Color SeparatorDark => header_colors[1];
        }
        // LOAD SOFTWARE SETTINGS
        // ======================================================================================================
        private void RunSoftwareEngine(){
            // DOUBLE BUFFERS
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, FAF_DGV, new object[] { true });
            // ADD TEMPORT COLUMNS
            FAF_DGV.Columns.Add("FP", "Path");
            FAF_DGV.Columns.Add("FP", "Size");
            FAF_DGV.Columns.Add("FP", "Status");
            FAF_DGV.Columns.Add("FP", "PrivateColumn");
            FAF_DGV.Columns[3].Visible = false;
            FAF_DGV.RowTemplate.Height = (int)(26 * this.DeviceDpi / 96f);
            FAF_DGV.Columns[1].Width = (int)(50 * this.DeviceDpi / 96f);
            FAF_DGV.Columns[2].Width = (int)(200 * this.DeviceDpi / 96f);
            foreach (DataGridViewColumn columnPadding in FAF_DGV.Columns){
                int scaledPadding = (int)(3 * this.DeviceDpi / 96f);
                columnPadding.DefaultCellStyle.Padding = new Padding(scaledPadding, 0, 0, 0);
            }
            // NOT SORTABLE SET
            foreach (DataGridViewColumn OSD_Column in FAF_DGV.Columns){
                OSD_Column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            FAF_DGV.ClearSelection();
            // DPI SET TEXTBOX BUTTONS
            BtnShowPassword.Height = TextBox_Password.Height + 2;
            BtnRndPssGen.Height = TextBox_Password.Height + 2;
            BtnCopyPassword.Height = TextBox_Password.Height + 2;
            BtnSavePath.Height = TextBox_SaveFolder.Height + 2;
            // COMBOBOX ADD ITEMS
            Combo_Compress.Items.Add("Low");
            Combo_Compress.Items.Add("Medium");
            Combo_Compress.Items.Add("High");
            Combo_Compress.SelectedIndex = 0;
            // THEME - LANG - STARTUP MODE PRELOADER
            // ======================================================================================================
            TSSettingsModule software_read_settings = new TSSettingsModule(ts_sf);
            //
            int theme_mode = int.TryParse(software_read_settings.TSReadSettings(ts_settings_container, "ThemeStatus"), out int the_status) && (the_status == 0 || the_status == 1 || the_status == 2) ? the_status : 1;
            if (theme_mode == 2) { themeSystem = 2; Theme_engine(TSThemeModeHelper.GetSystemTheme(2)); } else Theme_engine(theme_mode);
            darkThemeToolStripMenuItem.Checked = theme_mode == 0;
            lightThemeToolStripMenuItem.Checked = theme_mode == 1;
            systemThemeToolStripMenuItem.Checked = theme_mode == 2;
            string lang_mode = software_read_settings.TSReadSettings(ts_settings_container, "LanguageStatus");
            var languageFiles = new Dictionary<string, (object langResource, ToolStripMenuItem menuItem, bool fileExists)>{
                { "ar", (ts_lang_ar, arabicToolStripMenuItem, File.Exists(ts_lang_ar)) },
                { "zh", (ts_lang_zh, chineseToolStripMenuItem, File.Exists(ts_lang_zh)) },
                { "en", (ts_lang_en, englishToolStripMenuItem, File.Exists(ts_lang_en)) },
                { "nl", (ts_lang_nl, dutchToolStripMenuItem, File.Exists(ts_lang_nl)) },
                { "fr", (ts_lang_fr, frenchToolStripMenuItem, File.Exists(ts_lang_fr)) },
                { "de", (ts_lang_de, germanToolStripMenuItem, File.Exists(ts_lang_de)) },
                { "hi", (ts_lang_hi, hindiToolStripMenuItem, File.Exists(ts_lang_hi)) },
                { "it", (ts_lang_it, italianToolStripMenuItem, File.Exists(ts_lang_it)) },
                { "ja", (ts_lang_ja, japaneseToolStripMenuItem, File.Exists(ts_lang_ja)) },
                { "ko", (ts_lang_ko, koreanToolStripMenuItem, File.Exists(ts_lang_ko)) },
                { "pl", (ts_lang_pl, polishToolStripMenuItem, File.Exists(ts_lang_pl)) },
                { "pt", (ts_lang_pt, portugueseToolStripMenuItem, File.Exists(ts_lang_pt)) },
                { "ru", (ts_lang_ru, russianToolStripMenuItem, File.Exists(ts_lang_ru)) },
                { "es", (ts_lang_es, spanishToolStripMenuItem, File.Exists(ts_lang_es)) },
                { "tr", (ts_lang_tr, turkishToolStripMenuItem, File.Exists(ts_lang_tr)) },
            };
            foreach (var langLoader in languageFiles) { langLoader.Value.menuItem.Enabled = langLoader.Value.fileExists; }
            var (langResource, selectedMenuItem, _) = languageFiles.ContainsKey(lang_mode) ? languageFiles[lang_mode] : languageFiles["en"];
            Lang_engine(Convert.ToString(langResource), lang_mode);
            selectedMenuItem.Checked = true;
            //
            string startup_mode = software_read_settings.TSReadSettings(ts_settings_container, "StartupStatus");
            startup_status = int.TryParse(startup_mode, out int ini_status) && (ini_status == 0 || ini_status == 1) ? ini_status : 0;
            WindowState = startup_status == 1 ? FormWindowState.Maximized : FormWindowState.Normal;
            windowedToolStripMenuItem.Checked = startup_status == 0;
            fullScreenToolStripMenuItem.Checked = startup_status == 1;
            //
            string safety_mode = software_read_settings.TSReadSettings(ts_settings_container, "SafetyWarnings");
            safety_warnings_status = int.TryParse(safety_mode, out int safetywar_status) && (safetywar_status == 0 || safetywar_status == 1) ? safetywar_status : 0;
            safetyWarningsOnToolStripMenuItem.Checked = safety_warnings_status == 1;
            safetyWarningsOffToolStripMenuItem.Checked = safety_warnings_status == 0;
            //
            TextBox_SaveFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
        // MAIN TOOLTIP SETTINGS
        // ======================================================================================================
        private void MainToolTip_Draw(object sender, DrawToolTipEventArgs e){ e.DrawBackground(); e.DrawBorder(); e.DrawText(); }
        // LOAD
        // ====================================================================================================== 
        private void Encryphix_Load(object sender, EventArgs e){
            Text = TS_VersionEngine.TS_SoftwareVersion(0);
            HeaderMenu.Cursor = Cursors.Hand;
            RunSoftwareEngine();
            //
            Task softwareUpdateCheck = Task.Run(() => Software_update_check(0));
        }
        // CLEAR SELECTION
        // ======================================================================================================
        private void FAF_DGV_SelectionChanged(object sender, EventArgs e){ FAF_DGV.ClearSelection(); }
        // FOLDER AND FILE SELECT DRAG AND DROP
        // ====================================================================================================== 
        private void Encryphix_DragEnter(object sender, DragEventArgs e){
            if (e.Data.GetDataPresent(DataFormats.FileDrop)){
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void Encryphix_DragDrop(object sender, DragEventArgs e){
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            ProcessSelectedPaths(droppedPaths);
        }
        // FOLDER AND FILE SELECT DIALOG
        // ====================================================================================================== 
        private void BtnSelect_Click(object sender, EventArgs e){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            var selectedPaths = new List<string>();
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            // Folder Select
            using (var fbd = new FolderBrowserDialog()){
                fbd.Description = software_lang.TSReadLangs("EncryphixUI", "eui_folder_select");
                fbd.ShowNewFolderButton = false;
                fbd.SelectedPath = desktopPath;
                if (fbd.ShowDialog() == DialogResult.OK && Directory.Exists(fbd.SelectedPath)){
                    selectedPaths.Add(fbd.SelectedPath);
                }
            }
            // File Select
            using (var ofd = new OpenFileDialog()){
                ofd.Multiselect = true;
                ofd.Title = string.Format(software_lang.TSReadLangs("EncryphixUI", "eui_file_select"), Application.ProductName);
                ofd.InitialDirectory = desktopPath;
                if (ofd.ShowDialog() == DialogResult.OK && ofd.FileNames.Length > 0){
                    selectedPaths.AddRange(ofd.FileNames);
                }
            }
            if (selectedPaths.Count > 0){ ProcessSelectedPaths(selectedPaths); }
        }
        // FOLDER AND FILE SELECT MODULE
        // ====================================================================================================== 
        private void ProcessSelectedPaths(IEnumerable<string> paths){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            var selectedItems = new List<string>();
            var allFiles = new List<string>();
            foreach (var path in paths){
                if (File.Exists(path)){
                    selectedItems.Add(path);
                    allFiles.Add(path);
                }else if (Directory.Exists(path)){
                    selectedItems.Add(path);
                    allFiles.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
                }
            }
            bool hasAes = allFiles.Any(f => string.Equals(Path.GetExtension(f), EncryptedExtension, StringComparison.OrdinalIgnoreCase));
            bool hasNonAes = allFiles.Any(f => !string.Equals(Path.GetExtension(f), EncryptedExtension, StringComparison.OrdinalIgnoreCase));
            if (hasAes && hasNonAes){
                TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("EncryphixUI", "eui_multi_select"));
                return;
            }
            FAF_DGV.Rows.Clear();
            foreach (var item in selectedItems.OrderBy(TS_NaturalSortKey)){
                File_select_process_function(item);
            }
            FAF_DGV.ClearSelection();
            p_mode = hasAes;
            //
            var image = (theme == 0) ? (hasAes ? Properties.Resources.ct_decrypt_dark : Properties.Resources.ct_encrypt_dark) : (hasAes ? Properties.Resources.ct_decrypt_light : Properties.Resources.ct_encrypt_light);
            TSImageRenderer(BtnBurner, image, 19, ContentAlignment.MiddleLeft);
            //
            BtnBurner.Text = hasAes ? " " + software_lang.TSReadLangs("EncryphixUI", "eui_decrypt_text") : " " + software_lang.TSReadLangs("EncryphixUI", "eui_encrypt_text");
        }
        // RENDER DATA
        // ======================================================================================================
        private void File_select_process_function(string path){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            double size = 0;
            string extension = "";
            if (File.Exists(path)){
                size = new FileInfo(path).Length;
                extension = Path.GetExtension(path);
            }else if (Directory.Exists(path)){
                size = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);
            }
            int rowIndex = FAF_DGV.Rows.Add(path, TS_FormatSize(size));
            bool isEncrypted = string.Equals(extension, EncryptedExtension, StringComparison.OrdinalIgnoreCase);
            FAF_DGV.Rows[rowIndex].Cells[2].Value = isEncrypted ? software_lang.TSReadLangs("EncryphixUI", "eui_encrypted_text") : software_lang.TSReadLangs("EncryphixUI", "eui_decrypted_text");
            FAF_DGV.Rows[rowIndex].Cells[3].Value = isEncrypted;
        }
        // COMPRESS MODE SELECTOR
        // ======================================================================================================
        private void Combo_Compress_SelectedIndexChanged(object sender, EventArgs e){
            switch (Combo_Compress.SelectedIndex){
                case 0: compress_level = CompressionLevel.NoCompression; break;
                case 1: compress_level = CompressionLevel.Fastest; break;
                case 2: compress_level = CompressionLevel.Optimal; break;
            }
        }
        // ENCRYPT AND DECRYPT START BTN
        // ======================================================================================================
        private void BtnBurner_Click(object sender, EventArgs e){
            _ = SuperPursuitMode_ASYNC();
        }
        // ENCRYPT AND DECRYPT PURSUIT MODE (FIXED - No deadlock)
        // ======================================================================================================
        private bool _isProcessing = false;
        private string _currentOutputDirectory = "";
        private string _currentPassword = "";
        private bool _currentDeleteOriginal = false;
        private async Task SuperPursuitMode_ASYNC(){
            if (_isProcessing) return;
            _isProcessing = true;
            TSGetLangs softwareLang = new TSGetLangs(lang_path);
            try{
                List<string> selectedFilePaths = new List<string>();
                foreach (DataGridViewRow dataRow in FAF_DGV.Rows){
                    if (dataRow.IsNewRow) continue;
                    string filePath = dataRow.Cells[0].Value?.ToString();
                    if (!string.IsNullOrEmpty(filePath)) { selectedFilePaths.Add(filePath); }
                }
                if (selectedFilePaths.Count == 0){
                    string msg = p_mode ? softwareLang.TSReadLangs("EncryphixUI", "eui_select_file") : softwareLang.TSReadLangs("EncryphixUI", "eui_import_file");
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, msg);
                    return;
                }
                string password = TextBox_Password.Text.Trim();
                if (string.IsNullOrEmpty(password)){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, softwareLang.TSReadLangs("EncryphixUI", "eui_input_password"));
                    return;
                }
                string outputDirectory = TextBox_SaveFolder.Text.Trim();
                if (!string.IsNullOrEmpty(outputDirectory)){
                    try { outputDirectory = Path.GetFullPath(outputDirectory); } catch { }
                }
                if (string.IsNullOrEmpty(outputDirectory)){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, softwareLang.TSReadLangs("EncryphixUI", "eui_target_folder"));
                    return;
                }
                if (!Directory.Exists(outputDirectory)){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, softwareLang.TSReadLangs("EncryphixUI", "eui_target_valid_folder"));
                    return;
                }
                bool deleteOriginal = CheckOrjFileDelete.Checked;
                // -------------------------------------------------------
                if (safety_warnings_status == 1){
                    if (!p_mode && deleteOriginal){
                        DialogResult result = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(softwareLang.TSReadLangs("EncryphixUI", "eui_encrypted_warning"), "\n\n"));
                        if (result != DialogResult.Yes) return;
                    }
                    if (p_mode && deleteOriginal){
                        DialogResult result = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(softwareLang.TSReadLangs("EncryphixUI", "eui_decrypted_warning"), "\n\n"));
                        if (result != DialogResult.Yes) return;
                    }
                }
                // -------------------------------------------------------
                _currentOutputDirectory = outputDirectory;
                _currentPassword = password;
                _currentDeleteOriginal = deleteOriginal;
                //
                AllowDrop = false;
                Progress_BG.Visible = true;
                Progress_FE.Width = 0;
                TextBox_Password.Enabled = false;
                BtnShowPassword.Enabled = false;
                TextBox_SaveFolder.Enabled = false;
                BtnSavePath.Enabled = false;
                CheckOrjFileDelete.Enabled = false;
                Combo_Compress.Enabled = false;
                BtnSelect.Enabled = false;
                BtnBurner.Enabled = false;
                //
                bool operationSuccess = false;
                string operationMessage = "";
                //
                string localOutputDir = _currentOutputDirectory;
                string localPassword = _currentPassword;
                bool localDeleteOriginal = _currentDeleteOriginal;
                bool localPmode = p_mode;
                await Task.Run(() => {
                    try{
                        int totalFileCount = selectedFilePaths.Count;
                        int completedFileCount = 0;
                        object progressLock = new object();
                        //
                        foreach (string currentFilePath in selectedFilePaths){
                            try{
                                if (!localPmode){
                                    // Encryption 
                                    if (Directory.Exists(currentFilePath)){
                                        EncryptFolder(currentFilePath, localPassword, localOutputDir, null, localDeleteOriginal, compress_level);
                                    }else if (File.Exists(currentFilePath)){
                                        string encryptedFilePath = GetUniquePath(Path.Combine(localOutputDir, Path.GetFileNameWithoutExtension(currentFilePath) + EncryptedExtension));
                                        EncryptFile(currentFilePath, encryptedFilePath, localPassword, null, localDeleteOriginal);
                                    }else{
                                        throw new FileNotFoundException(string.Format(softwareLang.TSReadLangs("EncryphixUI", "eui_file_and_folder_not_found"), currentFilePath));
                                    }
                                }else{
                                    // Decryption
                                    if (!File.Exists(currentFilePath)){
                                        throw new FileNotFoundException(string.Format(softwareLang.TSReadLangs("EncryphixUI", "eui_encrypted_file_not_found"), currentFilePath));
                                    }
                                    string tempPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(currentFilePath) + Guid.NewGuid().ToString("N") + ".tmp"));
                                    try{
                                        string originalExt = DecryptFile(currentFilePath, tempPath, localPassword, null);
                                        bool isEncryptedFolder = originalExt.Equals(ZipExtension, StringComparison.OrdinalIgnoreCase);
                                        string baseName = Path.GetFileNameWithoutExtension(currentFilePath);
                                        if (isEncryptedFolder){
                                            string folderName = Path.GetFileNameWithoutExtension(baseName);
                                            string outputFolder = Path.GetFullPath(Path.Combine(localOutputDir, folderName));
                                            if (Directory.Exists(outputFolder)) Directory.Delete(outputFolder, true);
                                            Directory.CreateDirectory(outputFolder);
                                            ZipFile.ExtractToDirectory(tempPath, outputFolder);
                                            if (localDeleteOriginal) SafeDeleteFile(currentFilePath);
                                        }else{
                                            string outputFile = Path.GetFullPath(Path.Combine(localOutputDir, baseName + originalExt));
                                            if (File.Exists(outputFile)) File.Delete(outputFile);
                                            File.Move(tempPath, outputFile);
                                            if (localDeleteOriginal) SafeDeleteFile(currentFilePath);
                                        }
                                    }
                                    finally{
                                        SafeDeleteFile(tempPath);
                                    }
                                }
                                //
                                lock (progressLock){
                                    completedFileCount++;
                                    int progress = (int)Math.Floor((completedFileCount * 100.0) / totalFileCount);
                                    if (progress > 100) progress = 100;
                                    BeginInvoke(new Action(() => {
                                        Progress_FE.Width = (Progress_BG.Width * progress) / 100;
                                        Text = $"{TS_VersionEngine.TS_SoftwareVersion(0)} - {(localPmode ? softwareLang.TSReadLangs("EncryphixUI", "eui_decrypted_process") : softwareLang.TSReadLangs("EncryphixUI", "eui_encrypted_process"))} - {progress}%";
                                    }));
                                }
                            }catch (Exception ex){
                                string errorMsg = ex is InvalidDataException || ex is CryptographicException ? string.Format(softwareLang.TSReadLangs("EncryphixUI", "eui_failed_pass_and_broken_file"), "\n") : string.Format(softwareLang.TSReadLangs("EncryphixUI", "eui_failed_process"), ex.Message);
                                Debug.WriteLine($"Encryption/Decryption error: {ex.Message}");
                            }
                        }
                        operationSuccess = true;
                    }catch (Exception ex){
                        operationMessage = ex is InvalidDataException || ex is CryptographicException ? string.Format(softwareLang.TSReadLangs("EncryphixUI", "eui_failed_pass_and_broken_file"), "\n") : string.Format(softwareLang.TSReadLangs("EncryphixUI", "eui_failed_process"), ex.Message);
                        Debug.WriteLine($"Operation error: {ex.Message}");
                    }
                });
                //
                FAF_DGV.Rows.Clear();
                Text = TS_VersionEngine.TS_SoftwareVersion(0);
                AllowDrop = true;
                Progress_BG.Visible = false;
                Progress_FE.Width = 0;
                TextBox_Password.Enabled = true;
                BtnShowPassword.Enabled = true;
                TextBox_SaveFolder.Enabled = true;
                BtnSavePath.Enabled = true;
                CheckOrjFileDelete.Enabled = true;
                Combo_Compress.Enabled = true;
                BtnSelect.Enabled = true;
                BtnBurner.Enabled = true;
                TextBox_Password.Text = string.Empty;
                TextBox_SaveFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                CheckOrjFileDelete.Checked = false;
                //
                if (operationSuccess){
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, p_mode ? softwareLang.TSReadLangs("EncryphixUI", "eui_decrypt_success") : softwareLang.TSReadLangs("EncryphixUI", "eui_encrypt_success"));
                }else if (!string.IsNullOrEmpty(operationMessage)){
                    TS_MessageBoxEngine.TS_MessageBox(this, 3, operationMessage);
                }
            }finally{
                _isProcessing = false;
            }
        }
        // SHOW PASSWORD
        // ======================================================================================================
        private void BtnShowPassword_Click(object sender, EventArgs e){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            p_visible = !p_visible;
            TextBox_Password.UseSystemPasswordChar = p_visible;
            MainToolTip.SetToolTip(BtnShowPassword, p_visible ? software_lang.TSReadLangs("EncryphixGraphics", "eg_password_show_hover") : software_lang.TSReadLangs("EncryphixGraphics", "eg_password_hide_hover"));
            TSImageRenderer(BtnShowPassword, theme == 0 ? (p_visible ? Properties.Resources.ct_hide_password_dark : Properties.Resources.ct_show_password_dark) : (p_visible ? Properties.Resources.ct_hide_password_light : Properties.Resources.ct_show_password_light), 10, ContentAlignment.MiddleCenter);
        }
        // RANDOM PASSWORD GENERATOR (CRYPTOGRAPHICALLY SECURE)
        // ======================================================================================================
        private readonly RandomNumberGenerator _secureRng = RandomNumberGenerator.Create();
        private void BtnRndPssGen_Click(object sender, EventArgs e){
            GenerateRandomPassword();
        }
        private void GenerateRandomPassword(){
            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string lower = "abcdefghijklmnopqrstuvwxyz";
            string digits = "0123456789";
            string symbols = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            string allChars = upper + lower + digits + symbols;
            int allCharsSize = allChars.Length;
            int allRejectionThreshold = byte.MaxValue - (byte.MaxValue % allCharsSize);
            int passLength;
            byte[] buffer = new byte[1];
            do { _secureRng.GetBytes(buffer); } while (buffer[0] >= byte.MaxValue - (byte.MaxValue % 9));
            passLength = 10 + (buffer[0] % 9);
            var chars = new char[passLength];
            (string category, int index)[] categories = new[] {
                (upper, 0), (lower, 1), (digits, 2), (symbols, 3)
            };
            foreach (var (cat, idx) in categories){
                int catSize = cat.Length;
                int catThreshold = byte.MaxValue - (byte.MaxValue % catSize);
                byte catByte;
                do { _secureRng.GetBytes(buffer); catByte = buffer[0]; } while (catByte >= catThreshold);
                chars[idx] = cat[catByte % catSize];
            }
            for (int i = categories.Length; i < passLength; i++){
                byte randByte;
                do { _secureRng.GetBytes(buffer); randByte = buffer[0]; } while (randByte >= allRejectionThreshold);
                chars[i] = allChars[randByte % allCharsSize];
            }
            for (int i = passLength - 1; i > 0; i--){
                byte swapByte;
                do { _secureRng.GetBytes(buffer); swapByte = buffer[0]; } while (swapByte >= byte.MaxValue - (byte.MaxValue % (i + 1)));
                int j = swapByte % (i + 1);
                (chars[j], chars[i]) = (chars[i], chars[j]);
            }
            TextBox_Password.Text = new string(chars);
        }
        // COPY PASSWORD (with security warning)
        // ======================================================================================================
        private void BtnCopyPassword_Click(object sender, EventArgs e){
            if (!string.IsNullOrWhiteSpace(TextBox_Password.Text)){
                string copiedPassword = TextBox_Password.Text.Trim();
                Clipboard.SetText(copiedPassword);
                Task.Delay(30000).ContinueWith(_ => {
                    try{
                        if (Clipboard.GetText() == copiedPassword){
                            Clipboard.Clear();
                        }
                    }catch { }
                }, TaskScheduler.FromCurrentSynchronizationContext());
                TSGetLangs lang = new TSGetLangs(lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 1, lang.TSReadLangs("EncryphixGraphics", "eg_password_copy_message"));
            }
        }
        // SAVE LOCATION SET BTN
        // ======================================================================================================
        private void BtnSavePath_Click(object sender, EventArgs e){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog()){
                folderDialog.Description = software_lang.TSReadLangs("EncryphixUI", "eui_target_folder_save");
                folderDialog.ShowNewFolderButton = true;
                if (folderDialog.ShowDialog() == DialogResult.OK){
                    TextBox_SaveFolder.Text = folderDialog.SelectedPath;
                }
            }
        }
        // THEME MODE
        // ======================================================================================================
        private ToolStripMenuItem selected_theme = null;
        private void Select_theme_active(object target_theme){
            if (target_theme == null)
                return;
            ToolStripMenuItem clicked_theme = (ToolStripMenuItem)target_theme;
            if (selected_theme == clicked_theme)
                return;
            Select_theme_deactive();
            selected_theme = clicked_theme;
            selected_theme.Checked = true;
        }
        private void Select_theme_deactive(){
            foreach (ToolStripMenuItem theme in themeToolStripMenuItem.DropDownItems){
                theme.Checked = false;
            }
        }
        private void SystemThemeToolStripMenuItem_Click(object sender, EventArgs e){
            themeSystem = 2; Theme_engine(TSThemeModeHelper.GetSystemTheme(2)); SaveTheme(2); Select_theme_active(sender);
        }
        private void LightThemeToolStripMenuItem_Click(object sender, EventArgs e){
            themeSystem = 0; Theme_engine(1); SaveTheme(1); Select_theme_active(sender);
        }
        private void DarkThemeToolStripMenuItem_Click(object sender, EventArgs e){
            themeSystem = 0; Theme_engine(0); SaveTheme(0); Select_theme_active(sender);
        }
        private void TSUseSystemTheme() { if (themeSystem == 2) Theme_engine(TSThemeModeHelper.GetSystemTheme(2)); }
        private void SaveTheme(int ts){
            // SAVE CURRENT THEME
            try{
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "ThemeStatus", Convert.ToString(ts));
            }catch (Exception){ }
        }
        // THEME ENGINE
        // ======================================================================================================
        private void Theme_engine(int ts){
            try{
                theme = ts;
                //
                TSThemeModeHelper.SetThemeMode(ts == 0);
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                if (theme == 1){
                    // SETTINGS
                    TSImageRenderer(settingsToolStripMenuItem, Properties.Resources.tm_settings_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(themeToolStripMenuItem, Properties.Resources.tm_theme_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(languageToolStripMenuItem, Properties.Resources.tm_language_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(startupToolStripMenuItem, Properties.Resources.tm_startup_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(safetyWarningsToolStripMenuItem, Properties.Resources.tm_safety_warnings_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(checkForUpdateToolStripMenuItem, Properties.Resources.tm_update_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(passwordGeneratorToolStripMenuItem, Properties.Resources.tm_password_generator_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(donateToolStripMenuItem, Properties.Resources.tm_donate_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(aboutToolStripMenuItem, Properties.Resources.tm_about_light, 0, ContentAlignment.MiddleRight);
                    //
                    TSImageRenderer(BtnShowPassword, p_visible ? Properties.Resources.ct_hide_password_light : Properties.Resources.ct_show_password_light, 10, ContentAlignment.MiddleCenter);
                    TSImageRenderer(BtnRndPssGen, Properties.Resources.ct_generate_light, 10, ContentAlignment.MiddleCenter);
                    TSImageRenderer(BtnCopyPassword, Properties.Resources.ct_copy_mc_light, 12, ContentAlignment.MiddleCenter);
                    TSImageRenderer(BtnSavePath, Properties.Resources.ct_folder_light, 12, ContentAlignment.MiddleCenter);
                    //
                    TSImageRenderer(BtnSelect, Properties.Resources.ct_folder_light, 19, ContentAlignment.MiddleLeft);
                    TSImageRenderer(BtnBurner, p_mode ? Properties.Resources.ct_decrypt_light : Properties.Resources.ct_encrypt_light, 19, ContentAlignment.MiddleLeft);
                }else if (theme == 0){
                    // SETTINGS
                    TSImageRenderer(settingsToolStripMenuItem, Properties.Resources.tm_settings_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(themeToolStripMenuItem, Properties.Resources.tm_theme_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(languageToolStripMenuItem, Properties.Resources.tm_language_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(startupToolStripMenuItem, Properties.Resources.tm_startup_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(safetyWarningsToolStripMenuItem, Properties.Resources.tm_safety_warnings_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(checkForUpdateToolStripMenuItem, Properties.Resources.tm_update_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(passwordGeneratorToolStripMenuItem, Properties.Resources.tm_password_generator_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(donateToolStripMenuItem, Properties.Resources.tm_donate_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(aboutToolStripMenuItem, Properties.Resources.tm_about_dark, 0, ContentAlignment.MiddleRight);
                    //
                    TSImageRenderer(BtnShowPassword, p_visible ? Properties.Resources.ct_hide_password_dark : Properties.Resources.ct_show_password_dark, 10, ContentAlignment.MiddleCenter);
                    TSImageRenderer(BtnRndPssGen, Properties.Resources.ct_generate_dark, 10, ContentAlignment.MiddleCenter);
                    TSImageRenderer(BtnCopyPassword, Properties.Resources.ct_copy_mc_dark, 12, ContentAlignment.MiddleCenter);
                    TSImageRenderer(BtnSavePath, Properties.Resources.ct_folder_dark, 12, ContentAlignment.MiddleCenter);
                    //
                    TSImageRenderer(BtnSelect, Properties.Resources.ct_folder_dark, 19, ContentAlignment.MiddleLeft);
                    TSImageRenderer(BtnBurner, p_mode ? Properties.Resources.ct_decrypt_dark : Properties.Resources.ct_encrypt_dark, 19, ContentAlignment.MiddleLeft);
                }
                // HEADER
                header_colors[0] = TS_ThemeEngine.ColorMode(theme, "HeaderBGColorMain");
                header_colors[1] = TS_ThemeEngine.ColorMode(theme, "HeaderFEColorMain");
                header_colors[2] = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                HeaderMenu.Renderer = new HeaderMenuColors();
                // TOOLTIP
                MainToolTip.ForeColor = TS_ThemeEngine.ColorMode(theme, "HeaderFEColor");
                MainToolTip.BackColor = TS_ThemeEngine.ColorMode(theme, "HeaderBGColor");
                // HEADER MENU
                var bg = TS_ThemeEngine.ColorMode(theme, "HeaderBGColor");
                var fg = TS_ThemeEngine.ColorMode(theme, "HeaderFEColor");
                HeaderMenu.ForeColor = fg;
                HeaderMenu.BackColor = bg;
                SetMenuStripColors(HeaderMenu, bg, fg);
                // UI
                BackColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor2");
                //
                FAF_DGV.BackgroundColor = TS_ThemeEngine.ColorMode(theme, "DataGridBGColor");
                FAF_DGV.GridColor = TS_ThemeEngine.ColorMode(theme, "DataGridColor");
                FAF_DGV.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "DataGridBGColor");
                FAF_DGV.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "DataGridFEColor");
                FAF_DGV.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "DataGridAlternatingColor");
                FAF_DGV.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                FAF_DGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                FAF_DGV.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "DataGridSelectionColor");
                FAF_DGV.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                FAF_DGV.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(theme, "DataGridSelectionColor");
                //
                Progress_BG.BackColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor");
                Progress_FE.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                PanelControl.BackColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor");
                //
                Label_Password.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColorText");
                TextBox_Password.BackColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor2");
                TextBox_Password.ForeColor = TS_ThemeEngine.ColorMode(theme, "TextBoxFEColor");
                Label_SaveFolder.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColorText");
                //
                CheckOrjFileDelete.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColorText");
                CheckOrjFileDelete.CheckedColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CheckOrjFileDelete.CheckMarkColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor2");
                CheckOrjFileDelete.UncheckedBorderColor = TS_ThemeEngine.ColorMode(theme, "CheckBoxUnCheckBorderColor");
                //
                TextBox_SaveFolder.BackColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor2");
                TextBox_SaveFolder.ForeColor = TS_ThemeEngine.ColorMode(theme, "TextBoxFEColor");
                Label_Compress.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColorText");
                //
                var combinedBtnsControls = PanelBtns.Controls.Cast<Control>().Concat(PanelControl.Controls.Cast<Control>());
                foreach (Control control in combinedBtnsControls){
                    if (control is Button button){
                        button.ForeColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                        button.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                        button.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                        button.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                        button.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                    }
                }
                //
                Combo_Compress.BackColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor2");
                Combo_Compress.ForeColor = TS_ThemeEngine.ColorMode(theme, "TextBoxFEColor");
                Combo_Compress.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor2");
                Combo_Compress.ButtonColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                Combo_Compress.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                Combo_Compress.ArrowColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                Combo_Compress.BorderColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor3");
                Combo_Compress.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor3");
                Combo_Compress.DisabledBackColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor2");
                Combo_Compress.DisabledForeColor = TS_ThemeEngine.ColorMode(theme, "TextBoxFEColor");
                Combo_Compress.DisabledButtonColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                Combo_Compress.DisabledArrowColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                Combo_Compress.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TextBoxFEColor");
                Combo_Compress.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                Combo_Compress.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "UIBGColor2");
                //
                Software_other_page_preloader();
            }catch (Exception){ }
        }
        private void SetMenuStripColors(MenuStrip menuStrip, Color bgColor, Color fgColor){
            if (menuStrip == null) return;
            foreach (ToolStripItem item in menuStrip.Items){
                if (item is ToolStripMenuItem menuItem){
                    SetMenuItemColors(menuItem, bgColor, fgColor);
                }
            }
        }
        private void SetMenuItemColors(ToolStripMenuItem menuItem, Color bgColor, Color fgColor){
            if (menuItem == null) return;
            menuItem.BackColor = bgColor;
            menuItem.ForeColor = fgColor;
            foreach (ToolStripItem item in menuItem.DropDownItems){
                if (item is ToolStripMenuItem subMenuItem){
                    SetMenuItemColors(subMenuItem, bgColor, fgColor);
                }
            }
        }
        private void SetContextMenuColors(ContextMenuStrip contextMenu, Color bgColor, Color fgColor){
            if (contextMenu == null) return;
            foreach (ToolStripItem item in contextMenu.Items){
                if (item is ToolStripMenuItem menuItem){
                    SetMenuItemColors(menuItem, bgColor, fgColor);
                }
            }
        }
        private void Software_other_page_preloader(){
            // PASSWORD GENERATOR
            try{
                EncryphixPasswordGenerator software_pass_generator = new EncryphixPasswordGenerator();
                string software_pass_generator_name = "encryphix_password_generator";
                software_pass_generator.Name = software_pass_generator_name;
                if (Application.OpenForms[software_pass_generator_name] != null)
                {
                    software_pass_generator = (EncryphixPasswordGenerator)Application.OpenForms[software_pass_generator_name];
                    software_pass_generator.Password_generator_preloader();
                }
            }catch (Exception) { }
            // SOFTWARE ABOUT
            try{
                EncryphixAbout software_about = new EncryphixAbout();
                string software_about_name = "encryphix_about";
                software_about.Name = software_about_name;
                if (Application.OpenForms[software_about_name] != null){
                    software_about = (EncryphixAbout)Application.OpenForms[software_about_name];
                    software_about.About_preloader();
                }
            }catch (Exception){ }
        }
        // LANG MODE
        // ======================================================================================================
        private void Select_lang_active(object target_lang){
            ToolStripMenuItem selected_lang = null;
            Select_lang_deactive();
            if (target_lang != null){
                if (selected_lang != (ToolStripMenuItem)target_lang){
                    selected_lang = (ToolStripMenuItem)target_lang;
                    selected_lang.Checked = true;
                }
            }
        }
        private void Select_lang_deactive(){
            foreach (ToolStripMenuItem disabled_lang in languageToolStripMenuItem.DropDownItems){
                disabled_lang.Checked = false;
            }
        }
        private void LanguageToolStripMenuItem_Click(object sender, EventArgs e){
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string langCode){
                if (lang != langCode && AllLanguageFiles.ContainsKey(langCode)){
                    Lang_preload(AllLanguageFiles[langCode], langCode);
                    Select_lang_active(sender);
                }
            }
        }
        private void Lang_preload(string lang_type, string lang_code){
            Lang_engine(lang_type, lang_code);
            try{
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "LanguageStatus", lang_code);
            }catch (Exception){ }
            // LANG CHANGE NOTIFICATION
            // TSGetLangs software_lang = new TSGetLangs(lang_path);
            // DialogResult lang_change_message = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("LangChange", "lang_change_notification"), "\n\n", "\n\n"));
            // if (lang_change_message == DialogResult.Yes) { Application.Restart(); }
        }
        private void Lang_engine(string lang_type, string lang_code){
            try{
                lang_path = lang_type;
                lang = lang_code;
                // GLOBAL ENGINE
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                // PROTECTION ERRORS
                TSProtectionErrorMessages.Messages["FolderEncryptionError"] = software_lang.TSReadLangs("TSProtection", "tsp_folder_encryption_error");
                TSProtectionErrorMessages.Messages["SaltReadError"] = software_lang.TSReadLangs("TSProtection", "tsp_salt_read_error");
                TSProtectionErrorMessages.Messages["FileTypeReadError"] = software_lang.TSReadLangs("TSProtection", "tsp_file_type_read_error");
                TSProtectionErrorMessages.Messages["ExtLengthReadError"] = software_lang.TSReadLangs("TSProtection", "tsp_ext_length_read_error");
                TSProtectionErrorMessages.Messages["InvalidExtensionLength"] = software_lang.TSReadLangs("TSProtection", "tsp_invalid_extension_length_error");
                TSProtectionErrorMessages.Messages["ExtensionReadError"] = software_lang.TSReadLangs("TSProtection", "tsp_extension_read_error");
                TSProtectionErrorMessages.Messages["IVReadError"] = software_lang.TSReadLangs("TSProtection", "tsp_iv_read_error");
                TSProtectionErrorMessages.Messages["InvalidPasswordOrCorruptFile"] = software_lang.TSReadLangs("TSProtection", "tsp_invalid_password_or_corrupt_file");
                TSProtectionErrorMessages.Messages["AccessError"] = software_lang.TSReadLangs("TSProtection", "tsp_access_error");
                TSProtectionErrorMessages.Messages["UnknownError"] = software_lang.TSReadLangs("TSProtection", "tsp_unknown_error");
                TSProtectionErrorMessages.Messages["InvalidFolderPath"] = software_lang.TSReadLangs("TSProtection", "tsp_invalid_folder_path");
                TSProtectionErrorMessages.Messages["InvalidPassword"] = software_lang.TSReadLangs("TSProtection", "tsp_invalid_password");
                TSProtectionErrorMessages.Messages["FolderNotFound"] = software_lang.TSReadLangs("TSProtection", "tsp_folder_not_found");
                TSProtectionErrorMessages.Messages["InvalidInputFile"] = software_lang.TSReadLangs("TSProtection", "tsp_invalid_input_file");
                TSProtectionErrorMessages.Messages["InvalidOutputFile"] = software_lang.TSReadLangs("TSProtection", "tsp_invalid_output_file");
                TSProtectionErrorMessages.Messages["FileNotFound"] = software_lang.TSReadLangs("TSProtection", "tsp_file_not_found");
                TSProtectionErrorMessages.Messages["CorruptedFileOrTampered"] = software_lang.TSReadLangs("TSProtection", "tsp_corrupted_file_or_tampered");
                TSProtectionErrorMessages.Messages["HMACReadError"] = software_lang.TSReadLangs("TSProtection", "tsp_hmac_read_error");
                // SETTINGS
                settingsToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_settings");
                // THEMES
                themeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_theme");
                lightThemeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderThemes", "theme_light");
                darkThemeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderThemes", "theme_dark");
                systemThemeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderThemes", "theme_system");
                // LANGS
                languageToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_language");
                arabicToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_ar");
                chineseToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_zh");
                englishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_en");
                dutchToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_nl");
                frenchToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_fr");
                germanToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_de");
                hindiToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_hi");
                italianToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_it");
                japaneseToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_ja");
                koreanToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_ko");
                polishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_pl");
                portugueseToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_pt");
                russianToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_ru");
                spanishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_es");
                turkishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_tr");
                // STARTUP MODE
                startupToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_start");
                windowedToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderViewMode", "header_view_mode_windowed");
                fullScreenToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderViewMode", "header_view_mode_full_screen");
                // SAFETY WARNINGS
                safetyWarningsToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_safety_warnings");
                safetyWarningsOnToolStripMenuItem.Text = software_lang.TSReadLangs("SafetyWarnings", "sw_on");
                safetyWarningsOffToolStripMenuItem.Text = software_lang.TSReadLangs("SafetyWarnings", "sw_off");
                // UPDATE
                checkForUpdateToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_update");
                // PASSWORD GENERATOR
                passwordGeneratorToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_pass_gen");
                // DONATE
                donateToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_donate");
                // ABOUT
                aboutToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_about");
                // UI
                FAF_DGV.Columns[0].HeaderText = software_lang.TSReadLangs("EncryphixGraphics", "eg_faf_path");
                FAF_DGV.Columns[1].HeaderText = software_lang.TSReadLangs("EncryphixGraphics", "eg_faf_size");
                FAF_DGV.Columns[2].HeaderText = software_lang.TSReadLangs("EncryphixGraphics", "eg_faf_status");
                // DGV LAST ROW CHANGER
                if (FAF_DGV.Rows.Count > 0){
                    string encryptedText = software_lang.TSReadLangs("EncryphixUI", "eui_encrypted_text");
                    string decryptedText = software_lang.TSReadLangs("EncryphixUI", "eui_decrypted_text");
                    for (int i = 0; i < FAF_DGV.Rows.Count; i++){
                        var row = FAF_DGV.Rows[i];
                        object cellValue = row.Cells[3].Value;
                        if (cellValue is bool isEncrypted){
                            row.Cells[2].Value = isEncrypted ? encryptedText : decryptedText;
                        }
                    }
                }
                //
                BtnSelect.Text = " " + software_lang.TSReadLangs("EncryphixGraphics", "eg_select");
                Label_Password.Text = software_lang.TSReadLangs("EncryphixGraphics", "eg_password_label");
                Label_SaveFolder.Text = software_lang.TSReadLangs("EncryphixGraphics", "eg_saved_folder_label");
                CheckOrjFileDelete.Text = software_lang.TSReadLangs("EncryphixGraphics", "eg_saved_orj_file_label");
                //
                Label_Compress.Text = software_lang.TSReadLangs("EncryphixGraphics", "eg_compress_label");
                Combo_Compress.Items[0] = software_lang.TSReadLangs("EncryphixGraphics", "eg_compress_1");
                Combo_Compress.Items[1] = software_lang.TSReadLangs("EncryphixGraphics", "eg_compress_2");
                Combo_Compress.Items[2] = software_lang.TSReadLangs("EncryphixGraphics", "eg_compress_3");
                //
                BtnBurner.Text = p_mode ? " " + software_lang.TSReadLangs("EncryphixUI", "eui_decrypt_text") : " " + software_lang.TSReadLangs("EncryphixUI", "eui_encrypt_text");
                //
                MainToolTip.RemoveAll();
                MainToolTip.SetToolTip(BtnShowPassword, p_visible ? software_lang.TSReadLangs("EncryphixGraphics", "eg_password_show_hover") : software_lang.TSReadLangs("EncryphixGraphics", "eg_password_hide_hover"));
                MainToolTip.SetToolTip(BtnRndPssGen, software_lang.TSReadLangs("EncryphixGraphics", "eg_password_generate_hover"));
                MainToolTip.SetToolTip(BtnCopyPassword, software_lang.TSReadLangs("EncryphixGraphics", "eg_password_copy_hover"));
                MainToolTip.SetToolTip(BtnSavePath, software_lang.TSReadLangs("EncryphixGraphics", "eg_saved_folder_hover"));
                MainToolTip.SetToolTip(CheckOrjFileDelete, software_lang.TSReadLangs("EncryphixGraphics", "eg_saved_orj_file_hover"));
            }catch (Exception){ }
        }
        // STARTUP SETINGS
        // ======================================================================================================
        private void Select_startup_mode_active(object target_startup_mode){
            ToolStripMenuItem selected_startup_mode = null;
            Select_startup_mode_deactive();
            if (target_startup_mode != null){
                if (selected_startup_mode != (ToolStripMenuItem)target_startup_mode){
                    selected_startup_mode = (ToolStripMenuItem)target_startup_mode;
                    selected_startup_mode.Checked = true;
                }
            }
        }
        private void Select_startup_mode_deactive(){
            foreach (ToolStripMenuItem disabled_startup in startupToolStripMenuItem.DropDownItems){
                disabled_startup.Checked = false;
            }
        }
        private void WindowedToolStripMenuItem_Click(object sender, EventArgs e){
            if (startup_status != 0){ startup_status = 0; Startup_mode_settings("0"); Select_startup_mode_active(sender); }
        }
        private void FullScreenToolStripMenuItem_Click(object sender, EventArgs e){
            if (startup_status != 1){ startup_status = 1; Startup_mode_settings("1"); Select_startup_mode_active(sender); }
        }
        private void Startup_mode_settings(string get_startup_value){
            try{
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "StartupStatus", get_startup_value);
            }catch (Exception){ }
        }

        // SAFETY WARNINGS SETINGS
        // ======================================================================================================
        private void Safety_warnings_mode_active(object target_safety_mode){
            ToolStripMenuItem selected_safety_mode = null;
            Safety_warnings_mode_deactive();
            if (target_safety_mode != null){
                if (selected_safety_mode != (ToolStripMenuItem)target_safety_mode){
                    selected_safety_mode = (ToolStripMenuItem)target_safety_mode;
                    selected_safety_mode.Checked = true;
                }
            }
        }
        private void Safety_warnings_mode_deactive(){
            foreach (ToolStripMenuItem disabled_safety in safetyWarningsToolStripMenuItem.DropDownItems){
                disabled_safety.Checked = false;
            }
        }
        private void SafetyOnToolStripMenuItem_Click(object sender, EventArgs e){
            if (safety_warnings_status != 1){ safety_warnings_status = 1; Safety_warnings_mode_settings("1"); Safety_warnings_mode_active(sender); }
        }
        private void SafetyOffToolStripMenuItem_Click(object sender, EventArgs e){
            if (safety_warnings_status != 0){ safety_warnings_status = 0; Safety_warnings_mode_settings("0"); Safety_warnings_mode_active(sender); }
        }
        private void Safety_warnings_mode_settings(string get_safety_warnings_value){
            try{
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "SafetyWarnings", get_safety_warnings_value);
            }catch (Exception){ }
        }
        // UPDATE CHECK ENGINE
        // ======================================================================================================
        private void CheckForUpdateToolStripMenuItem_Click(object sender, EventArgs e){
            Task.Run(() => Software_update_check(1));
        }
        public async void Software_update_check(int _check_update_ui){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                SetUpdateMenuEnabled(false);
                if (!await IsNetworkAvailable()){
                    if (_check_update_ui == 1){
                        TS_MessageBoxEngine.TS_MessageBox(this, 2, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_not_connection"), "\n\n"), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                    }
                    return;
                }
                using (HttpClientHandler handler = new HttpClientHandler()){
                    handler.UseProxy = false;
                    using (HttpClient httpClient = new HttpClient(handler)){
                        httpClient.Timeout = TimeSpan.FromSeconds(15);
                        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue{ NoCache = true, NoStore = true, MustRevalidate = true };
                        httpClient.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
                        string versionUrl = TS_LinkSystem.github_link_lv;
                        versionUrl += (versionUrl.Contains("?") ? "&" : "?") + "_ts=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string response = await httpClient.GetStringAsync(versionUrl);
                        string firstLine = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)[0];
                        string client_version_raw = TS_VersionParser.ParseUINormalize(Application.ProductVersion);
                        string last_version_raw = TS_VersionParser.ParseUINormalize(firstLine.Split(new[] { '=' }, 2)[1].Trim());
                        Version client_ver = Version.Parse(client_version_raw);
                        Version last_ver = Version.Parse(last_version_raw);
                        if (client_ver < last_ver){
                            DialogResult info_update = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_available"), Application.ProductName, "\n\n", client_version_raw, "\n", last_version_raw, "\n\n"), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                            if (info_update == DialogResult.Yes){
                                try{
                                    string updaterPath = Path.Combine(Application.StartupPath, Program.updater_exe_name);
                                    if (File.Exists(updaterPath)){
                                        string procName = Path.GetFileNameWithoutExtension(updaterPath);
                                        bool isRunning = Process.GetProcessesByName(procName).Length > 0;
                                        if (!isRunning){
                                            Process.Start(new ProcessStartInfo(updaterPath) { UseShellExecute = true, Arguments = $"-app={Application.ProductName}" });
                                        }else{
                                            TS_MessageBoxEngine.TS_MessageBox(this, 1, software_lang.TSReadLangs("SoftwareUpdate", "su_ts_updater_c_running"), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                                        }
                                        Application.Exit();
                                        return;
                                    }else{
                                        TS_MessageBoxEngine.TS_MessageBox(this, 2, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_ts_updater_not_available"), Program.updater_exe_name), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                                        Process.Start(new ProcessStartInfo(TS_LinkSystem.github_link_lr) { UseShellExecute = true });
                                        Application.Exit();
                                        return;
                                    }
                                }catch (Exception ex){
                                    Debug.WriteLine(ex, $"{Program.updater_exe_name} launch block.");
                                }
                            }
                        }else if (_check_update_ui == 1){
                            string update_msg = client_ver == last_ver ? string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_not_available"), Application.ProductName, "\n", client_version_raw) : string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_newer"), "\n\n", $"v{client_version_raw}");
                            TS_MessageBoxEngine.TS_MessageBox(this, 1, update_msg, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                        }
                    }
                }
            }catch (Exception ex){
                Debug.WriteLine(ex, "Software_update_check()");
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_error"), "\n\n", ex.Message), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
            }finally{
                SetUpdateMenuEnabled(true);
            }
        }
        private void SetUpdateMenuEnabled(bool enabled){
            if (InvokeRequired){
                BeginInvoke(new Action(() => checkForUpdateToolStripMenuItem.Enabled = enabled));
            }else{
                checkForUpdateToolStripMenuItem.Enabled = enabled;
            }
        }
        // DONATE LINK
        // ======================================================================================================
        private void DonateToolStripMenuItem_Click(object sender, EventArgs e){
            try{
                Process.Start(new ProcessStartInfo(TS_LinkSystem.ts_donate) { UseShellExecute = true });
            }catch (Exception){ }
        }
        // TS TOOL LAUNCHER MODULE
        // ======================================================================================================
        private void TSToolLauncher<T>(string formName, string langKey) where T : Form, new(){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                T tool = new T { Name = formName };
                if (Application.OpenForms[formName] == null){
                    tool.Show();
                }else{
                    if (Application.OpenForms[formName].WindowState == FormWindowState.Minimized){
                        Application.OpenForms[formName].WindowState = FormWindowState.Normal;
                    }
                    string public_message = string.Format(software_lang.TSReadLangs("HeaderHelp", "header_help_info_notification"), software_lang.TSReadLangs("HeaderMenu", langKey));
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, public_message);
                    Application.OpenForms[formName].Activate();
                }
            }catch (Exception){ }
        }
        // PASSWORD GENERATOR
        // ======================================================================================================
        private void PasswordGeneratorToolStripMenuItem_Click(object sender, EventArgs e){
            TSToolLauncher<EncryphixPasswordGenerator>("encryphix_password_generator", "header_menu_pass_gen");
        }
        // ABOUT
        // ======================================================================================================
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e){
            TSToolLauncher<EncryphixAbout>("encryphix_about", "header_menu_about");
        }
        // EXIT
        // ======================================================================================================
        private void Software_exit() { Application.Exit(); }
        private void Encryphix_FormClosing(object sender, FormClosingEventArgs e){ Software_exit(); }
    }
}