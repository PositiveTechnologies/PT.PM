using PT.PM.Matching;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace PT.PM.PatternEditor
{
    public class PatternViewModel : ReactiveObject
    {
        public PatternDto PatternDto { get; }

        public PatternViewModel(PatternDto patternDto)
        {
            PatternDto = patternDto ?? throw new NullReferenceException(nameof(patternDto));
        }

        public string Name
        {
            get => PatternDto.Name;
            set
            {
                if (PatternDto.Name != value)
                {
                    PatternDto.Name = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string Key
        {
            get => PatternDto.Key;
            set
            {
                if (PatternDto.Key != value)
                {
                    PatternDto.Key = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string CweId
        {
            get => PatternDto.CweId;
            set
            {
                if (PatternDto.CweId != value)
                {
                    PatternDto.CweId = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string Description
        {
            get => PatternDto.Description;
            set
            {
                if (PatternDto.Description != value)
                {
                    PatternDto.Description = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public string Value
        {
            get => PatternDto.Value;
            set
            {
                if (PatternDto.Value != value)
                {
                    PatternDto.Value = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(Title));
                }
            }
        }

        public HashSet<string> Languages => PatternDto.Languages;


        public bool AddLanguage(string language)
        {
            if (!PatternDto.Languages.Contains(language))
            {
                PatternDto.Languages.Add(language);
                this.RaisePropertyChanged(nameof(Title));
                return true;
            }
            return false;
        }

        public bool RemoveLanguage(string language)
        {
            if (PatternDto.Languages.Contains(language) && PatternDto.Languages.Count > 1)
            {
                PatternDto.Languages.Remove(language);
                this.RaisePropertyChanged(nameof(Title));
                return true;
            }
            return false;
        }

        public string Title => PatternDto.ToString();

        public override string ToString()
        {
            return PatternDto.ToString();
        }
    }
}
