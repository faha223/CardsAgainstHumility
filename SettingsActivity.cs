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
        private EditText txtPortNumber;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SettingsMenu);

            txtPlayerName = FindViewById<EditText>(Resource.Id.s_txtPlayerName);
            txtHostName = FindViewById<EditText>(Resource.Id.s_txtServerAddress);
            txtPortNumber = FindViewById<EditText>(Resource.Id.s_txtServerPort);

            txtPlayerName.Text = CardsAgainstHumility.PlayerName;
            txtHostName.Text = CardsAgainstHumility.Host;
            txtPortNumber.Text = CardsAgainstHumility.Port.ToString();

            Button btnSaveChanges = FindViewById<Button>(Resource.Id.s_btnSave);
            btnSaveChanges.Click += delegate
            {
                int port;
                if(!int.TryParse(txtPortNumber.Text, out port))
                {
                    return;
                }
                using (var settings = GetSharedPreferences("CardsAgainstHumility", FileCreationMode.Private))
                using (var editor = settings.Edit())
                {
                    editor.PutString("PlayerName", txtPlayerName.Text);
                    editor.PutString("Host", txtHostName.Text);
                    editor.PutInt("Port", port);
                    editor.Commit();
                }

                CardsAgainstHumility.PlayerName = txtPlayerName.Text;
                CardsAgainstHumility.Host = txtHostName.Text;
                CardsAgainstHumility.Port = port;
                Finish();
            };
        }
    }
}