﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SecuViewer.Cryption;

namespace SecuViewer.Droid
{
    [Activity(Label = "SFV file")]
    [IntentFilter(new[] { Intent.ActionSend, Intent.ActionOpenDocument, Intent.ActionView },
                  Categories = new[] { Intent.CategoryDefault },
                  DataMimeType = "text/plain")]
    public class FileSubscribedIntentListener : Activity
    {
        GridLayout SettingsGrid = null;
        RadioButton DecryptRadioButton = null, LoadPlainRadioButton = null;
        EditText PasswordBox = null, ContentViewer = null;
        Button OpenFile = null;

        Android.Net.Uri ContentReference;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.FileDecrypterLoader);

            // Create your application here
            SetupControl(out SettingsGrid, Resource.Id.SettingsGrid);
            SetupControl(out DecryptRadioButton, Resource.Id.DecryptRB);
            SetupControl(out LoadPlainRadioButton, Resource.Id.LoadPlainRB);
            SetupControl(out PasswordBox, Resource.Id.PasswordBox);
            SetupControl(out ContentViewer, Resource.Id.ContentViewer);
            SetupControl(out OpenFile, Resource.Id.OpenFile);

            DecryptRadioButton.CheckedChange += DecryptRadioButton_CheckedChange;
            LoadPlainRadioButton.CheckedChange += LoadPlainRadioButton_CheckedChange;
            PasswordBox.TextChanged += PasswordBox_TextChanged;
            OpenFile.Click += OpenFile_Click;

            ContentReference = (Android.Net.Uri)(Intent.Data ?? Intent.GetParcelableExtra(Intent.ExtraStream));
        }

        private void LoadPlainRadioButton_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            DecryptRadioButton.Checked = !LoadPlainRadioButton.Checked;
        }

        private void DecryptRadioButton_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            PasswordBox.Enabled = DecryptRadioButton.Checked;
            if (PasswordBox.Enabled)
            {
                PasswordBox_TextChanged(sender, null);
            }
            else
            {
                OpenFile.Enabled = true;
            }
            LoadPlainRadioButton.Checked = !DecryptRadioButton.Checked;
        }

        private void PasswordBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            OpenFile.Enabled = PasswordBox.Text.Length >= 0;
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            var stream = ContentResolver.OpenInputStream(ContentReference);
            if (DecryptRadioButton.Checked)
            {
                const int MEGABYTE = 1024 * 1024;
                var tempBuffers = new List<byte[]>();
                int totalRead = 0, read = 0;
                //read all into memory
                do
                {
                    var tempBuffer = new byte[MEGABYTE];
                    read = stream.Read(tempBuffer, 0, MEGABYTE);
                    if (read > 0)
                    {
                        totalRead += read;
                        tempBuffers.Add(tempBuffer);
                    }
                } while (read == MEGABYTE);
                //assemble back into one buffer 
                var buffer = new byte[totalRead];
                for (int i = 0; i < tempBuffers.Count; i++)
                {
                    Array.Copy(tempBuffers[i], 0, buffer, i * MEGABYTE, i == tempBuffers.Count - 1 ? totalRead % MEGABYTE : MEGABYTE);
                }
                //decrypt
                string decoded;
                using (var wrapper = new MemoryStream(buffer))
                {
                    decoded = Crypter.Decrypt(VocabularyProviderFactory.CreateProvider(PasswordBox.Text), wrapper);
                }
                //replace tabs with tab stops
                var builder = new StringBuilder();
                int counter = 0;
                for (int i = 0; i < decoded.Length; i++)
                {
                    switch (decoded[i])
                    {
                        case '\r':
                        case '\n':
                            counter = 0;
                            builder.Append(decoded[i]);
                            break;
                        case '\t':
                            var delta = 8 - (counter % 8);
                            counter += delta;
                            builder.Append(' ', delta);
                            break;
                        default:
                            counter++;
                            builder.Append(decoded[i]);
                            break;
                    }
                }
                //set content
                ContentViewer.Text = builder.ToString();
            }
            else
            {
                using (var istr = new StreamReader(stream))
                {
                    ContentViewer.Text = istr.ReadToEnd();
                }
            }
            SettingsGrid.Visibility = ViewStates.Gone;
            ContentViewer.Visibility = ViewStates.Visible;
        }

        private void SetupControl<T>(out T control, int id) where T : View
        {
            control = FindViewById<T>(id);
        }
    }
}