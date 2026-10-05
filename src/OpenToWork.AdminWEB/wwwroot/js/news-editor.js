// Barra de formato del editor de Noticias (Markdown sencillo, ver NewsMarkdown en el servidor).
// Cambia el texto del textarea y lanza "input", el mismo evento que al escribir: asi Blazor recibe el
// cambio en orden con lo tecleado (devolver el valor competia con los eventos de escritura).
window.newsEditor = {
    // Rodea la seleccion: **negrita**, *cursiva*, [texto](https://).
    wrap: function (el, before, after, placeholder) {
        var start = el.selectionStart, end = el.selectionEnd, v = el.value;
        var selected = v.substring(start, end) || placeholder;
        el.value = v.substring(0, start) + before + selected + after + v.substring(end);
        el.focus();
        el.setSelectionRange(start + before.length, start + before.length + selected.length);
        el.dispatchEvent(new Event('input', { bubbles: true }));
    },
    // Antepone a cada linea seleccionada: "## ", "- ", "1. ". Deja una linea en blanco antes y despues
    // para que forme su propio bloque.
    prefixLines: function (el, prefix, numbered) {
        var v = el.value;
        var start = v.lastIndexOf('\n', el.selectionStart - 1) + 1;
        var endIdx = v.indexOf('\n', el.selectionEnd);
        var end = endIdx === -1 ? v.length : endIdx;
        var lines = v.substring(start, end).split('\n').map(function (l, i) {
            return (numbered ? (i + 1) + '. ' : prefix) + l.replace(/^(#{2,3} |- |\* |\d+\. )/, '');
        });
        var before = v.substring(0, start), after = v.substring(end);
        if (before && !before.endsWith('\n\n')) before += before.endsWith('\n') ? '\n' : '\n\n';
        if (after && !after.startsWith('\n\n')) after = (after.startsWith('\n') ? '\n' : '\n\n') + after;
        el.value = before + lines.join('\n') + after;
        el.focus();
        el.dispatchEvent(new Event('input', { bubbles: true }));
    }
};
