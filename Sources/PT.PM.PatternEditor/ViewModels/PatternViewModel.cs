using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Dsl;
using PT.PM.Patterns;
using PT.PM.Patterns.Nodes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace PT.PM.PatternEditor
{
    public class PatternViewModel : ReactiveObject
    {
        private JsonUstNodeSerializer ustNodeJsonSerializer = new JsonUstNodeSerializer(typeof(UstNode), typeof(PatternVarDef))
        {
            IncludeTextSpans = false,
            Indented = true,
            ExcludeNulls = true
        };
        private static JsonConverter[] jsonConverters = new JsonConverter[] { new StringEnumConverter() };

        private ListBox patternsListBox;
        private TextBox patternTextBox;
        private ListBox patternErrorsListBox;
        private TextBox logger;

        private PatternDto selectedPattern;
        private LanguageFlags oldLanguages;
        private string oldPattern;
        private GuiLogger patternLogger;
        private DslProcessor dslProcessor = new DslProcessor();
        private StringBuilder log = new StringBuilder();

        public PatternViewModel(PatternUserControl patternUserControl)
        {
            patternsListBox = patternUserControl.Find<ListBox>("PatternsListBox");
            patternTextBox = patternUserControl.Find<TextBox>("PatternData");
            patternErrorsListBox = patternUserControl.Find<ListBox>("PatternErrors");
            logger = patternUserControl.Find<TextBox>("Logger");

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                bool error = false;
                if (!string.IsNullOrEmpty(Settings.PatternsFileName))
                {
                    try
                    {
                        PatternsFileName = Settings.PatternsFileName;
                        SelectedPattern = Patterns.FirstOrDefault(p => p.Key == Settings.SelectedPatternKey) ?? Patterns.FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        await MessageBox.ShowDialog(ex.Message);
                        error = true;
                    }
                }
                
                if (string.IsNullOrEmpty(Settings.PatternsFileName) || error)
                {
                    Settings.PatternsFileName = Settings.DefaultPatternsFileName;
                    SelectedPattern = Patterns.FirstOrDefault();
                    SavePatterns();
                    Settings.Save();
                }
            });

            patternsListBox.DoubleTapped += (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                OpenPattern.Execute(sender);
            };

            patternErrorsListBox.DoubleTapped += (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                GuiHelpers.ProcessErrorOnDoubleClick(patternErrorsListBox, patternTextBox);
            };

            patternLogger = new GuiLogger(PatternErrors) { LogPatternErrors = true };
            patternLogger.LogEvent += PatternLogger_LogEvent;
            dslProcessor.Logger = patternLogger;

            patternTextBox.GetObservable(TextBox.CaretIndexProperty)
                .Subscribe(UpdatePatternCaretIndex);

            patternTextBox.GetObservable(TextBox.TextProperty)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Subscribe(str => Value = str);

            OpenPatterns.Subscribe(async _ =>
            {
                SavePatterns();
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Select patterns database",
                };
                var fileNames = await openFileDialog.ShowAsync(ServiceLocator.MainWindow);
                if (fileNames != null)
                {
                    try
                    {
                        PatternsFileName = fileNames.First();
                    }
                    catch (Exception ex)
                    {
                        await MessageBox.ShowDialog(ex.Message);
                        Settings.PatternsFileName = Settings.DefaultPatternsFileName;
                        Settings.Save();
                        SelectedPattern = Patterns.FirstOrDefault();
                        ServiceLocator.MainWindowViewModel.ActivateWindow();
                    }
                }
            });

            CreatePattern.Subscribe(_ =>
            {
                SavePatterns();
                var newPattern = new PatternDto();
                newPattern.Key = Guid.NewGuid().ToString();
                newPattern.Name = "New Pattern";
                Patterns.Add(newPattern);
                SelectedPattern = newPattern;
                SavePatterns();
            });

            RemovePattern.Subscribe(async _ =>
            {
                if (SelectedPattern != null && await MessageBox.ShowDialog($"Do you want to remove {SelectedPattern}?", messageBoxType: MessageBoxType.YesNo))
                {
                    Patterns.Remove(SelectedPattern);
                    SelectedPattern = Patterns.LastOrDefault();
                    SavePatterns();
                }
                
            });

            OpenPattern.Subscribe(_ =>
            {
                if (patternsListBox.SelectedItem != null)
                {
                    var patternDto = (PatternDto)patternsListBox.SelectedItem;
                    SelectedPattern = patternDto;
                }
            });

            SavePattern.Subscribe(_ =>
            {
                SavePatterns();
            });
        }

        public ReactiveCommand<object> OpenPatterns { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> CreatePattern { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> RemovePattern { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> OpenPattern { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> SavePattern { get; } = ReactiveCommand.Create();

        public string PatternsFileName
        {
            get
            {
                return Settings.PatternsFileName;
            }
            set
            {
                Settings.PatternsFileName = value;
                Patterns.Clear();
                var patterns = LoadPatterns();
                foreach (var pattern in patterns)
                {
                    Patterns.Add(pattern);
                }
                this.RaisePropertyChanged();
            }
        }

        public string PatternsShortFileName => Path.GetFileName(Settings.PatternsFileName);

        public ObservableCollection<PatternDto> Patterns { get; set; } = new ObservableCollection<PatternDto>();

        public Settings Settings => ServiceLocator.Settings;

        public PatternDto SelectedPattern
        {
            get
            {
                if (selectedPattern == null || selectedPattern.Key != Settings.SelectedPatternKey)
                {
                    selectedPattern = Patterns.FirstOrDefault(p => p.Key == Settings.SelectedPatternKey);
                }
                return selectedPattern;
            }
            set
            {
                Settings.SelectedPatternKey = value?.Key ?? "";
                Settings.Save();
                selectedPattern = Patterns.FirstOrDefault(p => p.Key == Settings.SelectedPatternKey);
                this.RaisePropertyChanged(nameof(Name));
                this.RaisePropertyChanged(nameof(Key));
                this.RaisePropertyChanged(nameof(CweId));
                this.RaisePropertyChanged(nameof(IsCSharpLanguage));
                this.RaisePropertyChanged(nameof(IsJavaLanguage));
                this.RaisePropertyChanged(nameof(IsPhpLanguage));
                this.RaisePropertyChanged(nameof(IsPlSqlLanguage));
                this.RaisePropertyChanged(nameof(IsTSqlLanguage));
                this.RaisePropertyChanged(nameof(IsJavaScriptLanguage));
                this.RaisePropertyChanged(nameof(Description));
                patternTextBox.Text = selectedPattern?.Value ?? "";
                CheckPattern();
                this.RaisePropertyChanged();
            }
        }

        public bool IsDeveloperMode => Settings.IsDeveloperMode;

        public string Name
        {
            get { return SelectedPattern?.Name; }
            set
            {
                if (SelectedPattern != null && SelectedPattern.Name != value)
                {
                    SelectedPattern.Name = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public string Key
        {
            get
            {
                return SelectedPattern?.Key;
            }
            set
            {
                if (SelectedPattern != null && SelectedPattern.Key != value)
                {
                    SelectedPattern.Key = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public string CweId
        {
            get
            {
                return SelectedPattern?.CweId;
            }
            set
            {
                if (SelectedPattern != null && SelectedPattern.CweId != value)
                {
                    SelectedPattern.CweId = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public LanguageFlags Languages
        {
            get
            {
                return SelectedPattern?.Languages ?? LanguageFlags.None;
            }
            set
            {
                if (SelectedPattern != null && SelectedPattern.Languages != value)
                {
                    CheckPattern();
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsCSharpLanguage
        {
            get { return SelectedPattern?.Languages.Is(LanguageFlags.CSharp) ?? false; }
            set { ChangeLanguage(LanguageFlags.CSharp, value); }
        }

        public bool IsJavaLanguage
        {
            get { return SelectedPattern?.Languages.Is(LanguageFlags.Java) ?? false; }
            set { ChangeLanguage(LanguageFlags.Java, value); }
        }

        public bool IsPhpLanguage
        {
            get { return SelectedPattern?.Languages.Is(LanguageFlags.Php) ?? false; }
            set { ChangeLanguage(LanguageFlags.Php, value); }
        }

        public bool IsPlSqlLanguage
        {
            get { return SelectedPattern?.Languages.Is(LanguageFlags.PlSql) ?? false; }
            set { ChangeLanguage(LanguageFlags.PlSql, value); }
        }

        public bool IsTSqlLanguage
        {
            get { return SelectedPattern?.Languages.Is(LanguageFlags.TSql) ?? false; }
            set { ChangeLanguage(LanguageFlags.TSql, value); }
        }

        public bool IsJavaScriptLanguage
        {
            get { return SelectedPattern?.Languages.Is(LanguageFlags.JavaScript) ?? false; }
            set { ChangeLanguage(LanguageFlags.JavaScript, value); }
        }

        public string Description
        {
            get
            {
                return SelectedPattern?.Description ?? "";
            }
            set
            {
                if (SelectedPattern != null && SelectedPattern.Description != value)
                {
                    SelectedPattern.Description = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public string Value
        {
            get
            {
                return SelectedPattern?.Value ?? "";
            }
            set
            {
                if (SelectedPattern != null && SelectedPattern.Value != value)
                {
                    SelectedPattern.Value = value;
                    CheckPattern();
                }
            }
        }

        public bool PatternErrorsIsVisible { get; set; }

        public string PatternErrorsText { get; set; } = "ERRORS";

        public bool IsPatternErrorsExpanded
        {
            get
            {
                return Settings.IsPatternErrorsExpanded;
            }
            set
            {
                if (Settings.IsPatternErrorsExpanded != value)
                {
                    Settings.IsPatternErrorsExpanded = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsPatternJsonExpanded
        {
            get
            {
                return Settings.IsPatternJsonExpanded;
            }
            set
            {
                if (Settings.IsPatternJsonExpanded != value)
                {
                    Settings.IsPatternJsonExpanded = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                }
            }
        }

        public string PatternJson { get; set; }

        public ObservableCollection<object> PatternErrors { get; } = new ObservableCollection<object>();

        public string PatternTextBoxPosition { get; set; }

        public string Log { get; set; }

        public void UpdateDeveloperMode()
        {
            this.RaisePropertyChanged(nameof(IsDeveloperMode));
        }

        private void ChangeLanguage(LanguageFlags languages, bool value)
        {
            if (SelectedPattern != null)
            {
                var flags = value ? (SelectedPattern.Languages | languages) : (SelectedPattern.Languages & ~languages);
                if (SelectedPattern.Languages != flags)
                {
                    SelectedPattern.Languages = flags;
                    CheckPattern();
                    this.RaisePropertyChanged();
                }
            }
        }

        private void CheckPattern()
        {
            if (oldPattern != patternTextBox.Text || oldLanguages != Languages)
            {
                oldPattern = patternTextBox.Text;
                oldLanguages = Languages;

                Dispatcher.UIThread.InvokeAsync(PatternErrors.Clear);
                patternLogger.Clear();

                UstNode patternNode = null;
                try
                {
                    if (!string.IsNullOrEmpty(patternTextBox.Text))
                    {
                        patternNode = dslProcessor.Deserialize(patternTextBox.Text, Languages);
                    }
                }
                catch
                {
                }

                if (patternLogger.ErrorCount == 0)
                {
                    PatternErrorsIsVisible = false;
                    PatternErrorsText = "";
                    if (IsDeveloperMode && patternNode != null)
                    {
                        PatternJson = ustNodeJsonSerializer.Serialize(patternNode);
                        File.WriteAllText(Path.Combine(ServiceLocator.TempDirectory, "Pattern UST.json"), PatternJson);
                    }
                }
                else
                {
                    PatternErrorsIsVisible = true;
                    PatternErrorsText = $"ERRORS ({patternLogger.ErrorCount})";
                    if (IsDeveloperMode)
                    {
                        PatternJson = "";
                    }
                }
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.RaisePropertyChanged(nameof(PatternErrorsIsVisible));
                    this.RaisePropertyChanged(nameof(PatternErrorsText));
                    this.RaisePropertyChanged(nameof(PatternJson));
                });

                if (ServiceLocator.MainWindowViewModel != null)
                {
                    ServiceLocator.MainWindowViewModel.UpdateMatchings();
                }
            }
        }

        private void PatternLogger_LogEvent(object sender, string e)
        {
            if (ServiceLocator.MainWindowViewModel != null && IsDeveloperMode)
            {
                lock (log)
                {
                    log.AppendLine(e);
                    Log = log.ToString();
                }
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.RaisePropertyChanged(nameof(Log));
                    logger.CaretIndex = int.MaxValue;
                });
            }
        }

        private void UpdatePatternCaretIndex(int caretIndex)
        {
            int line, column;
            TextHelper.LinearToLineColumn(caretIndex, patternTextBox.Text, out line, out column);
            PatternTextBoxPosition = $"Caret: {line}:{column-1}";
            Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(nameof(PatternTextBoxPosition)));
        }

        public List<PatternDto> LoadPatterns()
        {
            var patternsJson = File.ReadAllText(Settings.PatternsFileName);
            return JsonConvert.DeserializeObject<List<PatternDto>>(patternsJson, jsonConverters);
        }

        public void SavePatterns()
        {
            var json = JsonConvert.SerializeObject(Patterns.ToList(), Formatting.Indented, jsonConverters);
            File.WriteAllText(Settings.PatternsFileName, json);
        }
    }
}
