/*global $, jQuery, tinyMCE, tinymce */
/// <reference path="jquery-1.8.3.js" />
/// <reference path="jquery-ui-1.8.24.js" />
/// <reference path="modernizr-2.6.2.js" />
/// <reference path="tinymce/tinymce.jquery.js" />
/// <reference path="tinymce/tiny_mce_jquery.js" />
(function () {
    "use strict";

    $(document).ready(function () {

        $('textarea.rich-text').tinymce({
            mode: "exact",
            theme: "advanced",
            plugins: "safari,spellchecker,paste",
            gecko_spellcheck: true,
            theme_advanced_buttons1: "bold,italic,underline,|,undo,redo,|,spellchecker,code",
            theme_advanced_statusbar_location: "none",
            spellchecker_rpc_url: "/TinyMCESpellcheckGateway", //<-- point TinyMCE to GoolgeSpell adaptor controller
            /*strip pasted microsoft office styles*/
            paste_strip_class_attributes: "mso"
        });
       
    });
}());