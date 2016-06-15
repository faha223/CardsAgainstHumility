using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CardsAgainstHumility
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

            txtPlayerName.Text = CardsAgainstHumility.PlayerName;
            txtHostName.Text = CardsAgainstHumility.Host;

            Button btnSaveChanges = FindViewById<Button>(Resource.Id.s_btnSave);
            btnSaveChanges.Click += delegate
            {
                using (var settings = GetSharedPreferences("CardsAgainstHumility", FileCreationMode.Private))
                using (var editor = settings.Edit())
                {
                    editor.PutString("PlayerName", txtPlayerName.Text);
                    editor.PutString("Host", txtHostName.Text);
                    editor.Commit();
                }

                CardsAgainstHumility.PlayerName = txtPlayerName.Text;
                CardsAgainstHumility.Host = txtHostName.Text;
                Finish();
            };
        }
    }
}