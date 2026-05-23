using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
// TS MODULES
using static Encryphix.TSModules;

namespace Encryphix{
    public partial class EncryphixPasswordGenerator : Form{
        // ENCRYPHİX PASS GENERATOR CLASS
        // ======================================================================================================
        public class TS_EncryphixPasswordGenerator{
            private readonly Random random_gen = new Random();
            private readonly Dictionary<string, (string upper, string lower, string digit, string special)> modes;
            public TS_EncryphixPasswordGenerator(){
                modes = new Dictionary<string, (string, string, string, string)> {
                    { "readable", ("ABCDEFGHJKLMNPQRSTUVWXYZ", "abcdefghjkmnpqrstuvwxyz", "23456789", "") },
                    { "writable", ("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz", "0123456789", "-_") },
                    { "random", ("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz", "0123456789", "!@#$%^&*()-_=+[]{}|;:,.<>?") }
                };
            }
            public string EncryphixGeneratePassword(bool includeUppercase, bool includeLowercase, bool includeNumeric, bool includeSpecialChars, string mode, int passwordLength){
                var (upper, lower, digit, special) = modes[mode];
                var charSet = new StringBuilder();
                //
                foreach (var (condition, chars) in new[]{
                    (includeUppercase, upper),
                    (includeLowercase, lower),
                    (includeNumeric, digit),
                    (includeSpecialChars, special)
                }){
                    if (condition) charSet.Append(chars);
                }
                //
                if (charSet.Length == 0){
                    TSGetLangs lang = new TSGetLangs(EncryphixMain.lang_path);
                    throw new ArgumentException(lang.TSReadLangs("EncryphixPasswordGenerator", "epg_feature_info"));
                }
                //
                var password = new StringBuilder();
                for (int i = 0; i < passwordLength; i++){
                    password.Append(charSet[random_gen.Next(charSet.Length)]);
                }
                return password.ToString();
            }
        }
        // AUXILIARY METHODS
        // ======================================================================================================
        private void SetControlColors<T>(Control container, Action<T> setColors) where T : Control{
            foreach (Control control in container.Controls){
                if (control is T typedControl)
                    setColors(typedControl);
            }
        }
        // FORM EVENTS & INIT
        // ======================================================================================================
        private readonly TS_EncryphixPasswordGenerator generator;
        private readonly Dictionary<string, (bool upperEnabled, bool lowerEnabled, bool numericEnabled, bool specialEnabled, bool upperChecked, bool lowerChecked, bool numericChecked, bool specialChecked)> modes;
        public EncryphixPasswordGenerator(){
            InitializeComponent();
            generator = new TS_EncryphixPasswordGenerator();
            modes = new Dictionary<string, (bool, bool, bool, bool, bool, bool, bool, bool)>(){
                { "readable", (false, true, true, false, false, true, true, false) },
                { "writable", (true, true, true, true, true, true, true, true) },
                { "random", (true, true, true, true, true, true, true, true) }
            };
            //
            RadioRead.CheckedChanged += RadioButton_CheckedChanged;
            RadioWrite.CheckedChanged += RadioButton_CheckedChanged;
            RadioMixed.CheckedChanged += RadioButton_CheckedChanged;
        }
        public void Password_generator_preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                MainToolTip.ForeColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "HeaderFEColor2");
                MainToolTip.BackColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "HeaderBGColor2");
                //
                BackColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "PageContainerUIBGColor");
                Panel_BG.BackColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "HeaderBGColor2");
                //
                SetControlColors<Label>(Panel_BG, lbl => lbl.ForeColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "ContentLabelLeftColor"));
                SetControlColors<TextBox>(Panel_BG, tb => {
                    tb.BackColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "TextboxBGColor");
                    tb.ForeColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "TextboxFEColor");
                });
                SetControlColors<Button>(Panel_BG, btn => {
                    btn.ForeColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "DynamicThemeActiveBtnBGColor");
                    var color = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                    btn.BackColor = color;
                    btn.FlatAppearance.BorderColor = color;
                    btn.FlatAppearance.MouseDownBackColor = color;
                    btn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColorHover");
                });
                //
                LabelHeader.BackColor = Panel_Feature.BackColor = Panel_Mode.BackColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "PageContainerUIBGColor");
                LabelHeader.ForeColor = LabelFeature.ForeColor = LabelMode.ForeColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "ContentLabelLeftColor");
                LabelFeature.BackColor = LabelMode.BackColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "HeaderBGColor2");
                //
                SetControlColors<CheckBox>(Panel_Feature, cb => cb.ForeColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "ContentLabelLeftColor"));
                SetControlColors<RadioButton>(Panel_Mode, rb => rb.ForeColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "ContentLabelLeftColor"));
                //
                CheckUppercase.CheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                CheckUppercase.CheckMarkColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "SelectBoxBGColor");
                CheckUppercase.UncheckedBorderColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "CheckBoxUnCheckBorderColor");
                CheckLowercase.CheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                CheckLowercase.CheckMarkColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "SelectBoxBGColor");
                CheckLowercase.UncheckedBorderColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "CheckBoxUnCheckBorderColor");
                CheckNumeric.CheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                CheckNumeric.CheckMarkColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "SelectBoxBGColor");
                CheckNumeric.UncheckedBorderColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "CheckBoxUnCheckBorderColor");
                CheckSpecialChars.CheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                CheckSpecialChars.CheckMarkColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "SelectBoxBGColor");
                CheckSpecialChars.UncheckedBorderColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "CheckBoxUnCheckBorderColor");
                //
                RadioRead.UnCheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "CheckBoxUnCheckBorderColor");
                RadioRead.CheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                RadioWrite.UnCheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "CheckBoxUnCheckBorderColor");
                RadioWrite.CheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                RadioMixed.UnCheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "CheckBoxUnCheckBorderColor");
                RadioMixed.CheckedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                //
                PassLenghtLabel.BackColor = PassGenLenght.BackColor = PassResultLabel.BackColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "PageContainerUIBGColor");
                PassLenghtLabel.ForeColor = PassGenLenght.ForeColor = PassResultLabel.ForeColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "ContentLabelLeftColor");
                PassGenLenght.TrackColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "TrackColor");
                PassGenLenght.ThumbColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                PassGenLenght.ThumbHoverColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColorHover");
                PassGenLenght.ThumbPressedColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColorHover");
                PassGenLenght.TrackFillColor = TS_ThemeEngine.ColorMode(EncryphixMain.theme, "AccentColor");
                //
                TSImageRenderer(BtnGenPass, EncryphixMain.theme == 1 ? Properties.Resources.ct_generate_light : Properties.Resources.ct_generate_dark, 18, ContentAlignment.MiddleRight);
                //
                var lang = new TSGetLangs(EncryphixMain.lang_path);
                Text = string.Format(lang.TSReadLangs("EncryphixPasswordGenerator", "epg_title"), Application.ProductName);
                LabelHeader.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_header");
                LabelFeature.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_feature_title");
                LabelMode.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_mode_title");
                CheckUppercase.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_feature_uppercase");
                CheckLowercase.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_feature_lowercase");
                CheckNumeric.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_feature_numeric");
                CheckSpecialChars.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_feature_special_chars");
                RadioRead.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_mode_easy_read");
                RadioWrite.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_mode_easy_write");
                RadioMixed.Text = lang.TSReadLangs("EncryphixPasswordGenerator", "epg_mode_mixed");
                //
                MainToolTip.RemoveAll();
                MainToolTip.SetToolTip(PassResultLabel, lang.TSReadLangs("EncryphixPasswordGenerator", "epg_pass_copy"));
                BtnGenPass.Text = " " + lang.TSReadLangs("EncryphixPasswordGenerator", "epg_gen_pass_btn");
            }catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
        // TOOLTIP SETTINGS
        // ======================================================================================================
        private void MainToolTip_Draw(object sender, DrawToolTipEventArgs e) { e.DrawBackground(); e.DrawBorder(); e.DrawText(); }
        // LOAD
        // ======================================================================================================
        private void EncryphixPasswordGenerator_Load(object sender, EventArgs e){
            AcceptButton = BtnGenPass;
            TSGetLangs lang = new TSGetLangs(EncryphixMain.lang_path);
            PassLenghtLabel.Text = string.Format(lang.TSReadLangs("EncryphixPasswordGenerator", "epg_password_length"), PassGenLenght.Value);
            Password_generator_preloader();
        }
        // UI FUNCTIONS
        // ======================================================================================================
        private void RadioButton_CheckedChanged(object sender, EventArgs e){
            string mode = RadioRead.Checked ? "readable" : RadioWrite.Checked ? "writable" : "random";
            UpdateCheckboxesByMode(mode);
        }
        private void UpdateCheckboxesByMode(string mode){
            var (_, _, _, _, upperChecked, lowerChecked, numericChecked, specialChecked) = modes[mode];
            CheckUppercase.Checked = upperChecked;
            CheckLowercase.Checked = lowerChecked;
            CheckNumeric.Checked = numericChecked;
            CheckSpecialChars.Checked = specialChecked;
        }
        private void PassGenLenght_ValueChanged(object sender, EventArgs e){
            TSGetLangs lang = new TSGetLangs(EncryphixMain.lang_path);
            PassLenghtLabel.Text = string.Format(lang.TSReadLangs("EncryphixPasswordGenerator", "epg_password_length"), PassGenLenght.Value);
        }
        private void Encryphix_pass_gen_engine(){
            string mode = RadioRead.Checked ? "readable" : RadioWrite.Checked ? "writable" : "random";
            try{
                string password = generator.EncryphixGeneratePassword(
                    CheckUppercase.Checked,
                    CheckLowercase.Checked,
                    CheckNumeric.Checked,
                    CheckSpecialChars.Checked,
                    mode,
                    PassGenLenght.Value
                );
                PassResultLabel.Text = password;
            }catch (Exception ex){
                TS_MessageBoxEngine.TS_MessageBox(this, 2, ex.Message);
            }
        }
        // LAUNCHER
        // ======================================================================================================
        private void BtnGenPass_Click(object sender, EventArgs e) => Encryphix_pass_gen_engine();
        // COPY PASSWORD
        // ======================================================================================================
        private void PassResultLabel_DoubleClick(object sender, EventArgs e){
            if (!string.IsNullOrWhiteSpace(PassResultLabel.Text)){
                Clipboard.SetText(PassResultLabel.Text.Trim());
                TSGetLangs lang = new TSGetLangs(EncryphixMain.lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 1, lang.TSReadLangs("EncryphixPasswordGenerator", "epg_copy_password"));
            }
        }
        // PASSWORD HOVER TEXT
        // ======================================================================================================
        private void PassResultLabel_MouseEnter(object sender, EventArgs e){
            MainToolTip.RemoveAll();
            if (!string.IsNullOrEmpty(PassResultLabel.Text)){
                TSGetLangs lang = new TSGetLangs(EncryphixMain.lang_path);
                MainToolTip.SetToolTip(PassResultLabel, lang.TSReadLangs("EncryphixPasswordGenerator", "epg_pass_copy"));
            }
        }
    }
}