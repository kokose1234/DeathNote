//  Copyright 2022 Jonguk Kim
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using DynamicData.Binding;
using ReactiveUI;

namespace DeathNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Hide();
            ViewModel = new MainViewModel();

            var notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName),
                Text = "DeathNote",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };
            var closeMenu = new ToolStripMenuItem("닫기");
            closeMenu.Click += (_, _) => Environment.Exit(0);
            notifyIcon.ContextMenuStrip.Items.Add(closeMenu);
            notifyIcon.DoubleClick += (_, _) =>
            {
                notifyIcon.Visible = false;
                Show();
            };

            this.Events().Closing.Subscribe(e =>
            {
                e.Cancel = true;
                Hide();
                notifyIcon.Visible = true;
            });

            Keywords.Events().SelectionChanged.Subscribe(e =>
            {
                if (Keywords.SelectedItem is string selectedItem)
                {
                    ViewModel.SelectedItem = selectedItem;
                }
            });

            TextBox.Events().KeyDown.Subscribe(e =>
            {
                if (e.Key != Key.Enter) return;

                ViewModel.AddKeyword();
                TextBox.Text = string.Empty;
            });

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel, vm => vm.KeywordList, v => v.Keywords.ItemsSource)
                    .DisposeWith(disposable);
                this.Bind(ViewModel, vm => vm.Text, v => v.TextBox.Text)
                    .DisposeWith(disposable);
                this.BindCommand(ViewModel, vm => vm.RemoveCommand, v => v.RemoveButton, "PreviewMouseDown")
                    .DisposeWith(disposable);
            });
        }
    }
}