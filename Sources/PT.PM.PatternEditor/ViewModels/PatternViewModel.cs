using PT.PM.Matching;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace PT.PM.PatternEditor
{
    public class PatternViewModel : ReactiveObject
    {
        public PatternDto patternDto { get; }
            LineColumnTextSpans = true

        public PatternViewModel(PatternDto patternDto)
        {
            this.patternDto = patternDto ?? throw new NullReferenceException(nameof(patternDto));
        }

        public string Name
        {
            get => patternDto.Name;
            set
            {
                if (patternDto.Name != value)
                {
                    patternDto.Name = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string Key
        {
            get => patternDto.Key;
            set
            {
                if (patternDto.Key != value)
                {
                    patternDto.Key = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string CweId
        {
            get => patternDto.CweId;
            set
            {
                if (patternDto.CweId != value)
                {
                    patternDto.CweId = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string Description
        {
            get => patternDto.Description;
            set
            {
                if (patternDto.Description != value)
                {
                    patternDto.Description = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string Value
        {
            get => patternDto.Value;
            set
            {
                if (patternDto.Value != value)
                {
                    patternDto.Value = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public HashSet<string> Languages => patternDto.Languages;


        public bool AddLanguage(string language)
        {
            if (!patternDto.Languages.Contains(language))
            {
                patternDto.Languages.Add(language);
                this.RaisePropertyChanged(nameof(Title));
                return true;
            }
            return false;
        }

        public bool RemoveLanguage(string language)
        {
            if (patternDto.Languages.Contains(language) && patternDto.Languages.Count > 1)
            {
                patternDto.Languages.Remove(language);
                this.RaisePropertyChanged(nameof(Title));
                return true;
            }
            return false;
        }

        public string Title => patternDto.ToString();

        public override string ToString()
        {
            return patternDto.ToString();
        }
    }
}
