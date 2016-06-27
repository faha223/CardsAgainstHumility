using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using CardsAgainstHumility.Android.Settings;
using CardsAgainstHumility.Android.UIHelpers;

namespace CardsAgainstHumility.Android
{
    [Activity(Label = "SettingsActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class SettingsActivity : Activity
    {
        private EditText txtPlayerName;
        private EditText txtHostName;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SettingsMenu);

            txtPlayerName = FindViewById<EditText>(Resource.Id.s_txtPlayerName);
            txtHostName = FindViewById<EditText>(Resource.Id.s_txtServerAddress);
            Button btnSaveChanges = FindViewById<Button>(Resource.Id.s_btnSave);
            if (UIAssets.AppFont != null)
            {
                FindViewById<TextView>(Resource.Id.s_lblPlayerName).SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                FindViewById<TextView>(Resource.Id.s_lblServerAddress).SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                txtPlayerName.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                txtHostName.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                btnSaveChanges.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            }

            txtPlayerName.Text = CardsAgainstHumility.PlayerName;
            txtHostName.Text = CardsAgainstHumility.Host;

            btnSaveChanges.Click += delegate
            {
                using (var settings = GetSharedPreferences("CardsAgainstHumility", FileCreationMode.Private))
                using (var editor = settings.Edit())
                {
                    editor.PutString(SettingsConstants.playerNameKey, txtPlayerName.Text);
                    editor.PutString(SettingsConstants.hostKey, txtHostName.Text);
                    editor.Commit();
                }

                CardsAgainstHumility.PlayerName = txtPlayerName.Text;
                CardsAgainstHumility.Host = txtHostName.Text;
                Finish();
            };
        }
    }
}