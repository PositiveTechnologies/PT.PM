using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.CSharpParseTreeUst;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace PT.PM.PatternEditor
{
    public class MainWindowViewModel: ReactiveObject
    {
        private Window window;
        private ColumnDefinition patternsPanelColumn;
        private TextBox sourceCodeTextBox;
        private ListBox sourceCodeErrorsListBox;
        private ListBox matchResultListBox;
        private TextBox logger;
        private GuiLogger sourceCodeLogger;
        private string oldSelectedLanguage;
        private string sourceCodeFileName;
        private bool fileOpened;
        private string oldSourceCode = "";
        private Stage oldEndStage;
        private JavaScriptType oldJavaScriptType;
        private int sourceCodeSelectionStart, sourceCodeSelectionEnd;
        private LanguageDetector languageDetector = new ParserLanguageDetector();
        private string tokensHeader;
        private string parseTreeHeader;
        private string sourceCodeErrorsText = "Errors";
        private bool sourceCodeErrorsIsVisible;
        private string tokens;
        private string parseTree;
        private string ustJson;
        private string matchResultText = "MATCHINGS";
        private bool oldIsIncludeTextSpans;
        private SourceCodeFile sourceCode;

        public MainWindowViewModel(Window w)
        {
            window = w;
            window.WindowState = Settings.WindowState;
            if (Settings.Width > 0)
            {
                window.Width = Settings.Width;
            }
            if (Settings.Height > 0)
            {
                window.Height = Settings.Height;
            }
            if (Settings.Left != -1 && Settings.Top != -1)
            {
                window.Position = new Point(Settings.Left, Settings.Top);
            }

            patternsPanelColumn = window.Find<Grid>("MainGrid").ColumnDefinitions[0];
            sourceCodeTextBox = window.Find<TextBox>("SourceCode");
            sourceCodeErrorsListBox = window.Find<ListBox>("SourceCodeErrors");
            matchResultListBox = window.Find<ListBox>("MatchingResult");
            logger = window.Find<TextBox>("Logger");

            patternsPanelColumn.Width = GridLength.Parse(Settings.PatternsPanelWidth.ToString(), CultureInfo.InvariantCulture);
            sourceCodeErrorsListBox.DoubleTapped +=
            (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                GuiHelpers.ProcessErrorOnDoubleClick(sourceCodeErrorsListBox, sourceCodeTextBox);
            };
            matchResultListBox.DoubleTapped += MatchingResultListBox_DoubleTapped;

            sourceCodeLogger = new GuiLogger(SourceCodeErrors) { LogPatternErrors = false };
            languageDetector.Logger = sourceCodeLogger;

            OpenSourceCodeFile.Subscribe(async _ =>
            {
                var dialog = new OpenFileDialog();
                string[] fileNames = await dialog.ShowAsync(window);
                if (fileNames != null)
                {
                    string fileName = fileNames.Single();
                    OpenedFileName = fileName;
                    fileOpened = true;
                    sourceCodeTextBox.Text = File.ReadAllText(sourceCodeFileName);
                }
            });

            SaveSourceCodeFile.Subscribe(_ =>
            {
                if (!string.IsNullOrEmpty(sourceCodeFileName))
                {
                    File.WriteAllText(sourceCodeFileName, sourceCodeTextBox.Text);
                }
            });

            ReloadFile.Subscribe(_ =>
            {
                if (!string.IsNullOrEmpty(sourceCodeFileName))
                {
                    sourceCodeTextBox.Text = File.ReadAllText(sourceCodeFileName);
                }
            });

            Reset.Subscribe(_ =>
            {
                OpenedFileName = "";
                sourceCodeTextBox.Text = "";
            });

            OpenDumpDirectory.Subscribe(_ =>
            {
                Process.Start(ServiceLocator.TempDirectory);
            });

            if (string.IsNullOrEmpty(Settings.SourceCodeFile) || !File.Exists(Settings.SourceCodeFile))
            {
                fileOpened = false;
                sourceCodeFileName = "";
                sourceCodeTextBox.Text = Settings.SourceCode;
            }
            else
            {
                fileOpened = true;
                sourceCodeFileName = Settings.SourceCodeFile;
                sourceCodeTextBox.Text = File.ReadAllText(Settings.SourceCodeFile);
            }

            CheckSourceCode();

            this.RaisePropertyChanged(nameof(SelectedLanguage));
            this.RaisePropertyChanged(nameof(OpenedFileName));

            sourceCodeTextBox.GetObservable(TextBox.CaretIndexProperty)
                .Subscribe(UpdateSourceCodeCaretIndex);
            sourceCodeTextBox.GetObservable(TextBox.SelectionStartProperty)
                .Subscribe(selectionStart =>
                {
                    if (sourceCodeTextBox.IsFocused)
                    {
                        sourceCodeSelectionStart = selectionStart;
                    }
                });
            sourceCodeTextBox.GetObservable(TextBox.SelectionEndProperty)
                .Subscribe(selectionEnd =>
                {
                    if (sourceCodeTextBox.IsFocused)
                    {
                        sourceCodeSelectionEnd = selectionEnd;
                    }
                });

            sourceCodeTextBox.GetObservable(TextBox.TextProperty)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(str => CheckSourceCode());

            SetupWindowSubscriptions();

            this.RaisePropertyChanged(nameof(IsErrorsExpanded));
            this.RaisePropertyChanged(nameof(IsTokensExpanded));
            this.RaisePropertyChanged(nameof(IsParseTreeExpanded));
            this.RaisePropertyChanged(nameof(IsUstExpanded));
            this.RaisePropertyChanged(nameof(IsMatchingsExpanded));
        }

        public void ActivateWindow()
        {
            window.Activate();
        }

        private void SetupWindowSubscriptions()
        {
            window.GetObservable(Window.WidthProperty)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Subscribe(width =>
                {
                    if (window.WindowState != WindowState.Maximized)
                    {
                        Settings.Width = width;
                    }
                    Settings.WindowState = window.WindowState;
                    Settings.Save();
                });

            window.GetObservable(Window.HeightProperty)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Subscribe(height =>
                {
                    if (window.WindowState != WindowState.Maximized)
                    {
                        Settings.Height = height;
                    }
                    Settings.WindowState = window.WindowState;
                    Settings.Save();
                });

            Observable.FromEventPattern<PointEventArgs>(
                ev => window.PositionChanged += ev, ev => window.PositionChanged -= ev)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Subscribe(ev =>
                {
                    if (window.WindowState != WindowState.Maximized)
                    {
                        Settings.Left = window.Position.X;
                        Settings.Top = window.Position.Y;
                    }
                    Settings.Save();
                });

            Observable.FromEventPattern(
                ev => window.Closed += ev, ev => window.Closed -= ev)
                .Subscribe(ev =>
                {
                    ServiceLocator.PatternViewModel.SavePatterns();
                    Settings.PatternsPanelWidth = patternsPanelColumn.Width.Value;
                    Settings.Save();
                });

            Observable.FromEventPattern<PointerPressedEventArgs>(
                ev => window.PointerPressed += ev, ev => window.PointerPressed -= ev)
                .Subscribe(async ev =>
                {
                    var position = ev.EventArgs.GetPosition(window);
                    int offset = 7;
                    if (ev.EventArgs.ClickCount == 3 && position.X <= offset && position.Y >= window.Height - offset)
                    {
                        IsDeveloperMode = !IsDeveloperMode;
                        await MessageBox.ShowDialog($"DeveloperMode turned {(Settings.IsDeveloperMode ? "on" : "off")}.");
                    }
                });
        }

        private void UpdateSourceCodeCaretIndex(int caretIndex)
        {
            if (sourceCode != null)
            {
                sourceCode.GetLineColumnFromLinear(caretIndex, out int line, out int column);
                SourceCodeTextBoxPosition = $"Caret: {line}:{column}";
            }
            else
            {
                SourceCodeTextBoxPosition = $"";
            }
            Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(nameof(SourceCodeTextBoxPosition)));
        }

        private void MatchingResultListBox_DoubleTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (matchResultListBox.SelectedItem is MatchResultDtoWrapper matchResultWrapper)
            {
                var matchResult = matchResultWrapper.MatchResult;
                sourceCodeTextBox.Focus();
                sourceCodeTextBox.SelectionStart = matchResult.TextSpan.Start;
                sourceCodeTextBox.SelectionEnd = matchResult.TextSpan.End;
                sourceCodeTextBox.CaretIndex = sourceCodeTextBox.SelectionEnd;
            }
        }

        public bool IsDeveloperMode
        {
            get
            {
                return Settings.IsDeveloperMode;
            }
            set
            {
                if (Settings.IsDeveloperMode != value)
                {
                    Settings.IsDeveloperMode = value;
                    Settings.Save();
                    
                    this.RaisePropertyChanged(nameof(IsTokensVisible));
                    this.RaisePropertyChanged(nameof(IsTreeVisible));
                    this.RaisePropertyChanged(nameof(IsUstJsonVisible));
                    this.RaisePropertyChanged();

                    ServiceLocator.PatternViewModel.UpdateDeveloperMode();
                }
            }
        }

        public Settings Settings => ServiceLocator.Settings;

        public string SourceCodeTextBoxPosition { get; set; }

        public ObservableCollection<Stage> Stages { get; } = new ObservableCollection<Stage>(new[] { Stage.ParseTree, Stage.Ust, Stage.Match });

        public Stage Stage
        {
            get
            {
                return Settings.SelectedStage;
            }
            set
            {
                if (Settings.SelectedStage != value)
                {
                    Settings.SelectedStage = value;
                    Settings.Save();
                    this.RaisePropertyChanged(nameof(IsMatchingStage));
                    this.RaisePropertyChanged(nameof(IsUstJsonVisible));
                    this.RaisePropertyChanged();
                    CheckSourceCode();
                }
            }
        }

        public ObservableCollection<Language> Languages
        {
            get
            {
                return new ObservableCollection<Language>(LanguageUtils.Languages.Values);
            }
        }

        public Language SelectedLanguage
        {
            get
            {
                if (LanguageUtils.Languages.TryGetValue(Settings.SourceCodeLanguage, out Language language))
                {
                    return language;
                }
                return CSharp.Language;
            }
            set
            {
                if (Settings.SourceCodeLanguage != value.Key)
                {
                    Settings.SourceCodeLanguage = value.Key;
                    Settings.Save();
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(IsTokensVisible));
                    this.RaisePropertyChanged(nameof(IsTreeVisible));
                    this.RaisePropertyChanged(nameof(IsJavaScriptTypeVisible));
                    CheckSourceCode();
                }
            }
        }

        public ObservableCollection<JavaScriptType> JavaScriptTypes
        {
            get
            {
                return new ObservableCollection<JavaScriptType>((JavaScriptType[])Enum.GetValues(typeof(JavaScriptType)));
            }
        }

        public JavaScriptType JavaScriptType
        {
            get
            {
                return Settings.JavaScriptType;
            }
            set
            {
                if (Settings.JavaScriptType != value)
                {
                    Settings.JavaScriptType = value;
                    Settings.Save();
                    this.RaisePropertyChanged(nameof(JavaScriptType));
                    CheckSourceCode();
                }
            }
        }

        public bool IsJavaScriptTypeVisible => SelectedLanguage == JavaScript.Language;

        public ReactiveCommand<object> OpenSourceCodeFile { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> SaveSourceCodeFile { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> ReloadFile { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> Reset { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> OpenDumpDirectory { get; } = ReactiveCommand.Create();

        public string OpenedFullFileName => sourceCodeFileName;

        public string OpenedFileName
        {
            get
            {
                return Path.GetFileName(sourceCodeFileName);
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    SelectedLanguage = languageDetector.DetectIfRequired(value);
                }
                Settings.SourceCodeFile = value;
                Settings.Save();
                this.RaiseAndSetIfChanged(ref sourceCodeFileName, value);
            }
        }

        public string SourceCodeErrorsText
        {
            get => sourceCodeErrorsText;
            set => this.RaiseAndSetIfChanged(ref sourceCodeErrorsText, value);
        }

        public bool SourceCodeErrorsIsVisible
        {
            get => sourceCodeErrorsIsVisible;
            set => this.RaiseAndSetIfChanged(ref sourceCodeErrorsIsVisible, value);
        }

        public ObservableCollection<object> SourceCodeErrors { get; } = new ObservableCollection<object>();

        public string TokensHeader
        {
            get => tokensHeader;
            set => this.RaiseAndSetIfChanged(ref tokensHeader, value);
        }

        public string Tokens
        {
            get => tokens;
            set => this.RaiseAndSetIfChanged(ref tokens, value);
        }

        public string ParseTreeHeader
        {
            get => parseTreeHeader;
            set => this.RaiseAndSetIfChanged(ref parseTreeHeader, value);
        }

        public string ParseTree
        {
            get => parseTree;
            set => this.RaiseAndSetIfChanged(ref parseTree, value);
        }

        public string UstJson
        {
            get => ustJson;
            set => this.RaiseAndSetIfChanged(ref ustJson, value);
        }

        public bool IsTokensVisible => SelectedLanguage?.HaveAntlrParser == true && IsDeveloperMode;

        public bool IsTreeVisible => SelectedLanguage?.HaveAntlrParser == true && IsDeveloperMode;

        public bool IsUstJsonVisible => Stage >= Stage.Ust && IsDeveloperMode;

        public string MatchingResultText
        {
            get => matchResultText;
            set => this.RaiseAndSetIfChanged(ref matchResultText, value);
        }

        public ObservableCollection<MatchResultDtoWrapper> MatchingResults { get; } = new ObservableCollection<MatchResultDtoWrapper>();

        public bool IsMatchingStage => Stage >= Stage.Match;

        public bool IsErrorsExpanded
        {
            get
            {
                return Settings.IsErrorsExpanded;
            }
            set
            {
                if (Settings.IsErrorsExpanded != value)
                {
                    Settings.IsErrorsExpanded = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsTokensExpanded
        {
            get
            {
                return Settings.IsTokensExpanded;
            }
            set
            {
                if (Settings.IsTokensExpanded != value)
                {
                    Settings.IsTokensExpanded = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsParseTreeExpanded
        {
            get
            {
                return Settings.IsParseTreeExpanded;
            }
            set
            {
                if (Settings.IsParseTreeExpanded != value)
                {
                    Settings.IsParseTreeExpanded = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsUstExpanded
        {
            get
            {
                return Settings.IsUstExpanded;
            }
            set
            {
                if (Settings.IsUstExpanded != value)
                {
                    Settings.IsUstExpanded = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsMatchingsExpanded
        {
            get
            {
                return Settings.IsMatchingsExpanded;
            }
            set
            {
                if (Settings.IsMatchingsExpanded != value)
                {
                    Settings.IsMatchingsExpanded = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsIncludeTextSpans
        {
            get
            {
                return Settings.IsIncludeTextSpans;
            }
            set
            {
                if (Settings.IsIncludeTextSpans != value)
                {
                    Settings.IsIncludeTextSpans = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                    CheckSourceCode();
                }
            }
        }

        private void CheckSourceCode()
        {
            if (oldSourceCode != sourceCodeTextBox.Text ||
                oldSelectedLanguage != Settings.SourceCodeLanguage ||
                oldEndStage != Settings.SelectedStage ||
                oldJavaScriptType != Settings.JavaScriptType ||
                oldIsIncludeTextSpans != Settings.IsIncludeTextSpans)
            {
                Dispatcher.UIThread.InvokeAsync(SourceCodeErrors.Clear);
                string sourceCode = sourceCodeTextBox.Text;
                Settings.SourceCode = !string.IsNullOrEmpty(OpenedFileName) ? "" : sourceCode;
                Settings.Save();

                RunWorkflow();

                oldSourceCode = sourceCodeTextBox.Text;
                oldSelectedLanguage = Settings.SourceCodeLanguage;
                oldEndStage = Settings.SelectedStage;
                oldJavaScriptType = Settings.JavaScriptType;
                oldIsIncludeTextSpans = Settings.IsIncludeTextSpans;
            }
        }

        internal void RunWorkflow()
        {
            sourceCodeLogger.Clear();

            var sourceCodeRep = new MemoryCodeRepository(sourceCodeTextBox.Text, language: SelectedLanguage ?? CSharp.Language);
            IPatternsRepository patternRepository;
            if (!string.IsNullOrEmpty(ServiceLocator.PatternViewModel.Value))
            {
                patternRepository = new DslPatternRepository(ServiceLocator.PatternViewModel.Value, ServiceLocator.PatternViewModel.Languages);
            }
            else
            {
                patternRepository = new MemoryPatternsRepository();
            }
            var workflow = new Workflow(sourceCodeRep, patternRepository, stage: Stage)
            {
                IsIncludeIntermediateResult = true,
                DumpWithTextSpans = IsIncludeTextSpans,
                Logger = sourceCodeLogger,
                RenderFormat = GraphvizOutputFormat.Svg,
                DumpDir = ServiceLocator.TempDirectory,
                RenderStages = new HashSet<Stage>() { Stage.Ust }
            };
            if (SelectedLanguage == JavaScript.Language)
            {
                workflow.JavaScriptType = JavaScriptType;
            }
            if (IsDeveloperMode)
            {
                var dumpStages = new HashSet<Stage>();
                if (Stage >= Stage.ParseTree)
                {
                    dumpStages.Add(Stage.ParseTree);
                    if (Stage >= Stage.Ust)
                    {
                        dumpStages.Add(Stage.Ust);
                    }
                }
                workflow.DumpStages = dumpStages;
            }
            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchResultDto> matchResults = workflowResult.MatchResults.ToDto();
            sourceCode = workflowResult.SourceCodeFiles.FirstOrDefault();

            if (IsDeveloperMode)
            {
                string tokensFileName = Path.Combine(ServiceLocator.TempDirectory, "tokens");
                string parseTreeFileName = Path.Combine(ServiceLocator.TempDirectory, "parseTree");
                Tokens = File.Exists(tokensFileName) ? File.ReadAllText(tokensFileName) : "";
                ParseTree = File.Exists(parseTreeFileName) ? File.ReadAllText(parseTreeFileName) : "";

                TokensHeader = "Tokens" + (SelectedLanguage?.HaveAntlrParser == true ? " (ANTLR)" : "");
                ParseTreeHeader = "Parse Tree" + (SelectedLanguage?.HaveAntlrParser == true ? " (ANTLR)" : "");

                if (Stage >= Stage.Ust && workflowResult.Usts.FirstOrDefault() != null)
                {
                    UstJson = File.ReadAllText(Path.Combine(ServiceLocator.TempDirectory, "", "ust.json"));
                }
            }

            MatchingResultText = "MATCHINGS" + (matchResults.Count() > 0 ? $" ({matchResults.Count()})" : "");

            if (sourceCodeLogger.ErrorCount == 0)
            {
                SourceCodeErrorsIsVisible = false;
                SourceCodeErrorsText = "ERRORS";
            }
            else
            {
                SourceCodeErrorsIsVisible = true;
                SourceCodeErrorsText = $"ERRORS ({sourceCodeLogger.ErrorCount})";
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                MatchingResults.Clear();
                foreach (MatchResultDto matchResult in matchResults)
                {
                    MatchingResults.Add(new MatchResultDtoWrapper(matchResult));
                }
            });
        }

        private void DetectLanguageIfRequired()
        {
            string newSourceCode = sourceCodeTextBox.Text;
            if (!fileOpened && (!string.IsNullOrEmpty(newSourceCode) && string.IsNullOrEmpty(oldSourceCode)))
            {
                Task.Factory.StartNew(() =>
                {
                    Language detectedLanguage = languageDetector.Detect(newSourceCode);
                    Dispatcher.UIThread.InvokeAsync(() => SelectedLanguage = detectedLanguage);
                });
                Dispatcher.UIThread.InvokeAsync(() => OpenedFileName = "");
            }
            
            fileOpened = false;
        }
    }
}
