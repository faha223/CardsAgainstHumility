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
using Android.Graphics;

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
            Button btnSaveChanges = FindViewById<Button>(Resource.Id.s_btnSave);
            if (UIAssets.AppFont != null)
            {
                FindViewById<TextView>(Resource.Id.s_lblPlayerName).SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                FindViewById<TextView>(Resource.Id.s_lblServerAddress).SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                FindViewById<TextView>(Resource.Id.s_lblServerPort).SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                txtPlayerName.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                txtHostName.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                txtPortNumber.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
                btnSaveChanges.SetTypeface(UIAssets.AppFont, TypefaceStyle.Normal);
            }

            txtPlayerName.Text = CardsAgainstHumility.PlayerName;
            txtHostName.Text = CardsAgainstHumility.Host;
            txtPortNumber.Text = CardsAgainstHumility.Port.ToString();

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