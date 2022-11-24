using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DeathNote;

public class MainViewModel : ReactiveObject
{
    public IObservableCollection<string> KeywordList { get; } = new ObservableCollectionExtended<string>();

    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

    public string SelectedItem { get; set; }

    [Reactive]
    public string Text { get; set; }

    private ReaderWriterLockSlim _readerWriterLock = new();


    public MainViewModel()
    {
        if (File.Exists("./Keywords.json"))
            KeywordList.AddRange(JsonConvert.DeserializeObject<string[]>(File.ReadAllText("./Keywords.json")) ?? Array.Empty<string>());

        RemoveCommand = ReactiveCommand.Create(() =>
        {
            _readerWriterLock.EnterWriteLock();

            try
            {
                KeywordList.Remove(SelectedItem);
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }

            SaveKeywords();
        });

        new Thread(ProcessKillJob)
        {
            Priority = ThreadPriority.Lowest,
            Name = "ProcessKillThread"
        }.Start();
    }

    public void AddKeyword()
    {
        if (string.IsNullOrEmpty(Text) || string.IsNullOrWhiteSpace(Text) || KeywordList.Any(x => x == Text.ToLower()))
            return;

        _readerWriterLock.EnterWriteLock();

        try
        {
            KeywordList.Add(Text.ToLower());
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }

        SaveKeywords();
    }

    private void SaveKeywords()
    {
        _readerWriterLock.EnterReadLock();

        try
        {
            File.WriteAllText("./Keywords.json", JsonConvert.SerializeObject(KeywordList.ToArray(), Formatting.Indented));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }

    private void ProcessKillJob()
    {
        while (true)
        {
            _readerWriterLock.EnterReadLock();

            try
            {
                if (KeywordList.Count > 0)
                {
                    foreach (var process in Process.GetProcesses())
                    {
                        var name = process.ProcessName.ToLower();

                        // if (KeywordList.Any(x => name.Contains(x)) && !process.CloseMainWindow())
                        // {
                        //     process.Kill(true);
                        // }

                        if (KeywordList.Any(x => name.Contains(x)))
                        {
                            WinApi.SuspendProcess(process.Id);
                        }
                    }
                }
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }

            Thread.Sleep(1000);
        }
    }
}