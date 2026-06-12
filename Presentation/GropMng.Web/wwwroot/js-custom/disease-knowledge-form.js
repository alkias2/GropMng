// DiseaseKnowledge dual-quill form initialization.
// Attaches to the global scope so it can be called from Razor views.
(function () {
    'use strict';

    var GropDiseaseKnowledgeForm = {
        init: function (config) {
            var descEditor = document.getElementById(config.descriptionEditorId);
            var treatEditor = document.getElementById(config.treatmentEditorId);
            var descHidden = document.getElementById(config.descriptionHiddenId);
            var treatHidden = document.getElementById(config.treatmentHiddenId);

            if (!descEditor || !treatEditor) return;

            var descQuill = new Quill(descEditor, {
                theme: 'snow',
                modules: { toolbar: [['bold', 'italic', 'underline'], [{ list: 'ordered' }, { list: 'bullet' }], ['link', 'clean']] },
                placeholder: 'Description...'
            });

            var treatQuill = new Quill(treatEditor, {
                theme: 'snow',
                modules: { toolbar: [['bold', 'italic', 'underline'], [{ list: 'ordered' }, { list: 'bullet' }], ['link', 'clean']] },
                placeholder: 'Treatment guidelines...'
            });

            if (config.descriptionContent) descQuill.root.innerHTML = config.descriptionContent;
            if (config.treatmentContent) treatQuill.root.innerHTML = config.treatmentContent;

            var form = document.getElementById('diseaseKnowledgeForm');
            if (form) {
                form.addEventListener('submit', function () {
                    if (descHidden) descHidden.value = descQuill.root.innerHTML;
                    if (treatHidden) treatHidden.value = treatQuill.root.innerHTML;
                });
            }
        }
    };

    window.GropDiseaseKnowledgeForm = GropDiseaseKnowledgeForm;
})();