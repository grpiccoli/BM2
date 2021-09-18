var data = document.getElementById("params").dataset;
$('#documentViewer').FlowPaperViewer(
    {
        config: {
            PDFFile: `docs/${data.name}.pdf?reload=${data.reload}`,
            IMGFiles: `docs/${data.name}.pdf_{page}.jpg?reload=${data.reload}`,
            HighResIMGFiles: '',
            JSONFile: `docs/${data.name}.pdf_{page}.js?reload=${data.reload}`,
            ThumbIMGFiles: `docs/${data.name}.pdf_{page}_thumb.jpg?reload=${data.reload}`,
            SWFFile: '',
            FontsToLoad: ["g_font_1", "g_font_2", "g_font_3", "g_font_4", "g_font_5", "g_font_6", "g_font_7", "g_font_8"],

            Scale: 0.1,
            ZoomTransition: 'easeOut',
            ZoomTime: 0.4,
            ZoomInterval: 0.1,
            FitPageOnLoad: true,
            FitWidthOnLoad: false,
            AutoAdjustPrintSize: true,
            PrintPaperAsBitmap: false,
            AutoDetectLinks: true,
            ImprovedAccessibility: false,
            FullScreenAsMaxWindow: false,
            ProgressiveLoading: false,
            MinZoomSize: 0.1,
            MaxZoomSize: 10,
            SearchMatchAll: true,
            InitViewMode: 'Zine',
            RenderingOrder: 'html,html',
            StartAtPage: 1,
            EnableWebGL: true,
            PreviewMode: '',
            PublicationTitle: '',
            MixedMode: true,
            ViewModeToolsVisible: true,
            ZoomToolsVisible: true,
            NavToolsVisible: true,
            CursorToolsVisible: true,
            SearchToolsVisible: true,

            UIConfig: `UI_Zine.xml?reload=${data.reload}`,
            BrandingLogo: '',
            BrandingUrl: '',

            WMode: 'transparent',

            key: '#V2ZzcGBFXVpeTBhxAklCXlVZaw',
            TrackingNumber: '',
            localeDirectory: 'locale/',
            localeChain: data.locale
        }
    }
);

var url = window.location.href.toString();
if (location.length == 0) {
    url = document.URL.toString();
}
if (url.indexOf("file:") >= 0) {
    jQuery('#documentViewer');
}