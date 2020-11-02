﻿using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.UI.Presentation.DesignTime;
using pdfforge.PDFCreator.UI.Presentation.DesignTime.Helper;
using pdfforge.PDFCreator.UI.Presentation.Helper.Tokens;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles.SelectFiles;
using pdfforge.PDFCreator.Utilities.Pdf;
using pdfforge.PDFCreator.Utilities.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles.ModifyTab.Cover
{
    public class CoverUserControlViewModel : ProfileUserControlViewModel<CoverSettingsTranslation>
    {
        private readonly ITokenHelper _tokenHelper;
        private readonly IPdfVersionHelper _pdfVersionHelper;
        private readonly ISelectFilesUserControlViewModelFactory _selectFilesUserControlViewModelFactory;

        public TokenReplacer TokenReplacer { get; private set; }

        public SelectFilesUserControlViewModel CoverPageSelectFilesUserControlViewModel { get; set; }

        public CoverUserControlViewModel(ITranslationUpdater translationUpdater,
            ISelectedProfileProvider selectedProfile, ITokenHelper tokenHelper,
            IDispatcher dispatcher, IPdfVersionHelper pdfVersionHelper,
            ISelectFilesUserControlViewModelFactory selectFilesUserControlViewModelFactory)
            : base(translationUpdater, selectedProfile, dispatcher)
        {
            _tokenHelper = tokenHelper;
            _pdfVersionHelper = pdfVersionHelper;
            _selectFilesUserControlViewModelFactory = selectFilesUserControlViewModelFactory;
        }

        public override void MountView()
        {
            if (_tokenHelper != null)
            {
                TokenReplacer = _tokenHelper.TokenReplacerWithPlaceHolders;
                var tokens = _tokenHelper.GetTokenListForExternalFiles();
                var filter = Translation.PDFFiles
                             + @" (*.pdf)|*.pdf|"
                             + Translation.AllFiles
                             + @" (*.*)|*.*";

                CoverPageSelectFilesUserControlViewModel = _selectFilesUserControlViewModelFactory.Builder()
                    .WithTitleGetter(() => Translation.SelectCoverFile)
                    .WithFileListGetter(profile => profile.CoverPage.Files)
                    .WithFileFilter(filter)
                    .WithTokens(tokens)
                    .Build();
                CoverPageSelectFilesUserControlViewModel.PropertyChanged += (s, a) =>
                {
                    if (a.PropertyName == nameof(CoverPageSelectFilesUserControlViewModel.FileListDictionary))
                    {
                        CheckIfVersionIsPdf20();
                    }
                };

                RaisePropertyChanged(nameof(CoverPageSelectFilesUserControlViewModel));
                CoverPageSelectFilesUserControlViewModel.MountView();
            }

            CheckIfVersionIsPdf20();

            base.MountView();
        }

        public override void UnmountView()
        {
            base.UnmountView();
            CoverPageSelectFilesUserControlViewModel.UnmountView();
        }

        private void CheckIfVersionIsPdf20()
        {
            Pdf20Files = CurrentProfile.CoverPage.Files.FindAll(_pdfVersionHelper.CheckIfVersionIsPdf20);

            RaisePropertyChanged(nameof(IsAnyPdf20));
            RaisePropertyChanged(nameof(Pdf20Files));
        }

        public bool IsAnyPdf20 => Pdf20Files?.Any() ?? false;

        public List<string> Pdf20Files
        {
            get;
            set;
        }
    }

    public class DesignTimeCoverUserControlViewModel : CoverUserControlViewModel
    {
        public DesignTimeCoverUserControlViewModel() : base(new DesignTimeTranslationUpdater(), new DesignTimeCurrentSettingsProvider(),
                                                null, null,
                                                new PdfVersionHelper(), new DesignTimeSelectFilesUserControlViewModelFactory())
        {
        }
    }
}
