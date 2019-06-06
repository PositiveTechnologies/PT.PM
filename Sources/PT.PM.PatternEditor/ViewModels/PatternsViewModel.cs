using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PT.PM.Common;
using PT.PM.Common.Utils;
using PT.PM.Dsl;
using PT.PM.Matching;
using PT.PM.Matching.Json;
using PT.PM.PatternEditor.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using PT.PM.Common.Files;

namespace PT.PM.PatternEditor.Pattern
{
    public class PatternsViewModel : ReactiveObject
    {
        private static readonly JsonConverter[] jsonConverters = { new StringEnumConverter() };

        private bool oldIsIncludeTextSpans;
        private bool oldIsLinearTextSpans;
        private bool oldIsIncludeCode;

        private readonly ListBox patternsListBox;
        private readonly TextEditor patternTextBox;
        private readonly ListBox patternErrorsListBox;
        private readonly TextBox logger;

        private PatternViewModel selectedPattern;
        private HashSet<string> oldLanguages = new HashSet<string>();
        private string oldPattern;
        private GuiLogger patternLogger;
        private DslProcessor dslProcessor = new DslProcessor();
        private StringBuilder log = new StringBuilder();
        private TextFile patternFile;
        private int prevCaretOffset;

        public PatternsViewModel(PatternUserControl patternUserControl)
        {
            patternsListBox = patternUserControl.Find<ListBox>("PatternsListBox");
            patternTextBox = patternUserControl.Find<TextEditor>("PatternData");
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
                        await MessageBox.ShowDialog(ServiceLocator.MainWindow, ex.Message);
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

            patternsListBox.DoubleTapped += (sender, e) =>
            {
                OpenPatternAction();
            };

            patternErrorsListBox.DoubleTapped += (sender, e) =>
            {
                GuiUtils.ProcessErrorOnDoubleClick(patternErrorsListBox, patternTextBox);
            };

            patternLogger = GuiLogger.CreatePatternLogger(PatternErrors);
            patternLogger.LogEvent += PatternLogger_LogEvent;
            dslProcessor.Logger = patternLogger;

            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(200), RxApp.MainThreadScheduler)
                .Subscribe(_ => UpdatePatternCaretIndex());

            var patternTextBoxObserable = Observable.FromEventPattern<EventHandler, EventArgs>(
                h => patternTextBox.TextChanged += h,
                h => patternTextBox.TextChanged -= h);

            patternTextBoxObserable
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(str =>
                {
                    if (SelectedPattern != null)
                    {
                        Value = patternTextBox.Text;
                    }
                });

            OpenPatterns = ReactiveCommand.Create(async () =>
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
                        await MessageBox.ShowDialog(ServiceLocator.MainWindow,ex.Message);
                        Settings.PatternsFileName = Settings.DefaultPatternsFileName;
                        Settings.Save();
                        SelectedPattern = Patterns.FirstOrDefault();
                        ServiceLocator.MainWindowViewModel.ActivateWindow();
                    }
                }
            });

            CreatePattern = ReactiveCommand.Create(() =>
            {
                SavePatterns();
                PatternViewModel newPatternViewMode = CreateNewPattern();
                Patterns.Add(newPatternViewMode);
                SelectedPattern = newPatternViewMode;
                SavePatterns();
            });

            RemovePattern = ReactiveCommand.Create(async () =>
            {
                if (SelectedPattern != null && await MessageBox.ShowDialog(ServiceLocator.MainWindow, $"Do you want to remove {SelectedPattern}?", messageBoxType: MessageBoxType.YesNo))
                {
                    Patterns.Remove(SelectedPattern);
                    SelectedPattern = Patterns.LastOrDefault();
                    SavePatterns();
                }
            });

            OpenPattern = ReactiveCommand.Create(OpenPatternAction);

            SavePattern = ReactiveCommand.Create(() => SavePatterns());

            void OpenPatternAction()
            {
                if (patternsListBox.SelectedItem != null)
                {
                    SelectedPattern = (PatternViewModel)patternsListBox.SelectedItem;
                }
            }
        }

        public ReactiveCommand OpenPatterns { get; }

        public ReactiveCommand CreatePattern { get; }

        public ReactiveCommand RemovePattern { get; }

        public ReactiveCommand OpenPattern { get; }

        public ReactiveCommand SavePattern { get; }

        public string PatternsFileName
        {
            get => Settings.PatternsFileName;
            set
            {
                Settings.PatternsFileName = value;
                Patterns.Clear();
                var patterns = LoadPatterns();
                foreach (PatternDto pattern in patterns)
                {
                    Patterns.Add(new PatternViewModel(pattern));
                }
                this.RaisePropertyChanged();
            }
        }

        public string PatternsShortFileName => Path.GetFileName(Settings.PatternsFileName);

        public ObservableCollection<PatternViewModel> Patterns { get; set; } = new ObservableCollection<PatternViewModel>();

        public Settings Settings => ServiceLocator.Settings;

        public PatternViewModel SelectedPattern
        {
            get
            {
                if (selectedPattern == null || selectedPattern.Key != Settings.SelectedPatternKey)
                {
                    selectedPattern = Patterns.FirstOrDefault(p => p.Key == Settings.SelectedPatternKey)
                        ?? Patterns.FirstOrDefault();
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
                this.RaisePropertyChanged(nameof(IsHtmlLanguage));
                this.RaisePropertyChanged(nameof(IsCLanguage));
                this.RaisePropertyChanged(nameof(IsCPlusPlusLanguage));
                this.RaisePropertyChanged(nameof(IsObjectiveCLanguage));
                this.RaisePropertyChanged(nameof(Description));
                patternTextBox.Text = selectedPattern?.Value ?? "";
                CheckPattern();
                this.RaisePropertyChanged();
            }
        }

        public string Name
        {
            get => SelectedPattern?.Name;
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
            get => SelectedPattern?.Key;
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
            get => SelectedPattern?.CweId;
            set
            {
                if (SelectedPattern != null && SelectedPattern.CweId != value)
                {
                    SelectedPattern.CweId = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsCSharpLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.CSharp) ?? false;
            set => ChangeLanguage(Language.CSharp, value);
        }

        public bool IsJavaLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.Java) ?? false;
            set => ChangeLanguage(Language.Java, value);
        }

        public bool IsPhpLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.Php) ?? false;
            set => ChangeLanguage(Language.Php, value);
        }

        public bool IsPlSqlLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.PlSql) ?? false;
            set => ChangeLanguage(Language.PlSql, value);
        }

        public bool IsTSqlLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.TSql) ?? false;
            set => ChangeLanguage(Language.TSql, value);
        }

        public bool IsJavaScriptLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.JavaScript) ?? false;
            set => ChangeLanguage(Language.JavaScript, value);
        }

        public bool IsHtmlLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.Html) ?? false;
            set => ChangeLanguage(Language.Html, value);
        }

        public bool IsCLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.C) ?? false;
            set => ChangeLanguage(Language.C, value);
        }

        public bool IsCPlusPlusLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.CPlusPlus) ?? false;
            set => ChangeLanguage(Language.CPlusPlus, value);
        }

        public bool IsObjectiveCLanguage
        {
            get => SelectedPattern?.ContainsLanguage(Language.ObjectiveC) ?? false;
            set => ChangeLanguage(Language.ObjectiveC, value);
        }

        public string Description
        {
            get => SelectedPattern?.Description ?? "";
            set
            {
                if (SelectedPattern != null && SelectedPattern.Description != value)
                {
                    SelectedPattern.Description = value;
                }
            }
        }

        public string Value
        {
            get => SelectedPattern?.Value ?? "";
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
            get => Settings.IsPatternErrorsExpanded;
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

        public bool IsPatternExtraInfoExpanded
        {
            get => Settings.IsPatternExtraInfoExpanded;
            set
            {
                if (Settings.IsPatternExtraInfoExpanded != value)
                {
                    Settings.IsPatternExtraInfoExpanded = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                }
            }
        }

        public string PatternJson { get; set; }

        public ObservableCollection<ErrorViewModel> PatternErrors { get; } = new ObservableCollection<ErrorViewModel>();

        public string PatternTextBoxPosition { get; set; }

        public string Log { get; set; }

        private void ChangeLanguage(Language language, bool set)
        {
            if (SelectedPattern != null)
            {
                if (set)
                {
                    if (SelectedPattern.AddLanguage(language))
                    {
                        CheckPattern();
                    }
                }
                else
                {
                    if (SelectedPattern.RemoveLanguage(language))
                    {
                        CheckPattern();
                    }
                }
            }
        }

        public void CheckPattern()
        {
            if (SelectedPattern == null)
            {
                return;
            }

            if (oldPattern != patternTextBox.Text || !oldLanguages.SequenceEqual(SelectedPattern.Languages) ||
                oldIsIncludeTextSpans != Settings.IsIncludeTextSpans ||
                oldIsLinearTextSpans != Settings.IsLinearTextSpans ||
                oldIsIncludeCode != Settings.IsIncludeCode)
            {
                oldPattern = patternTextBox.Text;
                oldLanguages = new HashSet<string>(SelectedPattern.Languages);
                oldIsIncludeTextSpans = Settings.IsIncludeTextSpans;
                oldIsLinearTextSpans = Settings.IsLinearTextSpans;
                oldIsIncludeCode = Settings.IsIncludeCode;

                ServiceLocator.MainWindowViewModel.SourceLogger.Clear();
                patternLogger.Clear();

                patternFile = TextFile.Empty;
                PatternRoot patternNode = null;
                try
                {
                    if (!string.IsNullOrEmpty(patternTextBox.Text))
                    {
                        patternNode = dslProcessor.Deserialize(new TextFile(patternTextBox.Text) { PatternKey = Key });
                        patternNode.Languages = SelectedPattern.Languages.ParseLanguages(allByDefault: false, patternLanguages: true);
                    }
                }
                catch
                {
                }
                patternFile = patternNode?.File;

                if (patternLogger.ErrorCount == 0)
                {
                    PatternErrorsIsVisible = false;
                    PatternErrorsText = "";
                    if (patternNode != null)
                    {
                        var jsonPatternSerializer = new JsonPatternSerializer
                        {
                            IncludeCode = Settings.IsIncludeCode,
                            IncludeTextSpans = Settings.IsIncludeTextSpans,
                            LineColumnTextSpans = !Settings.IsLinearTextSpans,
                            ExcludeDefaults = true,
                            Indented = true
                        };

                        jsonPatternSerializer.SourceFiles = new HashSet<IFile> { patternNode.File };
                        jsonPatternSerializer.CurrectSourceFile = patternNode.File;
                        PatternJson = jsonPatternSerializer.Serialize(patternNode);
                        FileExt.WriteAllText(Path.Combine(ServiceLocator.TempDirectory, "pattern.ust.json"), PatternJson);
                    }
                }
                else
                {
                    PatternErrorsIsVisible = true;
                    PatternErrorsText = $"ERRORS ({patternLogger.ErrorCount})";
                    PatternJson = "";
                }
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.RaisePropertyChanged(nameof(PatternErrorsIsVisible));
                    this.RaisePropertyChanged(nameof(PatternErrorsText));
                    this.RaisePropertyChanged(nameof(PatternJson));
                });

                if (ServiceLocator.MainWindowViewModel != null)
                {
                    ServiceLocator.MainWindowViewModel.RunWorkflow();
                }
            }
        }

        private void PatternLogger_LogEvent(object sender, string e)
        {
            if (ServiceLocator.MainWindowViewModel != null)
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

        private void UpdatePatternCaretIndex()
        {
            if (patternTextBox.CaretOffset != prevCaretOffset)
            {
                prevCaretOffset = patternTextBox.CaretOffset;
                if (patternFile != null)
                {
                    patternFile.GetLineColumnFromLinear(patternTextBox.CaretOffset, out int line, out int column);
                    PatternTextBoxPosition = $"Caret: {line}:{column}";
                }
                else
                {
                    PatternTextBoxPosition = "";
                }
                this.RaisePropertyChanged(nameof(PatternTextBoxPosition));
            }
        }

        public List<PatternDto> LoadPatterns()
        {
            var patternsJson = FileExt.ReadAllText(Settings.PatternsFileName);
            return JsonConvert.DeserializeObject<List<PatternDto>>(patternsJson, jsonConverters);
        }

        public void SavePatterns()
        {
            var json = JsonConvert.SerializeObject(Patterns.Select(pattern => pattern.PatternDto), Formatting.Indented, jsonConverters);
            FileExt.WriteAllText(Settings.PatternsFileName, json);
        }

        private static PatternViewModel CreateNewPattern()
        {
            var newPattern = new PatternDto
            {
                Languages = new HashSet<string>(LanguageUtils.PatternLanguages.Select(lang => lang.ToString()))
            };
            newPattern.Key = Guid.NewGuid().ToString();
            newPattern.Name = "New Pattern";
            var newPatternViewMode = new PatternViewModel(newPattern);
            return newPatternViewMode;
        }
    }
}
