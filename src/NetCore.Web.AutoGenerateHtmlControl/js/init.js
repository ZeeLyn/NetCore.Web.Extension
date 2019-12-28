window.onload = function () {
    if ($("script[id='uploader']")) {
        $("body").append("<script id='uploader' src='/resources/auto_generate_html_control/js/uploader.min.js'></script>");
    }
}