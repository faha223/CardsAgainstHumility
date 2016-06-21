using Android.Accounts;
using Android.Content;
using Android.Database;
using Android.Provider;
using System;
using System.Linq;

namespace CardsAgainstHumility.Helpers
{
    public static class UserInfo
    {
        public static string GetUserName(Context ctx)
        {
            string UserName = null;
            UserName = GetProfileName(ctx);
            if (UserName != null) return UserName;
            UserName = GetAccountName(ctx);
            if (UserName != null) return UserName;

            return Environment.UserName;
        }

        private static string GetProfileName(Context ctx)
        {
            var loader = new Android.Support.V4.Content.CursorLoader(ctx, Android.Net.Uri.WithAppendedPath(ContactsContract.Profile.ContentUri, ContactsContract.Contacts.Data.ContentDirectory), Projection, null, null, null);
            var cursor = (ICursor)loader.LoadInBackground();
            if (cursor.MoveToFirst())
            {
                var DisplayName = "";
                do
                {
                    DisplayName = cursor.GetString(cursor.GetColumnIndex(Projection[0]));
                } while (cursor.MoveToNext());
                return DisplayName;
            }
            return null;
        }

        private static string GetAccountName(Context ctx)
        {
            var manager = AccountManager.Get(ctx);
            var accounts = manager.GetAccountsByType("com.google");
            foreach(var acc in accounts)
            {
                return acc.Name.Substring(0, acc.Name.IndexOf("@"));
            }
            return null;
        }

        private static string[] Projection
        {
            get
            {
                return new string[]
                {
                    ContactsContract.Profile.InterfaceConsts.NameRawContactId
                };
            }
        }
    }
}