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
using PT.PM.PatternEditor.ViewModels;
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
    public class MainWindowViewModel : ReactiveObject
    {
        private Window window;
        private ColumnDefinition patternsPanelColumn;
        private TextBox sourceCodeTextBox;
        private ListBox sourceCodeErrorsListBox;
        private ListBox matchResultListBox;
        private TextBox logger;
        private string oldSelectedLanguage;
        private string sourceCodeFileName;
        private bool fileOpened;
        private string oldSourceCode = "";
        private Stage oldEndStage;
        private JavaScriptType oldJavaScriptType;
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
        private bool oldIsLinearTextSpans;
        private bool oldIsIncludeCode;
        private bool oldIsLeftRightDir;
        private CodeFile sourceCode;

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
                GuiUtils.ProcessErrorOnDoubleClick(sourceCodeErrorsListBox, sourceCodeTextBox);
            };
            matchResultListBox.DoubleTapped += MatchingResultListBox_DoubleTapped;

            SourceCodeLogger = GuiLogger.CreateSourceCodeLogger(SourceCodeErrors);
            languageDetector.Logger = SourceCodeLogger;

            OpenSourceCodeFile = ReactiveCommand.Create(async () =>
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

            SaveSourceCodeFile = ReactiveCommand.Create(() =>
            {
                if (!string.IsNullOrEmpty(sourceCodeFileName))
                {
                    File.WriteAllText(sourceCodeFileName, sourceCodeTextBox.Text);
                }
            });

            ReloadFile = ReactiveCommand.Create(() =>
            {
                if (!string.IsNullOrEmpty(sourceCodeFileName))
                {
                    sourceCodeTextBox.Text = File.ReadAllText(sourceCodeFileName);
                }
            });

            Reset = ReactiveCommand.Create(() =>
            {
                OpenedFileName = "";
                sourceCodeTextBox.Text = "";
            });

            OpenDumpDirectory = ReactiveCommand.Create(() =>
            {
                try
                {
                    GuiUtils.OpenDirectory(ServiceLocator.TempDirectory);
                }
                catch (Exception ex)
                {
                    new MessageBox($"Unable to open {ServiceLocator.TempDirectory} due to {ex}").ShowDialog();
                }
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

            sourceCodeTextBox.GetObservable(TextBox.SelectionStartProperty)
                .Subscribe(UpdateSourceCodeSelection);
            sourceCodeTextBox.GetObservable(TextBox.SelectionEndProperty)
                .Subscribe(UpdateSourceCodeSelection);

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
                    ServiceLocator.PatternsViewModel.SavePatterns();
                    Settings.PatternsPanelWidth = patternsPanelColumn.Width.Value;
                    Settings.Save();
                });
        }

        private void UpdateSourceCodeSelection(int index)
        {
            if (sourceCode != null)
            {
                int start = sourceCodeTextBox.SelectionStart;
                int end = sourceCodeTextBox.SelectionEnd;
                if (start > end)
                {
                    int t = start;
                    start = end;
                    end = t;
                }
                var textSpan = TextSpan.FromBounds(start, end);
                var lineColumnTextSpan = sourceCode.GetLineColumnTextSpan(textSpan);
                SourceCodeTextBoxPosition = $"Range: {lineColumnTextSpan}; LineColumn: {textSpan}";
            }
            else
            {
                SourceCodeTextBoxPosition = $"";
            }
            Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(nameof(SourceCodeTextBoxPosition)));
        }

        private void MatchingResultListBox_DoubleTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (matchResultListBox.SelectedItem is MatchResultViewModel matchResultViewModel)
            {
                var matchResult = matchResultViewModel.MatchResult;
                sourceCodeTextBox.Focus();
                sourceCodeTextBox.SelectionStart = matchResult.TextSpan.Start;
                sourceCodeTextBox.SelectionEnd = matchResult.TextSpan.End;
                sourceCodeTextBox.CaretIndex = sourceCodeTextBox.SelectionEnd;
            }
        }

        public GuiLogger SourceCodeLogger { get; }

        public Settings Settings => ServiceLocator.Settings;

        public string SourceCodeTextBoxPosition { get; set; }

        public ObservableCollection<Stage> Stages { get; } = new ObservableCollection<Stage>(new[] { Stage.ParseTree, Stage.Ust, Stage.Match });

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
                    this.RaisePropertyChanged(nameof(IsUstJsonVisible));
                    this.RaisePropertyChanged();
                    CheckSourceCode();
                }
            }
        }

        public ObservableCollection<Language> Languages { get; }
            = new ObservableCollection<Language>(LanguageUtils.LanguagesWithParser);

        public Language SelectedLanguage
        {
            get
            {
                HashSet<Language> languages = LanguageUtils.ParseLanguages(Settings.SourceCodeLanguage);
                if (languages.Count > 0)
                {
                    return languages.First();
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
                    CheckSourceCode();
                }
            }
        }

        public bool IsJavaScriptTypeVisible => SelectedLanguage == JavaScript.Language;

        public ReactiveCommand OpenSourceCodeFile { get; }

        public ReactiveCommand SaveSourceCodeFile { get; }

        public ReactiveCommand ReloadFile { get; }

        public ReactiveCommand Reset { get; }

        public ReactiveCommand OpenDumpDirectory { get; }

        public string OpenedFullFileName => sourceCodeFileName;

        public string OpenedFileName
        {
            get => Path.GetFileName(sourceCodeFileName);
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    SelectedLanguage = languageDetector.DetectIfRequired(value).Language;
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

        public ObservableCollection<ErrorViewModel> SourceCodeErrors { get; } = new ObservableCollection<ErrorViewModel>();

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

        public bool IsTokensVisible => SelectedLanguage?.HaveAntlrParser == true;

        public bool IsTreeVisible => SelectedLanguage?.HaveAntlrParser == true;

        public bool IsUstJsonVisible => Stage >= Stage.Ust;

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
                    CheckSourceCode();
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
                    CheckSourceCode();
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
                    CheckSourceCode();
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
                oldIsIncludeTextSpans != Settings.IsIncludeTextSpans ||
                oldIsLinearTextSpans != Settings.IsLinearTextSpans ||
                oldIsIncludeCode != Settings.IsIncludeCode ||
                oldIsLeftRightDir != Settings.IsLeftRightDir)
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
                oldIsLinearTextSpans = Settings.IsLinearTextSpans;
                oldIsIncludeCode = Settings.IsIncludeCode;
                oldIsLeftRightDir = Settings.IsLeftRightDir;
            }
        }

        internal void RunWorkflow()
        {
            SourceCodeLogger.Clear();

            var sourceCodeRep = new MemoryCodeRepository(sourceCodeTextBox.Text, language: SelectedLanguage ?? CSharp.Language);
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
            var workflow = new Workflow(sourceCodeRep, patternRepository, stage: Stage)
            {
                IndentedDump = true,
                DumpWithTextSpans = IsIncludeTextSpans,
                LineColumnTextSpans = !IsLinearTextSpans,
                IncludeCodeInDump = IsIncludeCode,
                Logger = SourceCodeLogger,
                RenderFormat = GraphvizOutputFormat.Svg,
                DumpDir = ServiceLocator.TempDirectory,
                RenderStages = new HashSet<Stage>() { Stage.Ust },
                RenderDirection = IsLeftRightDir ? GraphvizDirection.LeftRight : GraphvizDirection.TopBottom
            };
            if (SelectedLanguage == JavaScript.Language)
            {
                workflow.JavaScriptType = JavaScriptType;
            }

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

            WorkflowResult workflowResult = workflow.Process();
            IEnumerable<MatchResultDto> matchResults = workflowResult.MatchResults.ToDto();
            sourceCode = workflowResult.SourceCodeFiles.FirstOrDefault();

            string tokensFileName = Path.Combine(ServiceLocator.TempDirectory, ParseTreeDumper.TokensSuffix);
            string parseTreeFileName = Path.Combine(ServiceLocator.TempDirectory, ParseTreeDumper.ParseTreeSuffix);
            Tokens = File.Exists(tokensFileName) ? File.ReadAllText(tokensFileName) : "";
            ParseTree = File.Exists(parseTreeFileName) ? File.ReadAllText(parseTreeFileName) : "";

            TokensHeader = "Tokens" + (SelectedLanguage?.HaveAntlrParser == true ? " (ANTLR)" : "");
            ParseTreeHeader = "Parse Tree" + (SelectedLanguage?.HaveAntlrParser == true ? " (ANTLR)" : "");

            if (Stage >= Stage.Ust && workflowResult.Usts.FirstOrDefault() != null)
            {
                UstJson = File.ReadAllText(Path.Combine(ServiceLocator.TempDirectory, "", ParseTreeDumper.UstSuffix));
            }

            MatchingResultText = "MATCHINGS" + (matchResults.Count() > 0 ? $" ({matchResults.Count()})" : "");

            if (SourceCodeLogger.ErrorCount == 0)
            {
                SourceCodeErrorsIsVisible = false;
                SourceCodeErrorsText = "ERRORS";
            }
            else
            {
                SourceCodeErrorsIsVisible = true;
                SourceCodeErrorsText = $"ERRORS ({SourceCodeLogger.ErrorCount})";
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                MatchingResults.Clear();
                foreach (MatchResultDto matchResult in matchResults)
                {
                    MatchingResults.Add(new MatchResultViewModel(matchResult));
                }
            });
        }

        private void DetectLanguageIfRequired()
        {
            if (!fileOpened && (!string.IsNullOrEmpty(sourceCodeTextBox.Text) && string.IsNullOrEmpty(oldSourceCode)))
            {
                Task.Factory.StartNew(() =>
                {
                    Language detectedLanguage = languageDetector.Detect(new CodeFile(sourceCodeTextBox.Text)).Language;
                    Dispatcher.UIThread.InvokeAsync(() => SelectedLanguage = detectedLanguage);
                });
                Dispatcher.UIThread.InvokeAsync(() => OpenedFileName = "");
            }
            
            fileOpened = false;
        }
    }
}
