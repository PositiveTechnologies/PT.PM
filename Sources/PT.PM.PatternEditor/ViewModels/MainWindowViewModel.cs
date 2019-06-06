using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Common.Utils;
using PT.PM.JavaScriptParseTreeUst;
using PT.PM.Matching;
using PT.PM.Matching.PatternsRepository;
using PT.PM.PatternEditor.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PT.PM.Common.Files;

namespace PT.PM.PatternEditor
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly Window window;
        private readonly ColumnDefinition patternsPanelColumn;
        private readonly TextEditor sourceTextBox;
        private readonly ListBox sourceErrorsListBox;
        private readonly ListBox matchResultListBox;
        private Language oldSelectedLanguage;
        private string sourceFileName;
        private string oldSource = "";
        private Stage oldEndStage;
        private JavaScriptType oldJavaScriptType;
        private readonly LanguageDetector languageDetector = new LanguageDetector();
        private string tokensHeader;
        private string parseTreeHeader;
        private string sourceErrorsText = "Errors";
        private bool sourceErrorsIsVisible;
        private string tokens;
        private string parseTree;
        private string ustJson;
        private string matchResultText = "MATCHINGS";
        private bool oldIsIncludeTextSpans;
        private bool oldIsLinearTextSpans;
        private bool oldIsIncludeCode;
        private bool oldIsLeftRightDir;
        private TextFile source;
        private int prevSourceStart, prevSourceLength;

        private readonly Dictionary<Language, string> highlightings = new Dictionary<Language, string>
        {
            [Language.CSharp] = "C#",
            [Language.Java] = "Java",
            [Language.Php] = "PHP",
            [Language.JavaScript] = "JavaScript",
            [Language.Html] = "HTML"
        };

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
                window.Position = new PixelPoint(Settings.Left, Settings.Top);
            }

            patternsPanelColumn = window.Find<Grid>("MainGrid").ColumnDefinitions[0];
            sourceTextBox = window.Find<TextEditor>("Source");
            sourceErrorsListBox = window.Find<ListBox>("SourceErrors");
            matchResultListBox = window.Find<ListBox>("MatchingResult");

            patternsPanelColumn.Width = GridLength.Parse(Settings.PatternsPanelWidth.ToString());
            sourceErrorsListBox.DoubleTapped += (sender, e) =>
            {
                GuiUtils.ProcessErrorOnDoubleClick(sourceErrorsListBox, sourceTextBox);
            };
            matchResultListBox.DoubleTapped += MatchingResultListBox_DoubleTapped;

            SourceLogger = GuiLogger.CreateSourceLogger(SourceErrors, MatchingResults);
            languageDetector.Logger = SourceLogger;

            OpenSourceFile = ReactiveCommand.Create(async () =>
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Open source code file"
                };
                string[] fileNames = await dialog.ShowAsync(window);
                if (fileNames?.Any() == true)
                {
                    OpenedFileName = fileNames[0];
                    sourceTextBox.Text = FileExt.ReadAllText(sourceFileName);
                }
            });

            SaveSourceFile = ReactiveCommand.Create(() =>
            {
                if (!string.IsNullOrEmpty(sourceFileName))
                {
                    FileExt.WriteAllText(sourceFileName, sourceTextBox.Text);
                }
            });

            ReloadFile = ReactiveCommand.Create(() =>
            {
                if (!string.IsNullOrEmpty(sourceFileName))
                {
                    sourceTextBox.Text = FileExt.ReadAllText(sourceFileName);
                }
            });

            Reset = ReactiveCommand.Create(() =>
            {
                OpenedFileName = "";
                sourceTextBox.Text = "";
            });

            OpenDumpDirectory = ReactiveCommand.Create(() =>
            {
                try
                {
                    GuiUtils.OpenDirectory(ServiceLocator.TempDirectory);
                }
                catch (Exception ex)
                {
                    new MessageBox($"Unable to open {ServiceLocator.TempDirectory} due to {ex}").ShowDialog(window);
                }
            });

            if (string.IsNullOrEmpty(Settings.SourceFile) || !FileExt.Exists(Settings.SourceFile))
            {
                sourceFileName = "";
                sourceTextBox.Text = Settings.Source;
            }
            else
            {
                sourceFileName = Settings.SourceFile;
                sourceTextBox.Text = FileExt.ReadAllText(Settings.SourceFile);
            }

            CheckSource();

            this.RaisePropertyChanged(nameof(SelectedLanguage));
            this.RaisePropertyChanged(nameof(OpenedFileName));

            highlightings.TryGetValue(SelectedLanguage, out string highlighting);
            sourceTextBox.SyntaxHighlighting = highlighting != null
                ? HighlightingManager.Instance.GetDefinition(highlighting)
                : null;

            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(200), RxApp.MainThreadScheduler)
                .Subscribe(_ => UpdateSourceSelection());

            Observable.FromEventPattern<EventHandler, EventArgs>(
                    h => sourceTextBox.TextChanged += h,
                    h => sourceTextBox.TextChanged -= h)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(str => CheckSource());

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
                .ObserveOn(RxApp.MainThreadScheduler)
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
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(height =>
                {
                    if (window.WindowState != WindowState.Maximized)
                    {
                        Settings.Height = height;
                    }

                    Settings.WindowState = window.WindowState;
                    Settings.Save();
                });

            Observable.FromEventPattern<PixelPointEventArgs>(
                    ev => window.PositionChanged += ev, ev => window.PositionChanged -= ev)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(RxApp.MainThreadScheduler)
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
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(ev =>
                {
                    ServiceLocator.PatternsViewModel.SavePatterns();
                    Settings.PatternsPanelWidth = patternsPanelColumn.Width.Value;
                    Settings.Save();
                });
        }

        private void UpdateSourceSelection(bool force = false)
        {
            try
            {
                int sourceStart = sourceTextBox.SelectionStart;
                int sourceLength = sourceTextBox.SelectionLength;
                if (force || sourceStart != prevSourceStart || sourceLength != prevSourceLength)
                {
                    prevSourceStart = sourceStart;
                    prevSourceLength = sourceLength;

                    int start = sourceTextBox.SelectionStart;
                    int end = sourceTextBox.SelectionStart + sourceTextBox.SelectionLength;
                    if (start > end)
                    {
                        int t = start;
                        start = end;
                        end = t;
                    }

                    var textSpan = TextSpan.FromBounds(start, end);
                    var lineColumnTextSpan = source?.GetLineColumnTextSpan(textSpan);
                    SourceTextBoxPosition = $"Range: {textSpan}; LineColumn: {lineColumnTextSpan}";
                }
            }
            catch
            {
                SourceTextBoxPosition = $"";
            }

            this.RaisePropertyChanged(nameof(SourceTextBoxPosition));
        }

        private void MatchingResultListBox_DoubleTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (matchResultListBox.SelectedItem is MatchResultViewModel matchResultViewModel)
            {
                var matchResult = matchResultViewModel.MatchResult;
                sourceTextBox.Focus();
                sourceTextBox.SelectionStart = matchResult.TextSpan.Start;
                sourceTextBox.SelectionLength = matchResult.TextSpan.Length;
                sourceTextBox.CaretOffset = matchResult.TextSpan.End;
            }
        }

        public GuiLogger SourceLogger { get; }

        public Settings Settings => ServiceLocator.Settings;

        public string SourceTextBoxPosition { get; set; }

        public ObservableCollection<Stage> Stages { get; } =
            new ObservableCollection<Stage>(new[] {Stage.ParseTree, Stage.Ust, Stage.Match});

        public Stage Stage
        {
            get => Settings.SelectedStage;
            set
            {
                if (Settings.SelectedStage != value)
                {
                    Settings.SelectedStage = value;
                    Settings.Save();
                    this.RaisePropertyChanged(nameof(IsMatchingStage));
                    this.RaisePropertyChanged(nameof(IsParseTreeVisible));
                    this.RaisePropertyChanged(nameof(IsUstJsonVisible));
                    this.RaisePropertyChanged();
                    CheckSource();
                }
            }
        }

        public ObservableCollection<Language> Languages
        {
            get
            {
                var result = new List<Language>(LanguageUtils.LanguagesWithParser.Count + 1);
                result.AddRange(LanguageUtils.LanguagesWithParser);
                result.Add(Language.Json);
                return new ObservableCollection<Language>(result);
            }
        }

        public Language SelectedLanguage
        {
            get => Settings.SourceLanguage;
            set
            {
                if (Settings.SourceLanguage != value)
                {
                    highlightings.TryGetValue(value, out string highlighting);
                    sourceTextBox.SyntaxHighlighting = highlighting != null
                        ? HighlightingManager.Instance.GetDefinition(highlighting)
                        : null;

                    Settings.SourceLanguage = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(IsTokensVisible));
                    this.RaisePropertyChanged(nameof(IsJavaScriptTypeVisible));
                    this.RaisePropertyChanged(nameof(IsParseTreeVisible));
                    this.RaisePropertyChanged(nameof(IsUstJsonVisible));
                    CheckSource();
                }
            }
        }

        public ObservableCollection<JavaScriptType> JavaScriptTypes { get; }
            = new ObservableCollection<JavaScriptType>((JavaScriptType[])Enum.GetValues(typeof(JavaScriptType)));

        public JavaScriptType JavaScriptType
        {
            get => Settings.JavaScriptType;
            set
            {
                if (Settings.JavaScriptType != value)
                {
                    Settings.JavaScriptType = value;
                    Settings.Save();
                    this.RaisePropertyChanged(nameof(JavaScriptType));
                    CheckSource();
                }
            }
        }

        public bool IsJavaScriptTypeVisible => SelectedLanguage == Language.JavaScript;

        public ReactiveCommand OpenSourceFile { get; }

        public ReactiveCommand SaveSourceFile { get; }

        public ReactiveCommand ReloadFile { get; }

        public ReactiveCommand Reset { get; }

        public ReactiveCommand OpenDumpDirectory { get; }

        public string OpenedFullFileName => sourceFileName;

        public string OpenedFileName
        {
            get => Path.GetFileName(sourceFileName);
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    SelectedLanguage = languageDetector.DetectIfRequired(value, out TimeSpan _).Language;
                }
                Settings.SourceFile = value;
                Settings.Save();
                this.RaiseAndSetIfChanged(ref sourceFileName, value);
            }
        }

        public string SourceErrorsText
        {
            get => sourceErrorsText;
            set => this.RaiseAndSetIfChanged(ref sourceErrorsText, value);
        }

        public bool SourceErrorsIsVisible
        {
            get => sourceErrorsIsVisible;
            set => this.RaiseAndSetIfChanged(ref sourceErrorsIsVisible, value);
        }

        public ObservableCollection<ErrorViewModel> SourceErrors { get; } = new ObservableCollection<ErrorViewModel>();

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

        public bool IsTokensVisible => SelectedLanguage.HasAntlrParser();

        public bool IsParseTreeVisible => Stage >= Stage.ParseTree && SelectedLanguage != Language.Json;

        public bool IsUstJsonVisible => Stage >= Stage.Ust && SelectedLanguage != Language.Json;

        public string MatchingResultText
        {
            get => matchResultText;
            set => this.RaiseAndSetIfChanged(ref matchResultText, value);
        }

        public ObservableCollection<MatchResultViewModel> MatchingResults { get; } = new ObservableCollection<MatchResultViewModel>();

        public bool IsMatchingStage => Stage >= Stage.Match;

        public bool IsErrorsExpanded
        {
            get => Settings.IsErrorsExpanded;
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
            get => Settings.IsTokensExpanded;
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
            get => Settings.IsParseTreeExpanded;
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
            get => Settings.IsUstExpanded;
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
            get => Settings.IsMatchingsExpanded;
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
            get => Settings.IsIncludeTextSpans;
            set
            {
                if (Settings.IsIncludeTextSpans != value)
                {
                    Settings.IsIncludeTextSpans = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                    CheckSource();
                    ServiceLocator.PatternsViewModel.CheckPattern();
                }
            }
        }

        public bool IsLinearTextSpans
        {
            get => Settings.IsLinearTextSpans;
            set
            {
                if (Settings.IsLinearTextSpans != value)
                {
                    Settings.IsLinearTextSpans = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                    CheckSource();
                    ServiceLocator.PatternsViewModel.CheckPattern();
                }
            }
        }

        public bool IsIncludeCode
        {
            get => Settings.IsIncludeCode;
            set
            {
                if (Settings.IsIncludeCode != value)
                {
                    Settings.IsIncludeCode = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                    CheckSource();
                    ServiceLocator.PatternsViewModel.CheckPattern();
                }
            }
        }

        public bool IsLeftRightDir
        {
            get => Settings.IsLeftRightDir;
            set
            {
                if (Settings.IsLeftRightDir != value)
                {
                    Settings.IsLeftRightDir = value;
                    Settings.Save();
                    this.RaisePropertyChanged();
                    CheckSource();
                }
            }
        }

        private void CheckSource()
        {
            if (oldSource != sourceTextBox.Text ||
                oldSelectedLanguage != Settings.SourceLanguage ||
                oldEndStage != Settings.SelectedStage ||
                oldJavaScriptType != Settings.JavaScriptType ||
                oldIsIncludeTextSpans != Settings.IsIncludeTextSpans ||
                oldIsLinearTextSpans != Settings.IsLinearTextSpans ||
                oldIsIncludeCode != Settings.IsIncludeCode ||
                oldIsLeftRightDir != Settings.IsLeftRightDir)
            {
                Dispatcher.UIThread.InvokeAsync(SourceErrors.Clear);
                Settings.Source = !string.IsNullOrEmpty(OpenedFileName) ? "" : sourceTextBox.Text;
                Settings.Save();

                RunWorkflow();

                oldSource = sourceTextBox.Text;
                oldSelectedLanguage = Settings.SourceLanguage;
                oldEndStage = Settings.SelectedStage;
                oldJavaScriptType = Settings.JavaScriptType;
                oldIsIncludeTextSpans = Settings.IsIncludeTextSpans;
                oldIsLinearTextSpans = Settings.IsLinearTextSpans;
                oldIsIncludeCode = Settings.IsIncludeCode;
                oldIsLeftRightDir = Settings.IsLeftRightDir;
            }
        }

        internal void RunWorkflow()
        {
            SourceLogger.Clear();

            string origFileName = "source";
            Language? selectedLanguage;
            if (SelectedLanguage == Language.Json)
            {
                origFileName += "." + ParseTreeDumper.UstSuffix;
                selectedLanguage = null;
            }
            else
            {
                origFileName += SelectedLanguage.GetExtensions()[0];
                selectedLanguage = SelectedLanguage;
            }

            var sourceRep = new MemorySourceRepository(sourceTextBox.Text, origFileName, selectedLanguage);
            IPatternsRepository patternRepository;
            if (!string.IsNullOrEmpty(ServiceLocator.PatternsViewModel.Value))
            {
                patternRepository = new DslPatternRepository(ServiceLocator.PatternsViewModel.Value,
                    ServiceLocator.PatternsViewModel.SelectedPattern.Languages);
            }
            else
            {
                patternRepository = new MemoryPatternsRepository();
            }
            var workflow = new Workflow(sourceRep, patternRepository, Stage)
            {
                IndentedDump = true,
                DumpWithTextSpans = IsIncludeTextSpans,
                LineColumnTextSpans = !IsLinearTextSpans,
                IncludeCodeInDump = IsIncludeCode,
                Logger = SourceLogger,
                RenderFormat = GraphvizOutputFormat.Svg,
                DumpDir = ServiceLocator.TempDirectory,
                RenderStages = new HashSet<Stage> { Stage.Ust },
                RenderDirection = IsLeftRightDir ? GraphvizDirection.LeftRight : GraphvizDirection.TopBottom
            };
            if (SelectedLanguage == Language.JavaScript)
            {
                workflow.JavaScriptType = JavaScriptType;
            }

            if (SelectedLanguage != Language.Json)
            {
                var dumpStages = new HashSet<Stage>();

                if (Stage >= Stage.Tokens)
                {
                    dumpStages.Add(Stage.Tokens);
                    if (Stage >= Stage.ParseTree)
                    {
                        if (SelectedLanguage != Language.CSharp)
                        {
                            // TODO: ignore C# parse tree dump due to the huge size
                            dumpStages.Add(Stage.ParseTree);
                        }

                        if (Stage >= Stage.Ust)
                        {
                            dumpStages.Add(Stage.Ust);
                        }
                    }
                }

                workflow.DumpStages = dumpStages;
            }

            workflow.FileRead += (obj, file) => source = (TextFile)file;
            WorkflowResult workflowResult = workflow.Process();
            UpdateSourceSelection(true);

            ParseTreeDumper dumper = Utils.CreateParseTreeDumper(SelectedLanguage);

            string tokensFileName = Path.Combine(ServiceLocator.TempDirectory, origFileName + "." + ParseTreeDumper.TokensSuffix);
            string parseTreeFileName = Path.Combine(ServiceLocator.TempDirectory, origFileName + "." + dumper.ParseTreeSuffix);
            Tokens = FileExt.Exists(tokensFileName) ? FileExt.ReadAllText(tokensFileName) : "";
            ParseTree = FileExt.Exists(parseTreeFileName) ? FileExt.ReadAllText(parseTreeFileName) : "";

            TokensHeader = "Tokens" + (SelectedLanguage.HasAntlrParser() ? " (ANTLR)" : "");
            ParseTreeHeader = "Parse Tree" + (SelectedLanguage.HasAntlrParser() ? " (ANTLR)" : "");

            if (Stage >= Stage.Ust && SelectedLanguage != Language.Json)
            {
                try
                {
                    UstJson = FileExt.ReadAllText(Path.Combine(ServiceLocator.TempDirectory, "", $"{origFileName}.{ParseTreeDumper.UstSuffix}"));
                }
                catch
                {
                    UstJson = "";
                }
            }

            MatchingResultText = "MATCHES" + (workflowResult.TotalMatchesCount > 0 ? $" ({workflowResult.TotalMatchesCount})" : "");

            if (SourceLogger.ErrorCount == 0)
            {
                SourceErrorsIsVisible = false;
                SourceErrorsText = "ERRORS";
            }
            else
            {
                SourceErrorsIsVisible = true;
                SourceErrorsText = $"ERRORS ({SourceLogger.ErrorCount})";
            }
        }
    }
}
