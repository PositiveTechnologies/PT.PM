using PT.PM.AntlrUtils;
using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Patterns;
using PT.PM.Patterns.Nodes;
using PT.PM.Patterns.PatternsRepository;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.PatternEditor
{
    public class MainWindowViewModel: ReactiveObject
    {
        private JsonUstNodeSerializer jsonSerializer = new JsonUstNodeSerializer(typeof(UstNode), typeof(PatternVarDef))
        {
            IncludeTextSpans = false,
            Indented = true,
            ExcludeNulls = true
        };

        private Window window;
        private ColumnDefinition patternsPanelColumn;
        private TextBox sourceCodeTextBox;
        private ListBox sourceCodeErrorsListBox;
        private ListBox matchingResultListBox;
        private TextBox logger;
        private GuiLogger sourceCodeLogger;
        private Language oldSelectedLanguage;
        private string sourceCodeFileName;
        private bool fileOpened;
        private string oldSourceCode = "";
        private Stage oldEndStage;
        private int sourceCodeSelectionStart, sourceCodeSelectionEnd;
        private LanguageDetector languageDetector = new ParserLanguageDetector();

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
            matchingResultListBox = window.Find<ListBox>("MatchingResult");
            logger = window.Find<TextBox>("Logger");

            patternsPanelColumn.Width = GridLength.Parse(Settings.PatternsPanelWidth.ToString(), CultureInfo.InvariantCulture);
            sourceCodeErrorsListBox.DoubleTapped += (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                GuiHelpers.ProcessErrorOnDoubleClick(sourceCodeErrorsListBox, sourceCodeTextBox);
            };
            matchingResultListBox.DoubleTapped += MatchingResultListBox_DoubleTapped;

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

            this.RaisePropertyChanged(nameof(SelectedLanguageInfo));
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
                    Settings.Width = width;
                    Settings.WindowState = window.WindowState;
                    Settings.Save();
                });

            window.GetObservable(Window.HeightProperty)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Subscribe(height =>
                {
                    Settings.Height = height;
                    Settings.WindowState = window.WindowState;
                    Settings.Save();
                });

            Observable.FromEventPattern(
                ev => window.Closed += ev, ev => window.Closed -= ev)
                .Subscribe(ev =>
                {
                    ServiceLocator.PatternViewModel.SavePatterns();
                    Settings.Left = window.Position.X;
                    Settings.Top = window.Position.Y;
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
            int line, column;
            TextHelper.LinearToLineColumn(caretIndex, sourceCodeTextBox.Text, out line, out column);
            SourceCodeTextBoxPosition = $"Caret: {line}:{column-1}";
            Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(nameof(SourceCodeTextBoxPosition)));
        }

        private void MatchingResultListBox_DoubleTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MathingResultDtoWrapper matchingResultWrapper = matchingResultListBox.SelectedItem as MathingResultDtoWrapper;
            if (matchingResultWrapper != null)
            {
                var matchingResult = matchingResultWrapper.MatchingResult;
                sourceCodeTextBox.Focus();
                sourceCodeTextBox.SelectionStart = TextHelper.LineColumnToLinear(sourceCodeTextBox.Text, matchingResult.BeginLine, matchingResult.BeginColumn);
                sourceCodeTextBox.SelectionEnd = TextHelper.LineColumnToLinear(sourceCodeTextBox.Text, matchingResult.EndLine, matchingResult.EndColumn);
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

        public ObservableCollection<Stage> Stages { get; } = new ObservableCollection<Stage>(new[] { Stage.Parse, Stage.Convert, Stage.Match });

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

        public ObservableCollection<LanguageInfo> Languages
        {
            get
            {
                return new ObservableCollection<LanguageInfo>(LanguageExt.LanguageInfos.Select(info => info.Value));
            }
        }
        
        public LanguageInfo SelectedLanguageInfo
        {
            get
            {
                return LanguageExt.LanguageInfos[Settings.SourceCodeLanguage];
            }
            set
            {
                if (Settings.SourceCodeLanguage != value.Language)
                {
                    Settings.SourceCodeLanguage = value.Language;
                    Settings.Save();
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(IsTokensVisible));
                    this.RaisePropertyChanged(nameof(IsTreeVisible));
                    CheckSourceCode();
                }
            }
        }

        public Language SelectedLanguage
        {
            get
            {
                return Settings.SourceCodeLanguage;
            }
            set
            {
                if (Settings.SourceCodeLanguage != value)
                {
                    Settings.SourceCodeLanguage = value;
                    Settings.Save();
                    this.RaisePropertyChanged(nameof(SelectedLanguageInfo));
                    CheckSourceCode();
                }
            }
        }

        public ReactiveCommand<object> OpenSourceCodeFile { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> SaveSourceCodeFile { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> ReloadFile { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> Reset { get; } = ReactiveCommand.Create();

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
                    Language? language = languageDetector.DetectIfRequired(value);
                    SelectedLanguage = language ?? Language.CSharp;
                }
                Settings.SourceCodeFile = value;
                Settings.Save();
                this.RaiseAndSetIfChanged(ref sourceCodeFileName, value);
            }
        }

        public string SourceCodeErrorsText { get; set; } = "Errors";

        public bool SourceCodeErrorsIsVisible { get; set; }

        public ObservableCollection<object> SourceCodeErrors { get; } = new ObservableCollection<object>();

        public string Tokens { get; set; }

        public string ParseTree { get; set; }

        public string UstJson { get; set; }

        public bool IsTokensVisible => SelectedLanguageInfo.HaveAntlrParser && IsDeveloperMode;

        public bool IsTreeVisible => SelectedLanguageInfo.HaveAntlrParser && IsDeveloperMode;

        public bool IsUstJsonVisible => Stage >= Stage.Convert && IsDeveloperMode;

        public string MatchingResultText { get; set; } = "MATCHINGS";

        public ObservableCollection<MathingResultDtoWrapper> MatchingResults { get; } = new ObservableCollection<MathingResultDtoWrapper>();

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

        private void CheckSourceCode()
        {
            if (oldSourceCode != sourceCodeTextBox.Text || oldSelectedLanguage != Settings.SourceCodeLanguage || oldEndStage != Settings.SelectedStage)
            {
                Dispatcher.UIThread.InvokeAsync(SourceCodeErrors.Clear);
                string sourceCode = sourceCodeTextBox.Text;
                DetectLanguageIfRequired();
                Settings.SourceCode = !string.IsNullOrEmpty(OpenedFileName) ? "" : sourceCode;
                Settings.Save();

                UpdateMatchings();

                oldSourceCode = sourceCodeTextBox.Text;
                oldSelectedLanguage = Settings.SourceCodeLanguage;
                oldEndStage = Settings.SelectedStage;
            }
        }

        internal void UpdateMatchings()
        {
            sourceCodeLogger.Clear();

            var sourceCodeRep = new MemoryCodeRepository(sourceCodeTextBox.Text);
            IPatternsRepository patternRepository;
            if (!string.IsNullOrEmpty(ServiceLocator.PatternViewModel.Value))
            {
                patternRepository = new DslPatternRepository(ServiceLocator.PatternViewModel.Value, ServiceLocator.PatternViewModel.Languages);
            }
            else
            {
                patternRepository = new MemoryPatternsRepository();
            }
            var workflow = new Workflow(sourceCodeRep, SelectedLanguage, patternRepository, stage: Stage);
            workflow.Logger = sourceCodeLogger;
            IEnumerable<MatchingResultDto> matchingResults = workflow.Process() ?? ArrayUtils<MatchingResultDto>.EmptyArray;

            if (IsDeveloperMode)
            {
                AntlrParseTree antlrParseTree = workflow.LastParseTree as AntlrParseTree;
                if (antlrParseTree != null && antlrParseTree.SyntaxTree != null)
                {
                    Antlr4.Runtime.Parser antlrParser = (workflow.ParserConverterSets[antlrParseTree.SourceLanguage].Parser as AntlrParser).Parser;
                    var tokensString = AntlrHelper.GetTokensString(antlrParseTree.Tokens, antlrParser.Vocabulary, onlyDefaultChannel: true);
                    string treeString = antlrParseTree.SyntaxTree.ToStringTreeIndented(antlrParser);

                    Tokens = tokensString;
                    ParseTree = treeString;
                    File.WriteAllText(Path.Combine(ServiceLocator.TempDirectory, "Tokens.txt"), Tokens);
                    File.WriteAllText(Path.Combine(ServiceLocator.TempDirectory, "Tree.txt"), ParseTree);
                }
                if (Stage >= Stage.Convert && workflow.LastUst != null)
                {
                    UstJson = jsonSerializer.Serialize(workflow.LastUst.Root);
                    File.WriteAllText(Path.Combine(ServiceLocator.TempDirectory, "UST.json"), UstJson);
                }
            }

            MatchingResultText = "MATCHINGS";
            if (matchingResults.Count() > 0)
            {
                MatchingResultText += " (" + matchingResults.Count() + ")";
            }

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
                foreach (var matchingResult in matchingResults)
                {
                    MatchingResults.Add(new MathingResultDtoWrapper(matchingResult));
                }
                this.RaisePropertyChanged(nameof(Tokens));
                this.RaisePropertyChanged(nameof(ParseTree));
                this.RaisePropertyChanged(nameof(UstJson));
                this.RaisePropertyChanged(nameof(MatchingResultText));
                this.RaisePropertyChanged(nameof(SourceCodeErrorsIsVisible));
                this.RaisePropertyChanged(nameof(SourceCodeErrorsText));
            });
        }

        private void DetectLanguageIfRequired()
        {
            string newSourceCode = sourceCodeTextBox.Text;
            if (!fileOpened && (!string.IsNullOrEmpty(newSourceCode) && string.IsNullOrEmpty(oldSourceCode)))
            {
                Task.Factory.StartNew(() =>
                {
                    var detectedLanguage = (Language)languageDetector.Detect(newSourceCode);
                    Dispatcher.UIThread.InvokeAsync(() => SelectedLanguage = detectedLanguage);
                });
                Dispatcher.UIThread.InvokeAsync(() => OpenedFileName = "");
            }
            
            fileOpened = false;
        }
    }
}
